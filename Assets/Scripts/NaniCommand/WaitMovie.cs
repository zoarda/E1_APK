using Naninovel;
using UnityEngine;
using UnityEngine.Video;

[CommandAlias("waitMovie")]
public class WaitMovie : Command
{
    [ParameterAlias("choiceTime")]
    public DecimalParameter waitTime;

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await WaitForTimerAsync(waitTime, asyncToken);
    }

    private static async UniTask WaitForTimerAsync(float waitTime, AsyncToken asyncToken)
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;
        // var videoplayer = GameObject.Find("VideoBackground").GetComponentInChildren<VideoPlayer>();

        float HISplayerLenght = webGLStreamController.GetVideoLenght() / 1000f;
        IInputManager inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;
        Debug.Log("shrimp: startWhile");
        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
        {
            // await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
            await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
            if (webGLStreamController == null)
                return;
            var waitedEnough = HISplayerLenght - webGLStreamController.GetVideotime() / 1000f <= waitTime;
            if (waitedEnough) break;
        }
        Debug.Log("shrimp: endwhile");
        await UniTask.CompletedTask;
    }
}