using Naninovel;
using UnityEngine;

[CommandAlias("speedButtonDisable")]
public class SpeedButtonDisable : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await ButtonDesableAsync(asyncToken);
    }

    private static async UniTask ButtonDesableAsync(AsyncToken asyncToken)
    {
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        startNani.buttonController.gameObject.SetActive(false);
        await UniTask.CompletedTask;
    }
}
