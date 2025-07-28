using UnityEngine;
using HISPlayerAPI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

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
            {
                instance = FindObjectOfType<WebGLStreamController>();
            }
            return instance;
        }
    }

    bool firstPlay = true;
    bool haveVideoReady = false;
    bool waitready = false;
    public bool EndPlay = false;
    public bool waitseek = false;
    string curPlayingUrl = null;
    NameToUrl nameToUrl;
    private Dictionary<string, string> urlToName = new Dictionary<string, string>();

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("===== [HISPlayer Debug] Awake =====");
        Debug.Log($"[HISPlayer Debug] Device: {SystemInfo.deviceModel}, OS: {SystemInfo.operatingSystem}");
        Debug.Log($"[HISPlayer Debug] HISPlayer Version: 4.6.1");

        SetUpPlayer();
        LoadYaml();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            AddTime(-addTimeMillisecond);
    }

    void OnDestroy()
    {
        Debug.Log("[HISPlayer Debug] OnDestroy - release HISPlayer");
        Release();
    }

    protected override async void EventPlaybackReady(HISPlayerEventInfo eventInfo)
    {
        Debug.Log($"EventPlaybackReady triggered: playerIndex={eventInfo.playerIndex}");

        try
        {
            EndPlay = false;
            waitready = true; // 一定要確保這裡被呼叫且正確設定

            StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha = 1;
            curPlayingUrl = multiStreamProperties[eventInfo.playerIndex].url[0];
            block.SetActive(false);
            Play(eventInfo.playerIndex);

            float volume = 1f;
            if (GameSettingPage.Instance != null)
            {
                volume = GameSettingPage.Instance.VideoVolum;
            }
            else
            {
                Debug.LogWarning("GameSettingPage Instance not found, default volume = 1");
            }
            SetVolume(eventInfo.playerIndex, volume);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception in EventPlaybackReady: {ex}");
        }
    }

    protected override void EventOnTrackChange(HISPlayerEventInfo eventInfo)
    {
        Debug.Log($"[HISPlayer Debug] ▶ EventOnTrackChanged | playerIndex={eventInfo.playerIndex} | url={GetUrl(eventInfo.playerIndex)} | time={GetVideotime()}");
    }

    protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
    {
        Debug.Log($"[HISPlayer Debug] ▶ EventPlaybackSeek | playerIndex={eventInfo.playerIndex} | url={GetUrl(eventInfo.playerIndex)} | time={GetVideotime()}");

        if (eventInfo.eventType == HISPlayerEvent.HISPLAYER_EVENT_PLAYBACK_SEEK)
        {
            Debug.Log($"Seek to time in {GetVideotime()}");
            waitseek = true;
            if (NaniCommandManger.Instance.videoOnLoop)
            {
                NaniCommandManger.Instance.isLooping = true;
            }
        }
    }

    public void extendEndOfContent(HISPlayerEventInfo eventInfo)
    {
        EventEndOfContent(eventInfo);
    }

    protected override void EventEndOfContent(HISPlayerEventInfo eventInfo)
    {
        Debug.Log($"[HISPlayer Debug] ▶ EventEndOfContent | playerIndex={eventInfo.playerIndex} | url={GetUrl(eventInfo.playerIndex)} | time={GetVideotime()}");

        if (EndPlay)
        {
            Debug.Log("EventEndOfContent 已觸發過，跳過");
            return;
        }
        EndPlay = true;
        StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha = 0;
        Debug.Log($"[HISPlayer Debug] Alphat to 0 {StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha}, haveVideoReady {haveVideoReady}");
    }

    protected override void ErrorInfo(HISPlayerErrorInfo errorInfo)
    {
        var log = $"[HISPlayer Debug] ▶ ErrorInfo | Code={errorInfo.errorType} | Msg={errorInfo.errorType}";
        Debug.LogError(log);
        DiscordLogger.Log(log);
    }

    protected override void ErrorNetworkFailed(HISPlayerErrorInfo errorInfo)
    {
        var log = $"[HISPlayer Debug] ▶ ErrorNetworkFailed | Code={errorInfo.errorType} | Msg={errorInfo.errorType}";
        Debug.LogError(log);
        DiscordLogger.Log(log);
    }

    public async UniTask PlayVideo()
    {
        Play(0);
        await UniTask.CompletedTask;
    }

    public async UniTask PlayPause()
    {
        Pause(0);
        await UniTask.CompletedTask;
    }

    public async UniTask LoadYaml()
    {
        try
        {
            nameToUrl = await YamlLoader.LoadStreamingAssetsYaml<NameToUrl>(Application.streamingAssetsPath + "/Yaml/URLToScence.yaml");
            Debug.Log("YAML 加載成功");
            if (nameToUrl?.videoDictionary != null)
            {
                foreach (var pair in nameToUrl.videoDictionary)
                {
                    Debug.Log($"[YAML解析結果] 名稱: {pair.Key} -> URL: {pair.Value}");
                }
            }
            else
            {
                Debug.LogError("YAML解析結果為空");
            }
            urlToName = nameToUrl.videoDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
        }
        catch (Exception e)
        {
            Debug.LogError($"YAML 加載錯誤: {e}");
        }
    }

    public string GetUrlByName(string name)
    {
        if (nameToUrl?.videoDictionary == null)
        {
            Debug.LogError("videoDictionary 尚未初始化");
            return null;
        }
        return nameToUrl.videoDictionary.TryGetValue(name, out string url) ? url : null;
    }

    public string GetNameByUrl(string url)
    {
        if (urlToName == null)
        {
            Debug.LogError("urlToName 尚未初始化");
            return null;
        }
        return urlToName.TryGetValue(url, out string name) ? name : null;
    }

    public async UniTask Play(string input)
    {
        Debug.Log($"[Play] input={input}");
        string url = GetNameByUrl(input) ?? input;
        Debug.Log($"[Play] resolved url={url}");
        await CheckStreamAvailability(url);
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"[WebGLStreamController] Play: 找不到對應的 URL，輸入值: {input}");
            return;
        }

        Debug.Log($"[Play] multiStreamProperties[0].url.Count={multiStreamProperties[0].url.Count}");

        EndPlay = false;
        waitready = false;

        if (multiStreamProperties[0].url.Count == 0)
        {
            Debug.Log("[Play] AddVideoContent");
            AddVideoContent(0, url);
        }
        else
        {
            Debug.Log("[Play] ChangeVideoContent");
            ChangeVideoContent(0, url);
        }

        bool ready = await WaitUntilReady(10000);
        Debug.Log($"[Play] WaitUntilReady finished: ready={ready}");

        if (!ready)
        {
            Debug.LogWarning("[Play] 等待播放準備超時，再試一次");
            // 再次嘗試
            if (multiStreamProperties[0].url.Count == 0)
            {
                Debug.Log("[Play] Retry AddVideoContent");
                AddVideoContent(0, url);
            }
            else
            {
                Debug.Log("[Play] Retry ChangeVideoContent");
                ChangeVideoContent(0, url);
            }

            ready = await WaitUntilReady(10000);
            Debug.Log($"[Play] Retry WaitUntilReady finished: ready={ready}");

            if (!ready)
            {
                Debug.LogError("[Play] 第二次嘗試仍超時，放棄等待");
            }
        }
    }
    /// <summary>
    /// 等待 waitready=true，最多 timeoutMs 毫秒
    /// </summary>
    private async UniTask<bool> WaitUntilReady(int timeoutMs)
    {
        int elapsed = 0;
        int interval = 100; // 每 100ms 檢查一次

        while (!waitready && elapsed < timeoutMs)
        {
            await UniTask.Delay(interval);
            elapsed += interval;
        }

        return waitready;
    }
    public long GetVideotime()
    {
        return GetVideoPosition(0);
    }

    public long GetVideoLenght()
    {
        return GetVideoDuration(0);
    }

    public void AddTime(int millisecond)
    {
        var curTime = GetVideoPosition(0);
        var newTime = curTime + millisecond;
        Seek(0, newTime);
    }

    public async UniTask SeekTime(long setsecond)
    {
        Seek(0, setsecond);
        waitseek = false;
        while (!waitseek)
        {
            await UniTask.DelayFrame(1);
            if (waitseek) break;
        }
        await SubtitlesManager.Instance.LoadSubtitles();
    }

    public async UniTask NaniSeekTime(long setsecond)
    {
        Seek(0, setsecond);
        Debug.Log($"shrimp seek time {setsecond}");
    }

    public void PlaySpeed(float speed)
    {
        SetPlaybackSpeedRate(0, speed);
    }

    public float GetPlaySpeed()
    {
        return GetPlaybackSpeedRate(0);
    }

    public void SetHisVolume(float volume)
    {
        SetVolume(0, volume);
    }

    public class NameToUrl
    {
        public Dictionary<string, string> videoDictionary;
    }

    /// <summary>
    /// 檢查 m3u8 串流是否可用：先抓 m3u8，再解析第一段 ts 串流嘗試下載
    /// </summary>
    public async UniTask CheckStreamAvailability(string url)
    {
        try
        {
            Debug.Log($"[StreamCheck] Start checking m3u8: {url}");

            using (UnityWebRequest m3u8Request = UnityWebRequest.Get(url))
            {
                await m3u8Request.SendWebRequest();

                if (m3u8Request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[StreamCheck] Failed to fetch m3u8: {m3u8Request.error}");
                    return;
                }

                string m3u8Content = m3u8Request.downloadHandler.text;
                Debug.Log("[StreamCheck] m3u8 fetched successfully");

                // 嘗試找出第一個 ts 段 URL
                string[] lines = m3u8Content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                string firstTsLine = lines.FirstOrDefault(l => !l.StartsWith("#"));

                if (string.IsNullOrEmpty(firstTsLine))
                {
                    Debug.LogError("[StreamCheck] m3u8 沒有找到 ts 段");
                    return;
                }

                // 如果 ts 是相對路徑，要組絕對 URL
                string tsUrl = firstTsLine;
                if (!tsUrl.StartsWith("http"))
                {
                    Uri baseUri = new Uri(url);
                    tsUrl = new Uri(baseUri, tsUrl).AbsoluteUri;
                }

                Debug.Log($"[StreamCheck] Checking first ts segment: {tsUrl}");

                using (UnityWebRequest tsRequest = UnityWebRequest.Head(tsUrl))
                {
                    await tsRequest.SendWebRequest();

                    if (tsRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[StreamCheck] Failed to fetch ts: {tsRequest.error}");
                    }
                    else
                    {
                        Debug.Log("[StreamCheck] First ts segment is accessible!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StreamCheck] Exception: {ex}");
        }
    }
    private string GetUrl(int playerIndex)
    {
        try
        {
            if (multiStreamProperties != null && playerIndex < multiStreamProperties.Count && multiStreamProperties[playerIndex].url.Count > 0)
            {
                return multiStreamProperties[playerIndex].url[0];
            }
        }
        catch { }
        return "N/A";
    }
}