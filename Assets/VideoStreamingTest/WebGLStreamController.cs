using UnityEngine;
using HISPlayerAPI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

public class WebGLStreamController : HISPlayerManager
{
    [SerializeField] int addTimeMillisecond = 5000;
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

    public enum PlayerState
    {
        Idle,
        Ready,
        Playing,
        Paused,
        Buffering,
        Stopped,
        TrackChanged,
        VideoSizeChanged,
        Ended,
        Error
    }

    private PlayerState currentState = PlayerState.Idle;

    bool haveVideoReady = false;
    bool waitready = false;
    public bool EndPlay = false;
    public bool waitseek = false;
    string curPlayingUrl = null;

    bool hasPlayed = false;
    NameToUrl nameToUrl;
    private Dictionary<string, string> urlToName = new Dictionary<string, string>();
    private Dictionary<string, string> nameToUrlReversed = new Dictionary<string, string>();

    // ★ 新增：防止多次 Ready 造成重播；讓 Play 只呼叫一次
    private bool startedOnce = false;    // 一次性的初始化
    private bool hasPlayedOnce = false;  // 保證 Play() 只呼叫一次

    protected override void Awake()
    {
        base.Awake();
        SetUpPlayer();
        LoadYaml().Forget();
        StartLoggingLoop().Forget();

    }
    //    Android 黑屏修復
#if !UNITY_EDITOR && UNITY_ANDROID
private bool wasPaused = false;
void OnApplicationPause(bool pause)
{
    if (pause)
    {
        wasPaused = true;
        Debug.Log("[WebGLStreamController] App paused → pausing HISPlayer");
        try
        {
            Pause(0); // 暫停播放
        }
        catch (Exception e)
        {
            Debug.LogWarning("[WebGLStreamController] Pause failed: " + e);
        }
    }
    else
    {
        if (wasPaused)
        {
            wasPaused = false;
            Debug.Log("[WebGLStreamController] App resumed → restoring HISPlayer");
            RestoreAfterResume().Forget();
        }
    }
}

private async UniTaskVoid RestoreAfterResume()
{
    // 等待 GPU surface 重建完成
    await UniTask.Delay(500);

    try
    {
        Debug.Log("[WebGLStreamController] Restarting video to restore surface...");
        Stop(0);                    // 強制釋放 Surface
        await UniTask.Delay(300);   // 等待底層釋放完成
        Play(0);                    // 重新播放 → SDK 會自動重建 Surface
    }
    catch (Exception e)
    {
        Debug.LogWarning("[WebGLStreamController] Resume video failed: " + e);
    }
}
#endif
    // 
    void OnDestroy()
    {
        Debug.Log("[WebGLStreamController] Release player");
        Release();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            AddTime(-addTimeMillisecond);
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

        // 一次性初始化（不要整段 return，避免卡住不播）
        if (!startedOnce)
        {
            startedOnce = true;
        }

        EndPlay = false;
        curPlayingUrl = multiStreamProperties[eventInfo.playerIndex].url[0];
        block?.SetActive(false);

        // ★ 關鍵：只讓 Play() 執行一次，避免多次 Ready 造成無限循環
        if (!hasPlayedOnce)
        {
            hasPlayedOnce = true;
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

        if (NaniCommandManger.Instance.videoOnLoop)
            NaniCommandManger.Instance.isLooping = true;
    }

    protected override void EventVideoSizeChange(HISPlayerEventInfo eventInfo)
    {
        base.EventVideoSizeChange(eventInfo);
        SetState(PlayerState.VideoSizeChanged, $"Size={eventInfo.param1}x{eventInfo.param2}");
        // ★ 重要：不要在這裡呼叫 Play()，Android 上此事件可能多次觸發，會導致重播循環
        // 若真的需要，可加一次性保護：
        // if (!hasPlayedOnce) { hasPlayedOnce = true; Play(eventInfo.playerIndex); }
    }

    protected override void EventOnTrackChange(HISPlayerEventInfo eventInfo)
    {
        base.EventOnTrackChange(eventInfo);
        SetState(PlayerState.TrackChanged, $"Track={eventInfo.stringInfo}");
    }

    // ★ 新增：Android VOD 常一定會觸發 EndOfContent（不一定有 EndOfPlaylist）
    protected override void EventEndOfContent(HISPlayerEventInfo eventInfo)
    {
        base.EventEndOfContent(eventInfo);
        Debug.Log("[WebGLStreamController] End of content");

        if (EndPlay) return;        // 防抖：只處理一次
        EndPlay = true;

        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }

        // 重置旗標，讓下一段/下一輪可以再次 Ready→Play
        hasPlayedOnce = false;
        startedOnce = false;

        SetState(PlayerState.Ended);
    }

    protected override void EventEndOfPlaylist(HISPlayerEventInfo eventInfo)
    {
        base.EventEndOfPlaylist(eventInfo);
        Debug.Log("[WebGLStreamController] End of playlist");

        if (EndPlay) return;        // 與 EndOfContent 保持一致
        EndPlay = true;

        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }

        // 與 EndOfContent 同步重置
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
            nameToUrlReversed = nameToUrl.videoDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);

            Debug.Log("[WebGLStreamController] YAML 加載成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebGLStreamController] YAML 加載失敗: {e}");
        }
    }

    public string GetUrlByName(string name) =>
        nameToUrlReversed?.TryGetValue(name, out var url) == true ? url : null;

    public string GetNameByUrl(string url) =>
        urlToName?.TryGetValue(url, out var name) == true ? name : null;

    public async UniTask Play(string input)
    {
        // 讓這次播放的事件能被偵測到
        hasPlayed = false;

        // 把 name 轉成 url（若你傳進來的是 key 而不是完整 url）
        string url = input;
        if (nameToUrlReversed != null && nameToUrlReversed.ContainsKey(input))
            url = nameToUrlReversed[input];

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"[WebGLStreamController] Play: 找不到對應的 URL，輸入值: {input}");
            return;
        }

        Debug.Log($"[WebGLStreamController] Play: 輸入 input = {input}, 轉換後 url = {url}");

        // 依你原本的流程：關閉自動 loop 狀態
        NaniCommandManger.Instance.videoOnLoop = false;

        // ========= 關鍵重置（每次切新片以前）=========
        // 讓下一段 Ready 事件可以再次觸發 Play()
        EndPlay = false;
        waitready = false;
        startedOnce = false;
        hasPlayedOnce = false;
        // =============================================

        // UI：隱藏遮罩、把影片容器顯示
        if (block != null) block.SetActive(false);
        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1;

        // 切換到新影片來源（這行會驅動 SDK 走一輪 Ready/Buffering/TrackChanged 等事件）
        ChangeVideoContent(0, url);

        // 等待影片進入可播放狀態（你的既有等待邏輯）
        float timeout = 10f;
        float timer = 0f;
        waitready = false;

        while (!waitready && timer < timeout)
        {
            // 判斷影片是否已經有長度，視為已 ready
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

        // 載入字幕（照你原本流程）
        await SubtitlesManager.Instance.LoadSubtitles();

        // 等待一次真正的播放事件（照你原本流程）
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

        if (!hasPlayed)
            Debug.LogWarning("[WebGLStreamController] 播放事件未觸發 (可能是 SDK 問題)");
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

    public class NameToUrl { public Dictionary<string, string> videoDictionary; }
}
