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
        // var uiManager = Engine.GetService<IUIManager>();
        // var naniStreamingVideo = uiManager.GetUI(streamingVideoName);
        // naniStreamingVideo.Show();

        var streamingVideo = GameObject.Find(streamingVideoName);
        var webGLStreamController = streamingVideo.GetComponentInChildren<WebGLStreamController>();

        try
        {
            await webGLStreamController.Play(Url);
        }
        catch (Exception e)
        {
            Debug.Log($"alex_StreamingVideo: {e.Message}");
        }
        await UniTask.CompletedTask;
    }
}
