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
        return relativePath.TrimStart('/', '\\');
    }

    public static string GetLocalPathFromRelative(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;

        string localPath = Path.Combine(Application.persistentDataPath, CleanRelativePath(relativePath));
        string dir = Path.GetDirectoryName(localPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return localPath;
    }

    public async UniTask<Dictionary<string, string>> DownloadVideos(
        Dictionary<string, string> videoDict,
        System.Action<float, string> onProgress = null // ✅ 新增 callback 參數
  )
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

            if (File.Exists(localPath) && IsValidMp4(localPath))
            {
                result[name] = localPath;
                continue;
            }

            if (File.Exists(localPath)) File.Delete(localPath);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerFile(localPath);
                var op = request.SendWebRequest();

                while (!op.isDone)
                {
                    float percent = (idx - 1 + request.downloadProgress) / total;
                    onProgress?.Invoke(percent, $"下載中 {name} ({idx}/{total}) {request.downloadProgress * 100:F1}%");
                    await UniTask.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success && IsValidMp4(localPath))
                {
                    result[name] = localPath;
                }
                else
                {
                    Debug.LogError($"[VideoDownloader]  下載失敗 {name} -> {request.error}");
                    if (File.Exists(localPath)) File.Delete(localPath);
                }
            }
        }

        Debug.Log("[VideoDownloader] 批次下載處理完成");
        return result;
    }

    /// <summary>
    /// 簡單檢查 MP4 是否有效 (是否包含 ftyp 標記)
    /// </summary>
    private bool IsValidMp4(string path)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[64];
                int read = fs.Read(buffer, 0, buffer.Length);
                string header = System.Text.Encoding.ASCII.GetString(buffer, 0, read);

                if (header.Contains("ftyp"))
                {
                    return true;
                }
                else
                {
                    Debug.LogError($"[VideoDownloader] 無效 MP4 (缺少 ftyp): {path}");
                    return false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[VideoDownloader] 無法檢查 MP4: {path}\n{ex}");
            return false;
        }
    }

    private void PrintStreamingAssetsVideos()
    {
        string videoDir = Path.Combine(Application.streamingAssetsPath, "Videos");

        if (Directory.Exists(videoDir))
        {
            string[] files = Directory.GetFiles(videoDir, "*.*", SearchOption.AllDirectories);

            Debug.Log($"[VideoDownloader] StreamingAssets/Videos 底下共有 {files.Length} 個檔案：");
            foreach (var file in files)
            {
                Debug.Log($" - {Path.GetFileName(file)}");
            }
        }
        else
        {
            Debug.LogWarning("[VideoDownloader] 沒有找到 StreamingAssets/Videos 資料夾");
        }
    }
}