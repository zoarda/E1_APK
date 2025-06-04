using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource BGMSource;
    public AudioSource VideoSFXSource;

    public AudioSource SFXSource;

    public AudioSource VideoSource;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }
            return instance;
        }
    }
    public void Awake()
    {
        BGMSource = GetComponent<AudioSource>();
    }
    // public void SetvideoAudio(string videoClipName)
    // {
    //     AudioClip clip = Resources.Load<AudioClip>("Audio/" + videoClipName);
    //     AudioClip sfxClip = Resources.Load<AudioClip>("Audio/" + videoClipName + "_sfx");
    //     AudioClip videoClip = Resources.Load<AudioClip>("Audio/" + videoClipName + "_VO");
    //     if (clip == null && sfxClip == null && videoClip == null)
    //     {
    //         Debug.LogError("Audio clip not found: " + videoClipName);
    //         return;
    //     }
    //     if (clip == null)
    //     {
    //         Debug.LogError("Audio clip not found: " + videoClipName);
    //         BGMSource.Pause();
    //     }
    //     else
    //     {
    //         BGMSource.clip = clip;
    //         BGMSource.Play();
    //     }
    //     if (videoClip == null)
    //     {
    //         Debug.LogError("Audio VideoSource not found: " + videoClipName);
    //         VideoSource.Pause();
    //     }
    //     {
    //         VideoSource.clip = videoClip;
    //         VideoSource.Play();
    //     }
    //     if (sfxClip == null)
    //     {
    //         Debug.LogError("Audio videoSFXClip not found: " + videoClipName);
    //         VideoSFXSource.Pause();
    //     }
    //     else
    //     {
    //         VideoSFXSource.clip = sfxClip;
    //         VideoSFXSource.Play();
    //     }
    //     // SyncAudioWithVideo(VideoManager.Instance.GetVideoPlayer());
    // }
    public IEnumerator SetvideoAudioAsync(string videoClipName)
    {
        // Load audio clips asynchronously
        ResourceRequest bgmRequest = Resources.LoadAsync<AudioClip>("Audio/" + videoClipName);
        ResourceRequest sfxRequest = Resources.LoadAsync<AudioClip>("Audio/" + videoClipName + "_sfx");
        ResourceRequest videoRequest = Resources.LoadAsync<AudioClip>($"{VideoManager.Instance.GetVideoPlayer()}");
        // Wait for all audio clips to load
        yield return bgmRequest;
        yield return sfxRequest;
        yield return videoRequest;

        AudioClip clip = bgmRequest.asset as AudioClip;
        AudioClip sfxClip = sfxRequest.asset as AudioClip;
        AudioClip videoClip = videoRequest.asset as AudioClip;

        if (clip == null && sfxClip == null && videoClip == null)
        {
            Debug.LogWarning("Audio clips not found: " + videoClipName);
            BGMSource.clip = null;
            VideoSFXSource.clip = null;
            VideoSource.clip = null;
            yield break;
        }

        // // Sync audio clips and then start playing
        if (clip != null)
        {
            BGMSource.clip = clip;
            // BGMSource.Play();
        }
        else
        {
            BGMSource.clip = null;
        }

        if (sfxClip != null)
        {
            VideoSFXSource.clip = sfxClip;
            // VideoSFXSource.Play();
        }
        else
        {
            VideoSFXSource.clip = null;
        }
        if (videoClip != null)
        {
            VideoSource.clip = videoClip;
            // yield return null;
            VideoPlayer videoPlayer = VideoManager.Instance.GetVideoPlayer();
            videoPlayer.time = 0;
            videoPlayer.seekCompleted += Play;
            // VideoSource.Play();
            // videoPlayer.Prepare();
            // while (!videoPlayer.isPrepared)
            // {
            //     yield return null;
            // }
        }
        else
        {
            VideoSource.clip = null;
        }
        // After all clips are loaded, start playing
        // BGMSource.Play();
        // VideoSource.Play();
        // VideoSFXSource.Play();
        void Play(VideoPlayer vp)
        {
            // videoPlayer.Play();
            PlayAllAudio();
        }
    }
    public void PlayAllAudio()
    {
        BGMSource.Play();
        SFXSource.Play();
        VideoSFXSource.Play();
        VideoSource.Play();
    }
    public void ClearClip()
    {
        BGMSource.clip = null;
        SFXSource.clip = null;
        VideoSFXSource.clip = null;
        VideoSource.clip = null;
    }
    public void SyncAudioWithVideo(VideoPlayer videoPlayer)
{
    try
    {
        float videoTime = (float)videoPlayer.time;

        // 同步每个音频源
        SyncAudioSourceWithVideo(BGMSource, videoTime, "BGMSource");
        SyncAudioSourceWithVideo(VideoSFXSource, videoTime, "VideoSFXSource");
        SyncAudioSourceWithVideo(VideoSource, videoTime, "VideoSource");

        // 根据视频播放状态控制音频播放和暂停
        if (videoPlayer.isPlaying)
        {
            if (!BGMSource.isPlaying) BGMSource.Play();
            if (!VideoSFXSource.isPlaying) VideoSFXSource.Play();
            if (!VideoSource.isPlaying) VideoSource.Play();
        }
        else
        {
            if (BGMSource.isPlaying) BGMSource.Pause();
            if (VideoSFXSource.isPlaying) VideoSFXSource.Pause();
            if (VideoSource.isPlaying) VideoSource.Pause();
        }
    }
    catch
    {
        Debug.Log("videoplayererror");
    }
}

private void SyncAudioSourceWithVideo(AudioSource audioSource, float videoTime, string sourceName)
{
    if (audioSource.clip != null)
    {
        float audioTime = audioSource.time;
        float diff = Mathf.Abs(audioTime - videoTime);

        if (diff > 0.1f)
        {
            audioSource.time = videoTime;
            Debug.Log($"{sourceName} synced. Audio time: {audioSource.time}, VideoPlayer time: {videoTime}");
        }
    }
}
    public void PlaySFX(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip == null)
        {
            Debug.LogError("Audio clip not found: " + clipName);
            return;
        }
        SFXSource.PlayOneShot(clip);
    }
    public void PauseBGM()
    {
        BGMSource.Pause();
        SFXSource.Pause();
        VideoSFXSource.Pause();
        VideoSource.Pause();
    }
    public void PlayBGM()
    {
        BGMSource.UnPause();
        SFXSource.UnPause();
        VideoSFXSource.UnPause();
        VideoSource.UnPause();
    }

}
