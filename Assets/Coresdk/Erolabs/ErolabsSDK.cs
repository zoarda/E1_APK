using System;
using System.Collections;

using Coresdk = Games.Coresdk.Unity.Coresdk;
using ProfileResult = Games.Coresdk.Unity.ProfileResult;
using LogoutResult = Games.Coresdk.Unity.LogoutResult;
using BindProfileResult = Games.Coresdk.Unity.BindProfileResult;
using AccountBindGameResult = Games.Coresdk.Unity.AccountBindGameResult;
using Language = Games.Coresdk.Unity.Language;
using Platform = Games.Coresdk.Unity.Platform;

namespace Erolabs.Sdk.Unity
{
    public class ErolabsSDK
    {
        public static string Token
        {
            get
            {
                return Coresdk.Token;
            }
        }

        public static IEnumerator Initialize(Action<bool> callback = null, Language lang = Language.en, Platform platform = Platform.R18)
        {
            Games.Coresdk.Unity.ConfigLoader.JSON_FILENAME = "erolabs_domain_v1";
            return Coresdk.Initialize(lang, platform, callback);
        }

        public static void SetLanguage(Language lang)
        {
            Coresdk.SetLanguage(lang);
        }

        public static void SetPlatform(Platform platform)
        {
            Coresdk.SetPlatform(platform);
        }

        public static string GetLoginURL()
        {
            return Coresdk.GetLoginURL();
        }

        public static void OpenLogin(string game_id, Action<ProfileResult> callback)
        {
            Coresdk.OpenLogin(game_id, callback);
        }

        public static void OpenLogout(Action<LogoutResult> callback)
        {
            Coresdk.OpenLogout(callback);
        }

        public static void OpenAccountBindGame(string game_id, string game_account, Action<BindProfileResult> callback)
        {
            Coresdk.OpenAccountBindGame(game_id, game_account, Token, callback);
        }

        public static void OpenPayment()
        {
            Coresdk.OpenPayment();
        }

        public static void PostAccountBindGame(string game_id, string game_account, Action<AccountBindGameResult> callback)
        {
            Coresdk.PostAccountBindGame(game_id, game_account, callback);
        }
    }
}
