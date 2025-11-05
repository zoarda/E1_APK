using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.IO;

public class DownloadPage : MonoBehaviour
{
    public static DownloadPage Instance { get; private set; }

    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;
    private static DiscordLogger instance;
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

        // 🔹 Editor 模式：模擬下載
        if (Application.isEditor)
        {
            for (int i = 0; i <= 100; i += 5)
            {
                if (progressSlider != null) progressSlider.value = i / 100f;
                if (progressText != null) progressText.text = $"模擬下載中 {i}%";
                await UniTask.Delay(100); // 模擬延遲
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
        gameObject.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0;
        if (progressText != null) progressText.text = "準備下載影片...";

        try
        {
            string yamlPath = Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml");
            var nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToUrl>(yamlPath);

            if (nameToUrl == null)
            {
                Debug.LogError($"[DownloadPage] YAML 解析失敗，path={yamlPath}");
                progressText.text = "YAML 解析失敗";
                return;
            }

            if (nameToUrl.videoDictionary == null || nameToUrl.videoDictionary.Count == 0)
            {
                Debug.LogWarning("[DownloadPage] videoDictionary 為空");
                progressText.text = "沒有影片需要下載";
                return;
            }

            int totalCount = nameToUrl.videoDictionary.Count;
            int finishedCount = 0;

            var downloader = new VideoDownloader();
            var localDict = await downloader.DownloadVideos(
                nameToUrl.videoDictionary,
                (progress, msg) =>
                {
                    finishedCount = Mathf.RoundToInt(progress * totalCount);
                    if (progressSlider != null) progressSlider.value = progress;
                    if (progressText != null)
                        progressText.text = $"下載中 {finishedCount}/{totalCount} ({Mathf.RoundToInt(progress * 100)}%)";
                });

            if (localDict != null && localDict.Count > 0)
            {
                if (WebGLStreamController.Instance != null)
                    WebGLStreamController.Instance.SetLocalVideoDictionary(localDict);

                string localYamlPath = Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");
                var localWrapper = new WebGLStreamController.NameToLocalPath { videoDictionary = localDict };
                YamlLoader.SaveToYaml(localWrapper, localYamlPath);

                progressText.text = $"下載完成 ✅ ({localDict.Count} 個檔案)";
            }
            else
            {
                progressText.text = "沒有影片需要下載";
            }
        }
        catch (System.Exception e)
        {
            // 🔹 錯誤代碼 + 停止下載
            string errorMessage = $"下載失敗 (錯誤代號: 001)\n原因: {e.Message}";
            progressText.text = errorMessage;
            Debug.LogError($"[DownloadPage] {errorMessage}");

            // 🔹 傳送至 Discord
            DiscordLogger.Log(errorMessage);
            return; // 停留畫面不關閉
        }

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