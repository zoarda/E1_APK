using Naninovel;
using UnityEngine;
public class GameOverBack : Command
{
    [ParameterAlias("Saying")]
    
    public StringParameter Saying;
    
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
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SetGameClearBackAsync(Saying,ConfirmScript ,Confirm,Cancel,CancelScript,asyncToken);
    }
    public static async UniTask SetGameClearBackAsync(string Saying ,string ConfirmScript,string Confirm,string Cancel,string CancelScript,AsyncToken asyncToken)
    {
        ICharacterManager actorManager = Engine.GetService<ICharacterManager>();
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        IScriptPlayer scriptPlayer = Engine.GetService<IScriptPlayer>();

        actorManager.RemoveAllActors();
        var prefab =  spawnManager.GetSpawned("GameOverPage");
        GameOverPage preOverPage = prefab.GameObject.GetComponent<GameOverPage>();
        preOverPage.ConfirmTxt.text = LanguageManager.Instance.GetLanguageValue(Confirm);
        preOverPage.CancelTxt.text = LanguageManager.Instance.GetLanguageValue(Cancel);
        preOverPage.SayingTxt.text = LanguageManager.Instance.GetLanguageValue(Saying);

        prefab.GameObject.GetComponent<GameOverPage>().ConfirmBtn.onClick.AddListener(async () =>
        {
            await scriptPlayer.PreloadAndPlayAsync(ConfirmScript);
            var sfx = Engine.GetService<IAudioManager>();
            if (sfx != null)
            {
                await sfx.PlayBgmAsync("GameStart", 0.2f, 0.5f, true);
            }
            // spawnManager.DestroySpawned("GameOverPage");
            NaniCommandManger.Instance.SpeedButtonClearSpawn();
        });
        prefab.GameObject.GetComponent<GameOverPage>().CancelBtn.onClick.AddListener(async () =>
        {
            await scriptPlayer.PreloadAndPlayAsync(CancelScript);
            // spawnManager.DestroySpawned("GameOverPage");
            NaniCommandManger.Instance.SpeedButtonClearSpawn();
        });

        await UniTask.CompletedTask;
    }
}