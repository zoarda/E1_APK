using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Erolabs.Sdk.Unity;
using System.IO;
using Unity.VisualScripting;


public class ServerManager : MonoBehaviour
{
    private static string erolabsApiUrl = "https://sadpki-portal.ebuajk.com"; // Erolabs 平台 API Host

    // const string serverUrl = "https://game-1005.6party.com";
    const string serverUrl = "https://av1-api-dev.funplaytech.com";
    // const string serverUrl = "https://user.love6.tv";
    // const string serverUrl = "http://localhost:5688";
    //for andriod
    // const string serverUrl = "https://192.168.11.89:5688";
    public static ServerManager Instance { get; private set; }

    public UrlData urlData = new UrlData();

    public UrlDataByUid urlDataByUid = new UrlDataByUid();

    public TokenData tokenData = new TokenData();

    string curToken;
    string game_id = "166";
    string game_account = "game_001";

    // 設定目前的平台（預設先用 TapDB）
    [SerializeField]
    private PlatformType currentPlatform = PlatformType.LocalDev;
    // public bool isTapMode = true; // ✅ 若你想寫死 Tap 模式（後續可改成 config 設定）
    [SerializeField]
    TextAsset config;

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
    public async UniTask<SaveData?> InitializeUrlQueryAndLoadAsync()
    {
        switch (currentPlatform)
        {
            case PlatformType.TapDB:
                Debug.Log("TAP 平台登入流程開始");
                await HandleTapDBLoginAsync();
                DiscordLogger.Log($"curToken after HandleTapDBLoginAsync: {curToken}");
                if (string.IsNullOrEmpty(curToken))
                {
                    Debug.LogError($"curToken is null after {currentPlatform} login!");
                    return null;
                }

                if (StartNani.Instance != null)
                    StartNani.Instance.isLoggedIn = true;
                break;

            // case PlatformType.Love6:
            //     Debug.Log("Love6 平台登入流程開始");
            //     await SetUrlQueryAsync();
            //     break;

            // case PlatformType.SixParty:
            //     Debug.Log("SixParty 平台登入流程開始");
            //     await SetUrlQueryAsync();
            //     break;

            case PlatformType.Erolabs:
                Debug.Log("Erolabs 平台登入流程開始");
                await HandleErolabsLoginAsync();
                break;

            case PlatformType.Nutaku:
                Debug.Log("Nutaku 平台登入流程開始");
                // await ();
                break;
            case PlatformType.LocalDev:
                Debug.Log("本地測試模式，使用 localhost");
                curToken = "dev-token"; // 測試用
                break;
        }

        return await Load();
    }

    // /// <summary>
    // /// 初始化 URL 查詢數據
    // /// </summary>
    // public async UniTask<SaveData?> InitializeUrlQueryAsync()
    // {
    //     Debug.Log("TAP 平台登入流程開始");
    //     await HandleTapDBLoginAsync();
    //     DiscordLogger.Log($"curToken after HandleTapDBLoginAsync: {curToken}");

    //     if (string.IsNullOrEmpty(curToken))
    //     {
    //         Debug.LogError("curToken is null after Tap login!");
    //         return null;
    //     }
    //     if (StartNani.Instance != null)
    //         StartNani.Instance.isLoggedIn = true;
    //     // 登入成功後才 Load
    //     return await Load();


    //     // ✅ 非 TAP 模式
    //     // Debug.Log("非 TAP 平台，開始網址解析 + 登入");
    //     // await SetUrlQueryAsync();

    //     // Debug.Log($"curToken after SetUrlQueryAsync: {curToken}");
    //     // if (string.IsNullOrEmpty(curToken))
    //     // {
    //     //     Debug.LogError("curToken is null after non-Tap login!");
    //     //     return null;
    //     // }

