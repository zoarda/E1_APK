using Naninovel;
using UnityEngine;
using UnityEngine.Video;

public class SeekTime : Command
{
    [ParameterAlias("videoTime")]
    public DecimalParameter videoTime;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SeekToTime(videoTime, asyncToken);
    }

    private static async UniTask SeekToTime(float time, AsyncToken asyncToken)
    {
        // VideoPlayer video = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        // VideoManager videoManager = VideoManager.Instance;
        // VideoPlayer video = videoManager.GetVideoPlayer();
        // video.time = time;
        await WebGLStreamController.Instance.NaniSeekTime((long)time * 1000);
        await UniTask.CompletedTask;
    }
}
