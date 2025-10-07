#if UNITY_ANDROID
using UnityEngine;
using System;

namespace Games.Coresdk.Unity
{
    class AndroidDeepLink : IDeepLinkBridge
    {
        AndroidJavaObject unityActivity;
        AndroidJavaObject native;

        public AndroidDeepLink()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            native = new AndroidJavaObject("games.coresdk.DeepLink");
        }

        public void OpenURL(string url, Action<string> onDeepLinkActivated)
        {
            native.CallStatic("openURL", unityActivity, url, new AndroidDeepLinkCallback(onDeepLinkActivated));
        }
    }

}
#endif