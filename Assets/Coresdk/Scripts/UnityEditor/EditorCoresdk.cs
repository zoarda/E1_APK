using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Games.Coresdk.Unity.Editor
{
    public class EditorCoresdk
    {
        static Coresdk sdk
        {
            get
            {
                if (_sdk != null)
                    return _sdk;

                var field = typeof(Coresdk).GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                _sdk = (Coresdk)field.GetValue(null);

                return _sdk;
            }
        }

        static Coresdk _sdk;

        public static void PostLogin(string account, string password, string game_id, Action<LoginResult> callback)
        {
            sdk.PostLogin(account, password, game_id, callback);
        }

        public static void PostAccountBindGame(string account, string password, string game_id, string game_account, Action<AccountLoginBindGameResult> callback)
        {
            sdk.PostAccountLoginBindGame(account, password, game_id, game_account, callback);
        }

        public static void PostGuestLogin(Action<LoginResult> callback)
        {
            sdk.PostGuestLogin(callback);
        }
    }
}