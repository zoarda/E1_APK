using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LanguageManager : MonoBehaviour
{
    [SerializeField] private SubtitlesManager subtitlesManager;
    public List<Text> languageTexts;
    static LanguageManager instance;
    LanguageBundle languageBundle;
    
    public static LanguageManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LanguageManager>();
            }
            return instance;

        }
    }
    public void SetLanguage()
    {
        try
        {
        foreach (var textitem in languageTexts)
        {
            LangugeText langugeText = textitem.GetComponent<LangugeText>();
            if (langugeText != null)
            {
                // Debug.Log("langugeText.Id: " + langugeText.Id);
                textitem.text = GetLanguageValue(langugeText.Id);
            }
        }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async UniTask Init()
    {
        try
        {
            languageBundle = await YamlLoader.LoadStreamingAssetsYaml<LanguageBundle>(Application.streamingAssetsPath + "/Yaml/OptionLanguage.yaml");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
    public string SetLanguageValue(string id)
    {
        return GetLanguageValue(id);
    }
    public string GetLanguageValue(string id)
    {
        // var bundle = LoadYaml(Application.streamingAssetsPath + "/Yaml/OptionLanguage.yaml");
        // var bundle = YamlLoader.LoadYaml<LanguageBundle>(Application.streamingAssetsPath + "/Yaml/OptionLanguage.yaml");
        
        var bundle = languageBundle;
        if (bundle == null)
        {
            Debug.Log($"LanguageBundle not found.");
            return null;
        }

        var pack = bundle.Packs.FirstOrDefault(i => i.Id == id);
        if (pack == null)
        {
            Debug.Log($"pack not found.");
            return null;
        }

        switch (subtitlesManager.LanguageCase)
        {
            case SubtitlesManager.Language.中文:
                return pack.Label.CH;
            case SubtitlesManager.Language.英文:
                return pack.Label.EN;
            case SubtitlesManager.Language.日文:
                return pack.Label.JP;
        }
        return null;
    }
    public class LanguageBundle
    {
        public Pack[] Packs;
        // public Dictionary<string, Label> Packs;
        // public Dictionary<string, Dictionary<string, string>> Packs;
    }
    public class Pack
    {
        public string Id;
        public Label Label;
    }
    public class Label
    {
        public string CH;
        public string EN;
        public string JP;
    }
    // public enum LanguageTypeEnum{
    //     CH,
    //     EN,
    //     JP,
    //     KR,
    // }
}
