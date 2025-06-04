using System.Collections;
using Naninovel;
using UnityEngine;
using UnityEngine.Video;

[CommandAlias("setVideoPlayer")]
public class SetVideoPlayer : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await setVideoPlayer(asyncToken);
    }

    private static async UniTask setVideoPlayer(AsyncToken asyncToken)
    {
        
        // SubtitlesManager subtitlesManager = GameObject.Find("SubtitlesManager")?.GetComponent<SubtitlesManager>();
        SubtitlesManager subtitlesManager = SubtitlesManager.Instance;

        if (subtitlesManager == null)
        {
            // Debug.LogError("SubtitlesManager not found!");
            return;
        }

        VideoPlayer video = GameObject.Find("VideoBackground")?.GetComponentInChildren<VideoPlayer>();
        if (video == null)
        {
            // Debug.LogError("VideoPlayer not found!");
            return;
        }
        // subtitlesManager.videoPlayer = video;
        await subtitlesManager.LoadSubtitles();
        //
        VideoManager videoManager = VideoManager.Instance;
        videoManager.SetVideoPlayer(video);
        // AudioManager audioManager = AudioManager.Instance;
        // StartCoroutine(audioManager.SetvideoAudioAsync(video.clip.name));
        //
        // AudioManager.Instance.SyncAudioWithVideo(VideoManager.Instance.GetVideoPlayer());
        // Debug.Log(video.clip.name + " loaded subtitles");
        await UniTask.CompletedTask;
    }
}
