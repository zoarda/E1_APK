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
    // private const string baseUrl = "https://mgwan.love6.tv/";
    private bool useLoopSegment = false;
    private bool waitingForFirstFrame = false;
    private float loopStart = 0f;
    private float loopEnd = 0f;
    // 新增事件訂閱變數
    private UniTaskCompletionSource waitPlayedTcs;

    public bool waitingForChoice { get; private set; } = false;
    public float choiceAppearTime { get; private set; } = 0f;

    static WebGLStreamController instance;
    public GameObject block;
    public Action OnPlaybackReadyEvent; // 新增 public 事件

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
    public Action OnVideoEnded;
    bool waitready = false;
    public bool EndPlay = false;
    public bool waitseek = false;
    string curPlayingUrl = null;
    bool hasPlayed = false;
    NameToUrl nameToUrl;
    private Dictionary<string, string> nameToLocalPath = new Dictionary<string, string>();
    private Dictionary<string, string> urlToName = new Dictionary<string, string>();
    private Dictionary<string, string> nameToUrlReversed = new Dictionary<string, string>();
    private bool startedOnce = false;
    private bool hasPlayedOnce = false;
    private string localYamlPath => Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");

    protected override void Awake()
    {
        base.Awake();
        SetUpPlayer();
        LoadYamlWithDebug().Forget();
        StartLoggingLoop().Forget();
    }

    void OnDestroy() => Release();

    void Update()
    {
        float curSec = GetVideotime() / 1000f;

        if (useLoopSegment && curSec >= loopEnd)
        {
            Seek(0, (long)(loopStart * 1000));
            Debug.Log($"[WebGLStreamController] 循環回 {loopStart}s");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) AddTime(-addTimeMillisecond);
    }

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

    private async UniTaskVoid LoadYamlWithDebug()
    {
        try
        {
            string yamlPath = Path.Combine(Application.streamingAssetsPath, "Yaml", "URLToScence.yaml");
            nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToUrl>(yamlPath);

            if (nameToUrl?.videoDictionary == null)
            {
                Debug.LogWarning("[LoadYamlWithDebug] YAML 沒有資料");
                return;
            }

            urlToName = nameToUrl.videoDictionary;
            nameToUrlReversed = nameToUrl.videoDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);

            Debug.Log("[LoadYamlWithDebug] 原始 YAML 對照表:");
            foreach (var kv in nameToUrl.videoDictionary)
                Debug.Log($"  RelativePath={kv.Key}, Name={kv.Value}");

            string localYamlPath = Path.Combine(Application.persistentDataPath, "LocalVideoPath.yaml");
            if (File.Exists(localYamlPath))
            {
                var localDict = await YamlLoader.LoadStreamingAssetsYaml<WebGLStreamController.NameToLocalPath>(localYamlPath);
                nameToLocalPath = localDict?.videoDictionary ?? new Dictionary<string, string>();

                Debug.Log("[LoadYamlWithDebug] 本地影片字典:");
                foreach (var kv in nameToLocalPath)
                {
                    bool exists = File.Exists(kv.Value);
                    Debug.Log($"  Name={kv.Key}, LocalPath={kv.Value}, Exists={exists}");
                }
            }
            else
            {
                Debug.Log("[LoadYamlWithDebug] 尚無本地影片 YAML");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadYamlWithDebug] 載入 YAML 失敗: {e}");
        }
    }

    public async UniTask Play(string input)
    {
        float totalStart = Time.realtimeSinceStartup;

        hasPlayed = false;
        string url = ResolveUrl(input);
        // 重置循環
        useLoopSegment = false;
        loopStart = 0f;
        loopEnd = 0f;
        choiceAppearTime = 0f;
        waitingForChoice = false;
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

        // ChangeVideoContent
        float t0 = Time.realtimeSinceStartup;
        ChangeVideoContent(0, url);
        Debug.Log($"[Play][耗時] ChangeVideoContent 花費 {(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");

        // 等待 HISPlayer 發出 PlaybackReady
        bool isReady = false;
        void OnReady()
        {
            isReady = true;
            waitready = true;
            Debug.Log($"[Play] EventPlaybackReady 觸發，耗時 {(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");
        }

        OnPlaybackReadyEvent += OnReady;

        float timeout = 10f;
        float timer = 0f;
        while (!isReady && timer < timeout)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
        }

        OnPlaybackReadyEvent -= OnReady;

        if (isReady)
        {
            Debug.Log("[Play] 影片準備好，自動播放");
            Play(0);
        }
        else
        {
            Debug.LogWarning("[Play] 影片未準備好");
        }

        // 載入字幕
        float t2 = Time.realtimeSinceStartup;
        await SubtitlesManager.Instance.LoadSubtitles();
        Debug.Log($"[Play][耗時] 載入字幕 花費 {(Time.realtimeSinceStartup - t2) * 1000f:F1} ms");

        // 等待第一次播放完成
        float t3 = Time.realtimeSinceStartup;
        // await WaitForPlayedOnce();
        Debug.Log($"[Play][耗時] WaitForPlayedOnce 花費 {(Time.realtimeSinceStartup - t3) * 1000f:F1} ms");

        Debug.Log($"[Play][總耗時] 整個流程完成 花費 {(Time.realtimeSinceStartup - totalStart) * 1000f:F1} ms");
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

    public void SetLocalVideoDictionary(Dictionary<string, string> localDict)
    {
        if (localDict == null) return;
        foreach (var kv in localDict)
        {
            string keyUpper = kv.Key.ToUpper();
            nameToLocalPath[keyUpper] = kv.Value;
            Debug.Log($"[WebGLStreamController] 註冊本地影片: {keyUpper} -> {kv.Value}");
        }
    }

    public void RegisterLocalVideo(string relativePath, string localPath)
    {
        if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(localPath)) return;

        if (urlToName != null && urlToName.TryGetValue(relativePath, out var name))
        {
            nameToLocalPath[name.ToLower()] = localPath;
            Debug.Log($"[WebGLStreamController] RegisterLocalVideo: {name.ToLower()} -> {localPath}");
        }
        else
        {
            Debug.LogWarning($"[WebGLStreamController] RegisterLocalVideo 未找到對應 name: {relativePath}");
        }
    }

    private string ResolveUrl(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        string resolvedUrl = null;

        if (nameToUrlReversed != null)
        {
            var key = nameToUrlReversed.Keys.FirstOrDefault(k => string.Equals(k, input, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                resolvedUrl = nameToUrlReversed[key];
                Debug.Log($"[ResolveUrl] input={input} => YAML URL={resolvedUrl}");
                return resolvedUrl;
            }
        }

        if (input.StartsWith("Videos/") || input.StartsWith("/Videos/"))
        {
            resolvedUrl = input.TrimStart('/');
            Debug.Log($"[ResolveUrl] input={input} => relative URL={resolvedUrl}");
            return resolvedUrl;
        }

        if (input.StartsWith("http://") || input.StartsWith("https://"))
        {
            resolvedUrl = input;
            Debug.Log($"[ResolveUrl] input={input} => full URL={resolvedUrl}");
            return resolvedUrl;
        }

        Debug.LogError($"[ResolveUrl] 無法解析 input={input}");
        return null;
    }
    private UniTaskCompletionSource seekTcs;
    public async UniTask NaniSeekTime(long setMillisecond)
    {
        seekTcs = new UniTaskCompletionSource();
        Seek(0, setMillisecond);
        Debug.Log($"[WebGLStreamController] NaniSeekTime: Seek to {setMillisecond} ms");

        await seekTcs.Task; // 等待事件觸發
        await SubtitlesManager.Instance.LoadSubtitles();
    }

    public async UniTask SeekTime(long targetMs)
    {
        seekTcs = new UniTaskCompletionSource();
        Seek(0, targetMs);
        Debug.Log($"[WebGLStreamController] SeekTime: Seek to {targetMs} ms");

        await seekTcs.Task;
        await SubtitlesManager.Instance.LoadSubtitles();
    }

    // public async UniTask NaniSeekTime(long setMillisecond) { waitseek = false; Seek(0, setMillisecond); Debug.Log($"[WebGLStreamController] NaniSeekTime: Seek to {setMillisecond} ms"); await UniTask.WaitUntil(() => waitseek); await SubtitlesManager.Instance.LoadSubtitles(); }
    public async UniTask PlayVideo() { Play(0); await UniTask.CompletedTask; }
    public async UniTask PlayPause() { Pause(0); await UniTask.CompletedTask; }
    public long GetVideoLenght() => GetVideoDuration(0);
    public long GetVideotime() => GetVideoPosition(0);
    public void AddTime(int millisecond) => Seek(0, GetVideoPosition(0) + millisecond);
    // public async UniTask SeekTime(long targetMs) { waitseek = false; Seek(0, targetMs); await UniTask.WaitUntil(() => waitseek); await SubtitlesManager.Instance.LoadSubtitles(); }
    public async UniTask SetLoopSegment(float start, float end, bool enableLoop) { loopStart = start; loopEnd = end; useLoopSegment = enableLoop; }
    public void SetChoiceAppear(float appearTime) { choiceAppearTime = appearTime; waitingForChoice = true; }
    public void ClearChoice() => waitingForChoice = false;
    private float waitTimeSec = 0f;
    public void SetWaitTime(float seconds) { waitTimeSec = seconds; }
    public void PlaySpeed(float speed) => SetPlaybackSpeedRate(0, speed);
    public float GetPlaySpeed() => GetPlaybackSpeedRate(0);
    public void SetHisVolume(float volume) => SetVolume(0, volume);
    public bool IsLooping() => useLoopSegment;
    public void GetLoopSegment(out float start, out float end)
    {
        start = loopStart;
        end = loopEnd;
    }
    // EventPlaybackSeek 事件
    protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackSeek(eventInfo);
        Debug.Log($"[WebGLStreamController] EventPlaybackSeek 完成: {eventInfo}");
        seekTcs?.TrySetResult(); // 通知等待完成
    }
    protected override void EventEndOfContent(HISPlayerEventInfo eventInfo)
    {
        base.EventEndOfContent(eventInfo);
        Debug.Log("[WebGLStreamController] EventEndOfContent 播放結束，進入 Ended 狀態");

        EndPlay = true;
        SetState(PlayerState.Ended);

        // ✅ 這裡不再讓 HISPlayer 自動 seek 到 0，避免畫面閃回
        if (block != null) block.SetActive(true);
        var canvasGroup = StartNani.Instance.VideoImage?.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0;

        OnVideoEnded?.Invoke();
    }
    protected override void EventPlaybackReady(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackReady(eventInfo);

        Debug.Log("[WebGLStreamController] EventPlaybackReady 觸發");
        OnPlaybackReadyEvent?.Invoke(); // 通知外部
    }
    protected override void EventPlaybackPlay(HISPlayerEventInfo eventInfo)
    {
        base.EventPlaybackPlay(eventInfo);
        Debug.Log("[WebGLStreamController] EventPlaybackStarted 觸發");

        if (!hasPlayedOnce)
        {
            hasPlayedOnce = true;
            hasPlayed = true;
        }

        if (!startedOnce)
        {
            startedOnce = true;
            if (waitTimeSec > 0f)
            {
                Debug.Log($"[WebGLStreamController] 初次播放，等待 {waitTimeSec} 秒");
                Pause(0);
                UniTask.Delay(TimeSpan.FromSeconds(waitTimeSec)).ContinueWith(() =>
                {
                    Debug.Log("[WebGLStreamController] 等待結束，自動播放");
                    Play(0);
                }).Forget();
                waitTimeSec = 0f; // 重置
            }
        }
    }
    [Serializable]
    public class NameToUrl { public Dictionary<string, string> videoDictionary; }
    [Serializable]
    public class NameToLocalPath { public Dictionary<string, string> videoDictionary; }
}
