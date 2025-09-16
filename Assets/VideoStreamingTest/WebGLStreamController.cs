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

    // name -> localPath  (example: "C0_S0" -> "/storage/.../Videos/EP1_mp4_720p/1/0.mp4")
    private Dictionary<string, string> nameToLocalPath = new Dictionary<string, string>();

    // yaml structures:
    // urlToName: relativePath -> name   (eg "Videos/EP1_mp4_720p/1/0.mp4" -> "C0_S0")
    private Dictionary<string, string> urlToName = new Dictionary<string, string>();

    // nameToUrlReversed: name -> relativePath (eg "C0_S0" -> "Videos/EP1_mp4_720p/1/0.mp4")
    private Dictionary<string, string> nameToUrlReversed = new Dictionary<string, string>();

    // 保護旗標
    private bool startedOnce = false;
    private bool hasPlayedOnce = false;

    protected override void Awake()
    {
        base.Awake();
        SetUpPlayer();
        LoadYaml().Forget();
        StartLoggingLoop().Forget();
    }

    void OnDestroy()
    {
        Debug.Log("[WebGLStreamController] Release player");
        Release();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) AddTime(-addTimeMillisecond);
    }

    protected override void ErrorInfo(HISPlayerErrorInfo errorInfo)
    {
        Debug.Log($"[WebGL] Player {errorInfo.playerIndex} Error: {errorInfo.errorType}, Info: {errorInfo.stringInfo}");
        DiscordLogger.Log($"[WebGL] Player {errorInfo.playerIndex} Error: {errorInfo.errorType}, Info: {errorInfo.stringInfo}");
        SetState(PlayerState.Error, errorInfo.stringInfo);
        base.ErrorInfo(errorInfo);
    }

    #region 狀態更新與心跳
    private void SetState(PlayerState state, string extra = "")
    {
        currentState = state;
        Debug.Log($"[HISPlayer] State = {state} {extra}");
        DiscordLogger.Log($"[HISPlayer] State = {state} {extra}");
    }

    private async UniTaskVoid StartLoggingLoop()
    {
        while (true)
        {
            DiscordLogger.Log($"[HISPlayer] Current state = {currentState}, Pos={GetVideotime()}/{GetVideoLenght()} ms");
            await UniTask.Delay(TimeSpan.FromSeconds(10));
        }
    }
    #endregion

    #region 事件覆寫
    protected override void EventPlaybackReady(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackReady(eventInfo);
        Debug.Log($"[WebGLStreamController] Playback ready for player {eventInfo.playerIndex}");

        if (!startedOnce) startedOnce = true;

        EndPlay = false;
        curPlayingUrl = multiStreamProperties[eventInfo.playerIndex].url[0];
        block?.SetActive(false);

        if (!hasPlayedOnce)
        {
            hasPlayedOnce = true;
            // 注意：這裡呼叫的是 SDK 的 Play(playerIndex)（不是我們的 Play(string)）
            Play(eventInfo.playerIndex);
        }

        if (GameSettingPage.Instance != null)
            SetVolume(eventInfo.playerIndex, GameSettingPage.Instance.VideoVolum);
        else
            SetVolume(eventInfo.playerIndex, 1);

        waitready = true;
        SetState(PlayerState.Ready);
    }

    protected override void EventPlaybackPlay(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackPlay(eventInfo);
        hasPlayed = true;
        SetState(PlayerState.Playing);
    }

    protected override void EventPlaybackStop(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackStop(eventInfo);
        SetState(PlayerState.Stopped);
    }

    protected override void EventPlaybackPause(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackPause(eventInfo);
        SetState(PlayerState.Paused);
    }

    protected override void EventPlaybackBuffering(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackBuffering(eventInfo);
        SetState(PlayerState.Buffering);
    }

    protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackSeek(eventInfo);
        Debug.Log($"[WebGLStreamController] Seek complete, current time: {GetVideotime()}");
        waitseek = true;
        if (NaniCommandManger.Instance.videoOnLoop) NaniCommandManger.Instance.isLooping = true;
    }

    protected override void EventVideoSizeChange(HISPlayerEventInfo eventInfo)
    {
        base.EventVideoSizeChange(eventInfo);
        SetState(PlayerState.VideoSizeChanged, $"Size={eventInfo.param1}x{eventInfo.param2}");
    }

    protected override void EventOnTrackChange(HISPlayerEventInfo eventInfo)
    {
        base.EventOnTrackChange(eventInfo);
        SetState(PlayerState.TrackChanged, $"Track={eventInfo.stringInfo}");
    }

    protected override void EventEndOfContent(HISPlayerEventInfo eventInfo)
    {
        base.EventEndOfContent(eventInfo);
        Debug.Log("[WebGLStreamController] End of content");

        if (EndPlay) return;
        EndPlay = true;

        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0;

        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }

        hasPlayedOnce = false;
        startedOnce = false;

        SetState(PlayerState.Ended);
    }

    protected override void EventEndOfPlaylist(HISPlayerEventInfo eventInfo)
    {
        base.EventEndOfPlaylist(eventInfo);
        Debug.Log("[WebGLStreamController] End of playlist");

        if (EndPlay) return;
        EndPlay = true;

        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0;

        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }

        hasPlayedOnce = false;
        startedOnce = false;

        SetState(PlayerState.Ended);
    }
    #endregion

    #region 對外方法
    public async UniTask LoadYaml()
    {
        try
        {
            nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<NameToUrl>(
                Application.streamingAssetsPath + "/Yaml/URLToScence.yaml");

            urlToName = nameToUrl.videoDictionary;
            // name -> relativePath
            nameToUrlReversed = nameToUrl.videoDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);

            Debug.Log("[WebGLStreamController] YAML 加載成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebGLStreamController] YAML 加載失敗: {e}");
        }
    }

    // 取得雲端相對路徑（由 name 取得）
    public string GetRelativePathByName(string name) =>
        nameToUrlReversed?.TryGetValue(name, out var rel) == true ? rel : null;

    // ResolveUrl：輸入可以是 name / relativePath / full url
    private string ResolveUrl(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        // Case A: input is name (eg "C1_S0")
        if (nameToUrlReversed != null && nameToUrlReversed.ContainsKey(input))
        {
            // 1. 如果本地有，使用本地
            if (nameToLocalPath != null && nameToLocalPath.ContainsKey(input))
            {
                var local = nameToLocalPath[input];
                // 大多數播放器在 Android 上需要 file:// 前綴
                if (Application.platform == RuntimePlatform.Android && !local.StartsWith("file://"))
                    return "file://" + local;
                return local;
            }

            // 2. 否則使用雲端 URL
            var relative = nameToUrlReversed[input];
            return baseUrl + relative;
        }

        // Case B: input is relative path (eg "Videos/EP1_mp4_720p/1/0.mp4")
        if (input.StartsWith("Videos/") || input.StartsWith("/Videos/"))
        {
            // try to find a name for this relative (to check local mapping)
            if (urlToName != null && urlToName.ContainsKey(input))
            {
                var name = urlToName[input];
                if (nameToLocalPath != null && nameToLocalPath.ContainsKey(name))
                {
                    var local = nameToLocalPath[name];
                    if (Application.platform == RuntimePlatform.Android && !local.StartsWith("file://"))
                        return "file://" + local;
                    return local;
                }
            }
            // fallback remote
            return baseUrl + input.TrimStart('/');
        }

        // Case C: full url already provided
        if (input.StartsWith("http://") || input.StartsWith("https://"))
            return input;

        Debug.LogError($"[ResolveUrl] 無法解析 input={input}");
        return null;
    }

    public async UniTask Play(string input)
    {
        hasPlayed = false;

        string url = ResolveUrl(input);

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"[WebGLStreamController] Play: 找不到對應的 URL，輸入值: {input}");
            return;
        }

        Debug.Log($"[WebGLStreamController] Play: 輸入 input = {input}, 解析後 url = {url}");

        NaniCommandManger.Instance.videoOnLoop = false;

        EndPlay = false;
        waitready = false;
        startedOnce = false;
        hasPlayedOnce = false;

        if (block != null) block.SetActive(false);
        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1;

        // 切換到新影片來源
        ChangeVideoContent(0, url);

        // 等待 ready
        float timeout = 10f;
        float timer = 0f;
        waitready = false;
        while (!waitready && timer < timeout)
        {
            if (GetVideoDuration(0) > 0)
            {
                waitready = true;
                break;
            }
            await Cysharp.Threading.Tasks.UniTask.DelayFrame(1);
            timer += Time.deltaTime;
        }

        if (!waitready)
            Debug.LogWarning("[WebGLStreamController] 影片未準備好，可能 SDK 未觸發 PlaybackReady");

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

        if (!hasPlayed) Debug.LogWarning("[WebGLStreamController] 播放事件未觸發 (可能是 SDK 問題)");
    }

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

    public async UniTask NaniSeekTime(long setMillisecond)
    {
        waitseek = false;
        Seek(0, setMillisecond);
        Debug.Log($"[WebGLStreamController] NaniSeekTime: Seek to {setMillisecond} ms");
        await UniTask.WaitUntil(() => waitseek);
        await SubtitlesManager.Instance.LoadSubtitles();
    }

    public void PlaySpeed(float speed) => SetPlaybackSpeedRate(0, speed);
    public float GetPlaySpeed() => GetPlaybackSpeedRate(0);
    public void SetHisVolume(float volume) => SetVolume(0, volume);
    #endregion

    // 用來接收下載後的 name -> localPath 字典（不要覆寫 YAML 映射）
    public void SetLocalVideoDictionary(Dictionary<string, string> localDict)
    {
        if (localDict == null) return;
        foreach (var kv in localDict)
        {
            nameToLocalPath[kv.Key] = kv.Value;
            Debug.Log($"[WebGLStreamController] SetLocalVideoDictionary: {kv.Key} -> {kv.Value}");
        }
    }

    // 若想要直接註冊 single file (relativePath, localPath)，可用此方法
    public void RegisterLocalVideo(string relativePath, string localPath)
    {
        if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(localPath)) return;
        // 嘗試找 name（yaml 中的 mapping）
        if (urlToName != null && urlToName.TryGetValue(relativePath, out var name))
        {
            nameToLocalPath[name] = localPath;
            Debug.Log($"[WebGLStreamController] RegisterLocalVideo: {name} -> {localPath}");
        }
        else
        {
            Debug.LogWarning($"[WebGLStreamController] RegisterLocalVideo 未找到對應 name: relativePath={relativePath}");
        }
    }

    // YAML wrapper
    public class NameToUrl { public Dictionary<string, string> videoDictionary; }
}