using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadFile : MonoBehaviour
{
    string downloadUrl = "https://drive.google.com/uc?export=download&id=1ZnhZ4-b73PKoaAh6hj1G6DzlZ55aYkik";

    string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "DownloadedVideo.webm");

        StartCoroutine(DownloadFileCoroutine(downloadUrl, filePath));
    }

    IEnumerator DownloadFileCoroutine(string url, string filePath)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                byte[] fileData = request.downloadHandler.data;

                File.WriteAllBytes(filePath, fileData);
                Debug.Log("File downloaded and saved to " + filePath);
            }
        }
    }
}