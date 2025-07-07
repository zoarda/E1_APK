using UnityEngine;
using Naninovel;
using Unity.VisualScripting;

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
        ServerManager serverManager = ServerManager.Instance;
        //暫時修改條件startNani.isLoggedIn 改成tap登入就先不做
        if (!serverManager.isTapMode)
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
