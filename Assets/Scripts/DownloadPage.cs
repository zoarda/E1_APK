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
            Destroy(gameObject); // 確保場景只有一個
            return;
        }
        Instance = this;

        // 如果想在場景切換後保留，可以加這行
        // DontDestroyOnLoad(gameObject);

        gameObject.SetActive(false); // 預設隱藏
    }
    public async UniTask ShowAndDownloadAsync()
    {
        gameObject.SetActive(true);
        progressSlider.value = 0;
        progressText.text = "準備下載影片...";

        try
        {
            // 🔹 載入 YAML
            string yamlPath = Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml");
            var nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToUrl>(yamlPath);

            if (nameToUrl?.videoDictionary != null)
            {
                var downloader = new VideoDownloader();
                var localDict = await downloader.DownloadVideos(
                    nameToUrl.videoDictionary,
                    (progress, msg) =>
                    {
                        progressSlider.value = progress;
                        progressText.text = msg;
                    });

                if (localDict != null && localDict.Count > 0)
                {
                    WebGLStreamController.Instance.SetLocalVideoDictionary(localDict);

                    string localYamlPath = Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");
                    var localWrapper = new WebGLStreamController.NameToLocalPath { videoDictionary = localDict };
                    YamlLoader.SaveToYaml(localWrapper, localYamlPath);

                    progressText.text = "下載完成 ✅";
                }
                else
                {
                    progressText.text = "沒有影片需要下載";
                }
            }
        }
        catch (System.Exception e)
        {
            progressText.text = $"下載失敗: {e.Message}";
            Debug.LogError(e);
        }

        await UniTask.Delay(1000);
        gameObject.SetActive(false);
        var openpage = StartNani.Instance.OpenPage;
        openpage.SetActive(true);
        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1;

        // 啟用後再取得組件實例
        var controller = openpage.GetComponent<WebGLStreamController>();
        if (controller != null)
        {
            await controller.Play("Videos/loading.mp4");
            Debug.Log("播放開頭動畫");
        }
        StartNani.Instance.OpenPageMessage();
    }
}
