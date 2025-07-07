using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class NativeSDK : MonoBehaviour
{

    #region Internal

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void OnCall(string method, string paramString);
#endif

    #endregion

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
        Debug.Log("[NativeSDK]Awake");
    }

    void OnDestroy()
    {
        // 销毁时清理单例实例
        _inst = null;
        Debug.Log("[NativeSDK]OnDestroy");
    }

    private readonly JObject EPMTY_PARAM = new JObject();
    private Dictionary<String, CallbackInfo> _map = new Dictionary<String, CallbackInfo>();

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
        Call("sdk", EPMTY_PARAM.ToString());
    }

    // 初始化
    public void init(JObject param, CallbackDelegate success, CallbackDelegate fail)
    {
        _map["init"] = new CallbackInfo(success, fail);
        Call("init", param.ToString());
    }

    // 打开登录界面
    public void login(CallbackDelegate success, CallbackDelegate fail)
    {
        _map["login"] = new CallbackInfo(success, fail);
        Call("login", EPMTY_PARAM.ToString());
    }

    // 打开登出界面
    public void logout(JObject param, CallbackDelegate success, CallbackDelegate fail)
    {
        _map["logout"] = new CallbackInfo(success, fail);
        Call("logout", param.ToString());
    }

    // 打开支付界面
    public void pay(JObject param)
    {
        _map["pay"] = new CallbackInfo(null, null);
        Call("pay", param.ToString());
    }

    // 打开平台币充值界面
    public void ptbpay()
    {
        _map["ptbpay"] = new CallbackInfo(null, null);
        Call("ptbpay", EPMTY_PARAM.ToString());
    }

    // 打开账单界面
    public void bill()
    {
        _map["bill"] = new CallbackInfo(null, null);
        Call("bill", EPMTY_PARAM.ToString());
    }

    // 打开客服界面
    public void chat()
    {
        _map["chat"] = new CallbackInfo(null, null);
        Call("chat", EPMTY_PARAM.ToString());
    }

    // 获取用户信息
    public void user(CallbackDelegate success, CallbackDelegate fail)
    {
        _map["user"] = new CallbackInfo(success, fail);
        Call("user", EPMTY_PARAM.ToString());
    }

    void Call(string method, string paramString)
    {
        Debug.Log(String.Format("[Unity Call]{0}:{1}", method, paramString));
#if UNITY_IOS
        OnCall(method, paramString);
#elif UNITY_ANDROID
        AndroidJavaObject jo = new AndroidJavaObject("com.external.UnityNativeInterface");
        jo.CallStatic("OnCall", method, paramString);
#endif
    }

    void OnCall(string retString)
    {
        Debug.Log(String.Format("[Unity OnCall]{0}", retString));
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
            Debug.Log("[ToJson]失败:" + str);
        }
        return json;
    }
}
