using Naninovel;
using UnityEngine;
using UnityEngine.Video;

[CommandAlias("forcedChoice")]
public class ForcedChoice : Command
{
    [ParameterAlias("choiceTime")]
    public DecimalParameter choiceTime;

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await ForcedChoiceAsync(choiceTime, asyncToken);
    }

    private static async UniTask ForcedChoiceAsync(float choiceTime, AsyncToken asyncToken)
    {
        IInputManager inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;

        StartNani startNani = StartNani.Instance;
        startNani.choiceTime = (int)choiceTime;
        await UniTask.CompletedTask;
    }
}
