using Naninovel;

public class VideoControll : Command
{
    [ParameterAlias("StartLoopTime")]
    public DecimalParameter startLoopTime;
    [ParameterAlias("EndLoopTime")]
    public DecimalParameter endLoopTime;
    [ParameterAlias("isLooping")]
    public BooleanParameter isLooping;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await VideoControllAsync(startLoopTime, endLoopTime, isLooping, asyncToken);
    }
    private static async UniTask VideoControllAsync(float SLT, float ELT, bool isLooping, AsyncToken asyncToken)
    {
        await NaniCommandManger.Instance.SetVideoTime(SLT, ELT, isLooping);
        NaniCommandManger.Instance.SetCheckLoop(isLooping);
        await UniTask.CompletedTask;
    }
}
