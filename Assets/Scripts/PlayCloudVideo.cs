using UnityEngine;
using UnityEngine.Video;

public class PlayCloudVideo : MonoBehaviour
{
    [SerializeField] RenderTexture targetTexture;
    public string videoURL = "https://drive.google.com/uc?export=download&id=1BnKjIp2Ypu6c4fMadN_voI5mZQhfmqNx";
    private VideoPlayer videoPlayer;
    private void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.targetTexture = targetTexture;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoURL;
        videoPlayer.Play();
    }
}
