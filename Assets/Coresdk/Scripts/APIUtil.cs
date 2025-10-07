using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Games.Coresdk.Unity
{
    public class APIUtil
    {
        public static string GetQueryString(Dictionary<string, string> query)
        {
            List<string> queryMemberList = new List<string>();
            foreach (var pair in query)
            {
                string member = pair.Key + "=" + pair.Value;
                queryMemberList.Add(member);
            }

            return string.Join("&", queryMemberList.ToArray());
        }

        internal static void Get(string domain, string endpoint, Dictionary<string, string> queryStringMap, string authorization, System.Action<RawResponse> callback, MonoBehaviour monoBehaviour)
        {
            var url = domain + endpoint;

            monoBehaviour.StartCoroutine(RequestAPI(url, queryStringMap, authorization, null, callback));
        }

        internal static void Post(string domain, string endpoint, Dictionary<string, string> formMap, string authorization, System.Action<RawResponse> callback, MonoBehaviour monoBehaviour)
        {
            var url = domain + endpoint;
            var body = GetQueryString(formMap);

            monoBehaviour.StartCoroutine(RequestAPI(url, null, authorization, body, callback));
        }

        internal static IEnumerator RequestAPI(
            string url, 
            Dictionary<string, string> queryStringMap,
            string authorization,
            string body,
            System.Action<RawResponse> callback)
        {
            if (queryStringMap != null)
            {
                var queryString = GetQueryString(queryStringMap);
                url += "?" + queryString;
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.method = string.IsNullOrEmpty(body) ? "GET" : "POST";

                if (string.IsNullOrEmpty(authorization) == false)
                {
                    webRequest.SetRequestHeader("Authorization", authorization);
                }

                if (string.IsNullOrEmpty(body) == false)
                {
                    webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                    webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                }

                if (Application.isEditor || Debug.isDebugBuild)
                {
                    DumpRequest(webRequest, authorization, body);
                }

#if UNITY_2017_2_OR_NEWER
                yield return webRequest.SendWebRequest();
#else
                yield return webRequest.Send();
#endif

                if (Application.isEditor || Debug.isDebugBuild)
                {
                    DumpResponse(webRequest);
                }

                int statusCode = (int)webRequest.responseCode;
                string data = webRequest.downloadHandler.text;
                string exception = string.Empty;
#if UNITY_2017_1_OR_NEWER
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    exception = webRequest.error;
                }
#else
                if (webRequest.isError)
                {
                    exception = webRequest.error;
                }
                if (statusCode >= 400 && string.IsNullOrEmpty(exception))
                {
                    exception = data;
                }
#endif

                RawResponse response = new RawResponse(statusCode, data, exception);
                callback(response);
            }
        }

        static void DumpRequest(UnityWebRequest request, string authorization, string body)
        {
            var buffer = new StringBuilder();
            buffer.AppendFormat("[{0}] {1}", request.method, request.url).AppendLine(); ;

            if (string.IsNullOrEmpty(authorization) == false)
                buffer.AppendFormat("Authorization:{0}", authorization).AppendLine(); ;

            if (string.IsNullOrEmpty(body) == false)
                buffer.AppendFormat("Body:{0}", body);

            Debug.Log(buffer.ToString());
        }

        static void DumpResponse(UnityWebRequest request)
        {
            var buffer = new StringBuilder();
            buffer.AppendFormat("Response Status Code:{0}", request.responseCode).AppendLine();

#if UNITY_2017_1_OR_NEWER
            if (request.isNetworkError || request.isHttpError)
#else
            if (request.isError)
#endif
            {
                buffer.AppendFormat("Error:{0}", request.error).AppendLine();
            }
            else
            {
                buffer.AppendFormat("Data:{0}", request.downloadHandler.text);
            }

            Debug.Log(buffer.ToString());
        }
    }
}
