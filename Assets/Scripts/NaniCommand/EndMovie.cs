using Naninovel;
using UnityEngine;

public class EndMovie : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        var webGLController = WebGLStreamController.Instance;
        if (webGLController == null)
        {
            Debug.LogError("[EndMovie] 找不到 WebGLStreamController");
            return;
        }

        IInputManager inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;

        // 用 TaskCompletionSource 來等待影片結束
        var tcs = new UniTaskCompletionSource();
        void Handler() => tcs.TrySetResult();

        webGLController.OnVideoEnded += Handler;

        float timer = 0f;
        const float timeout = 3f;

        while (Application.isPlaying && asyncToken.EnsureNotCanceledOrCompleted())
        {
            await UniTask.Yield();

            timer += Time.deltaTime;
            if (timer >= timeout)
            {
                Debug.Log("[EndMovie] Timeout reached, force stop video.");

                webGLController.EndPlay = true;
                typeof(WebGLStreamController)
                    .GetMethod("EventEndOfPlaylist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(webGLController, new object[] { new HISPlayerAPI.HISPlayerEventInfo() });

                break;
            }
        }

        await tcs.Task; // 等待影片結束回調

        webGLController.OnVideoEnded -= Handler;

        Debug.Log("[EndMovie] 結束播放流程");
        await UniTask.CompletedTask;
    }
}
