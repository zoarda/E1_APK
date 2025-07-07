using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class App : MonoBehaviour
{
    [SerializeField]
    TextAsset config;
    private LoginPage loginPage; // 新增對 LoginPage 的引用

    public void SetLoginPage(LoginPage page)
    {
        loginPage = page;
    }
    private void ShowDebug(string text)
    {
        if (loginPage != null)
            loginPage.ShowDebug(text);
        else
            Debug.LogWarning("LoginPage 尚未設定！");
    }

    // 测试：sdk初始化
    public void testInit()
    {
        // 绑定SDK事件
        NativeSDK.Instance.bindEvent(
            (ret) =>
            {
                // 打印内容
                Debug.Log(String.Format("SDK事件：{0}", ret));
                // 切换账号而退出
                if (ret["event"]?.ToString() == "SWITCH_ACCOUNT")
                {
                    ShowDebug("切換帳號已退出");
                }
            }
        );

        // 初始化
        JObject param = new JObject();
        param["config"] = JObject.Parse(config.text); // sdk配置
        param["debug"] = true; // 是否开启调试模式
        NativeSDK.Instance.init(
            param,
            (ret) =>
            {
                JObject dynaConfig = (JObject)ret["dyna_config"];
                ShowDebug(String.Format("初始化成功：{0}", dynaConfig));
            },
            (ret) =>
            {
                ShowDebug(String.Format("初始化失败：{0}", ret));
            }
        );
    }

    // 测试：打开登录界面
    public void testLogin()
    {
        NativeSDK.Instance.login(
            (ret) =>
            {
                ShowDebug(String.Format("登录成功：{0}", ret));
            },
            (ret) =>
            {
                ShowDebug(String.Format("登录失败：{0}", ret));
            }
        );
    }

    // 测试：打开登出界面
    public void testLogout()
    {
        JObject param = new JObject();
        param["silent"] = true;
        NativeSDK.Instance.logout(param,
            (ret) =>
            {
               ShowDebug(String.Format("登出成功：{0}", ret));
            },
            (ret) =>
            {
                ShowDebug(String.Format("登出失败：{0}", ret));
            }
        );
    }

    // 测试：打开支付界面
    public void testPay()
    {
        JObject param = new JObject();
        param["title"] = "支付标题";
        param["body"] = "支付描述";
        param["price"] = 100; // 价格（单位：元）
        param["order_sn"] = DateTimeOffset.Now.ToUnixTimeMilliseconds(); ;  // 游戏订单号
        NativeSDK.Instance.pay(param);
    }

    // 测试：打开平台币充值界面
    public void testPTBPay()
    {
        NativeSDK.Instance.ptbpay();
    }

    // 测试：打开账单界面
    public void testBill()
    {
        NativeSDK.Instance.bill();
    }

    // 测试：打开客服界面
    public void testChat()
    {
        NativeSDK.Instance.chat();
    }

    // 测试：获取用户信息
    public void testUser()
    {
        NativeSDK.Instance.user(
            (ret) =>
            {
                ShowDebug(String.Format("获取信息成功：{0}", ret));
            },
            (ret) =>
            {
                ShowDebug(String.Format("获取信息失败：{0}", ret));
            }
        );
    }
}
