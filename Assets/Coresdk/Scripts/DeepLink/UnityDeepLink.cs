#if UNITY_EDITOR
using UnityEngine;
using System;
using Games.Coresdk.Unity.Editor;

namespace Games.Coresdk.Unity
{
    class UnityDeepLink : IDeepLinkBridge
    {
        private GameObject sdkGameObject;

        public UnityDeepLink(GameObject sdkGameObject)
        {
            this.sdkGameObject = sdkGameObject;
        }

        public void OpenURL(string url, Action<string> onDeepLinkActivated)
        {
            if (url.Contains("/login") && url.Contains("&login=true"))
            {
                OpenLoginURL(url, onDeepLinkActivated);
            }
            else if (url.Contains("/bind"))
            {
                OpenBindURL(url, onDeepLinkActivated);
            }
            else
            {
                Application.OpenURL(url);
                onDeepLinkActivated("");
            }
        }

        private void OpenLoginURL(string url, Action<string> onDeepLinkActivated)
        {
            // Load UI
            var sandboxLoginUI = GameObject.Instantiate(
                Resources.Load<SandboxLoginUI>("UnityEditor/CoresdkLoginUI"),
                sdkGameObject.transform
            );

            sandboxLoginUI.SetDeepLinkURL(url);

            // Set UI callback
            sandboxLoginUI.ClickCallback = (token) =>
            {
                GameObject.Destroy(sandboxLoginUI.gameObject);

                // 拼合成網址型態
                onDeepLinkActivated(url + "&token=" + token);
            };
        }

        private void OpenBindURL(string url, Action<string> onDeepLinkActivated)
        {
            // Load UI
            var sandboxBindUI = GameObject.Instantiate(
                Resources.Load<SandboxBindUI>("UnityEditor/CoresdkBindUI"),
                sdkGameObject.transform
            );

            sandboxBindUI.SetDeepLinkURL(url);

            // Set UI callback
            sandboxBindUI.ClickCallback = (result) =>
            {
                GameObject.Destroy(sandboxBindUI.gameObject);

                var bindResult = result.Result == "0000" ? "success" : "fail";
                var token = result.Token;
                // 拼合成網址型態
                onDeepLinkActivated(url + "&token=" + token + "&bind=" + bindResult);
            };
        }
    }

}
#endif