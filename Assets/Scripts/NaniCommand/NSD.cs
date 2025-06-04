using UnityEngine;
using Naninovel;

public class NSD : Command
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
        await startNani.SaveYaml(name);
        // await startNani.SelectOptionSwtich();
        await UniTask.CompletedTask;
    }
}
