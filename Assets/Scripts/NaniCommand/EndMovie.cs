using Naninovel;
using UnityEngine;

public class EndMovie : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        WebGLStreamController webGLStreamController = WebGLStreamController.Instance;

        IInputManager inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;
        //TODO TEMP TO SKIP ENDMOVIE ENDMOVIE IN C3_S4_P2 HAVE ERROE
        float timer = 0f; // 計時器
        const float timeout = 3f; // 10秒的限制

        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
        {
            await AsyncUtils.WaitEndOfFrameAsync(asyncToken);

            if (webGLStreamController == null)
            {
                Debug.Log("still loop");
                return;
            }
            var waitedEnough = webGLStreamController.EndPlay;

            // 更新計時器
            timer += Time.deltaTime;

            // 如果超過10秒，設置 waitedEnough 為 true
            if (timer >= timeout)
            {
                Debug.Log("Timeout reached, setting waitedEnough to true");
                webGLStreamController.extendEndOfContent(new HISPlayerAPI.HISPlayerEventInfo());
                // waitedEnough = true;
                // webGLStreamController.EndPlay = true; // 確保狀態同步到控制器
            }

            if (waitedEnough) break;
        }

        Debug.Log("unloop");
        await UniTask.CompletedTask;
    }
}
