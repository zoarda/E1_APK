using Naninovel;
using UnityEngine;

public class Descriptionbox : Command
{
    [ParameterAlias("Description")]
    public StringParameter Description;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SetDescriptionboxAsync(Description);
    }
    public static async UniTask SetDescriptionboxAsync(string Description)
    {
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        var prefab = spawnManager.GetSpawned("Descriptionbox");
        LangugeText langugeText = prefab.GameObject.GetComponent<LangugeText>();
        langugeText.text.text = LanguageManager.Instance.GetLanguageValue(Description);

        await UniTask.CompletedTask;
    }
}
