using UnityEngine;
using UnityEngine.Video;


public class OnlineVideoLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoURL = "https://drive.google.com/uc?export=download&id=14sE9ybXGnnrcViHMejyEvQl3CVZyrGIJ";
    void Start()
    {
        videoPlayer.url = videoURL;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
} 
