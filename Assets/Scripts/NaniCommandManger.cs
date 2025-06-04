using System.Collections;
using Naninovel;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Video;
using System.Linq;
using System;
using System.Threading.Tasks;

public class NaniCommandManger : MonoBehaviour
{

    public Slider WaitTimerSlider;
    WebGLStreamController webGLStreamController;
    private List<GameObject> ChoiceButtonList = new List<GameObject>();
    private List<GameObject> ReplayButtonList = new List<GameObject>();
    [SerializeField] private Button Btn_C1_VB, Btn_C2_VB, Btn_C3_VB, Btn_C4_VB, Btn_C5_VB;
    // [SerializeField] private float Seeweight;
    Coroutine corSlider;
    RenderTexture CurrentRenderTexture;

    public VideoPlayer video;

    public VideoPlayer CloneVideo;
    // VideoPlayer currentVideo = null;
    UrlToScence urlToScence;

    private string videoName;
    public bool isLooping = false;

    private float SetSLT;
    private float SetELT;

    private bool videoSeekDone = true;

    public Camera MainCamera;
    public Canvas canvas;
    public bool videoOnLoop;
    public Sprite SprSpeed_1, SprSpeed_2, SprSpeed_3;

    private int currentSpeedIndex = 0; // 追蹤當前速度模式 (0=1x, 1=2x, 2=3x)
    private readonly float[] playbackSpeeds = { 1f, 2f, 3f }; // 播放速度對應表