    //     // return await Load();
    // }
//     private async UniTask SetUrlQueryAsync()
//     {
//         StartNani startNani = StartNani.Instance;
//         // 測試用 URL，實際使用 Application.absoluteURL
//         string absoluteURL = Application.absoluteURL;
// #if UNITY_EDITOR
//         // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZmYwOTIwNDYwNTA5QGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IiIsImlzcyI6IkxvdmU2RGV2TG9jYWwyIiwic3ViIjoiTDZNLTI1MDMxNy0wMDAwMDA2NCIsImVtYWlsIjoiZmYwOTIwNDYwNTA5QGdtYWlsLmNvbSIsImF1ZCI6ImZmMDkyMDQ2MDUwOUBnbWFpbC5jb20iLCJleHAiOjE3NDc2MjE2NDgsImp0aSI6IjUxZmQ2YzM4LTBjZDItNGNlYy04M2IzLTM2MTVkYTI3ZjBkOCIsImlhdCI6MTc0NTIyMTY0OCwibmJmIjoxNzQ1MjIxNjQ4fQ.VWMsw2sr-b0648obThW8mMx0KHy15L37tZGBtaVajRQ&lang=456";
//         // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwianRpIjoiYmZjMWE5NzItY2E1OC00ODYxLWI1MDEtZjBlMDdjZTg2M2I5IiwibmJmIjoxNzQ1MzA0MzkxLCJleHAiOjE3NDUzOTA3OTEsImlhdCI6MTc0NTMwNDM5MSwiaXNzIjoiQXZEaXJlY3RvckRldiJ9.zssvxrrCJypvvRIeFub-ZqWaGcxMZ8SJNSUNCXSmhVw&lang=456";
//         // absoluteURL = "http://localhost:13948/?";
//         absoluteURL = "http://localhost:13948/?uid=cc6f97b7-9660-4a37-aa9d-11b4f284869f";

//         // absoluteURL = "http://localhost:13948/?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRlBzZXJ2aWNlMDFAZnVucGxheXRlY2guY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiIiwiaXNzIjoiU2l4IFBhcnR5IFBsYXllciIsInN1YiI6IjZQTS0yNTA0MDgtMDAwMjU4MjUiLCJlbWFpbCI6IkZQc2VydmljZTAxQGZ1bnBsYXl0ZWNoLmNvbSIsImF1ZCI6IkZQc2VydmljZTAxQGZ1bnBsYXl0ZWNoLmNvbSIsImV4cCI6MTc0NzA4NTgyMCwianRpIjoiY2QyNjQyNmEtYzJiYy00ODE5LWIyYTYtNDA0ZTU3YzhiNDMzIiwiaWF0IjoxNzQ0Njg1ODIwLCJuYmYiOjE3NDQ2ODU4MjB9.eHTTs1wkGw2N09LC5qjRiUcUR09-6U_mcYqVxMqj4VY&lang=456";

// #endif
//         Debug.Log($"absoluteURL: {absoluteURL}");
//         // string p = "https:d1seruguac4v04.cloudfront.net/?token=123&lang=";
//         string? token = null;
//         string? uid = null;
//         string? language = null;

//         if (absoluteURL.Contains("?"))
//         {
//             string[] stringP = absoluteURL.Split('?');
//             string qs = stringP[1];
//             string[] data = qs.Split('&');

//             foreach (var a in data)
//             {
//                 if (a.Contains("="))
//                 {
//                     string[] b = a.Split('=');
//                     if (b.Length == 2)
//                     {
//                         var key = b[0];
//                         var value = b[1];

//                         switch (key)
//                         {
//                             case "token":
//                                 token = value;
//                                 break;
//                             case "uid":
//                                 uid = value;
//                                 break;
//                             case "lang":
//                                 language = value;
//                                 break;
//                             default:
//                                 Debug.LogWarning($"Unrecognized key: {key}, value: {value}");
//                                 break;
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"Invalid data format: {a}");
//                     }
//                 }
//             }

//             // 檢查登入憑證
//             if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(uid))
//             {
//                 await ShowErrorPageAsync("Token and UID are both missing. Please log in again.");
//                 if (startNani != null)
//                 {
//                     startNani.isLoggedIn = false;
//                 }
//                 else
//                 {
//                     Debug.LogWarning("startNani is null!");
//                 }
//                 return;
//             }

