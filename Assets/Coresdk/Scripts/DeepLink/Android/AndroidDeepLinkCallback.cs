#if UNITY_ANDROID
using UnityEngine;

namespace Games.Coresdk.Unity
{
    public class AndroidDeepLinkCallback : AndroidJavaProxy
    {
        System.Action<string> m_callback;

        public AndroidDeepLinkCallback(System.Action<string> callback) : base("games.coresdk.DeepLinkCallback")
        {
            m_callback = callback;
        }

        public void onSuccess(string url)
        {
            HandleCallback(url);
        }

        private void HandleCallback(string url)
        {
            m_callback(url);
        }
    }
}
#endif
