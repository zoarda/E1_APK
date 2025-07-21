using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using System;
using Cysharp.Threading.Tasks;


public class SubtitlesManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Text subtitlesText;
    private List<(double start, double end, string text)> subtitles = new();
    private int currentSubtitleIndex = -1;
    private string videoName;
    public string lastChoiceVideoName;
    [SerializeField] private StartNani startNani;
    public Language LanguageCase = Language.中文;

    static SubtitlesManager instance;
    WebGLStreamController webGLStreamController;

    UrlToScence urlToScence;

    void Start()
    {
        subtitlesText = GetComponent<Text>();
    }

    void Update()
    {
        if (webGLStreamController == null)
        {
            webGLStreamController = WebGLStreamController.Instance;
            return;
        }
        if (StartNani.Instance.VideoImage.GetComponent<CanvasGroup>().alpha == 0)
        {
            subtitlesText.text = "";
            return;
        }
        UpdateSubtitle(webGLStreamController.GetVideotime());
        // Debug.Log($"{webGLStreamController.GetVideotime()}" + "/" + $"{webGLStreamController.GetVideoLenght()}" );
    }
    public static SubtitlesManager Instance
    {

        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SubtitlesManager>();
            }
            return instance;

        }
    }
    public async UniTask Init()
    {
        try
        {
            urlToScence = await YamlLoader.LoadStreamingAssetsYaml<UrlToScence>(Application.streamingAssetsPath + "/Yaml/URLToScence.yaml");
            Debug.Log("URLToScence.yaml 加載成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"載入 URLToScence.yaml 發生錯誤: {e}");
        }
    }
    public async UniTask LoadSubtitles()
    {
        webGLStreamController = WebGLStreamController.Instance;

        string url = webGLStreamController.multiStreamProperties[0].url[0];
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("無法取得影片 URL！");
            return;
        }

        if (!urlToScence.videoDictionary.TryGetValue(url, out string scriptBaseName))
        {
            Debug.LogError($"找不到對應的字幕名稱：URL = {url}");
            subtitles.Clear(); // ✅
            return;
        }

        switch (LanguageCase)
        {
            case Language.中文:
                videoName = scriptBaseName;
                break;
            case Language.日文:
                videoName = scriptBaseName + "_JP";
                break;
            case Language.英文:
                videoName = scriptBaseName + "_EN";
                break;
            default:
                Debug.LogError($"未匹配的 LanguageCase 值: {LanguageCase}");
                return;
        }

        Debug.Log($"選擇字幕語言：{LanguageCase}, 對應檔名：{videoName}");
        string subtitlePath = Application.streamingAssetsPath + "/Subtitles/" + videoName + ".srt";

        try
        {
            string content = null;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                WWW reader = new WWW(subtitlePath);
                await reader.ToUniTask();
                content = reader.text;
            }
            else
            {
                content = File.ReadAllText(subtitlePath);
            }

            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning($"字幕檔為空：{subtitlePath}");
                subtitles.Clear(); // ✅ 加這行：字幕檔為空 → 清空字幕
            }
            else
            {
                Debug.Log($"字幕檔加載成功：{subtitlePath}");
                ParseSubtitles(content);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"讀取字幕檔時發生錯誤：{subtitlePath}\n{e}");
            subtitles.Clear(); // ✅ 加這行：讀取失敗 → 清空字幕
        }
    }
    void ParseSubtitles(string subtitlesContent)
    {
        subtitles.Clear();
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (StringReader reader = new StringReader(subtitlesContent))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string timeLine = reader.ReadLine();
                        string[] times = timeLine.Split(new string[] { " --> " }, StringSplitOptions.None);
                        double startTime = ParseTime(times[0].Trim());
                        double endTime = ParseTime(times[1].Trim());
                        string text = reader.ReadLine();
                        subtitles.Add((startTime, endTime, text));
                        reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("加载字幕文件出错：" + ex.Message);
            }
        }
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            try
            {
                using (StringReader reader = new StringReader(subtitlesContent))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string timeLine = reader.ReadLine();
                        string[] times = timeLine.Split(new string[] { " --> " }, StringSplitOptions.None);
                        double startTime = ParseTime(times[0].Trim());
                        double endTime = ParseTime(times[1].Trim());
                        string text = reader.ReadLine();
                        subtitles.Add((startTime, endTime, text));
                        reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("加载字幕文件出错：" + ex.Message);
            }
        }
        else
        {
            try
            {
                using (StreamReader reader = new StreamReader(subtitlesContent))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string timeLine = reader.ReadLine();
                        string[] times = timeLine.Split(new string[] { " --> " }, StringSplitOptions.None);
                        double startTime = ParseTime(times[0].Trim());
                        double endTime = ParseTime(times[1].Trim());
                        string text = reader.ReadLine();
                        subtitles.Add((startTime, endTime, text));
                        reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("加载字幕文件出错：" + ex.Message);
            }

        }
    }
    double ParseTime(string time)
    {
        string[] parts = time.Split(':');
        double hours = double.Parse(parts[0]);
        double minutes = double.Parse(parts[1]);
        double seconds = double.Parse(parts[2].Replace(',', '.'));
        return hours * 3600 + minutes * 60 + seconds;
    }

    void UpdateSubtitle(long HISplayertime)
    {
        float currentTime = HISplayertime / 1000f;
        if (currentSubtitleIndex >= 0 && currentSubtitleIndex < subtitles.Count)
        {
            var subtitle = subtitles[currentSubtitleIndex];
            if (currentTime >= subtitle.start && currentTime <= subtitle.end)
            {
                return;
            }
        }

        int index = subtitles.FindIndex(s => currentTime >= s.start && currentTime <= s.end);
        if (index != -1)
        {
            subtitlesText.text = subtitles[index].text;
            currentSubtitleIndex = index;
        }
        else
        {
            subtitlesText.text = "";
        }
    }
    public enum Language
    {
        中文,
        日文,
        英文,
    }
    public class UrlToScence
    {
        public Dictionary<string, string> videoDictionary;
    }
}