//             // 檢查語言
//             if (string.IsNullOrEmpty(language))
//             {
//                 Debug.LogWarning("Language not specified. Defaulting to English.");
//                 language = "en";
//             }
//         }
//         else
//         {
//             await ShowErrorPageAsync("No query string found in the URL. Please try again.");
//             return;
//         }

//         if (startNani != null)
//         {
//             startNani.isLoggedIn = true;
//         }
//         else
//         {
//             Debug.LogWarning("startNani is null!");
//         }

//         // 判斷登入方式，並儲存對應資料結構
//         if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token))
//         {
//             urlData = new UrlData
//             {
//                 PlayerId = uid,
//                 token = token,
//                 language = language,
//                 platform = 4, // TapDB
//                 version = Application.version,
//                 gameId = 1
//             };
//             Debug.Log($"[SetUrlQuery] Using TapDB login.");
//         }
//         else if (!string.IsNullOrEmpty(uid))
//         {
//             urlDataByUid = new UrlDataByUid
//             {
//                 PlayerId = uid,
//                 language = language,
//                 platform = 3
//             };
//             Debug.Log($"[SetUrlQuery] Using UID login.");
//         }
//         await Login();

//         await UniTask.CompletedTask;
//     }

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
    public async UniTask<bool> Login()
    {
        if (urlData != null && !string.IsNullOrEmpty(urlData.token))
        {
            Debug.Log($"[SetUrlQuery] Using LoginByToken.");
            await LoginByToken();
            return !string.IsNullOrEmpty(curToken);
        }
        else if (urlDataByUid != null && !string.IsNullOrEmpty(urlDataByUid.PlayerId))
        {
            Debug.Log($"[SetUrlQuery] Using LoginByUid.");
            await LoginByUid();
            return !string.IsNullOrEmpty(curToken);
        }
        else
        {
            Debug.LogError("No valid login data.");
            await ShowErrorPageAsync("Invalid login state. Please login again.");
            return false;
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
            DiscordLogger.Log($"UID Login Response: {responseBody}");

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

            // 外層 parse
            var apiJson = JObject.Parse(responseBody);
            bool success = apiJson["success"]?.Value<bool>() ?? false;
            if (!success)
            {
                Debug.Log($"Token Login Failed");
                return;
            }

            // data 是個 JSON string，要 parse 第二次
            string dataStr = apiJson["data"]?.Value<string>();
            if (string.IsNullOrEmpty(dataStr))
            {
                Debug.Log($"Token Login Missing Data");
                return;
            }

            var dataJson = JObject.Parse(dataStr);
            int isPay = dataJson["IsPay"]?.Value<int>() ?? 0;

            DiscordLogger.Log($"TapVerifyData: code={dataJson["TapVerify"]?["code"]}, msg={dataJson["TapVerify"]?["msg"]}, user_id={dataJson["TapVerify"]?["user_id"]}");
            DiscordLogger.Log($"IsPay={isPay}");

            if (isPay != 1)
            {
                StartNani.Instance.ispay = false;
                Debug.LogWarning("用戶未購買，禁止進入遊戲");
                return;
            }

            // 已購買：取得 JwtToken
            string jwtToken = dataJson["JwtToken"]?.Value<string>();
            Debug.Log($"curToken (from token): {jwtToken}");
            curToken = jwtToken;
            StartNani.Instance.ispay = true;
        }
        catch (Exception ex)
        {
            Debug.Log($"Token Login Exception: {ex.Message}");
        }
    }
    public async UniTask<SaveData?> Load()
    {

        SaveData? saveData = null;

        switch (currentPlatform)
        {
            case PlatformType.TapDB:
                {
                    if (string.IsNullOrEmpty(curToken))
                    {
                        Debug.LogWarning("curToken is null or empty before Load! 嘗試讀本地檔案...");
                        return await LoadLocalSave();
                    }

                    string url = $"{serverUrl}/api/a/Player/Load";
                    LoadRequest requestData = new LoadRequest
                    {
                        PlatformName = "4",
                        GameIdentifier = "1"
                    };

                    string json = JsonUtility.ToJson(requestData);
                    Debug.Log("Sending TapDB Load JSON: " + json);

                    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.SetRequestHeader("Authorization", $"Bearer {curToken}");
                        request.SetRequestHeader("Content-Type", "application/json");

                        await request.SendWebRequest().ToUniTask();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogWarning($"TapDB Load Request Failed: {request.error}, 嘗試讀本地檔案...");
                            return await LoadLocalSave();
                        }

                        string responseText = request.downloadHandler.text;
                        var wrapper = JsonUtility.FromJson<SaveDataResponse>(responseText);

                        if (!string.IsNullOrEmpty(wrapper.data))
                        {
                            saveData = JsonUtility.FromJson<SaveData>(wrapper.data);
                        }
                    }
                    break;
                }

            case PlatformType.Erolabs:
                {
                    string url = $"{serverUrl}/api/o/Player/ErolabsLoad";
                    ErolabsLoadRequest erolabsRequest = new ErolabsLoadRequest()
                    {
                        Account = game_account,
                        Game = EnumGame.E1
                    };
                    string jsonPayload = JsonUtility.ToJson(erolabsRequest);
                    Debug.Log("Sending Erolabs Load JSON: " + jsonPayload);

                    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.SetRequestHeader("Content-Type", "application/json");

                        await request.SendWebRequest().ToUniTask();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogWarning($"Erolabs Load Request Failed: {request.error}, 嘗試讀本地檔案...");
                            return await LoadLocalSave();
                        }

                        string responseText = request.downloadHandler.text;
                        var apiResponse = JsonUtility.FromJson<ApiResponse>(responseText);

                        if (apiResponse.success == true && !string.IsNullOrEmpty(apiResponse.data))
                        {
                            saveData = JsonUtility.FromJson<SaveData>(apiResponse.data);
                        }
                    }
                    break;
                }

            case PlatformType.Love6:
            case PlatformType.SixParty:
            case PlatformType.LocalDev:
                {
                    Debug.LogWarning($"{currentPlatform} 本地模式，讀取本地 YAML...");
                    return await LoadLocalSave();
                }
        }

        // 如果 saveData 為 null（伺服器失敗）
        if (saveData == null)
        {
            Debug.LogWarning("伺服器沒資料，讀取本地 YAML...");
            saveData = await LoadLocalSave();
        }

        Debug.Log($"Parsed SaveData: friendship_CiciXie={saveData.friendship_CiciXie}, friendship_RosieLin={saveData.friendship_RosieLin}, friendship_CherryZhao={saveData.friendship_CherryZhao}, scripts={string.Join(",", saveData.scriptName)}");

        return saveData;
    }

    /// <summary>
    /// 讀取本地 YAML 檔案
    /// </summary>
    private async UniTask<SaveData> LoadLocalSave()
    {
        string localPath = Path.Combine(Application.persistentDataPath, "LocalSaveData.yaml");
        if (File.Exists(localPath))
        {
            try
            {
                var localData = await YamlLoader.LoadStreamingAssetsYaml<SaveData>(localPath);
                if (localData != null)
                {
                    Debug.Log($"本地存檔讀取成功: {localPath}");
                    return localData;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"讀取本地 YAML 失敗: {ex}");
            }
        }

        // 若本地檔案不存在或讀取失敗，回傳預設 SaveData
        Debug.LogWarning("本地 YAML 不存在或讀取失敗，回傳空 SaveData");
        return new SaveData
        {
            friendship_CiciXie = 0,
            friendship_RosieLin = 0,
            friendship_CherryZhao = 0,
            scriptName = new List<string>(),
            PlatformName = "Local",
            GameIdentifier = "1"
        };
    }
    public async UniTask Save(SaveData saveData)
    {
        await SaveByToken(saveData);
    }
    public async UniTask SaveByToken(SaveData saveData)
    {
        switch (currentPlatform)
        {
            case PlatformType.TapDB:
                Debug.Log("TapDB 平台 SaveByToken");
                string url = $"{serverUrl}/api/a/Player/Save";

                // 將 SaveData 轉為 JSON 字串
                string saveJson = JsonUtility.ToJson(saveData);
                Debug.Log($"Serialized SaveData: {saveJson}");

                // 包裝成 SaveRequest 結構
                SaveRequest saveRequest = new SaveRequest()
                {
                    Data = saveJson,
                    PlatformName = "4",
                    GameIdentifier = "1"
                };

                // 將整個 SaveRequest 序列化為 JSON
                string jsonPayload = JsonUtility.ToJson(saveRequest);
                Debug.Log($"Final JSON Payload: {jsonPayload}");

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
                break;

            case PlatformType.Love6:
                Debug.Log("Love6 平台 SaveByToken");
                break;

            case PlatformType.SixParty:
                Debug.Log("SixParty 平台 SaveByToken");
                break;

            case PlatformType.Erolabs:
                Debug.Log("Erolabs 平台 SaveByToken (無 Token)");

                string erolabsUrl = $"{serverUrl}/api/o/Player/ErolabsSave";
                string erolabsJson = JsonUtility.ToJson(saveData);
                Debug.Log($"Serialized SaveData for Erolabs: {erolabsJson}");

                ErolabsSaveRequest erolabsRequest = new ErolabsSaveRequest()
                {
                    Account = game_account,
                    Data = erolabsJson,
                    Game = EnumGame.E1
                };

                string erolabsPayload = JsonUtility.ToJson(erolabsRequest);
                Debug.Log($"Final JSON Payload for Erolabs: {erolabsPayload}");

                using (UnityWebRequest request = new UnityWebRequest(erolabsUrl, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(erolabsPayload);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();

                    request.SetRequestHeader("Content-Type", "application/json");

                    await request.SendWebRequest().ToUniTask();

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError($"Erolabs Save Request Failed: {request.error}\nResponse: {request.downloadHandler.text}");
                        return;
                    }

                    Debug.Log("Erolabs Save request success!");

                    try
                    {
                        string responseText = request.downloadHandler.text;
                        var apiResponse = JsonUtility.FromJson<ApiResponse>(responseText);

                        if (apiResponse.success == false || apiResponse.success == null)
                        {
                            Debug.LogError($"Erolabs Save Failed: {apiResponse.message}");
                            return;
                        }

                        Debug.Log($"Erolabs Save completed: {apiResponse.message}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error parsing Erolabs response: {e.Message}\n{e.StackTrace}");
                    }
                }
                break;

            case PlatformType.LocalDev:
                Debug.Log("本地測試模式 SaveByToken，模擬存檔完成 ✅");
                Debug.Log($"Local SaveData Snapshot: {JsonUtility.ToJson(saveData)}");
                break;
        }
    }
    private async UniTask HandleTapDBLoginAsync()
    {
        var tcs = new UniTaskCompletionSource<bool>();

        // Step 1: 初始化 SDK
        NativeSDK.Instance.bindEvent((ret) =>
        {
            if (ret["event"]?.ToString() == "SWITCH_ACCOUNT")
                Debug.Log("帳號已切換退出");
        });

        JObject param = new JObject();
        param["config"] = JObject.Parse(config.text); // sdk配置
        param["debug"] = true;

        NativeSDK.Instance.init(
            param,
            (ret) =>
            {
                Debug.Log("TapDB SDK 初始化成功");

                // Step 2: 開始登入
                NativeSDK.Instance.login(
                  async (loginRet) =>
                    {
                        DiscordLogger.Log($"登入成功：{loginRet}");
                        string uid = loginRet["uid"]?.ToString();
                        string token = loginRet["token"]?.ToString();

                        if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token))
                        {
                            urlData = new UrlData
                            {
                                PlayerId = uid,
                                token = token,
                                platform = 4,
                                language = "zh",
                                version = Application.version,
                                gameId = 1
                            };

                            await Login();
                            tcs.TrySetResult(true);
                        }
                        else
                        {
                            Debug.LogError("TapDB 登入回傳資料缺失");
                            tcs.TrySetResult(false);
                        }
                    },
                    (failRet) =>
                    {
                        Debug.LogError($"登入失敗：{failRet}");
                        tcs.TrySetResult(false);
                    }
                );
            },
            (ret) =>
            {
                Debug.LogError($"TapDB SDK 初始化失敗：{ret}");
                tcs.TrySetResult(false);
            }
        );

        await tcs.Task;
    }

