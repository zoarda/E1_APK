using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckPage : MonoBehaviour
{
    public Text text;
    void Awake()
    {
        if (!StartNani.Instance.ispay)
        {
            text.text = "尚未購買遊戲";
        }
    }
}
