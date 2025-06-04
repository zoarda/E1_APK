using Naninovel;

public class SwitchCamera : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        await SwitchMainCamera();
    }
    public static async UniTask SwitchMainCamera()
    {
        await NaniCommandManger.Instance.switchCamera();
    }
}
