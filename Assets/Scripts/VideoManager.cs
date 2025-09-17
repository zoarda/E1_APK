using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using Cysharp.Threading.Tasks;

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
            if (instance == null) instance = FindObjectOfType<VideoManager>();
            return instance;
        }
    }

    void Awake()
    {
        if (webGLStreamController == null)
            webGLStreamController = WebGLStreamController.Instance;
    }

    public async UniTask InitVideoControll()
    {
        Btn_SpeedViewFront.onClick.AddListener(async () =>
        {
            await FastForward(5f);
        });

        Btn_ChoiceViewFront.onClick.AddListener(async () =>
        {
            await SkipVideo();
        });

        Btn_PlayPause.onValueChanged.AddListener(OnPlayToggleChanged);

        async void OnPlayToggleChanged(bool isOn)
        {
            if (webGLStreamController == null) return;

            if (isOn)
            {
                var img = Btn_PlayPause.image;
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f; // 0 = 完全透明, 1 = 不透明
                    img.color = c;
                }
                await webGLStreamController.PlayPause();
            }
            else
            {
                var img = Btn_PlayPause.image;
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 1f; // 完全不透明
                    img.color = c;
                }
                await webGLStreamController.PlayVideo();
            }
        }

        await UniTask.CompletedTask;
    }

    // 計算安全上限時間
    private float GetSafeLimit()
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null) return 0;

        float totalLength = controller.GetVideoLenght() / 1000f;
        float choiceTime = controller.choiceAppearTime;

        float loopEnd = NaniCommandManger.Instance.SetELT;
        bool looping = NaniCommandManger.Instance.isLooping;

        float safeLimit = totalLength;

        // 如果有循環點，限制在循環結束前
        if (looping && loopEnd > 0) safeLimit = Mathf.Min(safeLimit, loopEnd);

        // 如果有選項點，限制在選項前（安全距離）
        if (choiceTime > 0) safeLimit = Mathf.Min(safeLimit, choiceTime);

        // 確保剩餘至少 9 秒（防止快進到影片尾端導致異常）
        safeLimit = Mathf.Min(safeLimit, totalLength - 9f);

        return safeLimit;
    }

    // 普通快進
    public async UniTask FastForward(float seconds)
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null) return;

        await UniTask.WaitUntil(() => controller.GetVideoLenght() > 0 || controller.EndPlay);

        float curTime = controller.GetVideotime() / 1000f;
        float safeLimit = GetSafeLimit();

        if (curTime >= safeLimit)
        {
            Debug.Log("快進無效：已接近選項點或循環結束或影片結尾");
            return;
        }

        float newTime = Mathf.Min(curTime + seconds, safeLimit);
        await controller.SeekTime((long)(newTime * 1000));
        Debug.Log($"快進到: {newTime:F1}s (安全上限: {safeLimit:F1}s)");
    }

    // 快進到選項
    public async UniTask SkipVideo()
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null) return;

        await UniTask.WaitUntil(() => controller.GetVideoLenght() > 0 || controller.EndPlay);

        float choiceTime = controller.choiceAppearTime;
        if (choiceTime <= 0)
        {
            Debug.LogWarning("沒有設定選項點，無法快進");
            return;
        }

        float safeLimit = GetSafeLimit();
        float targetTime = Mathf.Min(choiceTime, safeLimit);

        await controller.SeekTime((long)(targetTime * 1000));
        Debug.Log($"已快進到選項點 ({targetTime:F1}s, 安全上限: {safeLimit:F1}s)");
    }


    public void SetVideoPlayer(VideoPlayer video)
    {
        videoPlayer = video;
        NaniCommandManger.Instance.CheckAndFixPlaybackSpeed();
        AudioManager audioManager = AudioManager.Instance;
        StartCoroutine(audioManager.SetvideoAudioAsync(video.clip.name));
        StartCoroutine(WaitSyncAudioWithVideo());
    }

    public VideoPlayer GetVideoPlayer() => videoPlayer;
    public void PlayVideoPlayer() => videoPlayer.Play();

    IEnumerator WaitSyncAudioWithVideo()
    {
        yield return new WaitForSeconds(0.1f);
        try
        {
            AudioManager.Instance.SyncAudioWithVideo(videoPlayer);
        }
        catch
        {
            Debug.Log("Sync failed");
        }
    }
}
