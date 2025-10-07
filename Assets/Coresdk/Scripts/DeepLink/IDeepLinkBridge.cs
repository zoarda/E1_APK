using System;

namespace Games.Coresdk.Unity
{
    interface IDeepLinkBridge
    {
        void OpenURL(string url, Action<string> onDeepLinkActivated);
    }
}
