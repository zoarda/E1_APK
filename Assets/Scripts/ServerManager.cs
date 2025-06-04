using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;
using Unity.VisualScripting;

public class ServerManager : MonoBehaviour
{
    // const string serverUrl = "https://game-1005.6party.com";
    const string serverUrl = "https://av1-api-dev.funplaytech.com";
    // const string serverUrl = "https://user.love6.tv";
    // const string serverUrl = "http://localhost:5688";
    public static ServerManager Instance { get; private set; }

    public UrlData urlData = new UrlData();

    public UrlDataByUid urlDataByUid = new UrlDataByUid();

    public TokenData tokenData = new TokenData();

    string curToken;

    async void Awake()
    {
        // 單例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 可選，確保在場景切換時保留
    }

    /// <summary>
    /// 初始化 URL 查詢數據
    /// </summary>
    public async UniTask InitializeUrlQueryAsync()
    {
        await SetUrlQueryAsync();
    }

    private async UniTask SetUrlQueryAsync()
    {
        StartNani startNani = StartNani.Instance;
        // 測試用 URL，實際使用 Application.absoluteURL
        string absoluteURL = Application.absoluteURL;
#if UNITY_EDITOR
        // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZmYwOTIwNDYwNTA5QGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IiIsImlzcyI6IkxvdmU2RGV2TG9jYWwyIiwic3ViIjoiTDZNLTI1MDMxNy0wMDAwMDA2NCIsImVtYWlsIjoiZmYwOTIwNDYwNTA5QGdtYWlsLmNvbSIsImF1ZCI6ImZmMDkyMDQ2MDUwOUBnbWFpbC5jb20iLCJleHAiOjE3NDc2MjE2NDgsImp0aSI6IjUxZmQ2YzM4LTBjZDItNGNlYy04M2IzLTM2MTVkYTI3ZjBkOCIsImlhdCI6MTc0NTIyMTY0OCwibmJmIjoxNzQ1MjIxNjQ4fQ.VWMsw2sr-b0648obThW8mMx0KHy15L37tZGBtaVajRQ&lang=456";
        // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwianRpIjoiYmZjMWE5NzItY2E1OC00ODYxLWI1MDEtZjBlMDdjZTg2M2I5IiwibmJmIjoxNzQ1MzA0MzkxLCJleHAiOjE3NDUzOTA3OTEsImlhdCI6MTc0NTMwNDM5MSwiaXNzIjoiQXZEaXJlY3RvckRldiJ9.zssvxrrCJypvvRIeFub-ZqWaGcxMZ8SJNSUNCXSmhVw&lang=456";
        // absoluteURL = "http://localhost:13948/?";
        absoluteURL = "http://localhost:13948/?uid=cc6f97b7-9660-4a37-aa9d-11b4f284869f";

        // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRlBzZXJ2aWNlMDFAZnVucGxheXRlY2guY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiIiwiaXNzIjoiU2l4IFBhcnR5IFBsYXllciIsInN1YiI6IjZQTS0yNTA0MDgtMDAwMjU4MjUiLCJlbWFpbCI6IkZQc2VydmljZTAxQGZ1bnBsYXl0ZWNoLmNvbSIsImF1ZCI6IkZQc2VydmljZTAxQGZ1bnBsYXl0ZWNoLmNvbSIsImV4cCI6MTc0NzA4NTgyMCwianRpIjoiY2QyNjQyNmEtYzJiYy00ODE5LWIyYTYtNDA0ZTU3YzhiNDMzIiwiaWF0IjoxNzQ0Njg1ODIwLCJuYmYiOjE3NDQ2ODU4MjB9.eHTTs1wkGw2N09LC5qjRiUcUR09-6U_mcYqVxMqj4VY&lang=456";

#endif
        Debug.Log($"absoluteURL: {absoluteURL}");
        // string p = "https:d1seruguac4v04.cloudfront.net/?token=123&lang=";
        string? token = null;
        string? uid = null;
        string? language = null;

        if (absoluteURL.Contains("?"))
        {
            string[] stringP = absoluteURL.Split('?');
            string qs = stringP[1];
            string[] data = qs.Split('&');

            foreach (var a in data)
            {
                if (a.Contains("="))
                {
                    string[] b = a.Split('=');
                    if (b.Length == 2)
                    {
                        var key = b[0];
                        var value = b[1];

                        switch (key)
                        {
                            case "token":
                                token = value;
                                break;
                            case "uid":
                                uid = value;
                                break;
                            case "lang":
                                language = value;
                                break;
                            default:
                                Debug.LogWarning($"Unrecognized key: {key}, value: {value}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid data format: {a}");
                    }
                }
            }

            // 檢查登入憑證
            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(uid))
            {
                await ShowErrorPageAsync("Token and UID are both missing. Please log in again.");
                if (startNani != null)
                {
                    startNani.isLoggedIn = false;
                }
                else
                {
                    Debug.LogWarning("startNani is null!");
                }
                return;
            }

            // 檢查語言
            if (string.IsNullOrEmpty(language))
            {
                Debug.LogWarning("Language not specified. Defaulting to English.");
                language = "en";
            }
        }
        else
        {
            await ShowErrorPageAsync("No query string found in the URL. Please try again.");
            return;
        }

        if (startNani != null)
        {
            startNani.isLoggedIn = true;
        }
        else
        {
            Debug.LogWarning("startNani is null!");
        }

        // 判斷登入方式，並儲存對應資料結構
        if (!string.IsNullOrEmpty(token))
        {
            urlData = new UrlData
            {
                token = token,
                language = language,
                platform = 3 // 固定或可根據其他資訊決定
            };
            Debug.Log($"[SetUrlQuery] Using token login.");
        }
        else if (!string.IsNullOrEmpty(uid))
        {
            urlDataByUid = new UrlDataByUid
            {
                PlayerId = uid,
                language = language,
                platform = 3
            };
            Debug.Log($"[SetUrlQuery] Using UID login.");
        }
        await Login();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 顯示錯誤頁面（模擬異步行為）
    /// </summary>
    private async UniTask ShowErrorPageAsync(string message)
    {
        Debug.LogWarning(message);

        // 模擬等待，例如顯示錯誤提示頁面或等待用戶操作
        await UniTask.Delay(2000);

        // 可選：退出應用程式或執行其他操作
        // Application.Quit();
    }
    public async UniTask Login()
    {
        // Token 登入（urlData 不為 null 且 token 不為空）
        if (urlData != null && !string.IsNullOrEmpty(urlData.token))
        {
            Debug.Log($"[SetUrlQuery] Using LoginByToken.");
            await LoginByToken();
        }
        // UID 登入（urlDataByUid 不為 null 且 uid 不為空）
        else if (urlDataByUid != null && !string.IsNullOrEmpty(urlDataByUid.PlayerId))
        {
            Debug.Log($"[SetUrlQuery] Using LoginByUid.");
            await LoginByUid();
        }
        else
        {
            Debug.LogError("No valid login data.");
            await ShowErrorPageAsync("Invalid login state. Please login again.");
        }
    }
    public async UniTask LoginByUid()
    {
        var url = $"{serverUrl}/api/o/Player/CreateByUid";
        Debug.Log($"LoginByUid with url: {url}");
        try
        {
            var jurlData = JsonUtility.ToJson(urlDataByUid);
            Debug.Log($"jLoginData (UID): {jurlData}");
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jurlData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"UID Login Error: {request.error}");
                return;
            }

            string responseBody = request.downloadHandler.text;
            Debug.Log($"UID Login Response: {responseBody}");

            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);
            if (apiResponse.success == false || apiResponse.success == null)
            {
                Debug.Log($"UID Login Failed");
                return;
            }

