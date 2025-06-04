using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("StartGameEnable")]
public class StartGameEnable : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await ButtonEnableAsync(asyncToken);
    }

    private static async UniTask ButtonEnableAsync(AsyncToken asyncToken)
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        startNani.StartGamePage.gameObject.SetActive(true);
        await UniTask.CompletedTask;
    }
}
