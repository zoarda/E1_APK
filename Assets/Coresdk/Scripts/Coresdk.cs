using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class Coresdk
    {
        public static string Token
        {
            get
            {
                if (Instance == null || Instance.m_authClient == null)
                {
                    Debug.LogError("Please first call Coresdk.Initialize().");
                    return string.Empty;
                }   

                return Instance.m_authClient.Token;
            }
        }

        /// <summary>
        /// Get the instance
        /// </summary>
        static Coresdk Instance;

        /// <summary>
        /// Get the config from configuration file
        /// </summary>
        static Config Config
        {
            get
            {
                return Instance.m_config;
            }
        }

        /// <summary>
        /// Get the auth client
        /// </summary>
        static AuthClient AuthClient
        {
            get
            {
                return Instance.m_authClient;
            }
        }

        /// <summary>
        /// Get the payment client
        /// </summary>
        static PaymentClient PaymentClient
        {
            get
            {
                return Instance.m_paymentClient;
            }
        }

        public string language;
        public string platform;

        private MainThreadDispatcher m_mainThreadDispatcher;

        private Config m_config;
        private DeepLink m_deepLink;
        private AuthClient m_authClient;
        private PaymentClient m_paymentClient;

        public static IEnumerator Initialize(Language lang, Platform platform, Action<bool> callback = null)
        {
            if (Instance == null)
            {
                var go = new GameObject(typeof(Coresdk).Name);
                GameObject.DontDestroyOnLoad(go);
                Instance = new Coresdk();
                Instance.m_mainThreadDispatcher = go.AddComponent<MainThreadDispatcher>();
            }

            SetPlatform(platform);
            SetLanguage(lang);
            yield return Instance.CoInit();

            if (callback != null)
                callback(Instance.m_config != null);
        }

        IEnumerator CoInit()
        {
            var configLoader = new ConfigLoader();
            yield return configLoader.CoInit();

            m_config = configLoader.config;
            m_deepLink = DeepLink.Initialize(m_mainThreadDispatcher);
            m_authClient = new AuthClient(this, m_config, m_deepLink, m_mainThreadDispatcher);
            m_paymentClient = new PaymentClient(this, m_authClient, m_config, m_mainThreadDispatcher);
        }

        public static string GetLoginURL()
        {
            return Config.Domain + "/login/";
        }

        public static void OpenLogin(string game_id, Action<ProfileResult> callback)
        {
            AuthClient.Login(game_id, callback);
        }

        public static void OpenLogout(Action<LogoutResult> callback)
        {
            AuthClient.Logout(callback);
        }

        public static void OpenAccountBindGame(string game_id, string game_account, string jwt, Action<BindProfileResult> callback)
        {
            AuthClient.Bind(game_id, game_account, jwt, callback);
        }

        public static void OpenPayment()
        {
            PaymentClient.Purchase();
        }

        public static void PostAccountBindGame(string game_id, string game_account, Action<AccountBindGameResult> callback)
        {
            const string endpoint = "/accountBindGame";

            var query = new Dictionary<string, string>()
            {
                { "game_id", game_id },
                { "game_account", game_account },
                { "platform",  Coresdk.Instance.platform },
            };

            PostRequest(endpoint, query, rawResponse => {
                var result = AccountBindGameResult.Parse(rawResponse);
                callback(result);
            });
        }

        public static void SetLanguage(Language lang)
        {
            Instance.language = lang.ToString();
        }

        public static void SetPlatform(Platform platform)
        {
            Instance.platform = platform.ToString();
        }

        public void PostGuestLogin(Action<LoginResult> callback)
        {
            const string endpoint = "/guestLogin";

            var query = new Dictionary<string, string>()
            {
            };

            PostRequest(endpoint, query, rawResponse => {
                callback(LoginResult.Parse(rawResponse));
            });
        }

        public void PostLogin(string account, string password, string game_id, Action<LoginResult> callback)
        {
            const string endpoint = "/accountLogin";

            var query = new Dictionary<string, string>()
            {
                { "account", account },
                { "password", Base64Util.ToBase64(password) },
                { "game_id", game_id },
                { "platform",  Coresdk.Instance.platform },
            };

            PostRequest(endpoint, query, rawResponse => {
                callback(LoginResult.Parse(rawResponse));
            });
        }

        public void PostAccountLoginBindGame(string account, string password, string game_id, string game_account, Action<AccountLoginBindGameResult> callback)
        {
            const string endpoint = "/accountLoginBindGame";

            var query = new Dictionary<string, string>()
            {
                { "account", account },
                { "password", Base64Util.ToBase64(password) },
                { "game_id", game_id },
                { "game_account", game_account },
                { "platform",  Coresdk.Instance.platform },
            };

            PostRequest(endpoint, query, rawResponse => {
                var result = AccountLoginBindGameResult.Parse(rawResponse);
                callback(result);
            });
        }

        public void PostCheckBindGameStatus(string game_id, Action<CheckBindGameStatusResult> callback)
        {
            const string endpoint = "/checkBindGameStatus";

            var query = new Dictionary<string, string>()
            {
                { "game_id", game_id },
                { "platform",  Coresdk.Instance.platform },
            };

            PostRequest(endpoint, query, rawResponse => {
                var result = CheckBindGameStatusResult.Parse(rawResponse);
                callback(result);
            });
        }

        public void GetProfile(Action<RawResponse> callback)
        {
            const string endpoint = "/getUserInfo";

            var query = new Dictionary<string, string>
                {
                    { "merchantId", Config.MerchantId },
                    { "serviceId", Config.ServiceId },
                    { "platform",  Coresdk.Instance.platform },
                };

            GetRequest(endpoint, query, callback);
        }

        static void GetRequest(string endpoint, Dictionary<string, string> query, Action<RawResponse> callback)
        {
            APIUtil.Get(Config.ApiDomain, endpoint, query, AuthClient.Token, callback, Instance.m_mainThreadDispatcher);
        }

        static void PostRequest(string endpoint, Dictionary<string, string> query, Action<RawResponse> callback)
        {
            APIUtil.Post(Config.ApiDomain, endpoint, query, AuthClient.Token, callback, Instance.m_mainThreadDispatcher);
        }
    }
}
