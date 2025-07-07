// using TapSDK.Core;

public class Tracker
{
    private static Tracker instance;
    private Tracker() { }
    public static Tracker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Tracker();
            }
            return instance;
        }
    }

    public void init(string clientId, string clientToken, bool debug){
        // 核心配置
        // TapTapSdkOptions coreOptions = new TapTapSdkOptions
        // {
        //     // 客户端 ID，开发者后台获取
        //     clientId = clientId,
        //     // 客户端令牌，开发者后台获取
        //     clientToken = clientToken,
        //     // 地区，CN 为国内，Overseas 为海外
        //     region = TapTapRegionType.Overseas,
        //     // 语言，默认为 Auto，默认情况下，国内为 zh_Hans，海外为 en
        //     preferredLanguage = TapTapLanguageType.zh_Hans,
        //     // 是否开启日志，Release 版本请设置为 false
        //     enableLog = debug
        // };
        // // TapSDK 初始化
        // TapTapSDK.Init(coreOptions);
    }
    
    public void setUserId(string userId){
        // 设置用户 ID 及账号登录事件属性
        // TapTapEvent.SetUserID(userId);
    }

    public void clearUser(){
        // TapTapEvent.ClearUser();
    }

    public void logPurchasedEvent(string orderID, string productName, long amount, string currencyType, string paymentMethod, string properties){
        // TapTapEvent.LogPurchasedEvent(
        //     orderID: orderID,
        //     productName: productName,
        //     amount: amount,
        //     currencyType: currencyType,
        //     paymentMethod: paymentMethod,
        //     properties: properties
        // );
    }
}
