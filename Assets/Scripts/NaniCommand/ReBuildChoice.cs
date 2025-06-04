using Naninovel;
using UnityEngine.UI;
using UnityEngine;

public class ReBuildChoice : Command
{
    public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
    {
        GameObject ButtonObj = GameObject.Find("AvButtonList");
        if (ButtonObj != null)
        {
            GameObject Content;
            RectTransform rectTransform = ButtonObj.GetComponent<RectTransform>();
            foreach (Transform child in ButtonObj.transform)
            {
                if (child.name == "ContentPanel")
                {
                    Content = child.gameObject;
                    rectTransform = Content.GetComponent<RectTransform>();
                    break;
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
        GameObject BoxButtonObj = GameObject.Find("AvBoxButtonList");
        if (BoxButtonObj != null)
        {
            GameObject Content;
            RectTransform rectTransform = BoxButtonObj.GetComponent<RectTransform>();
            foreach (Transform child in BoxButtonObj.transform)
            {
                if (child.name == "ContentPanel")
                {
                    Content = child.gameObject;
                    rectTransform = Content.GetComponent<RectTransform>();
                    break;
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
        await UniTask.CompletedTask;
    }
}
