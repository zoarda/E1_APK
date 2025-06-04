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

        // string rawText = "";
        // try
        // {
        //     string finalPath = yamlFilePath;

        //     // ✅ 加入平台與路徑 Debug 紀錄
        //     Debug.Log($"[YAML載入] 平台: {Application.platform}, 原始路徑: {yamlFilePath}");

        //     if (Application.platform == RuntimePlatform.Android && !Application.isEditor)
        //     {
        //         finalPath = $"jar:file://{yamlFilePath}";
        //     }

        //     Debug.Log($"[YAML載入] 最終使用路徑: {finalPath}");

        //     if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
        //     {
        //         UnityWebRequest request = UnityWebRequest.Get(finalPath);
        //         await request.SendWebRequest();

        //         if (request.result == UnityWebRequest.Result.Success)
        //         {
        //             rawText = request.downloadHandler.text;

        //             // ✅ 確認是否真的有內容
        //             Debug.Log($"[YAML載入] 成功讀取內容，長度: {rawText.Length}");
        //             Debug.Log($"[YAML內容預覽] 前100字: {rawText.Substring(0, Math.Min(100, rawText.Length))}");
        //         }
        //         else
        //         {
        //             Debug.LogError($"[YAML載入] 請求錯誤: {request.error}");
        //         }
        //     }
        //     else
        //     {
        //         if (!File.Exists(finalPath))
        //         {
        //             Debug.LogError($"[YAML載入] 檔案不存在: {finalPath}");
        //             return default;
        //         }

        //         rawText = File.ReadAllText(finalPath, Encoding.UTF8);
        //         Debug.Log($"[YAML載入] 從本機讀取成功，長度: {rawText.Length}");
        //     }

        //     var deserializer = new DeserializerBuilder().Build();
        //     return deserializer.Deserialize<T>(rawText);
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError($"[YAML載入] 發生例外錯誤: {yamlFilePath}\n{e}");
        //     return default;
        // }
    }
    // public static async UniTask<T> LoadYaml<T>(string yamlFilePath)
    // {
    //     try
    //     {

    //         string a = "";

    //         if (!File.Exists(yamlFilePath))
    //         {
    //             StartNani.SaveData saveData = new()
    //             {
    //                 friendship = 0,
    //                 scriptName = new List<string>()
    //             };
    //             saveData.scriptName.Add("C1_VB");
    //             SaveYaml(saveData);
    //         }
    //         string yamlContent = File.ReadAllText(yamlFilePath);
    //         a = yamlContent;
    //         var deserializer = new DeserializerBuilder() { }.Build();
    //         var items = deserializer.Deserialize<T>(a);
    //         return items;
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"Error loading YAML file: {yamlFilePath}\n{e.StackTrace}");
    //         return default;
    //     }
    // }
    // public static void SaveYaml(StartNani.SaveData saveData)
    // {
    //     var serialization = new SerializerBuilder() { }.Build();
    //     string data = serialization.Serialize(saveData);
    //     File.WriteAllText(Application.persistentDataPath + "/SaveData.yaml", data);
    // }
}
