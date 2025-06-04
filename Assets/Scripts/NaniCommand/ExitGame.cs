using UnityEngine;
using Naninovel;

public class ExitGame : Command
{
    public override UniTask ExecuteAsync (AsyncToken cancellationToken = default)
    {
        Application.Quit();
        return UniTask.CompletedTask;
    }
}
