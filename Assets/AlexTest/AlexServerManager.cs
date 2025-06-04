using System.Net.Http;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class AlexServerManager : MonoBehaviour
{
    public static AlexServerManager Instance { get; private set; }
    public ApiResponse apiResponse;
    // const string serverUrl = "https://av1-api-dev.shugoz.com";
    const string serverUrl = "http://localhost:5688";

    UrlData urlData;
    string curToken;

    void Awake()
    {
        // 單例模式
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Init();
    }

    async void Init()
    {
        string absoluteURL = Application.absoluteURL;
#if UNITY_EDITOR
        absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZnVuQGV4YW1wbGUuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiIiwiaXNzIjoiU2l4IFBhcnR5IFBsYXllciIsInN1YiI6ImNjNmY5N2I3LTk2NjAtNGEzNy1hYTlkLTExYjRmMjg0ODY5ZiIsImVtYWlsIjoiZnVuQGV4YW1wbGUuY29tIiwiYXVkIjoiZnVuQGV4YW1wbGUuY29tIiwiZXhwIjoxNzM3NTk3NjE5LCJqdGkiOiJhMzRjNmJjZi05MmJhLTQ5NTktYTNmZi1iYjZjN2M3ZDAzMjkiLCJpYXQiOjE3MzUxOTc2MTksIm5iZiI6MTczNTE5NzYxOX0.wkjZb913JqIbH6PD7V90ckOGFGd9xE0I2Y7C0bOand8&lang=456";
#endif
        Debug.Log($"absoluteURL: {absoluteURL}");

        string token = null;
        string language = null;
        if (!absoluteURL.Contains("?"))
        {
            Debug.LogError($"invalid absoluteURL: {absoluteURL}");
            return;
        }

        string[] data = absoluteURL.Split('?')[1].Split('&');
        foreach (var a in data)
        {
            if (!a.Contains("="))
            {
                Debug.LogError($"invalid absoluteURL: {absoluteURL}");
                return;
            }

            string[] b = a.Split('=');
            if (b.Length != 2)
            {
                Debug.LogError($"invalid absoluteURL: {absoluteURL}");
                return;
            }

            var key = b[0];
            switch (key)
            {
                case "token":
                    token = b[1];
                    break;
                case "lang":
                    language = b[1];
                    break;
                default:
                    Debug.LogWarning($"Unrecognized key: {key}, value: {b[1]}");
                    break;
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError($"token is null: {absoluteURL}");
            return;
        }
        else if (string.IsNullOrEmpty(language))
        {
            Debug.LogError($"language is null: {absoluteURL}");
            return;
        }

        urlData = new()
        {
            token = token,
            language = language,
            platform = 1,
        };

        Debug.Log($"URL Data Initialized. Token: {urlData.token}, Language: {urlData.language}");
        await Login();
    }

    public async UniTask Login()
    {
        HttpClient httpClient = new();
        LoginData loginData = new()
        {
            token = urlData.token,
            platform = urlData.platform,
        };

        var url = $"{serverUrl}/api/o/Player/Create";
        Debug.Log($"login with url: {url}");
        try
        {
            var jLoginData = JsonUtility.ToJson(loginData);
            Debug.Log($"jLoginData: {jLoginData}");
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jLoginData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 发送请求
            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                return;
            }

            // 处理响应
            string responseBody = request.downloadHandler.text;
            Debug.Log($"Response: {responseBody}");

            // uncompiled code below:
            apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);
            Debug.Log($"apiresponse {apiResponse.message}");
            Debug.Log($"apiresponse {apiResponse.error}");
            Debug.Log($"apiresponse {apiResponse.success}");
            Debug.Log($"apiresponse {apiResponse.data}");

            // if (apiResponse.Success == false ||
            //     apiResponse.Success == null)
            // {
            //     Debug.Log($"Request failed");
            //     return;
            // }

            curToken = apiResponse.data;
            Debug.Log($"curToken: {curToken}");
        }
        catch (Exception ex)
        {
            Debug.Log($"Request failed: {ex.Message}");
            return;
        }
    }

    class UrlData
    {
        public string token;
        public string language;
        public int platform;
    }

    class LoginData
    {
        public string token;
        public int platform;
    }

    [System.Serializable]
    public class ApiResponse
    {
        /// <summary>
        /// Message
        /// </summary>
        public string message;

        /// <summary>
        /// Error code
        /// </summary>
        public string error;

        /// <summary>
        /// Success
        /// </summary>
        public bool success;

        /// <summary>
        /// Data
        /// </summary>
        public string data;
    }
}
