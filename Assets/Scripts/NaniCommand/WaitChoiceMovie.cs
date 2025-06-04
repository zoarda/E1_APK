using Naninovel;
using UnityEngine;
using UnityEngine.Video;

[CommandAlias("waitChoiceMovie")]
public class WaitChoiceMovie : Command
{
    [ParameterAlias("choiceTime")]
    public DecimalParameter waitTime;

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await WaitForTimerAsync(waitTime, asyncToken);
    }

    private static async UniTask WaitForTimerAsync(float waitTime, AsyncToken asyncToken)
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        var backChoiceVideo = Engine.GetService<IBackgroundManager>();
        var videoPlayer =GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        var Choicevideoplayer = GameObject.Find("VideoChoiceBackground").GetComponentInChildren<VideoPlayer>();
        IInputManager inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;
        startNani.txtSubtitles.text = "";
        videoPlayer.Pause();
        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
       {
            // await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
            await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
            if (Choicevideoplayer == null)
                Choicevideoplayer = GameObject.Find("VideoChoiceBackground").GetComponentInChildren<VideoPlayer>();
            var waitedEnough = Choicevideoplayer.clip.length - Choicevideoplayer.time <= waitTime;
            if (waitedEnough) break;
        }
        backChoiceVideo.RemoveActor("VideoChoiceBackground");
        videoPlayer.Play();
        await UniTask.CompletedTask;
    }
}