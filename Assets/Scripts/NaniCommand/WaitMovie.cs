using Naninovel;
using UnityEngine;

[CommandAlias("waitMovie")]
public class WaitMovie : Command
{
    [ParameterAlias("choiceTime")]
    public DecimalParameter waitTime; // 可以保留，但現在主要用剩餘時間判斷

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await WaitForRemainingTimeAsync(9f, asyncToken); // 剩餘 9 秒時結束
    }

    private static async UniTask WaitForRemainingTimeAsync(float remainingThreshold, AsyncToken asyncToken)
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null)
        {
            Debug.LogError("[WaitMovie] 找不到 WebGLStreamController");
            return;
        }

        Debug.Log($"[WaitMovie] 等待影片剩餘 <= {remainingThreshold} 秒");

        bool choiceHandled = false;

        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
        {
            float curSec = controller.GetVideotime() / 1000f;
            float totalLength = controller.GetVideoLenght() / 1000f;
            float remainingTime = totalLength - curSec;

            // 如果影片播放結束，直接退出
            if (controller.EndPlay) break;

            // 外部控制器提供選項點，確保 WaitMovie 不被跳過
            if (controller.waitingForChoice && !choiceHandled)
            {
                if (curSec < controller.choiceAppearTime)
                {
                    await controller.SeekTime((long)(controller.choiceAppearTime * 1000));
                    curSec = controller.choiceAppearTime;
                    Debug.Log($"[WaitMovie] 快轉到選項點 {controller.choiceAppearTime}s");
                }
                choiceHandled = true;
            }

            // 剩餘時間小於等於閾值，結束等待
            if (remainingTime <= remainingThreshold)
            {
                Debug.Log($"[WaitMovie] 剩餘時間 {remainingTime:F1}s <= {remainingThreshold}s，結束等待");
                break;
            }

            await UniTask.Yield();
        }

        // 緩衝延遲
        await UniTask.Delay(1000);

        Debug.Log("[WaitMovie] 等待結束，流程繼續");
    }
}
