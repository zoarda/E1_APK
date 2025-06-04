using Naninovel;

[CommandAlias("rmMovie")]
public class RmMovie : Command
{
    public override UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        IBackgroundManager backgroundManager = Engine.GetService<IBackgroundManager>();
        backgroundManager.RemoveAllActors();
        return UniTask.CompletedTask;
    }

}
