using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Alert : MonoBehaviour
{
    [SerializeField] private TMP_Text txtContent;
    [SerializeField] private Button btnOK;
    
    static Alert instance { get; set; }

    void Awake()
    {
        instance = this;
        txtContent.text = "";
        btnOK.onClick.AddListener(Close);
        
        // 默认隐藏
        Hide();
    }

    public static void Show(string content)
    {
        instance.gameObject.SetActive(true);
        instance.txtContent.text = content;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}
