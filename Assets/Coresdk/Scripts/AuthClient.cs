using UnityEngine;
using System;
using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class AuthClient
    {
        const string PREF_KEY_TOKEN = "Coresdk.Unity.Token";

        Config m_config;
        string m_prefToken;
        DeepLink m_deepLink;
        MainThreadDispatcher m_mainThreadDispatcher;
        Coresdk m_sdk;

        Action<ProfileResult> m_loginCallback = null;
        Action<LogoutResult> m_logoutResult = null;
        Action<BindProfileResult> m_bindGameResult = null;

        public string Token
        {
            get
            {
                return m_prefToken;
            }
            private set
            {
                if (value == null)
                {
                    m_prefToken = value;
                    PlayerPrefs.DeleteKey(PREF_KEY_TOKEN);
                }
                else if (m_prefToken != value)
                {
                    m_prefToken = value;
                    PlayerPrefs.SetString(PREF_KEY_TOKEN, value);
                }
            }
        }

        public AuthClient(Coresdk sdk, Config config, DeepLink deepLink, MainThreadDispatcher mainThreadDispatcher)
        {
            m_sdk = sdk;
            m_config = config;
            m_deepLink = deepLink;
            m_prefToken = PlayerPrefs.GetString(PREF_KEY_TOKEN, "");

            m_mainThreadDispatcher = mainThreadDispatcher;
        }

        /// <summary>
        /// Login
        /// </summary>
        public void Login(string game_id, Action<ProfileResult> callback)
        {
            m_loginCallback = callback;

            if (string.IsNullOrEmpty(Token))
            {
                StartLoginDeepLink(game_id);
            }
            else
            {
                GetProfile((profileResult) =>
                {
                    if (profileResult.IsSuccess)
                    {
                        m_loginCallback(profileResult);
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey(PREF_KEY_TOKEN);
                        StartLoginDeepLink(game_id);
                    }
                });
            }
        }

        private void StartLoginDeepLink(string game_id)
        {
            const string endpoint = "login";

            var url = GetDeepLinkURL(endpoint, new Dictionary<string, string>() {
                { "login", "true" },
                { "game_id", game_id },
                { "lang",  m_sdk.language },
                { "platform",  m_sdk.platform },
            });

            m_deepLink.OpenURL(url, OnLoginDeepLinkCallback);
        }

        string GetDeepLinkURL(string endpoint, Dictionary<string, string> queries)
        {
            var dict = new Dictionary<string, string>()
            {
                { "merchantId", m_config.MerchantId },
                { "serviceId", m_config.ServiceId },
                { "callback", URLUtility.GetDeepLinkURL(Application.identifier) }
            };

            foreach (var kv in queries)
            {
                dict[kv.Key] = kv.Value;
            }
            var queryString = APIUtil.GetQueryString(dict);

            return string.Format("{0}/{1}/?{2}", m_config.Domain, endpoint, queryString);
        }

        private void OnLoginDeepLinkCallback(string url)
        {
            var token = URLUtility.GetAuthorizationToken(url);

            Token = token;
            GetProfile(m_loginCallback);
        }

        /// <summary>
        /// Logout
        /// </summary>
        public void Logout(Action<LogoutResult> callback)
        {
            m_logoutResult = callback;
            StartLogoutDeepLink();
        }

        private void StartLogoutDeepLink()
        {
            const string endpoint = "login";

            var url = GetDeepLinkURL(endpoint, new Dictionary<string, string>()
            {
                { "logout", "true" },
                { "lang",  m_sdk.language },
                { "platform",  m_sdk.platform },
            });

            m_deepLink.OpenURL(url, (token) =>
            {
                Token = "";
                m_logoutResult(new LogoutResult());
            });
        }

        /// <summary>
        /// Bind
        /// </summary>
        public void Bind(string game_id, string game_account, string jwt, Action<BindProfileResult> callback)
        {
            m_bindGameResult = callback;
            StartAccountBindGameDeepLink(game_id, game_account, jwt);
        }

        private void StartAccountBindGameDeepLink(string game_id, string game_account, string jwt)
        {
            const string endpoint = "bind";

            var url = GetDeepLinkURL(endpoint, new Dictionary<string, string>()
            {
                { "game_id", game_id },
                { "game_account", game_account },
                { "lang",  m_sdk.language },
                { "platform",  m_sdk.platform },
                { "jwt",  jwt },
            });

            m_deepLink.OpenURL(url, OnBindDeepLinkCallback);
        }

        private void OnBindDeepLinkCallback(string url)
        {
            var query = TokenCollection.Parse(url);
            var token = query.GetValue("token");
            var bindResult = query.GetValue("bind") == "success";

            if (bindResult)
            {
                Token = token;
            }

            GetProfile(_ => {
                var bindProfileResult = BindProfileResult.Parse(_, bindResult);

                m_bindGameResult(bindProfileResult);
            });
        }

        public void GetProfile(Action<ProfileResult> callback)
        {
            m_sdk.GetProfile((rawResponse) =>
            {
                callback(ProfileResult.Parse(rawResponse));
            });
        }
    }
}
