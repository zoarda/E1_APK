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
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;

        // 存給 StartNani
        StartNani.Instance.choiceTime = (int)choiceTime;

        // 設置 WebGLStreamController 選項點
        var controller = WebGLStreamController.Instance;
        if (controller != null)
        {
            controller.SetChoiceAppear(choiceTime);
            Debug.Log($"[ForcedChoice] WebGLStreamController 選項點設置為: {choiceTime}s");
        }
        else
        {
            Debug.LogWarning("[ForcedChoice] 找不到 WebGLStreamController");
        }

        Debug.Log($"[ForcedChoice] 設置選項出現倒數時間: {choiceTime}s");

        await UniTask.CompletedTask; // 立即完成
    }
}
