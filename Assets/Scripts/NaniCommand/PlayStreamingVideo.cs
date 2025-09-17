using UnityEngine;
using Naninovel;
using System;
using HISPlayer;

[CommandAlias("streamingPlay")]
public class PlayStreamingVideo : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Url;

    static readonly string streamingVideoName = "StreamingVideo";

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        var streamingVideo = GameObject.Find(streamingVideoName);
        var webGLStreamController = streamingVideo?.GetComponentInChildren<WebGLStreamController>();

        if (webGLStreamController != null)
        {
            // 先等影片準備好
            await webGLStreamController.Play(Url);

            // 確保影片 ready
            float timeout = 10f;
            float timer = 0f;
            while (webGLStreamController.GetVideoLenght() <= 0 && !webGLStreamController.EndPlay && timer < timeout)
            {
                await UniTask.Yield();
                timer += Time.deltaTime;
            }

            if (webGLStreamController.GetVideoLenght() > 0)
                Debug.Log("[PlayStreamingVideo] 影片已準備好，可以進入 WaitMovie");
            else
                Debug.LogWarning("[PlayStreamingVideo] 影片準備超時");
        }

        await UniTask.CompletedTask; // 指令立即完成
    }
}