    static NaniCommandManger instance;
    public static NaniCommandManger Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NaniCommandManger>();
            }
            return instance;
        }
    }
    void Update()
    {
        if (isLooping)
        {
            LoopVideo();
            UpdateSlider();
        }
        // if (currentVideo != null)
        // {
        // }
    }
    public async UniTask Init()
    {
        try
        {
            urlToScence = await YamlLoader.LoadStreamingAssetsYaml<UrlToScence>(Application.streamingAssetsPath + "/Yaml/URLToScence.yaml");
        }
        catch (Exception e)
        {
            Debug.Log($"script urltosence{e}");
        }
    }

    public void StartSlider(float duration)
    {
        IEnumerator StartSliderCor()
        {
            float startTime = Time.time;
            WaitTimerSlider.gameObject.SetActive(true);
            while (true)
            {
                float weight = Mathf.Clamp01((Time.time - startTime) / duration);
                weight = 1f - weight;
                WaitTimerSlider.value = weight;
                // Seeweight = weight;
                if (weight == 0f)
                    break;

                yield return null;
            }
        }

        StopSlider();
        corSlider = StartCoroutine(StartSliderCor());

        // @back C2_S1_P1 id:VideoBackground time:0.5
        // new Naninovel.Commands.ModifyBackground()
        // {
        //     AppearanceAndTransition = new NamedString("C2_S1_P1", "Id"),
        //     Id = "VideoBackground",
        //     Duration = 0.5f
        // }.ExecuteAsync().Forget();
    }

    public void StopSlider()
    {
        if (corSlider != null)
        {
            StopCoroutine(corSlider);
            WaitTimerSlider.gameObject.SetActive(false);
        }

    }
    public void SpeedButtonClearSpawn()
    {
        try
        {
            ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
            var allSpawned = spawnManager.GetAllSpawned();
            foreach (var spawned in allSpawned)
            {
                spawnManager.DestroySpawned(spawned.Path);
            }
            isLooping = false;
        }
        catch (Exception e)
        {
            Debug.Log($"Clear fail {e}");
        }
    }
    public void SpawnLovePlayPageButton(LovePlayPage lovePlayPage, GameObject btnObject, string poseName, string label, string type)
    {       
        // varManager.TrySetVariableValue("friendship", saveData.friendship);
        // friednshipList.Add(saveData.friendship);

        var player = Engine.GetService<IScriptPlayer>();
        var script = Engine.GetService<IScriptPlayer>();
        GameObject objCum = lovePlayPage.NextVideoButtonPrefab;
        GameObject objHidenCum = lovePlayPage.HidenVideoButtonPrefab;
        GameObject objSetting = lovePlayPage.SettingButtonPrefab;
        // var startLineIndex = script.PlayedScript.GetLineIndexForLabel(label);
        //生成choice按钮
        GameObject objChoice = Instantiate(btnObject, lovePlayPage.ChoiceButtonParent.transform);
        Button choiceButton = objChoice.GetComponent<Button>();
        LovePlayPageButton lovePlayPageButtonChoice = objChoice.GetComponent<LovePlayPageButton>();
        Image imaChoice = lovePlayPageButtonChoice.ImaIcon;
        Image sdChoice = lovePlayPageButtonChoice.ImaSlider;
        Image imageOk = lovePlayPageButtonChoice.ImaOk;

        sdChoice.fillAmount = 0;
        lovePlayPageButtonChoice.scriptLabel = label;
        choiceButton.interactable = true;
        imaChoice.sprite = Resources.Load<Sprite>("LovePlayPage/" + poseName);
        imaChoice.SetNativeSize();
        objChoice.AddComponent<MouseHover>();
        ChoiceButtonList.Add(objChoice);

        // //生成replay按钮
        // GameObject objReplay = Instantiate(btnObject, lovePlayPage.ReplayButtonContent.transform);
        // Button replayButton = objReplay.GetComponent<Button>();
        // LovePlayPageButton lovePlayPageButtonReplay = objReplay.GetComponent<LovePlayPageButton>();
        // Image imaReplay = lovePlayPageButtonReplay.ImaIcon;
        // Image sdReplay = lovePlayPageButtonReplay.ImaSlider;

        // sdReplay.fillAmount = 0;
        // lovePlayPageButtonReplay.scriptLabel = label;
        // imaReplay.sprite = null;
        // imaReplay.AddComponent<MouseHover>();
        // ReplayButtonList.Add(objReplay);

        //设置choice按钮的label
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        var varManager = Engine.GetService<ICustomVariableManager>();
        var myValue = varManager.GetVariableValue("friendship");
        float a = float.Parse(myValue);
        choiceButton.onClick.RemoveAllListeners();
        choiceButton.onClick.AddListener(async () =>
        {
            await player.PreloadAndPlayAsync(script.PlayedScript.name, label: label);
            imageOk.gameObject.SetActive(true);
            // choiceButton.interactable = false;
            isLooping = false;
            if (type == "foreplay")
            {
                lovePlayPage.NextVideoButton.interactable = true;
                lovePlayPage.NextVideoButtonLock.SetActive(false);
            }
            if (type == "sex")
            {
                lovePlayPage.NextVideoButton.interactable = true;
                lovePlayPage.NextVideoButtonLock.SetActive(false);
            }
            if (type == "hidesex")
            {
                lovePlayPage.NextVideoButton.interactable = true;
                lovePlayPage.NextVideoButtonLock.SetActive(false);
                if (a >= 3)
                {
                    lovePlayPage.HidenVideoButton.interactable = true;
                    lovePlayPage.HidenVideoButtonLock.SetActive(false);
                }
            }
            if (type == "demo")
            {
                // lovePlayPage.NextVideoButton.interactable = true;
                // lovePlayPage.NextVideoButtonLock.SetActive(false);
                // if (a >= 3)
                // {
                //     lovePlayPage.HidenVideoButton.interactable = true;
                //     lovePlayPage.HidenVideoButtonLock.SetActive(false);
                // }
            }
            if (type == "demoForPlay")
            {

                lovePlayPage.NextVideoButton.interactable = true;
                lovePlayPage.NextVideoButtonLock.SetActive(false);
                if (a >= 3)
                {
                    lovePlayPage.HidenVideoButton.interactable = true;
                    lovePlayPage.HidenVideoButtonLock.SetActive(false);
                }
            }
            // btnCum.interactable = true;
            // for (int i = 0; i < ReplayButtonList.Count; i++)
            // {
            //     Image imaReplayTemp = ReplayButtonList[i].GetComponentInChildren<Image>();
            //     Button replayButtonTemp = ReplayButtonList[i].GetComponent<Button>();
            //     LovePlayPageButton lovePlayPageButtonReplayTemp = ReplayButtonList[i].GetComponent<LovePlayPageButton>();
            //     if (replayButtonTemp.interactable == false)
            //     {
            //         // 更新 Replay 按钮的图像和点击事件
            //         imaReplayTemp.sprite = imaChoice.sprite;
            //         imaReplayTemp.SetNativeSize();
            //         replayButtonTemp.interactable = true;
            //         replayButtonTemp.onClick.RemoveAllListeners();
            //         replayButtonTemp.onClick.AddListener(async () =>
            //         {
            //             await player.PreloadAndPlayAsync(script.PlayedScript.Name, label: label);
            //             isLooping = false;
            //         });
            //         break;
            //     }
            // }
        });
        // btnCum.onClick.RemoveAllListeners();
        // btnCum.onClick.AddListener(async () =>
        // {
        //     var Spawnobj = Engine.GetService<ISpawnManager>();
        //     Spawnobj.DestroySpawned("LovePlayPage");
        //     await player.PreloadAndPlayAsync(script.PlayedScript.Name, label: cumLabel);
        //     isLooping = false;
        // });

    }
    public void ClearLovePlayPage()
    {
        foreach (var item in ChoiceButtonList)
        {
            Destroy(item);
        }
        foreach (var item in ReplayButtonList)
        {
            Destroy(item);
        }
        ChoiceButtonList.Clear();
        ReplayButtonList.Clear();
    }
    public void SetLovePlayPageMode(LovePlayPage lovePlayPage, string cumLabel, string hidenCumLabel, string mode)
    {
        VideoManager videoManager = VideoManager.Instance;
        GameObject objCum = lovePlayPage.NextVideoButtonPrefab;
        GameObject objHidenCum = lovePlayPage.HidenVideoButtonPrefab;
        GameObject objSetting = lovePlayPage.SettingButtonPrefab;
        Button btnCum = lovePlayPage.NextVideoButtonPrefab.GetComponent<Button>();
        Button btnHidenCum = lovePlayPage.HidenVideoButtonPrefab.GetComponent<Button>();
        Button btnSetting = lovePlayPage.SettingButtonPrefab.GetComponent<Button>();
        Button btnSpeed = lovePlayPage.SpeedButton;
        Image imaCum = objCum.GetComponentInChildren<Image>();
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        lovePlayPage.NextVideoButtonLock.SetActive(true);
        lovePlayPage.HidenVideoButtonLock.SetActive(true);
        btnCum.interactable = false;
        btnHidenCum.interactable = false;
        btnCum.onClick.RemoveAllListeners();
        btnSetting.onClick.RemoveAllListeners();
        btnSpeed.onClick.RemoveAllListeners();
        btnHidenCum.onClick.RemoveAllListeners();
        if (mode == "foreplay")
        {
            objCum.SetActive(true);
            objHidenCum.SetActive(false);
            objSetting.SetActive(true);
            imaCum.sprite = Resources.Load<Sprite>("LovePlayPage/sex_P_02");
            imaCum.SetNativeSize();
            btnCum.onClick.AddListener(() =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                player.PreloadAndPlayAsync(player.PlayedScript.name, label: cumLabel);
                isLooping = false;
            });
            btnSetting.onClick.AddListener(async () =>
            {
                StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                startNani.OptionPage.SetActive(!startNani.OptionPage.activeSelf);
                if (webGLStreamController == null)
                {
                    webGLStreamController = WebGLStreamController.Instance;
                }
                await webGLStreamController.PlayPause();
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
            });
            // 速度切換按鈕
            btnSpeed.onClick.AddListener(() =>
            {
                CyclePlaybackSpeed();
            });
        }
        if (mode == "sex")
        {
            objCum.SetActive(true);
            objHidenCum.SetActive(false);
            objSetting.SetActive(true);
            imaCum.sprite = Resources.Load<Sprite>("LovePlayPage/sex_N_02");
            imaCum.SetNativeSize();
            btnCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: cumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");
            });
            btnSetting.onClick.AddListener(async () =>
            {
                StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                startNani.OptionPage.SetActive(!startNani.OptionPage.activeSelf);
                if (webGLStreamController == null)
                {
                    webGLStreamController = WebGLStreamController.Instance;
                }
                await webGLStreamController.PlayPause();
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                // spawnManager.DestroySpawned("LovePlayPage");
            });
            // 速度切換按鈕
            btnSpeed.onClick.AddListener(() =>
            {
                CyclePlaybackSpeed();
            });

        }
        if (mode == "hidesex")
        {
            GameObject objchoice = GameObject.Find("ChoiceButtonContent");
            var objs = objchoice.GetComponentsInChildren<LovePlayPageButton>();
            foreach (var obj in objs)
            {
                var objbs = obj.GetComponent<Button>();
                if (obj.scriptLabel == "Test")
                {
                    obj.Lock.gameObject.SetActive(true);
                    objbs.interactable = false;
                }
            }
            objCum.SetActive(true);
            objHidenCum.SetActive(true);
            objSetting.SetActive(true);
            imaCum.sprite = Resources.Load<Sprite>("LovePlayPage/sex_N_02");
            imaCum.SetNativeSize();
            objHidenCum.GetComponent<Image>().sprite = Resources.Load<Sprite>("LovePlayPage/sex_O_02");
            objHidenCum.GetComponent<Image>().SetNativeSize();
            btnCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: cumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                spawnManager.DestroySpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);

            });
            btnHidenCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: hidenCumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");
            });
            btnSetting.onClick.AddListener(async () =>
            {
                StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                startNani.OptionPage.SetActive(!startNani.OptionPage.activeSelf);
                if (webGLStreamController == null)
                {
                    webGLStreamController = WebGLStreamController.Instance;
                }
                await webGLStreamController.PlayPause();
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                // spawnManager.DestroySpawned("LovePlayPage");
            });
            // 速度切換按鈕
            btnSpeed.onClick.AddListener(() =>
            {
                CyclePlaybackSpeed();
            });
        }
        if (mode == "demo")
        {
            GameObject objchoice = GameObject.Find("ChoiceButtonContent");
            var objs = objchoice.GetComponentsInChildren<LovePlayPageButton>();
            foreach (var obj in objs)
            {
                var objbs = obj.GetComponent<Button>();
                if (obj.scriptLabel == "Test")
                {
                    obj.Lock.gameObject.SetActive(true);
                    objbs.interactable = false;
                }
            }
            objCum.SetActive(true);
            objHidenCum.SetActive(false);
            objSetting.SetActive(true);
            imaCum.sprite = Resources.Load<Sprite>("LovePlayPage/sex_N_02");
            imaCum.SetNativeSize();
            objHidenCum.GetComponent<Image>().sprite = Resources.Load<Sprite>("LovePlayPage/sex_O_02");
            objHidenCum.GetComponent<Image>().SetNativeSize();
            btnCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: cumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");

            });
            btnHidenCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: hidenCumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");
            });
            btnSetting.onClick.AddListener(async () =>
            {
                StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                startNani.OptionPage.SetActive(!startNani.OptionPage.activeSelf);
                if (webGLStreamController == null)
                {
                    webGLStreamController = WebGLStreamController.Instance;
                }
                await webGLStreamController.PlayPause();
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
            });
        }
        if (mode == "demoForPlay")
        {
            // GameObject objchoice = GameObject.Find("ChoiceButtonContent");
            // var objs = objchoice.GetComponentsInChildren<LovePlayPageButton>();
            // foreach (var obj in objs)
            // {
            //     var objbs = obj.GetComponent<Button>();
            //     if (obj.scriptLabel == "Test")
            //     {
            //         obj.Lock.gameObject.SetActive(true);
            //         objbs.interactable = false;
            //     }
            // }
            objCum.SetActive(true);
            objHidenCum.SetActive(false);
            objSetting.SetActive(true);
            imaCum.sprite = Resources.Load<Sprite>("LovePlayPage/sex_P_02");
            imaCum.SetNativeSize();
            objHidenCum.GetComponent<Image>().sprite = Resources.Load<Sprite>("LovePlayPage/sex_O_02");
            objHidenCum.GetComponent<Image>().SetNativeSize();
            btnCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: cumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");
            });
            btnHidenCum.onClick.AddListener(async () =>
            {
                var player = Engine.GetService<IScriptPlayer>();
                await player.PreloadAndPlayAsync(player.PlayedScript.name, label: hidenCumLabel);
                isLooping = false;
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                // lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
                spawnManager.DestroySpawned("LovePlayPage");

            });
            btnSetting.onClick.AddListener(async () =>
            {
                StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
                startNani.OptionPage.SetActive(!startNani.OptionPage.activeSelf);
                if (webGLStreamController == null)
                {
                    webGLStreamController = WebGLStreamController.Instance;
                }
                await webGLStreamController.PlayPause();
                var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
                lovePlayPagePrefab.GameObject.SetActive(!lovePlayPagePrefab.GameObject.activeSelf);
            });
            // 速度切換按鈕
            btnSpeed.onClick.AddListener(() =>
            {
                CyclePlaybackSpeed();
            });
        }
    }

    public void SetCheckLoop(bool isLoop)
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        var length = webGLStreamController.GetVideoLenght();
        var videolength = length / 1000f;
        if (videolength <= SetELT)
        {
            isLooping = isLoop;
            Debug.Log($"VideoLenght less SetELT");
            SetELT = videolength - 1;
            Debug.Log($"BaseVideoTime /ELT{SetELT}");
            SetSLT = videolength - 10;
            Debug.Log($"BaseVideoTime /SLT{SetSLT}");
            return;
        }
        isLooping = isLoop;
    }
    public async UniTask SetVideoTime(float SLT, float ELT, bool isLoop)
    {
        // video = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        // VideoManager videoManager = VideoManager.Instance;
        // WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        SetSLT = SLT;
        SetELT = ELT;
        videoOnLoop = isLoop;
        Debug.Log($"SetVideoTime / SLT{SetSLT}");
        Debug.Log($"SetVideoTime / ELT{SetELT}");
        // video = videoManager.GetVideoPlayer();
        // video.seekCompleted += seekCompleted;
        // videoSeekDone = true;
        // SetSLT = SLT;
        // SetELT = ELT;
        // CurrentRenderTexture = video.targetTexture;
        // if (CloneVideo != null)
        //     Destroy(CloneVideo.gameObject);
        // CloneVideo = Instantiate(video, video.transform.parent);
        // CloneVideo.gameObject.name = "CloneVideo";
        // CloneVideo.Pause();
        // CloneVideo.time = SetSLT;
        // CloneVideo.seekCompleted += seekCompleted;
        // CloneVideo.targetTexture = null;
        // currentVideo = video;

    }
    void seekCompleted(VideoPlayer par)
    {
        StartCoroutine(WaitToUpdateRenderTextureBeforeEndingSeek());
    }

    IEnumerator WaitToUpdateRenderTextureBeforeEndingSeek()
    {
        yield return new WaitForNextFrameUnit();
        videoSeekDone = true;
    }
    public async void LoopVideo()
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        if (webGLStreamController.GetVideotime() / 1000f >= SetELT)
        {
            isLooping = false;
            var nowtime = webGLStreamController.GetVideotime();
            Debug.Log($"now time{nowtime}");
            await webGLStreamController.SeekTime((long)SetSLT * 1000);
            await SubtitlesManager.Instance.LoadSubtitles();
        }
        // if (currentVideo == null)
        //     return;
        // if (currentVideo.time >= SetELT && videoSeekDone == true)
        // {
        //     currentVideo.time = SetSLT;
        //     currentVideo.Pause();
        //     currentVideo.targetTexture = null;
        //     currentVideo = currentVideo == video ? CloneVideo : video;

        //     currentVideo.targetTexture = CurrentRenderTexture;
        //     currentVideo.Play();
        //     videoSeekDone = false;

        //     // SubtitlesManager subtitlesManager = GameObject.Find("SubtitlesManager")?.GetComponent<SubtitlesManager>();
        //     SubtitlesManager subtitlesManager = SubtitlesManager.Instance;
        //     subtitlesManager.videoPlayer = currentVideo;
        //     CheckAndFixPlaybackSpeedLoop(currentVideo);
        //     await subtitlesManager.LoadSubtitles();
        // }
    }
    void UpdateSlider()
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        var spawneObjects = Engine.GetService<ISpawnManager>().GetAllSpawned();
        var lovePlayPageObject = spawneObjects.FirstOrDefault(i => i.Path == "LovePlayPage");
        if (lovePlayPageObject != null && lovePlayPageObject.GameObject.activeInHierarchy == true)
        {
            foreach (var item in ChoiceButtonList)
            {
                LovePlayPageButton lovePlayPageButton = item.GetComponent<LovePlayPageButton>();
                Image sdChoice = lovePlayPageButton.ImaSlider;
                string name = lovePlayPageButton.scriptLabel;
                WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
                var videoTime = webGLStreamController.GetVideotime() / 1000f;
                var videolength = webGLStreamController.GetVideoLenght() / 1000f;
                try
                {
                    string url = webGLStreamController.multiStreamProperties[0].url[0];
                    if (urlToScence.videoDictionary.TryGetValue(url, out string script))
                    {
                        videoName = script;
                    }
                    else
                    {
                        Debug.Log($"url not found");
                    }
                }
                catch (Exception e)
                {
                    sdChoice.fillAmount = 0;
                    Debug.Log($"{e}");
                }
                if (videoName == name)
                {
                    if (videoTime < SetSLT)
                    {
                        float a = LerpIF(0, SetSLT, videoTime);
                        sdChoice.fillAmount = a;
                    }
                    if (videoTime > SetSLT && videoTime < SetELT)
                    {
                        float a = LerpIF(SetSLT, SetELT, videoTime);
                        sdChoice.fillAmount = a;
                    }
                    if (videoTime > SetELT)
                    {
                        float a = LerpIF(SetELT, videolength, videoTime);
                        sdChoice.fillAmount = a;
                    }
                }
                else
                {
                    sdChoice.fillAmount = 0;
                }
            }
        }
        else
        {
            return;
        }
    }
    float LerpIF(float a, float b, float p)
    {
        if ((a - b) == 0)
        {
            return 0;
        }
        float result = (p - a) / (b - a);
        if (result > 1)
        {
            return 1;
        }
        if (result < 0)
        {
            return 0;
        }
        return result;
    }
    public async void CyclePlaybackSpeed()
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        var spawneObjects = Engine.GetService<ISpawnManager>().GetAllSpawned();
        var lovePlayPage = spawneObjects.FirstOrDefault(i => i.Path == "LovePlayPage");
        if (lovePlayPage == null)
            return;
        // VideoManager videoManager = VideoManager.Instance;
        currentSpeedIndex = (currentSpeedIndex + 1) % playbackSpeeds.Length;
        float newSpeed = playbackSpeeds[currentSpeedIndex];

        // 設定影片播放速度
        // videoManager.GetVideoPlayer().playbackSpeed = newSpeed;
        webGLStreamController.PlaySpeed(newSpeed);

        // 更新按鈕圖片
        UpdateSpeedButtonSprite();

        Debug.Log($"Playback speed set to {newSpeed}x");
    }
    /// <summary>
    /// 更新速度切換按鈕的圖片d
    /// </summary>
    public void UpdateSpeedButtonSprite()
    {
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
        LovePlayPage lovePlayPage = lovePlayPagePrefab.GameObject.GetComponent<LovePlayPage>();
        switch (currentSpeedIndex)
        {
            case 0: // 1x
                lovePlayPage.SpeedButton.image.sprite = SprSpeed_1;
                break;
            case 1: // 2x
                lovePlayPage.SpeedButton.image.sprite = SprSpeed_2;
                break;
            case 2: // 3x
                lovePlayPage.SpeedButton.image.sprite = SprSpeed_3;
                break;
        }
    }
    public async void CheckAndFixPlaybackSpeed()
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        // VideoManager videoManager = VideoManager.Instance;
        float currentPlaybackSpeed = webGLStreamController.GetPlaySpeed();
        float expectedSpeed = playbackSpeeds[currentSpeedIndex];

        if (!Mathf.Approximately(currentPlaybackSpeed, expectedSpeed))
        {
            Debug.Log($"Fixing playback speed: Expected {expectedSpeed}x, but got {currentPlaybackSpeed}x");
            // 修正播放速度為預期值
            currentSpeedIndex = (int)(currentPlaybackSpeed - 1);
            float newSpeed = playbackSpeeds[currentSpeedIndex];
            // 設定影片播放速度
            // videoManager.GetVideoPlayer().playbackSpeed = newSpeed;
            webGLStreamController.PlaySpeed(newSpeed);

            // 更新按鈕圖片
            UpdateSpeedButtonSprite();
        }
    }
    public void CheckAndFixPlaybackSpeedLoop(VideoPlayer videoPlayer)
    {
        float currentPlaybackSpeed = videoPlayer.playbackSpeed;
        float expectedSpeed = playbackSpeeds[currentSpeedIndex];

        if (!Mathf.Approximately(currentPlaybackSpeed, expectedSpeed))
        {
            Debug.Log($"Fixing playback speed: Expected {expectedSpeed}x, but got {currentPlaybackSpeed}x");
            videoPlayer.playbackSpeed = expectedSpeed; // 修正播放速度
            UpdateSpeedButtonSprite(); // 更新按鈕圖示
        }
    }
    public async UniTask switchCamera()
    {
        var camera = Engine.GetService<ICameraManager>();
        canvas.worldCamera = camera.UICamera;
        await UniTask.CompletedTask;
    }
    public class UrlToScence
    {
        public Dictionary<string, string> videoDictionary;
    }
}