#region ErolabsLogin
    private async UniTask HandleErolabsLoginAsync()
    {
        await ErolabsSDK.Initialize();
        Debug.Log("Erolabs SDK initialized");

        bool loginCompleted = false;

        while (!loginCompleted)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            void CallbackWrapper(Games.Coresdk.Unity.ProfileResult result)
            {
                // 使用 async lambda 搭配 Forget() 來處理警告
                _ = HandleCallbackAsync(result, tcs);
            }

            ErolabsSDK.OpenLogin(game_id, CallbackWrapper);

            loginCompleted = await tcs.Task;
        }

        Debug.Log("HandleErolabsLoginAsync 完成（登入成功）");

    }
    // 將 OnLoginCallback 改成這個方法，傳入 tcs
    private async UniTask HandleCallbackAsync(Games.Coresdk.Unity.ProfileResult result, UniTaskCompletionSource<bool> tcs)
    {
        bool loginSuccess = await OnLoginCallback(result);
        tcs.TrySetResult(loginSuccess);
    }
    private async UniTask<bool> OnLoginCallback(Games.Coresdk.Unity.ProfileResult result)
    {
        Exception exception = result.Exception;
        if (exception != null)
        {
            Debug.LogError("[Erolabs] Login exception: " + exception);
            return false;
        }

        string erolabsToken = ErolabsSDK.Token;
        string userId = result.Data.user_info.user_id;
        string account = result.Data.user_info.account;
        game_account = account; // 更新遊戲帳號

        Debug.Log($"user_id: {userId}\naccount: {account}\ntoken: {erolabsToken}");

        if (string.IsNullOrEmpty(erolabsToken))
        {
            Debug.LogError("[Erolabs] Token is null or empty");
            return false;
        }

        // 1️⃣ 檢查是否購買
        bool purchased = await CheckPurchase(game_id, erolabsToken);
        if (!purchased)
        {
            Debug.LogWarning("[Erolabs] 尚未購買，請先購買遊戲");
            return false;
        }

        // 2️⃣ 呼叫後端儲存玩家資料
        var apiResp = await BindAccountToBackend(erolabsToken, game_account, purchased);

        if (apiResp != null && apiResp.success == false && apiResp.error == "E1001")
        {
            Debug.LogWarning("[Erolabs] Token 過期，自動重新登入");
            return false; // 需要重試
        }

        return true; // 登入成功
    }
    /// <summary>
    /// 檢查是否購買過遊戲，回傳 true/false
    /// </summary>
    private async UniTask<bool> CheckPurchase(string gameId, string jwt)
    {
        string path = $"/api/v2/game/{gameId}/purchase/state?jwt={jwt}";
        string response = await Get(path);

        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("[Purchase] Empty response");
            return false;
        }

        Debug.Log("[Purchase] Raw Response = " + response);

        var result = JsonConvert.DeserializeObject<PurchaseResponse>(response);
        if (result.status == "SUCCESS")
        {
            if (result.data.purchased)
            {
                Debug.Log("[Purchase] 已購買 → 可以遊玩");
                StartNani.Instance.isLoggedIn = true;
                return true;
            }
            else
            {
                Debug.Log("[Purchase] 未購買 → 導向購買頁面");
                Application.OpenURL("https://l.hyenadata.com/s/1TrEoJ");
                return false;
            }
        }
        else
        {
            Debug.LogError($"[Purchase] FAIL: {result.message}");
            return false;
        }
    }
    /// <summary>
    /// 將 Erolabs 資料送到後端遊戲 API
    /// </summary>
    /// <param name="erolabsToken">Erolabs 平台 Token</param>
    /// <param name="gameAccount">遊戲帳號</param>
    /// <param name="purchased">是否購買</param>
    /// <returns>後端回傳結果</returns>
    public async Task<ApiResponse> BindAccountToBackend(string erolabsToken, string gameAccount, bool purchased)
    {
        // var bindReq = new
        // {
        //     Token = erolabsToken,
        //     Account = gameAccount,
        //     Purchased = purchased,
        //     Game = 1
        // };
        ErolabsEntity bindReq = new ErolabsEntity
        {
            Token = erolabsToken,
            Account = gameAccount,
            Purchased = purchased,
            Game = EnumGame.E1
        };

        string bindJson = JsonConvert.SerializeObject(bindReq);
        string bindResp = await PostJson("/api/o/player/ErolabsAccount", bindJson);
        Debug.Log($"BindErolabsAccount Resp={bindResp}");

        if (string.IsNullOrEmpty(bindResp))
            return new ApiResponse
            {
                success = false,
                message = "No response from server",
                error = "NoResponse"
            };

        var apiResp = JsonConvert.DeserializeObject<ApiResponse>(bindResp);
        return apiResp;
    }
    public static async Task<string> PostJson(string path, string json, string token = null)
    {
        string url = $"{serverUrl}{path}";

        Debug.Log($"[ApiClient] >>> POST {url}");
        Debug.Log($"[ApiClient] >>> Body JSON: {json}");
        if (!string.IsNullOrEmpty(token))
            Debug.Log($"[ApiClient] >>> Authorization: Bearer {token}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader("Authorization", $"Bearer {token}");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"[ApiClient] <<< Error: {request.error}");
                Debug.LogError($"[ApiClient] <<< Response: {request.downloadHandler.text}");
                return null;
            }

            string resp = request.downloadHandler.text;
            Debug.Log($"[ApiClient] <<< Response: {resp}");
            return resp;
        }
    }
    public static async Task<string> Get(string path)
    {
        using (var request = UnityWebRequest.Get($"{erolabsApiUrl}{path}"))
        {
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.SetRequestHeader("Accept", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"[ApiClient] GET Error: {request.error}");
                return null;
            }

            return request.downloadHandler.text;
        }
    }
    #endregion
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
        public string PlayerId;   // TapDB uid 對應後端 CreateRequest.PlayerId
        public string token;      // TapDB 登入後回傳 token
        public int platform;      // EnumPlatfromEntity.Tap = 4
        public string language;   // 可選: 用於前端語系設定
        public string version;    // 可選: Application.version
        public uint gameId; // 可選: 對應後端 GameId
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
        //新增區分成3種好感度
        // public float friendship;

        public float friendship_CiciXie; // 筱希
        public float friendship_RosieLin; // 林香
        public float friendship_CherryZhao; // 紫涵
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

    [Serializable]
    public class TapVerifyData
    {
        public int code;
        public string msg;
        public string user_id;
    }
    public enum PlatformType
    {
        TapDB,
        Love6,
        SixParty,
        Erolabs,
        Nutaku,
        LocalDev

    }
    [Serializable]
    public class PurchaseResponse
    {
        public string status;
        public string message;
        public PurchaseData data;
    }

    [Serializable]
    public class PurchaseData
    {
        public bool purchased;
    }
    [Serializable]
    public class ErolabsSaveRequest
    {
        public string Account;
        public string Data;
        public EnumGame Game;
    }
    [Serializable]
    private class ErolabsLoadRequest
    {
        public string Account;
        public EnumGame Game;
    }
    /// <summary>
    /// 遊戲枚舉
    /// </summary>
    public enum EnumGame
    {
        /// <summary>
        /// should not exist, is incorrect status
        /// </summary>
        Default = 0,
        /// <summary>
        /// E1
        /// </summary>
        E1 = 1,
        /// <summary>
        /// E2
        /// </summary>
        E2 = 2,
        /// <summary>
        /// E3
        /// </summary>
        E3 = 3,
        /// <summary>
        /// E4
        /// </summary>
        E4 = 4,
        /// <summary>
        /// E5
        /// </summary>
        E5 = 5,
    }
    public class ErolabsEntity
    {
        public string Token;
        public string Account;
        public bool Purchased;
        public EnumGame Game;
    }
}
