using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Games.Coresdk.Unity
{
    public class ConfigLoader
    {
        const string DEFAULT_FILENAME = "coresdk_config";

        public static string PREF_KEY_DOMAIN
        {
            get
            {
                return "Games.Coresdk.Unity." + JSON_FILENAME;
            }
        }
        public static string JSON_FILENAME = DEFAULT_FILENAME;


        public Config config { get; private set; }

        EnptyedJsonFile jsonFile;
        JSONNode domainConfig;

        public ConfigLoader()
        {
            jsonFile = new EnptyedJsonFile(Application.persistentDataPath + "/" + JSON_FILENAME);
        }

        public IEnumerator CoInit()
        {
            yield return CoUpdateConfig();
            yield return CoFindLoginDomain();
        }

        IEnumerator CoUpdateConfig()
        {
            var json = ReadConfig();
            if (json == null)
                yield break;

            foreach (var e in json["config"].AsArray)
            {
                yield return CoDownloadConfig(e.Value, jsonText => {
                    if (string.IsNullOrEmpty(jsonText))
                        return;

                    domainConfig = JSON.Parse(jsonText);
                    SaveConfig(jsonText);
                });

                if (domainConfig != null)
                    yield break;
            }
        }

        IEnumerator CoFindLoginDomain()
        {
            if (domainConfig == null)
                yield break;

            var loginDomain = PlayerPrefs.GetString(PREF_KEY_DOMAIN, "");
            if (string.IsNullOrEmpty(loginDomain) == false)
            {
                yield return CoCheckLoginDomain(loginDomain);
                if (config != null)
                    yield break;
            }

            foreach (var e in domainConfig["login"].AsArray)
            {
                yield return CoCheckLoginDomain(e.Value);
                if (config != null)
                    yield break;
            }
        }

        IEnumerator CoCheckLoginDomain(string domain)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(domain + "/api/checkLoginUrl"))
            {
#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isNetworkError == false && www.isHttpError == false)
#else
                if (www.isError == false)
#endif
                {
                    this.config = new Config(domain, domainConfig["payment"].Value);
                    PlayerPrefs.SetString(PREF_KEY_DOMAIN, domain);
                    yield break;
                }
            }
        }

        IEnumerator CoDownloadConfig(string url, Action<string> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isNetworkError == false && www.isHttpError == false)
#else
                if (www.isError == false)
#endif
                {
                    callback(www.downloadHandler.text);
                    yield break;
                }

                callback(string.Empty);
            }
        }

        JSONNode ReadConfig()
        {
            var jsonText = string.Empty;
            if (jsonFile.IsExists)
                jsonText = jsonFile.Read();

            if (string.IsNullOrEmpty(jsonText))
            {
                jsonText = Resources.Load<TextAsset>(JSON_FILENAME).text;
            }

            return JSON.Parse(jsonText);
        }

        void SaveConfig(string jsonText)
        {
            jsonFile.Write(jsonText);
        }

        class EnptyedJsonFile
        {
            public bool IsExists
            {
                get
                {
                    return File.Exists(filepath);
                }
            }

            string filepath;
            DESCryptoServiceProvider des; 

            public EnptyedJsonFile(string filepath)
            {
                this.filepath = filepath;

                this.des = new DESCryptoServiceProvider();
                des.Key = Encoding.ASCII.GetBytes("TM^jRt@0");
                des.IV = Encoding.ASCII.GetBytes("q6prn^Jb");
            }

            public string Read()
            {
                var dataByteArray = File.ReadAllBytes(filepath);
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                    catch (Exception)
                    {
                        File.Delete(filepath);
                        return string.Empty;
                    }
                }
            }

            public void Write(string contents)
            {
                var dataByteArray = Encoding.UTF8.GetBytes(contents);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();

                        File.WriteAllBytes(filepath, ms.ToArray());
                    }
                }
            }
        }
    }
}
