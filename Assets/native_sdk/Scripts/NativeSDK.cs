using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class NativeSDK : MonoBehaviour
{

#if UNITY_IOS && !UNITY_EDITOR
    private const string LIBRARY_NAME = "__Internal";
#else
    private const string LIBRARY_NAME = "NativeSDK";
#endif

    [DllImport(LIBRARY_NAME)]
    private static extern void UnityNativeInterface_onCall(string method, string paramString);

    public delegate void CallbackDelegate(JObject ret);
    class CallbackInfo
    {
        public CallbackDelegate Success { get; }
        public CallbackDelegate Fail { get; }

        public CallbackInfo(CallbackDelegate _success, CallbackDelegate _fail)
        {
            Success = _success;
            Fail = _fail;
        }
    }

    private static NativeSDK _inst;
    public static NativeSDK Instance => _inst;

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[NativeSDK Awake]");
    }

    private readonly JObject EPMTY_PARAM = new JObject();
    private Dictionary<string, CallbackInfo> _map = new Dictionary<string, CallbackInfo>();

    // 埋点：初始化
    public void logInit(string clientId, string clientToken, bool debug)
    {
        Tracker.Instance.init(clientId, clientToken, debug);
    }

    // 埋点：记录登录
    public void logSetUserId(string userId)
    {
        Tracker.Instance.setUserId(userId);
    }

    // 埋点：记录登出
    public void logClearUser()
    {
        Tracker.Instance.clearUser();
    }

    // 埋点：记录支付
    public void logPurchasedEvent(JObject param)
    {
        string orderID = (string)param["orderId"];
        string productName = (string)param["productName"];
        long amount = (long)param["amount"];
        string currencyType = (string)param["currencyType"];
        string paymentChannel = (string)param["paymentChannel"];
        JObject properties = (JObject)param["properties"];
        Tracker.Instance.logPurchasedEvent(orderID, productName, amount, currencyType, paymentChannel, properties.ToString());
    }

    // 绑定SDK事件
    public void bindEvent(CallbackDelegate success)
    {
        _map["sdk"] = new CallbackInfo(success, null);
        call("sdk", EPMTY_PARAM.ToString());
    }

    // 初始化
    public void init(JObject param, CallbackDelegate success, CallbackDelegate fail)
    {
        _map["init"] = new CallbackInfo(success, fail);
        call("init", param.ToString());
    }

    // 打开登录界面
    public void login(CallbackDelegate success, CallbackDelegate fail)
    {
        _map["login"] = new CallbackInfo(success, fail);
        call("login", EPMTY_PARAM.ToString());
    }

    // 打开登出界面
    public void logout(JObject param, CallbackDelegate success, CallbackDelegate fail)
    {
        _map["logout"] = new CallbackInfo(success, fail);
        call("logout", param.ToString());
    }

    // 打开支付界面
    public void pay(JObject param)
    {
        _map["pay"] = new CallbackInfo(null, null);
        call("pay", param.ToString());
    }

    // 打开充值界面
    public void ptbpay()
    {
        _map["ptbpay"] = new CallbackInfo(null, null);
        call("ptbpay", EPMTY_PARAM.ToString());
    }

    // 打开账单界面
    public void bill()
    {
        _map["bill"] = new CallbackInfo(null, null);
        call("bill", EPMTY_PARAM.ToString());
    }

    // 打开客服界面
    public void chat()
    {
        _map["chat"] = new CallbackInfo(null, null);
        call("chat", EPMTY_PARAM.ToString());
    }

    // 打开客服界面（未初始化时可用）
    public void chat2(JObject param)
    {
        _map["chat"] = new CallbackInfo(null, null);
        call("chat", param.ToString());
    }

    // 获取用户信息
    public void user(CallbackDelegate success, CallbackDelegate fail)
    {
        _map["user"] = new CallbackInfo(success, fail);
        call("user", EPMTY_PARAM.ToString());
    }

    // 上报角色信息
    public void uploadRole(JObject param, CallbackDelegate success, CallbackDelegate fail)
    {
        _map["uploadRole"] = new CallbackInfo(success, fail);
        call("uploadRole", param.ToString());
    }


    void call(string method, string paramString)
    {
        Debug.Log($"[NativeSDK::call]{method}:{paramString}");
#if UNITY_IOS
        UnityNativeInterface_onCall(method, paramString);
#elif UNITY_ANDROID
        AndroidJavaObject jo = new AndroidJavaObject("com.external.UnityNativeInterface");
        jo.CallStatic("onCall", method, paramString);
#endif
    }

    void onCall(string retString)
    {
        Debug.Log($"[Unity onCall]{retString}");
        JObject ret = ToJson(retString);
        string methodString = (string)ret["method"];
        string[] parts = methodString.Split(new char[] { ':' });
        string method = parts[0];
        string result = parts[1];
        ret.Remove("method");

        CallbackInfo info;
        if (_map.TryGetValue(method, out info))
        {
            if (result == "success")
            {
                if (info.Success != null) info.Success(ret);
            }
            else
            {
                if (info.Fail != null) info.Fail(ret);
            }
        }
    }

    JObject ToJson(string str)
    {
        JObject json = JObject.Parse("{}") as JObject;
        try
        {
            json = JObject.Parse(str) as JObject;
        }
        catch (Exception)
        {
            Debug.Log($"[ToJson]失败:{str}");
        }
        return json;
    }
}
