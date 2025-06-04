using UnityEngine;
using Naninovel;

public class WaitSlider : Command
{
   [ParameterAlias("time")] 
    public DecimalParameter time;
    public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
    {
        await TimerAsync(time, asyncToken);
    }
    public static async UniTask TimerAsync(float time, AsyncToken asyncToken)
    {
        NaniCommandManger naniCommandManger = GameObject.Find("NaniCommandManger").GetComponent<NaniCommandManger>();
        naniCommandManger.StartSlider(time);
        await UniTask.CompletedTask;
    }
}
