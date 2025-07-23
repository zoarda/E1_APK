using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
public class DiscordLogger : MonoBehaviour
{
    private const string webhookUrl = "https://discord.com/api/webhooks/1397109746049351751/qJwvnBl-Lk18U5toeuqZqU_0ehARrR7gGzpQbbcs_wDyA-HgEy9opkcAPGgSdgfVOWRF";

    private static List<string> logBuffer = new List<string>();
    private static bool isSending = false;
    private static DiscordLogger instance;

    private void Awake()
    {
        // 確保單例
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(SendBufferCoroutine());
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            Application.logMessageReceived += HandleUnityLog;
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Application.logMessageReceived -= HandleUnityLog;
#endif
    }

    /// <summary>
    /// 主動呼叫：註冊自訂訊息
    /// </summary>
    public static void Log(string message)
    {
        if (instance == null)
        {
            Debug.LogWarning("DiscordLogger is not initialized in scene!");
            return;
        }

        string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string device = SystemInfo.deviceModel;
        string version = Application.version;

        string formatted = $"[{time}] 📝 **自訂訊息**\nAppVersion: `{version}`\nDevice: `{device}`\n```\n{message}\n```";
        logBuffer.Add(formatted);
    }

    /// <summary>
    /// 攔截 Unity Error / Exception
    /// </summary>
    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string device = SystemInfo.deviceModel;
            string version = Application.version;

            string formatted = $"[{time}] ❗ **Unity {type}**\nAppVersion: `{version}`\nDevice: `{device}`\n```\n{logString}\n{stackTrace}\n```";
            logBuffer.Add(formatted);
        }
    }

    private IEnumerator SendBufferCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (!isSending && logBuffer.Count > 0)
            {
                isSending = true;

                StringBuilder sb = new StringBuilder();
                foreach (var log in logBuffer)
                {
                    sb.AppendLine(log);
                    sb.AppendLine("---");
                }
                logBuffer.Clear();

                yield return SendToDiscord(sb.ToString());
                isSending = false;
            }
        }
    }

    private IEnumerator SendToDiscord(string message)
    {
        string jsonPayload = JsonUtility.ToJson(new DiscordWebhookPayload { content = message });

        using (UnityWebRequest request = new UnityWebRequest(webhookUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to send log to Discord: " + request.error);
            }
        }
    }

    [System.Serializable]
    private class DiscordWebhookPayload
    {
        public string content;
    }
}
