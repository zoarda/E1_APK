using Naninovel;
using UnityEngine.UI;
using UnityEngine;
public class GameClearBack : Command
{
    [ParameterAlias("Saying")]
    
    public StringParameter Saying;

    
    [ParameterAlias("Name")]
    
    public StringParameter Name;
    
    [ParameterAlias("ConfirmScript")]
    
    public StringParameter ConfirmScript;
    
    [ParameterAlias("ConfirmLabel")]
    
    public StringParameter ConfirmLabel;
    
    [ParameterAlias("ConfirmText")]
    
    public StringParameter ConfirmText;
    
    [ParameterAlias("Confirm")]
    
    public StringParameter Confirm;
    
    [ParameterAlias("CancelScript")]
    
    public StringParameter CancelScript;
    
    [ParameterAlias("CancelLabel")]
    
    public StringParameter CancelLabel;
    
    [ParameterAlias("CancelText")]
    
    public StringParameter CancelText;

    [ParameterAlias("Cancel")]
    public StringParameter Cancel;

    [ParameterAlias("ImageTitle")]
    public StringParameter ImageTitle;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SetGameClearBackAsync(Saying,Name,ConfirmScript ,Confirm,Cancel,CancelScript,ImageTitle,asyncToken);
    }
    public static async UniTask SetGameClearBackAsync(string Saying,string Name ,string ConfirmScript,string Confirm,string Cancel,string CancelScript,string ImageTitle,AsyncToken asyncToken)
    {
        ICharacterManager characterManager = Engine.GetService<ICharacterManager>();
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        IScriptPlayer scriptPlayer = Engine.GetService<IScriptPlayer>();

        characterManager.RemoveAllActors();
        var prefab =  spawnManager.GetSpawned("GameClearPage");
        GameClearPage preClearPage = prefab.GameObject.GetComponent<GameClearPage>();
        preClearPage.ConfirmTxt.text = LanguageManager.Instance.GetLanguageValue(Confirm);
        preClearPage.CancelTxt.text = LanguageManager.Instance.GetLanguageValue(Cancel);

        string setResourceImaSaying = LanguageManager.Instance.GetLanguageValue(Saying);
        Texture2D imaSay = Resources.Load<Texture2D>("GameOver/" + setResourceImaSaying);
        if (imaSay != null)
        {
            preClearPage.ImaSaying.sprite = Sprite.Create(imaSay, new Rect(0, 0, imaSay.width, imaSay.height), new Vector2(0.5f, 0.5f));
            preClearPage.ImaSaying.SetNativeSize();
        }
        else
        {
            Debug.Log("Image not found");
        }
        string setResourceImaName = LanguageManager.Instance.GetLanguageValue(Name);
        Texture2D imaName = Resources.Load<Texture2D>("GameOver/" + setResourceImaName);
        if (imaName != null)
        {
            preClearPage.ImaName.sprite = Sprite.Create(imaName, new Rect(0, 0, imaName.width, imaName.height), new Vector2(0.5f, 0.5f));
            preClearPage.ImaName.SetNativeSize();
        }
        else
        {
            Debug.Log("Image not found");
        }      
        string setResourceImageTitle = LanguageManager.Instance.GetLanguageValue(ImageTitle);
        Texture2D imaTilte = Resources.Load<Texture2D>("GameOver/" + setResourceImageTitle);
        if (imaTilte != null)
        {
            preClearPage.ImaTitle.sprite = Sprite.Create(imaTilte, new Rect(0, 0, imaTilte.width, imaTilte.height), new Vector2(0.5f, 0.5f));
            preClearPage.ImaTitle.SetNativeSize();
        }
        else
        {
            Debug.Log("Image not found");
        }

        prefab.GameObject.GetComponent<GameClearPage>().ConfirmBtn.onClick.AddListener(async () =>
        {
            await scriptPlayer.PreloadAndPlayAsync(ConfirmScript);
            var sfx = Engine.GetService<IAudioManager>();
            if (sfx != null)
            {
                await sfx.PlayBgmAsync("GameStart", 0.2f, 0.5f, true);
            }
            // spawnManager.DestroySpawned("GameClearPage");
            NaniCommandManger.Instance.SpeedButtonClearSpawn();
        });
        prefab.GameObject.GetComponent<GameClearPage>().CancelBtn.onClick.AddListener(async () =>
        {
            await scriptPlayer.PreloadAndPlayAsync(CancelScript);
            // spawnManager.DestroySpawned("GameClearPage");
            NaniCommandManger.Instance.SpeedButtonClearSpawn();
        });

        await UniTask.CompletedTask;
    }
}