            curToken = apiResponse.data;
            Debug.Log($"curToken (from UID): {curToken}");
        }
        catch (Exception ex)
        {
            Debug.Log($"UID Login Exception: {ex.Message}");
        }
    }
    public async UniTask LoginByToken()
    {
        var url = $"{serverUrl}/api/o/Player/Create";
        Debug.Log($"LoginByToken with url: {url}");
        try
        {
            var jurlData = JsonUtility.ToJson(urlData);
            Debug.Log($"jLoginData: {jurlData}");
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jurlData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Token Login Error: {request.error}");
                return;
            }

            string responseBody = request.downloadHandler.text;
            Debug.Log($"Token Login Response: {responseBody}");

            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);
            if (apiResponse.success == false || apiResponse.success == null)
            {
                Debug.Log($"Token Login Failed");
                return;
            }

            curToken = apiResponse.data;
            Debug.Log($"curToken (from token): {curToken}");
        }
        catch (Exception ex)
        {
            Debug.Log($"Token Login Exception: {ex.Message}");
        }
    }
    public async UniTask<SaveData?> Load()
    {
        string url = $"{serverUrl}/api/a/Player/Load";

        LoadRequest requestData = new LoadRequest
        {
            PlatformName = "3",      // 替换为实际的 platform name
            GameIdentifier = "1"     // 替换为实际的 game identifier
        };

        string json = JsonUtility.ToJson(requestData);
        Debug.Log("Sending JSON: " + json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {curToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Request Failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                return null;
            }

            Debug.Log($"Request Success: {request.result}");

            string responseText = request.downloadHandler.text;
            Debug.Log("Response Body: " + responseText);

            // 反序列化响应体
            SaveDataResponse wrapper = JsonUtility.FromJson<SaveDataResponse>(responseText);

            // 判断返回的 data 是否为空
            SaveData? saveData = null;
            if (!string.IsNullOrEmpty(wrapper.data))
            {
                saveData = JsonUtility.FromJson<SaveData>(wrapper.data);
            }

            // 若返回的数据为空，创建默认的 SaveData 并上传
            if (saveData == null)
            {
                Debug.LogWarning("No SaveData found. Creating default save...");

                saveData = new SaveData
                {
                    friendship = 0,
                    scriptName = new List<string>
                {
                    "C1_VB",  // 示例，实际可根据需求初始化
                },
                    // 根据实际情况初始化其他字段
                };

                // 调用 Save 方法保存数据
                await Save(saveData);

                // 然后重新调用 Load 方法获取新的数据
                return await Load();
            }

            Debug.Log($"Parsed SaveData: friendship={saveData.friendship}, scripts={string.Join(",", saveData.scriptName)}");

            return saveData;
        }
    }
    public async UniTask Save(SaveData saveData)
    {
        await SaveByToken(saveData);
    }
    public async UniTask SaveByToken(SaveData saveData)
    {
        string url = $"{serverUrl}/api/a/Player/Save";

        // 將 SaveData 轉為 JSON 字串
        string saveJson = JsonUtility.ToJson(saveData);
        Debug.Log($"Serialized SaveData: {saveJson}");

        // 包裝成 SaveRequest 結構
        SaveRequest saveRequest = new SaveRequest()
        {
            Data = saveJson,
            PlatformName = "3",         // 替換為你的實際 platform
            GameIdentifier = "1"        // 替換為你的實際 gameId
        };

        // 將整個 SaveRequest 序列化為 JSON
        string jsonPayload = JsonUtility.ToJson(saveRequest);
        Debug.Log($"Final JSON Payload: {jsonPayload}");

        // 建立 POST 請求
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", $"Bearer {curToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Save Request Failed: {request.error}\nResponse: {request.downloadHandler.text}");
                return;
            }

            Debug.Log("Save request success!");

            try
            {
                string responseText = request.downloadHandler.text;
                var apiResponse = JsonUtility.FromJson<ApiResponse>(responseText);

                if (apiResponse.success == false || apiResponse.success == null)
                {
                    Debug.LogError($"Server Save Failed: {apiResponse.message}");
                    return;
                }

                Debug.Log($"Save completed: {apiResponse.message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing response: {e.Message}\n{e.StackTrace}");
            }
        }
    }
    class JasonSaveData
    {
        public string Data;
    }
    [Serializable]
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
    /// <summary>
    /// URL 數據結構
    /// </summary>
    public class UrlData
    {
        public string token;
        public string language;
        public int platform;
    }
    public class UrlDataByUid
    {
        public string PlayerId;              // 對應 CreateUidRequest.PlayerId
        public int platform;            // 對應 EnumPlatfromEntity（建議轉換 enum）
        public string language;         // 額外補充用，不影響後端
        public string version;          // 對應 CreateUidRequest.Version
        public uint gameId;             // 對應 CreateUidRequest.GameId
    }
    /// <summary>
    /// Load Request
    /// </summary>
    [Serializable]
    public class LoadRequest
    {
        public string PlatformName;       // 注意：首字母要大寫，跟後端一致
        public string GameIdentifier;
    }
    /// <summary>
    /// SaveDataResponse
    /// </summary>
    [Serializable]
    public class SaveDataResponse
    {
        public string message;
        public string error;
        public bool success;
        public string data; // 注意：這是 JSON 字串
    }
    /// <summary>
    /// TokenData
    /// </summary>
    public class TokenData
    {
        public string token;
    }

    /// <summary>
    /// SaveDat
    /// </summary>
    public class SaveData
    {
        public float friendship;
        public List<string> scriptName;

        public string PlatformName;
        public string GameIdentifier;
    }
    /// <summary>
    /// SaveRequest
    /// </summary>
    [Serializable]
    public class SaveRequest
    {
        public string Data;
        public string PlatformName;
        public string GameIdentifier;
    }


}
