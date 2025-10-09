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
    [ParameterAlias("Character")]
    public StringParameter Character;

    protected virtual string scriptName => ScriptName;
    protected virtual string good => Goodfriendship;
    protected virtual string bad => BadFriendship;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        if (Mode == "Set")
        {
            StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
            if (Character == "CiciXie") // 筱希
            {
                startNani.friednshipList_CiciXie.Add(friendship);
            }
            else if (Character == "RosieLin") // 林香
            {
                startNani.friednshipList_RosieLin.Add(friendship);
            }
            else if (Character == "CherryZhao") // 紫涵
            {
                startNani.friednshipList_CherryZhao.Add(friendship);
            }
            else
            {
                Debug.LogError("Character name is incorrect. Please use 'CiciXie', 'RosieLin', or 'CherryZhao'.");
            }
            // startNani.SetfriendshipList(friendship);
            await UniTask.CompletedTask;
        }
        else if (Mode == "Get")
        {
            StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
            var varManager = Engine.GetService<ICustomVariableManager>();
            //依照character來決定使用哪個好感度變數
            if (Character == "CiciXie") // 筱希
            {
                varManager.TrySetVariableValue("friendship_CiciXie", startNani.allFriendship_CiciXie());
            }
            else if (Character == "RosieLin") // 林香
            {
                varManager.TrySetVariableValue("friendship_RosieLin", startNani.allFriendship_RosieLin());
            }
            else if (Character == "CherryZhao") // 紫涵
            {
                varManager.TrySetVariableValue("friendship_CherryZhao", startNani.allFriendship_CherryZhao());
            }
            else
            {
                Debug.LogError("Character name is incorrect. Please use 'CiciXie', 'RosieLin', or 'CherryZhao'.");
            }
            // varManager.TrySetVariableValue("friendship", startNani.allFriendship());
            // var myValue = varManager.GetVariableValue("friendship");
            var myValue = Character == "CiciXie" ? varManager.GetVariableValue("friendship_CiciXie") :
                          Character == "RosieLin" ? varManager.GetVariableValue("friendship_RosieLin") :
                          Character == "CherryZhao" ? varManager.GetVariableValue("friendship_CherryZhao") : null;
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
}
