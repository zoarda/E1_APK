using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPage : MonoBehaviour
{

    private App app; // App 物件的引用
    private string debugText = ""; // 用來顯示回傳結果

    void Start()
    {
        app = FindObjectOfType<App>();
        if (app == null)
        {
            Debug.LogError("找不到 App 實例！");
        }
        else
        {
            app.SetLoginPage(this); // 註冊自己給 App 用
        }
    }
    void OnGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(600));

        if (GUILayout.Button("初始化 SDK", GUILayout.Height(50)))
        {
            debugText = "執行初始化...";
            app.testInit();
        }

        if (GUILayout.Button("登入", GUILayout.Height(50)))
        {
            debugText = "打開登入界面...";
            app.testLogin();
        }

        if (GUILayout.Button("登出", GUILayout.Height(50)))
        {
            debugText = "執行登出...";
            app.testLogout();
        }

        if (GUILayout.Button("支付", GUILayout.Height(50)))
        {
            debugText = "打開支付...";
            app.testPay();
        }

        if (GUILayout.Button("平台幣充值", GUILayout.Height(50)))
        {
            debugText = "打開平台幣充值...";
            app.testPTBPay();
        }

        if (GUILayout.Button("查詢帳單", GUILayout.Height(50)))
        {
            debugText = "打開帳單查詢...";
            app.testBill();
        }

        if (GUILayout.Button("客服", GUILayout.Height(50)))
        {
            debugText = "打開客服中心...";
            app.testChat();
        }

        if (GUILayout.Button("用戶資訊", GUILayout.Height(50)))
        {
            debugText = "查詢用戶資訊...";
            app.testUser();
        }

        GUILayout.Label("【Debug 回應】", GUILayout.Height(30));
        GUILayout.TextArea(debugText, GUILayout.Height(200));

        GUILayout.EndVertical();
    }

    // 這邊用來接收 Alert 顯示的文字並更新 debugText
    public void ShowDebug(string text)
    {
        debugText = text;
        Debug.Log(text);
    }
}
