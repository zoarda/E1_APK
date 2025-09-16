using UnityEngine;
using HISPlayerAPI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class WebGLStreamController : HISPlayerManager
{
    [SerializeField] int addTimeMillisecond = 5000;
    private const string baseUrl = "https://data-av.ymytmx.com/";

    static WebGLStreamController instance;
    public GameObject block;

    public static WebGLStreamController Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<WebGLStreamController>();
            return instance;
        }
    }

    public enum PlayerState { Idle, Ready, Playing, Paused, Buffering, Stopped, TrackChanged, VideoSizeChanged, Ended, Error }
    private PlayerState currentState = PlayerState.Idle;

    bool haveVideoReady = false;
    bool waitready = false;
    public bool EndPlay = false;
    public bool waitseek = false;
    string curPlayingUrl = null;

    bool hasPlayed = false;
    NameToUrl nameToUrl;

    // 本地檔案字典（下載完成後記錄）
    private Dictionary<string, string> nameToLocalPath = new Dictionary<string, string>();

    // YAML 對照表
    private Dictionary<string, string> urlToName = new Dictionary<string, string>();
    private Dictionary<string, string> nameToUrlReversed = new Dictionary<string, string>();

    // 保護旗標
    private bool startedOnce = false;
    private bool hasPlayedOnce = false;

    // 本地 YAML 路徑
    private string localYamlPath => Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");

    protected override void Awake()
    {
        base.Awake();
        SetUpPlayer();
        LoadYaml().Forget();
        StartLoggingLoop().Forget();
    }

    void OnDestroy() => Release();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) AddTime(-addTimeMillisecond);
    }

    #region 狀態與心跳
    private void SetState(PlayerState state, string extra = "")
    {
        currentState = state;
        Debug.Log($"[HISPlayer] State = {state} {extra}");
    }

    private async UniTaskVoid StartLoggingLoop()
    {
        while (true)
        {
            Debug.Log($"[HISPlayer] Current state = {currentState}, Pos={GetVideotime()}/{GetVideoLenght()} ms");
            await UniTask.Delay(TimeSpan.FromSeconds(10));
        }
    }
    #endregion

    #region YAML 載入與下載
    public async UniTask LoadYaml()
    {
        try
        {
            // 1️⃣ 原始雲端 YAML
            nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<NameToUrl>(
                Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml"));

            urlToName = nameToUrl.videoDictionary;
            nameToUrlReversed = nameToUrl.videoDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
            Debug.Log("[WebGLStreamController] 原始 YAML 加載成功");

            // 2️⃣ 載入本地 YAML（如果存在）
            if (File.Exists(localYamlPath))
            {
                var localDict = await YamlLoader.LoadStreamingAssetsYaml<NameToLocalPath>(localYamlPath);
                nameToLocalPath = localDict.videoDictionary ?? new Dictionary<string, string>();
                Debug.Log("[WebGLStreamController] 本地 YAML 載入完成");
            }

            // 3️⃣ 開始下載本地缺失影片
            await DownloadMissingVideos();
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebGLStreamController] YAML 加載失敗: {e}");
        }
    }

    private async UniTask DownloadMissingVideos()
    {
        if (nameToUrl?.videoDictionary == null) return;

        var downloader = new VideoDownloader();
        var videosToDownload = new Dictionary<string, string>();

        // 只下載本地沒有的影片
        foreach (var kv in nameToUrl.videoDictionary)
        {
            var name = kv.Value;
            if (!nameToLocalPath.ContainsKey(name) || !File.Exists(nameToLocalPath[name]))
                videosToDownload[kv.Key] = name;
        }

        if (videosToDownload.Count == 0)
        {
            Debug.Log("[WebGLStreamController] 本地影片已完整，不需下載");
            return;
        }

        var downloaded = await downloader.DownloadVideos(videosToDownload);

        // 更新本地字典
        foreach (var kv in downloaded)
            nameToLocalPath[kv.Key] = kv.Value;

        // 生成本地 YAML
        var localWrapper = new NameToLocalPath { videoDictionary = nameToLocalPath };
        YamlLoader.SaveToYaml(localWrapper, localYamlPath);
        Debug.Log("[WebGLStreamController] 本地 YAML 更新完成");
    }

    #endregion

    public async UniTask Play(string input)
    {
        hasPlayed = false;
        string url = ResolveUrl(input);

        // Debug 本地影片對照表
        Debug.Log("[Play] 當前本地影片字典:");
        foreach (var kv in nameToLocalPath)
            Debug.Log($"  Name={kv.Key}, LocalPath={kv.Value}");

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"[WebGLStreamController] Play: 找不到 URL, input={input}");
            return;
        }

        EndPlay = false;
        waitready = false;
        startedOnce = false;
        hasPlayedOnce = false;

        if (block != null) block.SetActive(false);
        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1;

        Debug.Log($"[Play] 播放影片 input={input}, 解析後 URL={url}");
        ChangeVideoContent(0, url);

        float timeout = 10f;
        float timer = 0f;
        while (!waitready && timer < timeout)
        {
            if (GetVideoDuration(0) > 0)
            {
                waitready = true;
                break;
            }
            await UniTask.Yield();
            timer += Time.deltaTime;
        }

        if (!waitready)
            Debug.LogWarning("[WebGLStreamController] 影片未準備好");

        await SubtitlesManager.Instance.LoadSubtitles();
        await WaitForPlayedOnce();
    }

    async UniTask WaitForPlayedOnce()
    {
        float timeout = 10f;
        float timer = 0f;
        while (!hasPlayed && timer < timeout)
        {
            await UniTask.DelayFrame(1);
            timer += Time.deltaTime;
        }

        if (!hasPlayed) Debug.LogWarning("[WebGLStreamController] 播放事件未觸發");
    }
    #region 本地影片註冊與解析（大小寫安全）
    /// <summary>
    /// 接收下載後的 name -> localPath 映射，註冊到播放器使用（大小寫安全）
    /// </summary>
    public void SetLocalVideoDictionary(Dictionary<string, string> localDict)
    {
        if (localDict == null) return;
        foreach (var kv in localDict)
        {
            string keyLower = kv.Key.ToUpper(); // key 統一大寫
            nameToLocalPath[keyLower] = kv.Value;
            Debug.Log($"[WebGLStreamController] 註冊本地影片: {keyLower} -> {kv.Value}");
        }
    }
    /// <summary>
    /// 單一影片註冊用（大小寫安全）
    /// </summary>
    public void RegisterLocalVideo(string relativePath, string localPath)
    {
        if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(localPath)) return;

        if (urlToName != null && urlToName.TryGetValue(relativePath, out var name))
        {
            nameToLocalPath[name.ToLower()] = localPath; // key 小寫化
            Debug.Log($"[WebGLStreamController] RegisterLocalVideo: {name.ToLower()} -> {localPath}");
        }
        else
        {
            Debug.LogWarning($"[WebGLStreamController] RegisterLocalVideo 未找到對應 name: {relativePath}");
        }
    }
    /// <summary>
    /// 解析影片 URL（大小寫安全）
    /// </summary>
    private string ResolveUrl(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        string resolvedUrl = null;

        // 1️⃣ 優先檢查原始 YAML 映射
        if (nameToUrlReversed != null)
        {
            var key = nameToUrlReversed.Keys.FirstOrDefault(k => string.Equals(k, input, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                resolvedUrl = nameToUrlReversed[key]; // 使用原始相對路徑
                Debug.Log($"[ResolveUrl] input={input} => YAML URL={resolvedUrl}");
                return resolvedUrl;
            }
        }

        // 2️⃣ 如果輸入就是相對路徑
        if (input.StartsWith("Videos/") || input.StartsWith("/Videos/"))
        {
            resolvedUrl = input.TrimStart('/');
            Debug.Log($"[ResolveUrl] input={input} => relative URL={resolvedUrl}");
            return resolvedUrl;
        }

        // 3️⃣ 如果輸入是完整 URL
        if (input.StartsWith("http://") || input.StartsWith("https://"))
        {
            resolvedUrl = input;
            Debug.Log($"[ResolveUrl] input={input} => full URL={resolvedUrl}");
            return resolvedUrl;
        }

        Debug.LogError($"[ResolveUrl] 無法解析 input={input}");
        return null;
    }
    #endregion

    public async UniTask NaniSeekTime(long setMillisecond) { waitseek = false; Seek(0, setMillisecond); Debug.Log($"[WebGLStreamController] NaniSeekTime: Seek to {setMillisecond} ms"); await UniTask.WaitUntil(() => waitseek); await SubtitlesManager.Instance.LoadSubtitles(); }
    public async UniTask PlayVideo() { Play(0); await UniTask.CompletedTask; }
    public async UniTask PlayPause() { Pause(0); await UniTask.CompletedTask; }

    public long GetVideoLenght() => GetVideoDuration(0);
    public long GetVideotime() => GetVideoPosition(0);
    public void AddTime(int millisecond) => Seek(0, GetVideoPosition(0) + millisecond);
    public async UniTask SeekTime(long targetMs)
    {
        waitseek = false;
        Seek(0, targetMs);
        await UniTask.WaitUntil(() => waitseek);
        await SubtitlesManager.Instance.LoadSubtitles();
    }

    public void PlaySpeed(float speed) => SetPlaybackSpeedRate(0, speed);
    public float GetPlaySpeed() => GetPlaybackSpeedRate(0);
    public void SetHisVolume(float volume) => SetVolume(0, volume);

    #region 本地 YAML 包裝類
    [Serializable]
    public class NameToUrl { public Dictionary<string, string> videoDictionary; }
    [Serializable]
    public class NameToLocalPath { public Dictionary<string, string> videoDictionary; }
    #endregion
}