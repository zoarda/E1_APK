using UnityEngine;
using Naninovel;

public class CloseSlider : Command
{
    public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
    {
        await Close(asyncToken);
    }
    public static async UniTask Close(AsyncToken asyncToken)
    {
        NaniCommandManger naniCommandManger = GameObject.Find("NaniCommandManger").GetComponent<NaniCommandManger>();
        naniCommandManger.StopSlider();
        await UniTask.CompletedTask;
    }
}
