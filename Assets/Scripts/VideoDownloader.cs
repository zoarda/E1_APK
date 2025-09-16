using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

public class VideoDownloader
{
    // 基底 URL，保持乾淨，不要多餘目錄
    private readonly string baseUrl = "https://data-av.ymytmx.com/";

    /// <summary>
    /// 清理 relativePath，移除不必要的前綴 (如 Videos/) 與斜線
    /// </summary>
    private static string CleanRelativePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return string.Empty;
        var cleaned = relativePath.TrimStart('/', '\\');
        if (cleaned.StartsWith("Videos/"))
            cleaned = cleaned.Substring("Videos/".Length);
        return cleaned;
    }

    /// <summary>
    /// 將相對路徑轉換為本地儲存路徑，並自動建立資料夾
    /// </summary>
    public static string GetLocalPathFromRelative(string relativePath)
    {
        string cleaned = CleanRelativePath(relativePath);
        string localPath = Path.Combine(Application.persistentDataPath, cleaned);
        string dir = Path.GetDirectoryName(localPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return localPath;
    }

    /// <summary>
    /// 批次下載影片，videoDict: key=relativePath, value=name
    /// 回傳: name -> localPath（成功下載或已存在的檔案）
    /// </summary>
    public async UniTask<Dictionary<string, string>> DownloadVideos(Dictionary<string, string> videoDict)
    {
        var result = new Dictionary<string, string>();
        if (videoDict == null || videoDict.Count == 0)
        {
            Debug.LogWarning("[VideoDownloader] videoDict 為空");
            return result;
        }

        int total = videoDict.Count;
        int idx = 0;

        foreach (var kv in videoDict)
        {
            idx++;
            string relativePath = CleanRelativePath(kv.Key);
            string name = kv.Value;
            string url = baseUrl + relativePath;
            string localPath = GetLocalPathFromRelative(relativePath);

            Debug.Log($"[VideoDownloader] ({idx}/{total}) 準備下載 {name}\nURL: {url}\nPath: {localPath}");

            if (File.Exists(localPath))
            {
                Debug.Log($"[VideoDownloader] 已存在: {localPath}");
                result[name] = localPath;
                continue;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerFile(localPath);
                var op = request.SendWebRequest();

                while (!op.isDone)
                {
                    Debug.Log($"[VideoDownloader] 下載中 {name}: {request.downloadProgress * 100:F1}%");
                    await UniTask.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[VideoDownloader] ✅ 下載完成: {name} -> {localPath}");
                    result[name] = localPath;
                }
                else
                {
                    Debug.LogError($"[VideoDownloader] ❌ 下載失敗 {name}\nURL: {url}\n錯誤: {request.error}");
                }
            }
        }

        Debug.Log("[VideoDownloader] 批次下載處理完成");
        return result;
    }
}