using Naninovel;
using UnityEngine;

public class VideoControll : Command
{
    [ParameterAlias("StartLoopTime")]
    public DecimalParameter startLoopTime;
    [ParameterAlias("EndLoopTime")]
    public DecimalParameter endLoopTime;
    [ParameterAlias("isLooping")]
    public BooleanParameter isLooping;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await VideoControllAsync(startLoopTime, endLoopTime, isLooping, asyncToken);
    }
    private static async UniTask VideoControllAsync(float SLT, float ELT, bool isLooping, AsyncToken asyncToken)
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null)
        {
            Debug.LogError("[VideoControll] 找不到 WebGLStreamController");
            return;
        }

        await controller.SetLoopSegment(SLT, ELT, isLooping);

        Debug.Log($"[VideoControll] 設定循環: Start={SLT}s, End={ELT}s, Loop={isLooping}");

        await UniTask.CompletedTask; // 立即完成，流程繼續
    }
}
