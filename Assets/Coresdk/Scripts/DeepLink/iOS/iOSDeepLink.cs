#if UNITY_IPHONE
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Games.Coresdk.Unity
{
    class iOSDeepLink : IDeepLinkBridge
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void DeepLinkActivatedDelegate(string url);

        [DllImport("__Internal")]
        static extern void openURL(string url, [MarshalAs(UnmanagedType.FunctionPtr)]DeepLinkActivatedDelegate callback);

        [AOT.MonoPInvokeCallback(typeof(DeepLinkActivatedDelegate))]
        static void OnDeepLinkActivated(string url)
        {
            if (m_onDeepLinkActivated != null)
                m_onDeepLinkActivated(url);
        }

        static Action<string> m_onDeepLinkActivated;

        public iOSDeepLink()
        {
        }

        public void OpenURL(string url, Action<string> onDeepLinkActivated)
        {
            m_onDeepLinkActivated = onDeepLinkActivated;

            openURL(url, OnDeepLinkActivated);

#if IOS_URL_SCHEME
            //Application.OpenURL(url);
#endif
        }
    }

}
#endif