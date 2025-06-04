using UnityEngine;
using UnityEngine.UI;
using Naninovel;

public class GameSettingPage : MonoBehaviour
{
    public Button Btn_Chinese, Btn_English, Btn_Japanese, Btn_Save, Btn_Cancel, selectedButton, Btn_Close;
    public Image Title, Volum, Language, Ch, En, Jp, Save, Cancel;
    public Text TxtVoice;
    public Toggle Tog_Language, Tog_Audio;
    public Slider Slider_StartGame;
    // Slider_BGM, Slider_SFX, Slider_Voice, 

    public Sprite SecltedSprite, UnSelectedSprite;
    public GameObject LanguagePage, VoicePage;
    private float originalBGMVolume, originalSFXVolume, originalVoiceVolume, originalStartGameVolume;
    private float tempBGMVolume, tempSFXVolume, tempVoiceVolume, tempStartGameVolume;
    private float BGMlv;
    public float VideoVolum = 1;
    private static GameSettingPage instance;
    private static readonly object lockObj = new object();
    public static GameSettingPage Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<GameSettingPage>();

                        // 確保場景中只有一個實例
                        if (FindObjectsOfType<GameSettingPage>().Length > 1)
                        {
                            Debug.LogError("多個 GameSettingPage 實例存在於場景中，這可能會導致問題。");
                        }
                    }
                }
            }
            return instance;
        }
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("GameSettingPage 已經存在，銷毀多餘的實例。");
            Destroy(gameObject);
        }
        var Bgm = Engine.GetService<IAudioManager>();
        LanguageManager.Instance.SetLanguage();
        AudioManager audioManager = AudioManager.Instance;
        TxtVoice.text = (Bgm.BgmVolume * 100).ToString("0");
        // Bgm.AudioMixer.SetFloat("BGM", 100);

        // 初始化音量初始值
        // originalBGMVolume = audioManager.BGMSource.volume;
        // originalSFXVolume = audioManager.SFXSource.volume;
        // originalVoiceVolume = audioManager.VideoSFXSource.volume;
        originalStartGameVolume = Bgm.BgmVolume;


        // 初始化臨時音量值
        // tempBGMVolume = originalBGMVolume;
        // tempSFXVolume = originalSFXVolume;
        // tempVoiceVolume = originalVoiceVolume;
        tempStartGameVolume = originalStartGameVolume;

        // 初始化 Slider 的值
        // Slider_BGM.value = originalBGMVolume;
        // Slider_SFX.value = originalSFXVolume;
        // Slider_Voice.value = originalVoiceVolume;
        Slider_StartGame.value = originalStartGameVolume;

        // // 初始化 Slider 的事件監聽
        // Slider_BGM.onValueChanged.AddListener((value) =>
        // {
        //     tempBGMVolume = value; // 更新臨時值
        //     AdjustVolume(audioManager.BGMSource, value);
        // });
        // Slider_SFX.onValueChanged.AddListener((value) =>
        // {
        //     tempSFXVolume = value; // 更新臨時值
        //     AdjustVolume(audioManager.SFXSource, value);
        // });
        // Slider_Voice.onValueChanged.AddListener((value) =>
        // {
        //     tempVoiceVolume = value; // 更新臨時值
        //     AdjustVolume(audioManager.VideoSFXSource, value);
        // });
        Slider_StartGame.onValueChanged.AddListener((value) =>
        {
            Bgm.BgmVolume = value;
            tempStartGameVolume = value;
            VideoVolum = value;
            AudioManager.Instance.SFXSource.volume = value;
            TxtVoice.text = (value * 100).ToString("0");
        });

        // 語言按鈕邏輯
        Btn_Chinese.onClick.AddListener(() =>
        {
            SubtitlesManager.Instance.LanguageCase = SubtitlesManager.Language.中文;
            // LanguageManager.Instance.SetLanguage();
            OnButtonSetLanguage("PackId123", Title);
            OnButtonSetLanguage("PackId124", Language);
            OnButtonSetLanguage("PackId125", Volum);
            OnButtonSetLanguage("PackId126", Cancel);
            OnButtonSetLanguage("PackId128", Save);
            OnButtonSetLanguage("PackId129", Ch);
            OnButtonSetLanguage("PackId130", En);
            OnButtonSetLanguage("PackId131", Jp);
            OnbuttonClick(Btn_Chinese);
        });
        Btn_English.onClick.AddListener(() =>
        {
            SubtitlesManager.Instance.LanguageCase = SubtitlesManager.Language.英文;
            LanguageManager.Instance.SetLanguage();
            OnButtonSetLanguage("PackId123", Title);
            OnButtonSetLanguage("PackId124", Language);
            OnButtonSetLanguage("PackId125", Volum);
            OnButtonSetLanguage("PackId126", Cancel);
            OnButtonSetLanguage("PackId128", Save);
            OnButtonSetLanguage("PackId129", Ch);
            OnButtonSetLanguage("PackId130", En);
            OnButtonSetLanguage("PackId131", Jp);
            OnbuttonClick(Btn_English);
        });
        Btn_Japanese.onClick.AddListener(() =>
        {
            SubtitlesManager.Instance.LanguageCase = SubtitlesManager.Language.日文;
            // LanguageManager.Instance.SetLanguage();
            OnButtonSetLanguage("PackId123", Title);
            OnButtonSetLanguage("PackId124", Language);
            OnButtonSetLanguage("PackId125", Volum);
            OnButtonSetLanguage("PackId126", Cancel);
            OnButtonSetLanguage("PackId128", Save);
            OnButtonSetLanguage("PackId129", Ch);
            OnButtonSetLanguage("PackId130", En);
            OnButtonSetLanguage("PackId131", Jp);
            OnbuttonClick(Btn_Japanese);
        });

        // 保存按鈕邏輯
        Btn_Save.onClick.AddListener(async () =>
        {
            await SubtitlesManager.Instance.LoadSubtitles();
            LanguageManager.Instance.SetLanguage();
            // originalBGMVolume = tempBGMVolume;
            // originalSFXVolume = tempSFXVolume;
            // originalVoiceVolume = tempVoiceVolume;
            originalStartGameVolume = tempStartGameVolume;

            // audioManager.BGMSource.volume = originalBGMVolume;
            // audioManager.SFXSource.volume = originalSFXVolume;
            // audioManager.VideoSFXSource.volume = originalVoiceVolume;
            Bgm.BgmVolume = originalStartGameVolume;

            StartNani.Instance.GameSettingPage.SetActive(false);
        });

        // 取消按鈕邏輯
        Btn_Cancel.onClick.AddListener(() =>
        {
            SubtitlesManager.Instance.LanguageCase = SubtitlesManager.Language.中文;
            LanguageManager.Instance.SetLanguage();
            OnButtonSetLanguage("PackId123", Title);
            OnButtonSetLanguage("PackId124", Language);
            OnButtonSetLanguage("PackId125", Volum);
            OnButtonSetLanguage("PackId126", Cancel);
            OnButtonSetLanguage("PackId128", Save);
            OnButtonSetLanguage("PackId129", Ch);
            OnButtonSetLanguage("PackId130", En);
            OnButtonSetLanguage("PackId131", Jp);

            // tempBGMVolume = originalBGMVolume;
            // tempSFXVolume = originalSFXVolume;
            // tempVoiceVolume = originalVoiceVolume;
            tempStartGameVolume = originalStartGameVolume;

            // Slider_BGM.value = originalBGMVolume;
            // Slider_SFX.value = originalSFXVolume;
            // Slider_Voice.value = originalVoiceVolume;
            Slider_StartGame.value = originalStartGameVolume;

            // audioManager.BGMSource.volume = originalBGMVolume;
            // audioManager.SFXSource.volume = originalSFXVolume;
            // audioManager.VideoSFXSource.volume = originalVoiceVolume;
            OnbuttonClick(Btn_Chinese);

            StartNani.Instance.GameSettingPage.SetActive(false);
        });
        Btn_Close.onClick.AddListener(() =>
      {
          SubtitlesManager.Instance.LanguageCase = SubtitlesManager.Language.中文;
          LanguageManager.Instance.SetLanguage();
          OnButtonSetLanguage("PackId123", Title);
          OnButtonSetLanguage("PackId124", Language);
          OnButtonSetLanguage("PackId125", Volum);
          OnButtonSetLanguage("PackId126", Cancel);
          OnButtonSetLanguage("PackId128", Save);
          OnButtonSetLanguage("PackId129", Ch);
          OnButtonSetLanguage("PackId130", En);
          OnButtonSetLanguage("PackId131", Jp);
          // tempBGMVolume = originalBGMVolume;
          // tempSFXVolume = originalSFXVolume;
          // tempVoiceVolume = originalVoiceVolume;
          tempStartGameVolume = originalStartGameVolume;

          // Slider_BGM.value = originalBGMVolume;
          // Slider_SFX.value = originalSFXVolume;
          // Slider_Voice.value = originalVoiceVolume;
          Slider_StartGame.value = originalStartGameVolume;

          // audioManager.BGMSource.volume = originalBGMVolume;
          // audioManager.SFXSource.volume = originalSFXVolume;
          // audioManager.VideoSFXSource.volume = originalVoiceVolume;
          OnbuttonClick(Btn_Chinese);

          StartNani.Instance.GameSettingPage.SetActive(false);
      });
        // // Toggle 邏輯
        // Tog_Language.onValueChanged.AddListener((isOn) => OnToggleClick(Tog_Language, LanguagePage, isOn));
        // Tog_Audio.onValueChanged.AddListener((isOn) => OnToggleClick(Tog_Audio, VoicePage, isOn));
    }
    void OnButtonSetLanguage(string packid, Image image)
    {
        string ImaResourceBtn = LanguageManager.Instance.GetLanguageValue(packid);
        Texture2D imaResource = Resources.Load<Texture2D>("GameSettingPage/" + ImaResourceBtn);
        image.sprite = Sprite.Create(imaResource, new Rect(0, 0, imaResource.width, imaResource.height), new Vector2(0.5f, 0.5f));
        // image.SetNativeSize();
    }
    void OnbuttonClick(Button button)
    {
        if (selectedButton != null)
        {
            selectedButton.image.sprite = UnSelectedSprite;
        }
        selectedButton = button;
        selectedButton.image.sprite = SecltedSprite;
    }

    // void OnToggleClick(Toggle toggle, GameObject targetPage, bool isOn)
    // {
    //     if (isOn)
    //     {
    //         CloseOtherToggles(toggle);
    //         if (targetPage != null) targetPage.SetActive(true);
    //     }
    // }

    // void CloseOtherToggles(Toggle activeToggle)
    // {
    //     if (activeToggle != Tog_Language)
    //     {
    //         Tog_Language.isOn = false;
    //         LanguagePage.SetActive(false);
    //     }
    //     if (activeToggle != Tog_Audio)
    //     {
    //         Tog_Audio.isOn = false;
    //         VoicePage.SetActive(false);
    //     }
    // }

    private void AdjustVolume(AudioSource audioSource, float value)
    {
        if (audioSource != null)
        {
            audioSource.volume = value;
        }
    }
}
