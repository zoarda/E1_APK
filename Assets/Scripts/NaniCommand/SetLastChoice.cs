using Naninovel;
using UnityEngine;
using UnityEngine.Video;

[CommandAlias("setLastChoice")]
public class SetLastChoice : Command
{

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await setLastChoiceAsync(asyncToken);
    }
    private static async UniTask setLastChoiceAsync(AsyncToken asyncToken)
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        VideoPlayer videoPlayer = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();
        startNani.lastChoiceName = videoPlayer.clip.name;
        await UniTask.CompletedTask;
    }
}