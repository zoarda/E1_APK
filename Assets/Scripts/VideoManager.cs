using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using UnityEngine.Video;
using System.Collections;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public static VideoManager instance;

    public Button Btn_SpeedViewFront, Btn_ChoiceViewFront, Btn_Speed;

    public Toggle Btn_PlayPause;

    WebGLStreamController webGLStreamController;

    public static VideoManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VideoManager>();
            }
            return instance;
        }
    }
    void Awake()
    {
        if (webGLStreamController == null)
        {
            webGLStreamController = WebGLStreamController.Instance;
        }
    }
    public async UniTask InitVideoControll()
    {
        //快進
        Btn_SpeedViewFront.onClick.AddListener(() =>
        {
            WebGLStreamController.Instance.AddTime(5000);
            // SkipVideo();
            // AudioManager.Instance.SyncAudioWithVideo(videoPlayer);
        });
        //快進到選擇
        Btn_ChoiceViewFront.onClick.AddListener(async () =>
        {
            if (webGLStreamController == null)
            {
                webGLStreamController = WebGLStreamController.Instance;
            }
            float videoPlayerTime = webGLStreamController.GetVideotime() / 1000f;
            float videoPlayerLength = webGLStreamController.GetVideoLenght() / 1000f;
            // NaniCommandManger.Instance.SpeedButtonClearSpawn();

            // 快退的条件检查
            int choiceTime = StartNani.Instance.choiceTime;
            float targetTime = videoPlayerLength - choiceTime - 2; // 目标时间（秒）

            if (videoPlayerTime >= targetTime)
            {
                Debug.Log("快退操作无效：播放时间已接近目标时间点。");
                return;
            }

            // 快退到目标时间（毫秒）
            long targetTimeInMilliseconds = (long)(targetTime * 1000);
            await webGLStreamController.SeekTime(targetTimeInMilliseconds);

            Debug.Log($"视频快退到时间点: {targetTime} 秒");
        });
        //快退
        // Btn_SpeedViewBack.onClick.AddListener(() =>
        // {
        //     NaniCommandManger.Instance.SpeedButtonClearSpawn();
        //     // videoPlayer = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        //     if (videoPlayer.time - 15 <= 0)
        //     {
        //         videoPlayer.time = 0;
        //         return;
        //     }
        //     videoPlayer.time -= 15;
        // });
        //播放暫停
        // Btn_PlayPause.onValueChanged.AddListener(async (value) =>
        // {
        //     // videoPlayer = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        //     if (value)
        //     {
        //         await WebGLStreamController.Instance.PlayPause();
        //         AudioManager.Instance.PauseBGM();
        //     }
        //     else
        //     {
        //         await WebGLStreamController.Instance.PlayVideo();
        //         AudioManager.Instance.PlayBGM();
        //     }
        // });
        Btn_PlayPause.onValueChanged.AddListener(OnPlayToggleChanged);
        async void OnPlayToggleChanged(bool isOn)
        {
            if (isOn)
            {
                Btn_PlayPause.image.SetOpacity(0);
                await WebGLStreamController.Instance.PlayPause();
                AudioManager.Instance.PauseBGM();
            }
            else
            {
                Btn_PlayPause.image.SetOpacity(1);
                await WebGLStreamController.Instance.PlayVideo();
                AudioManager.Instance.PlayBGM();
            }
        }
        await UniTask.CompletedTask;
    }
    public async UniTask SkipVideo()
    {
        if (webGLStreamController == null)
        {
            webGLStreamController = WebGLStreamController.Instance;
        }
        float videoPlayerTime = webGLStreamController.GetVideotime() / 1000f;
        float videoPlayerLength = webGLStreamController.GetVideoLenght() / 1000f;
        // NaniCommandManger.Instance.SpeedButtonClearSpawn();

        // 快退的条件检查
        int choiceTime = StartNani.Instance.choiceTime;
        float targetTime = videoPlayerLength - choiceTime - 2; // 目标时间（秒）

        if (videoPlayerTime >= targetTime)
        {
            Debug.Log("快退操作无效：播放时间已接近目标时间点。");
            return;
        }

        // 快退到目标时间（毫秒）
        long targetTimeInMilliseconds = (long)(targetTime * 1000);
        await webGLStreamController.SeekTime(targetTimeInMilliseconds);

        Debug.Log($"视频快退到时间点: {targetTime} 秒");
    }
    public void SetVideoPlayer(VideoPlayer video)
    {
        videoPlayer = video;
        NaniCommandManger.Instance.CheckAndFixPlaybackSpeed();
        AudioManager audioManager = AudioManager.Instance;
        StartCoroutine(audioManager.SetvideoAudioAsync(video.clip.name));
        StartCoroutine(WaitSyncAudioWithVideo());
    }
    public VideoPlayer GetVideoPlayer()
    {
        return videoPlayer;
    }
    public void PlayVideoPlayer()
    {
        videoPlayer.Play();
    }
    IEnumerator WaitSyncAudioWithVideo()
    {
        yield return new WaitForSeconds(0.1f);
        try
        {
            AudioManager.Instance.SyncAudioWithVideo(videoPlayer);
        }
        catch
        {
            Debug.Log("Syncfailed");
        }
    }
}
