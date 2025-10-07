using System;

namespace Games.Coresdk.Unity
{
    public class DeepLink
    {
        IDeepLinkBridge m_bridge;
        Action<string> m_callback;
        MainThreadDispatcher m_mainThreadDispatcher;

        public static DeepLink Initialize(MainThreadDispatcher mainThreadDispatcher)
        {
            var deepLink = new DeepLink();
            deepLink.m_mainThreadDispatcher = mainThreadDispatcher;

#if UNITY_EDITOR
            deepLink.m_bridge = new UnityDeepLink(mainThreadDispatcher.gameObject);
#elif UNITY_ANDROID
            deepLink.m_bridge = new AndroidDeepLink();
#elif UNITY_IPHONE
            deepLink.m_bridge = new iOSDeepLink();
#endif

            return deepLink;
        }

        public void OpenURL(string url, Action<string> callback)
        {
            m_callback = callback;
            m_bridge.OpenURL(url, OnDeepLinkActivated);

            if (UnityEngine.Application.isEditor || UnityEngine.Debug.isDebugBuild)
            {
                UnityEngine.Debug.Log(url);
            }
        }

        void OnDeepLinkActivated(string url)
        {
            m_mainThreadDispatcher.Add(() => {
                m_callback(url);
                if (UnityEngine.Application.isEditor || UnityEngine.Debug.isDebugBuild)
                {
                    UnityEngine.Debug.Log(url);
                }
            });
        }
    }
}
