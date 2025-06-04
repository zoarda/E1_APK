using Naninovel;
using System;
using System.Collections.Generic;

public class SpawnLovePlayPage : Command
{
    [ParameterAlias("PoseName")]
    public StringListParameter PoseName;
    [ParameterAlias("Label")]
    public StringListParameter Label;
    [ParameterAlias("CumLabel")]
    public StringParameter CumLabel;
    [ParameterAlias("HidenCumLabel"), ParameterDefaultValue("null")]
    public StringParameter HidenCumLabel;
    [ParameterAlias("Type")]
    public StringParameter Type;

    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SetLovePlayPageAsync(PoseName, Label, CumLabel, Type, asyncToken);
        await SetCumButtonAsync(CumLabel, HidenCumLabel, Type, asyncToken);
    }
    public static async UniTask SetLovePlayPageAsync(List<string> poseName, List<string> label, string cumLabel, string type, AsyncToken asyncToken)
    {
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
        lovePlayPagePrefab.GameObject.SetActive(true);
        LovePlayPage lovePlayPage = lovePlayPagePrefab.GameObject.GetComponent<LovePlayPage>();
        NaniCommandManger.Instance.ClearLovePlayPage();
        for (int i = 0; i < poseName.Count; i++)
        {
            NaniCommandManger.Instance.SpawnLovePlayPageButton(lovePlayPage, lovePlayPage.buttonPrefab, poseName[i], label[i], type);
        }
        await UniTask.CompletedTask;
    }
    public static async UniTask SetCumButtonAsync(string cumLabel, string hidenCumLabel, string type, AsyncToken asyncToken)
    {
        ISpawnManager spawnManager = Engine.GetService<ISpawnManager>();
        var lovePlayPagePrefab = spawnManager.GetSpawned("LovePlayPage");
        LovePlayPage lovePlayPage = lovePlayPagePrefab.GameObject.GetComponent<LovePlayPage>();
        NaniCommandManger.Instance.SetLovePlayPageMode(lovePlayPage, cumLabel, hidenCumLabel, type);
        await UniTask.CompletedTask;
    }
}
