using UnityEngine;
using Naninovel;

[CommandAlias("NaniSaveData")]
public class NaniSaveData : Command
{
    [ParameterAlias("ScriptName")]
    public StringParameter ScriptName;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SaveDataAsync(ScriptName, asyncToken);
    }
    private static async UniTask SaveDataAsync(string name, AsyncToken asyncToken)
    {
        StartNani startNani = StartNani.Instance;
        if (startNani.isLoggedIn)
        {
            await startNani.SaveYaml(name);
        }
        else
        {
            Debug.Log("nologinMode");
        }
        // await startNani.SelectOptionSwtich();
        await UniTask.CompletedTask;
    }
}
