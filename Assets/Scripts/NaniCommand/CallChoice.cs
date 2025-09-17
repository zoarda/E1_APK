using Naninovel;

public class CallChoice : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await Skip(asyncToken);
    }
    public static async UniTask Skip(AsyncToken asyncToken)
    {
        await VideoManager.Instance.SkipVideo();
        await UniTask.CompletedTask;
    }
}
