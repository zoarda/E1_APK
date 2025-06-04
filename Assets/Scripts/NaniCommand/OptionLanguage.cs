using System.Runtime.InteropServices;
using Naninovel;
using UnityEngine;

[CommandAlias("optionLanguage")]

public class OptionLanguage : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        IChoiceHandlerManager choiceHandlerManager = Engine.GetService<IChoiceHandlerManager>();
        var choiceHandler = await choiceHandlerManager.GetOrAddActorAsync("AvButtonList");
        // var choiceHandler = await choiceHandlerManager.GetOrAddActorAsync("ButtonList");
        foreach (var choice in choiceHandler.Choices)
        {
           var obj = choiceHandler.GetChoice(choice.Id);

        }
    }
}
