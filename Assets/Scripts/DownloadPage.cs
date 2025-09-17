using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.IO;

public class DownloadPage : MonoBehaviour
{
    public static DownloadPage Instance { get; private set; } // 單例

    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;

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
        // 先確保單例、UI、StartNani 都存在
        if (StartNani.Instance == null)
        {
            Debug.LogError("[DownloadPage] StartNani.Instance 尚未初始化");
            return;
        }

        var openPage = StartNani.Instance.OpenPage;
        var videoImage = StartNani.Instance.VideoImage;

        if (Application.isEditor)
        {
            // Editor 模式直接播放開頭動畫
            if (openPage != null) openPage.SetActive(true);
            videoImage?.GetComponent<CanvasGroup>()?.SetAlpha(1f);

            var controller = openPage?.GetComponent<WebGLStreamController>();
            if (controller != null)
            {
                try { await controller.Play("Videos/loading.mp4"); }
                catch (System.Exception e) { Debug.LogWarning($"Editor 播放影片失敗: {e}"); }
            }

            StartNani.Instance.OpenPageMessage();
            return;
        }

        // 非 Editor: 開啟 DownloadPage UI
        gameObject.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0;
        if (progressText != null) progressText.text = "準備下載影片...";

        try
        {
            string yamlPath = Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml");
            if (!File.Exists(yamlPath))
            {
                Debug.LogWarning($"[DownloadPage] YAML 不存在: {yamlPath}");
            }
            else
            {
                var nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToUrl>(yamlPath);
                if (nameToUrl?.videoDictionary != null && nameToUrl.videoDictionary.Count > 0)
                {
                    var downloader = new VideoDownloader();
                    var localDict = await downloader.DownloadVideos(nameToUrl.videoDictionary,
                        (progress, msg) =>
                        {
                            if (progressSlider != null) progressSlider.value = progress;
                            if (progressText != null) progressText.text = msg;
                        });

                    if (localDict != null && localDict.Count > 0)
                    {
                        WebGLStreamController.Instance?.SetLocalVideoDictionary(localDict);

                        string localYamlPath = Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");
                        var localWrapper = new WebGLStreamController.NameToLocalPath { videoDictionary = localDict };
                        YamlLoader.SaveToYaml(localWrapper, localYamlPath);

                        if (progressText != null) progressText.text = "下載完成 ✅";
                    }
                    else
                    {
                        if (progressText != null) progressText.text = "沒有影片需要下載";
                    }
                }
                else
                {
                    if (progressText != null) progressText.text = "YAML 解析結果為空";
                }
            }
        }
        catch (System.Exception e)
        {
            if (progressText != null) progressText.text = $"下載失敗: {e.Message}";
            Debug.LogError(e);
        }

        // 等 1 秒緩衝
        await UniTask.Delay(1000);

        // 隱藏 DownloadPage，顯示 OpenPage
        gameObject.SetActive(false);
        if (openPage != null) openPage.SetActive(true);
        videoImage?.GetComponent<CanvasGroup>()?.SetAlpha(1f);

        var controller2 = openPage?.GetComponent<WebGLStreamController>();
        if (controller2 != null)
        {
            try { await controller2.Play("Videos/loading.mp4"); }
            catch (System.Exception e) { Debug.LogWarning($"播放開頭動畫失敗: {e}"); }
        }

        StartNani.Instance.OpenPageMessage();
    }
}

// Extension method，方便設置 CanvasGroup alpha
public static class CanvasGroupExtensions
{
    public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
    }
}