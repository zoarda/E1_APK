using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

public class DownloadPage : MonoBehaviour
{
    public static DownloadPage Instance { get; private set; }

    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;
    bool haveVideoReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);
    }

    public async UniTask ShowAndDownloadAsync()
    {
        gameObject.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0;
        if (progressText != null) progressText.text = "準備下載影片...";

        try
        {
            // 🔹 Editor 模擬下載
            if (Application.isEditor)
            {
                for (int i = 0; i <= 100; i += 5)
                {
                    if (progressSlider != null) progressSlider.value = i / 100f;
                    if (progressText != null) progressText.text = $"模擬下載中 {i}%";
                    await UniTask.Delay(100);
                }

                progressText.text = "下載完成 ✅ (模擬)";
                await UniTask.Delay(500);
                gameObject.SetActive(false);

                StartNani.Instance.OpenPage.SetActive(true);
                if (!haveVideoReady)
                {
                    StartNani.Instance.OpenPageMessage();
                    haveVideoReady = true;
                }
                return;
            }

            // 🔹 讀 YAML
            string yamlPath = Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml");
            var nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToUrl>(yamlPath);

            if (nameToUrl == null || nameToUrl.videoDictionary == null)
            {
                Debug.LogWarning("[DownloadPage] YAML 解析失敗或 videoDictionary 為 null");
                if (progressText != null) progressText.text = "沒有影片需要下載";
                goto Finish;
            }

            var videosToDownload = new Dictionary<string, string>();
            var finalDict = new Dictionary<string, string>();

            foreach (var kvp in nameToUrl.videoDictionary)
            {
                string relativePath = kvp.Key.Replace("/", Path.DirectorySeparatorChar.ToString()); // YAML key -> 本地路徑
                string streamFile = Path.Combine(Application.streamingAssetsPath, relativePath);
                string persistentFile = Path.Combine(Application.persistentDataPath, relativePath);

                Debug.Log($"[DownloadPage] 檢查 StreamingAssets: {streamFile} -> {File.Exists(streamFile)}");
                Debug.Log($"[DownloadPage] 檢查 persistentDataPath: {persistentFile} -> {File.Exists(persistentFile)}");

                if (File.Exists(streamFile))
                {
                    finalDict[kvp.Value] = streamFile;
                    Debug.Log($"[DownloadPage] 已內建影片，略過下載：{kvp.Value}");
                    continue;
                }

                if (File.Exists(persistentFile))
                {
                    finalDict[kvp.Value] = persistentFile;
                    Debug.Log($"[DownloadPage] 已下載過影片，略過下載：{kvp.Value}");
                    continue;
                }

                videosToDownload.Add(kvp.Value, kvp.Key); // 下載用 URL 或相對路徑
            }
            // 🔹 沒有影片需要下載
            if (videosToDownload.Count == 0)
            {
                if (progressText != null) progressText.text = "所有影片已內建 ✅";
                if (WebGLStreamController.Instance != null)
                    WebGLStreamController.Instance.SetLocalVideoDictionary(finalDict);
                goto Finish;
            }

            // 🔹 下載缺少的影片
            var downloader = new VideoDownloader();
            var localDownloadedDict = await downloader.DownloadVideos(
                videosToDownload,
                (progress, msg) =>
                {
                    int totalCount = videosToDownload.Count;
                    int finishedCount = Mathf.RoundToInt(progress * totalCount);
                    if (progressSlider != null) progressSlider.value = progress;
                    if (progressText != null) progressText.text = $"下載中 {finishedCount}/{totalCount} ({Mathf.RoundToInt(progress * 100)}%)";
                });

            // 🔹 合併 StreamingAssets + 新下載的影片
            foreach (var kvp in localDownloadedDict)
                finalDict[kvp.Key] = kvp.Value;

            // 🔹 儲存 YAML
            string localYamlPath = Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");
            var localWrapper = new WebGLStreamController.NameToLocalPath { videoDictionary = finalDict };
            YamlLoader.SaveToYaml(localWrapper, localYamlPath);

            if (WebGLStreamController.Instance != null)
                WebGLStreamController.Instance.SetLocalVideoDictionary(finalDict);

            if (progressText != null) progressText.text = $"影片下載完成 ✅ (共 {finalDict.Count} 個檔案)";
        }
        catch (System.Exception e)
        {
            if (progressText != null) progressText.text = $"下載失敗: {e.Message}";
            Debug.LogError(e);
        }

    Finish:
        await UniTask.Delay(1000);
        gameObject.SetActive(false);

        StartNani.Instance.OpenPage.SetActive(true);
        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }
    }
}