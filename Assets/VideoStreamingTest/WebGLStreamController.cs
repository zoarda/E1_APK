using UnityEngine;
using HISPlayerAPI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
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
        SetUpPlayer();
        LoadYaml();
        // multiStreamProperties[0].renderMode = HISPlayerRenderMode.NONE;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1))
        //     Play(url_1);
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        //     Play(url_2);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            AddTime(addTimeMillisecond);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            AddTime(-addTimeMillisecond);

        // Debug.Log("alex_controller update", this);
    }

    void OnDestroy()
    {
        Debug.Log($"release");
        Release();
    }

    protected override async void EventPlaybackReady(HISPlayerEventInfo eventInfo)
    {
        if (EndPlay)
        {
            Debug.Log("播放已結束，忽略再次播放");
            return;
        }
        if (eventInfo.eventType == HISPlayerEvent.HISPLAYER_EVENT_PLAYBACK_READY)
        {
            EndPlay = false;
            // if (!haveVideoReady)
            // {
            //     Pause(0);
            //     haveVideoReady = true;
            //     return;
            // }
            StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha = 1;
            Debug.Log($"alpha to 1 {StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha = 1}");
            curPlayingUrl = multiStreamProperties[eventInfo.playerIndex].url[0];
            block.SetActive(false);
            Play(eventInfo.playerIndex);
            if (GameSettingPage.Instance != null)
            {
                SetVolume(eventInfo.playerIndex, GameSettingPage.Instance.VideoVolum);
            }
            else
            {
                Debug.Log("GameSettingPage 實例未找到！ 默認音量質");
                SetVolume(eventInfo.playerIndex, 1);
            }
            waitready = true;
        }
        else
        {
            block.SetActive(true);
        }
    }
    protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
    {
        if (eventInfo.eventType == HISPlayerEvent.HISPLAYER_EVENT_PLAYBACK_SEEK)
        {
            Debug.Log($"Seek to time in{GetVideotime()}");
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
        if (EndPlay)
        {
            Debug.Log("EventEndOfContent 已觸發過，跳過");
            return;
        }
        EndPlay = true;
        StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha = 0;
        Debug.Log($"Alphat to 0{StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha},haveVIdeoReady{haveVideoReady}");
        if (!haveVideoReady)
        {
            StartNani.Instance.OpenPageMessage();
            haveVideoReady = true;
        }
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
            // 建立反向映射 (URL → 名稱)
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
        EndPlay = false;
        string url = GetNameByUrl(input) ?? input; // 如果是名稱，轉換為 URL；如果是 URL，直接使用

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError($"Play: 找不到對應的 URL，輸入值: {input}");
            return;
        }

        Debug.Log($"Play: {url}");
        NaniCommandManger.Instance.videoOnLoop = false;

        // if (firstPlay)
        // {
        //     multiStreamProperties[0].renderMode = HISPlayerRenderMode.RenderTexture;
        //     firstPlay = false;
        // }

        waitready = false;
        if (multiStreamProperties[0].url.Count == 0)
        {
            AddVideoContent(0, url);
        }
        else
        {
            ChangeVideoContent(0, url);
            Debug.Log("ChangeVideo");
        }

        while (!waitready)
        {
            await UniTask.DelayFrame(1);
            if (waitready) break;
        }

        await SubtitlesManager.Instance.LoadSubtitles();
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
        // Debug.Log($"set video time from {curTime} to {newTime}");
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
        Debug.Log($"shrimp seek time{setsecond}");
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
}