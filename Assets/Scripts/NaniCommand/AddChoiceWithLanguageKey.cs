using UnityEngine;
using Naninovel;
using Naninovel.Commands;

[CommandAlias("choiceDLC")]
public class AddChoiceWithLanguageKey : AddChoice
{
    private string initialSummary;
    private bool isFristExecute = true;
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        NaniCommandManger naniCommandManger = NaniCommandManger.Instance;
        LanguageManager languageManager = LanguageManager.Instance;
        if (isFristExecute)
        {
            initialSummary = ChoiceSummary;
            isFristExecute = false;
        }
        else
        {
            ChoiceSummary = initialSummary;
        }
        ChoiceSummary = languageManager.GetLanguageValue(ChoiceSummary);
        StartNani startNani = GameObject.Find("StartNani").GetComponent<StartNani>();
        startNani.buttonController.gameObject.SetActive(false);
        await base.ExecuteAsync(asyncToken);
    }
}

