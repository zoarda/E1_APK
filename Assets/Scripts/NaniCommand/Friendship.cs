using Naninovel;
using Unity.VisualScripting;
using UnityEngine;

[CommandAlias("friendship")]
public class Friendship : Command
{
    [ParameterAlias("friendship")]
    public DecimalParameter friendship;
    [ParameterAlias("Mode")]
    public StringParameter Mode;
    [ParameterAlias("ScriptName")]
    public StringParameter ScriptName;
    [ParameterAlias("Goodfriendship")]
    public StringParameter Goodfriendship;
    [ParameterAlias("BadFriendship")]
    public StringParameter BadFriendship;

    protected virtual string scriptName => ScriptName;
    protected virtual string good => Goodfriendship;
    protected virtual string bad => BadFriendship;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        if (Mode == "Set")
        {
            StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
            startNani.SetfriendshipList(friendship);
            await UniTask.CompletedTask;
        }
        else if (Mode == "Get")
        {
            StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
            var varManager = Engine.GetService<ICustomVariableManager>();
            varManager.TrySetVariableValue("friendship", startNani.allFriendship());
            var myValue = varManager.GetVariableValue("friendship");
            Debug.Log($"GetFreindshipAsync: {myValue}");
            var a = float.Parse(myValue);
            var Player = Engine.GetService<IScriptPlayer>();
            if (scriptName == null || good == null || bad == null)
            {
                Debug.LogError("ScriptName or Goodfriendship or BadFriendship is null");
                return;
            }
            if (a >= 3)
            {
                await Player.PreloadAndPlayAsync(scriptName, label: good);
            }
            else
            {
                await Player.PreloadAndPlayAsync(scriptName, label: bad);
            }
        }
    }
    // public static async UniTask SetFriendshipAsync(float friendship, AsyncToken asyncToken)
    // {
    //     StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
    //     startNani.SetfriendshipList(friendship);
    //     await UniTask.CompletedTask;
    // }
    // public static async UniTask ChoiceStroy(AsyncToken asyncToken)
    // {
    //     StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
    //     var varManager = Engine.GetService<ICustomVariableManager>();
    //     varManager.TrySetVariableValue("friendship", startNani.allFriendship());
    //     var myValue = varManager.GetVariableValue("friendship");
    //     Debug.Log($"GetFreindshipAsync: {myValue}");
    //     int friendship = int.Parse(myValue);
    //     var Player = Engine.GetService<IScriptPlayer>();
    //     if (friendship >= 50)
    //     {
    //         await Player.PreloadAndPlayAsync($"{}");
    //     }
    //     else
    //     {
    //         await Player.PreloadAndPlayAsync("BadFriendship");
    //     }
    // }
}
