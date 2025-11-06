using System;
using System.IO;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;
using Naninovel;
using UnityEngine.Networking;

public static class YamlLoader
{
    public static async UniTask<T> LoadStreamingAssetsYaml<T>(string yamlFilePath)
    {
        string content = "";

        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // 使用 WWW 來處理 Android 的 StreamingAssets 資料夾
                using (var reader = new WWW(yamlFilePath))
                {
                    await reader.ToUniTask();
                    content = reader.text;
                }
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(yamlFilePath))
                {
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        content = request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError($"[YAML] WebGL 載入失敗: {request.error}");
                        return default;
                    }
                }
            }
            else
            {
                // 桌面平台 (Editor, Windows, macOS 等)
                content = File.ReadAllText(yamlFilePath);
            }

            Debug.Log($"[YAML] Loaded content:\n{content}");

            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<T>(content);
        }
        catch (Exception e)
        {
            Debug.LogError($"[YAML] 讀取失敗: {yamlFilePath}\n{e}");
            return default;
        }
    }
    public static void SaveToYaml<T>(T obj, string path)
    {
        var serializer = new Serializer();
        var yaml = serializer.Serialize(obj);
        File.WriteAllText(path, yaml);
    }
}
