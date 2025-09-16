using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

public class VideoDownloader
{
    private readonly string baseUrl = "https://data-av.ymytmx.com/";

    private static string CleanRelativePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return string.Empty;
        var cleaned = relativePath.TrimStart('/', '\\');
        if (cleaned.StartsWith("Videos/"))
            cleaned = cleaned.Substring("Videos/".Length);
        return cleaned;
    }

    public static string GetLocalPathFromRelative(string relativePath)
    {
        string cleaned = CleanRelativePath(relativePath);
        string localPath = Path.Combine(Application.persistentDataPath, cleaned);
        string dir = Path.GetDirectoryName(localPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return localPath;
    }

    public async UniTask<Dictionary<string, string>> DownloadVideos(Dictionary<string, string> videoDict)
    {
        var result = new Dictionary<string, string>();
        if (videoDict == null || videoDict.Count == 0) return result;

        int idx = 0;
        foreach (var kv in videoDict)
        {
            idx++;
            string relativePath = CleanRelativePath(kv.Key);
            string name = kv.Value;
            string url = baseUrl + relativePath;
            string localPath = GetLocalPathFromRelative(relativePath);

            if (File.Exists(localPath))
            {
                result[name] = localPath;
                continue;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerFile(localPath);
                var op = request.SendWebRequest();
                while (!op.isDone) await UniTask.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                    result[name] = localPath;
                else
                    Debug.LogError($"下載失敗 {name}: {request.error}");
            }
        }

        return result;
    }
}