using Naninovel;
using UnityEngine.Video;

[CommandAlias("VideoPlay")]
public class VideoPlay : Command
{
    [ParameterAlias("Mode")]
    public StringParameter Mode;

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        VideoPlayer videoPlayer = VideoManager.Instance.GetVideoPlayer();
        AudioManager audioManager = AudioManager.Instance;
        if (Mode == "Play")
        {
            videoPlayer.Play();
            audioManager.PlayBGM();
        }
        else if (Mode == "Stop")
        {
            videoPlayer.Pause();
            audioManager.PauseBGM();
        }
        await UniTask.CompletedTask;
    }
}
