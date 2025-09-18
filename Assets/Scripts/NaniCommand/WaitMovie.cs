using Naninovel;
using UnityEngine;

[CommandAlias("waitMovie")]
public class WaitMovie : Command
{
    [ParameterAlias("choiceTime")]
    public DecimalParameter waitTime; // 可以保留，但現在主要用剩餘時間判斷

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        // 將 DecimalParameter 隱式轉為 float?，再給預設值 0f
        float? tempThreshold = waitTime;
        float threshold = Mathf.Max(0f, tempThreshold ?? 0f);

        await WaitForRemainingTimeAsync(threshold, asyncToken);
    }

    private static async UniTask WaitForRemainingTimeAsync(float remainingThreshold, AsyncToken asyncToken)
    {
        var controller = WebGLStreamController.Instance;
        if (controller == null)
        {
            Debug.LogError("[WaitMovie] 找不到 WebGLStreamController");
            return;
        }

        float safeTime = Mathf.Max(0f, controller.GetVideotime() / 1000f); // 記錄開始時刻的安全時間
        Debug.Log($"[WaitMovie] 等待影片剩餘 <= {remainingThreshold} 秒或選項點");

        bool choiceHandled = false; // 防止選項重複觸發
        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
        {
            float curSec = controller.GetVideotime() / 1000f;
            float totalLength = controller.GetVideoLenght() / 1000f;
            float remainingTime = totalLength - curSec;

            Debug.Log($"[WaitMovie] curSec: {curSec:F2}s, totalLength: {totalLength:F2}s, remainingTime: {remainingTime:F2}s");

            if (controller.EndPlay)
            {
                Debug.Log("[WaitMovie] 影片播放結束，跳出等待");
                break;
            }

            if (remainingTime <= remainingThreshold)
            {
                Debug.Log($"[WaitMovie] 剩餘時間 {remainingTime:F2}s <= {remainingThreshold}s，結束等待");
                break;
            }
            if (!choiceHandled && controller.waitingForChoice && remainingTime <= controller.choiceAppearTime)
            {
                Debug.Log($"[WaitMovie] 已到達選項點 (剩餘時間 {remainingTime:F2}s) <= {controller.choiceAppearTime}s，結束等待");
                choiceHandled = true;
                break;
            }
            await UniTask.Yield();
        }

        Debug.Log("[WaitMovie] 等待結束，流程繼續");
    }
}
