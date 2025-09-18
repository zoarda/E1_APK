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
            bool isReady = false;
            void OnReady() { isReady = true; Debug.Log("[PlayStreamingVideo] 影片準備好"); }
            webGLStreamController.OnPlaybackReadyEvent += OnReady;

            await webGLStreamController.Play(Url);

            float timeout = 2f;
            float timer = 0f;
            while (!isReady && timer < timeout)
            {
                await UniTask.Yield();
                timer += Time.deltaTime;
            }

            webGLStreamController.OnPlaybackReadyEvent -= OnReady;
        }

        await UniTask.CompletedTask;
    }
}
