using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LovePlayPage : MonoBehaviour
{
    public GameObject buttonPrefab, NextVideoButtonPrefab, HidenVideoButtonPrefab, SettingButtonPrefab, NextVideoButtonLock, HidenVideoButtonLock;
    public Button NextVideoButton, HidenVideoButton, SettingButton,SpeedButton, ToggleDisplayButton;
    public Transform ChoiceButtonParent;
    public CanvasGroup DisplayCanvasGroup;
    public List<GameObject> ChoiceButtonList = new List<GameObject>();

    void Awake()
    {
        ToggleDisplayButton.onClick.AddListener(() =>
        {
            if (DisplayCanvasGroup.alpha == 1)
            {
                DisplayCanvasGroup.alpha = 0;
                DisplayCanvasGroup.blocksRaycasts = false;
            }
            else
            {
                DisplayCanvasGroup.alpha = 1;
                DisplayCanvasGroup.blocksRaycasts = true;
            }
        });
    }
}
