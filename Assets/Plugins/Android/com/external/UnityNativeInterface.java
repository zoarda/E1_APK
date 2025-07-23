package com.external;

import android.app.Activity;
import android.util.Log;

import com.mchsdk.extras.IActiveJsb;
import com.mchsdk.extras.Tracker;
import com.mchsdk.open.IGPUserObsv;
import com.mchsdk.open.MCApiFactory;
import com.mchsdk.open.OrderInfo;
import com.mchsdk.open.RoleInfo;
import com.mchsdk.open.ToastUtil;
import com.mchsdk.open.UploadRoleCallBack;

import com.mchsdk.paysdk.bean.ChannelAndGameInfo;
import com.mchsdk.paysdk.bean.PersonalCenterModel;
import com.mchsdk.paysdk.utils.MCLog;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

public class UnityNativeInterface {
    private static final String TAG = "UnityNativeInterface";

    //----------------主动调用
    public static void initResult(boolean success, JSONObject data) {
        String name = "init:" + (success ? "success" : "fail");
        Call(name, data);
    }

    public static void loginResult(boolean success, JSONObject data) {
        String name = "login:" + (success ? "success" : "fail");
        Call(name, data);
    }

    public static void logoutResult(boolean success, JSONObject data) {
        String name = "logout:" + (success ? "success" : "fail");
        Call(name, data);
    }

    public static void payResult(boolean success, JSONObject data) {
        String name = "pay:" + (success ? "success" : "fail");
        Call(name, data);
    }

    public static void userResult(boolean success, JSONObject data) {
        String name = "user:" + (success ? "success" : "fail");
        Call(name, data);
    }

    public static void sdkResult(boolean success, JSONObject data) {
        String name = "sdk:" + (success ? "success" : "fail");
        Call(name, data);
    }

    //----------------被动调用
    // 初始化
    static void _init(JSONObject data) {
        // 使用Unity的Activity
        Activity context = UnityPlayer.currentActivity;

        if (MCApiFactory.getMCApi().isInit()) {
            ToastUtil.show(context, "SDK已初始化");
            return;
        }

        JSONObject config = data.optJSONObject("config");
        boolean debug = data.optBoolean("debug", true);
        if (!MCApiFactory.getMCApi().isInit()) {
            IActiveJsb activeJsb = new UnityActiveJsb();
            MCApiFactory.getMCApi().init(context, config, activeJsb, debug);
        }
    }

    // 启动登录
    static void _login(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if(MCApiFactory.getMCApi().isLogin()){
            ToastUtil.show(context, "已登录");
            return;
        }

        //拉起登录
        IGPUserObsv userObsv = MCApiFactory.getMCApi().getLoginCallback();
        if (!MCApiFactory.getMCApi().isLogin()) {
            MCApiFactory.getMCApi().startLogin(context, userObsv);
        }
    }

    // 启动登出
    static void _logout(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();
        
        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        try {
            boolean silent = data.getBoolean("silent");
            MCApiFactory.getMCApi().loginOut(context, silent);
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    //购买
    static void _pay(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        MCLog.d(TAG, "_pay:"+data.toString());
        try {
            String productId = data.getString("body");      //productId
            String productName = data.getString("title");   //name
            String orderId = data.getString("order_sn");    //orderId
            int price = data.getInt("price");
            //设置道具参数，调用sdk支付界面（以下参数为必填参数，如果没有可传"0"）
            OrderInfo order = new OrderInfo();
            order.setProductName(productName);                  //游戏道具名称
            order.setProductDesc(productId);                    //游戏道具描述
            order.setAmount(price);                             //游戏道具价格（单位分）
            order.setServerName(ChannelAndGameInfo.getInstance().getGameName());                    //游戏区服名
            order.setGameServerId(ChannelAndGameInfo.getInstance().getGameId());                    //游戏区服ID
            order.setRoleName(PersonalCenterModel.getInstance().getAccount());                      //游戏角色名
            order.setRoleId(PersonalCenterModel.getInstance().getUserId());                         //游戏角色ID
            order.setRoleLevel(PersonalCenterModel.getInstance().getUserId());                      //游戏角色等级
            order.setExtra_param("extra_param");                //平台方的预留标识（默认值是平台域名，sdk用户登录成功时获取，不需改动） 
            order.setExtendInfo(orderId);                       //游戏方的透传参数，服务端支付回调时原样返回，建议传订单号（当前demo用系统时间模拟订单号，正式接入时请传订单号）
            MCApiFactory.getMCApi().pay(context, order);
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    //平台币充值
    static void _ptbpay(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        MCLog.d(TAG, "_ptbpay:"+data.toString());
        MCApiFactory.getMCApi().ptbpay(context/*, order*//*, sdkPayCallback*/);
    }

    static void _bill(JSONObject data){
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        MCLog.d(TAG, "_bill:"+data.toString());
        MCApiFactory.getMCApi().bill();
    }

    static void _chat(JSONObject data) {
        MCLog.d(TAG, "_chat:" + data.toString());
        boolean isInit = MCApiFactory.getMCApi().isInit();
        if (isInit) {
            MCApiFactory.getMCApi().chat();
        } else {
            // 参数判断
            if(data.length() <= 0) {
                return;
            }

            // 初始化（离线初始化）
            Activity context = UnityPlayer.currentActivity;
            IActiveJsb activeJsb = new UnityActiveJsb();
            MCApiFactory.getMCApi().init2(context, data, activeJsb, false);

            // 打开客服
            MCApiFactory.getMCApi().chat2();
        }
    }

    static void _user(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        MCLog.d(TAG, "_user:" + data.toString());
        MCApiFactory.getMCApi().user();
    }

    static void _uploadRole(JSONObject data) {
        Activity context = MCApiFactory.getMCApi().getContext();

        if(!MCApiFactory.getMCApi().isInit()){
            ToastUtil.show(context, "SDK未初始化");
            return;
        }

        if (!MCApiFactory.getMCApi().isLogin()) {
            ToastUtil.show(context, "未登录");
            return;
        }

        MCLog.d(TAG, "_uploadRole:" + data.toString());

        RoleInfo info = new RoleInfo();
        info.setServerId(data.optString("serverId",""));
        info.setServerName(data.optString("serverName",""));
        info.setRoleId(data.optString("roleId",""));
        info.setRoleName(data.optString("roleName",""));
        info.setRoleLevel(data.optString("roleLevel",""));
        info.setRoleCombat(data.optString("roleCombat",""));
        MCApiFactory.getMCApi().uploadRole(info, new UploadRoleCallBack() {
            @Override
            public void onUploadComplete(String result) {
                MCLog.d(TAG, "上传角色信息结果："+result);
            }
        });
    }

    // --埋点
    // 记录登录
    static void _logSetUserId(JSONObject data) {
        String userId = data.optString("userId");
        Tracker.getInstance().SetUserId(userId);
    }

    // 记录登出
    static void _logClearUser(JSONObject data) {
        Tracker.getInstance().ClearUser();
    }

    // 记录支付
    static void _logPurchasedEvent(JSONObject data) {
        String orderId = data.optString("orderId");
        String productName = data.optString("productName");
        double amount = data.optDouble("amount");
        String currencyType = data.optString("currencyType");
        String paymentChannel = data.optString("paymentChannel");
        JSONObject properties = data.optJSONObject("properties");
        Tracker.getInstance().LogPurchasedEvent(orderId, productName, amount, currencyType, paymentChannel, properties);
    }

    // -------------- 与JS交互
    static void Call(String name, JSONObject data) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    data.put("method",name);
                    String strData = data.toString();
                    MCLog.e(TAG, "[UnitySendMessage]" + strData);
                    UnityPlayer.UnitySendMessage("NativeSDK","OnCall",strData);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public static void OnCall(String name, String strData) {
        // 全部放到UI线程里去执行
        Activity context = UnityPlayer.currentActivity;
        context.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    JSONObject data = ToJson(strData);
                    switch (name) {
                        case "init":
                            _init(data);
                            break;
                        case "login":
                            _login(data);
                            break;
                        case "logout":
                            _logout(data);
                            break;
                        case "pay":
                            _pay(data);
                            break;
                        case "ptbpay":
                            _ptbpay(data);
                            break;
                        case "bill":
                            _bill(data);
                            break;
                        case "chat":
                            _chat(data);
                            break;
                        case "user":
                            _user(data);
                            break;
                        case "uploadRole":
                            _uploadRole(data);
                            break;
                        case "logSetUserId":
                            _logSetUserId(data);
                            break;
                        case "logClearUser":
                            _logClearUser(data);
                            break;
                        case "logPurchasedEvent":
                            _logPurchasedEvent(data);
                            break;
                    }
                } catch (Exception ex) {
                    MCLog.e(TAG, "[OnCall]错误" + ex.getStackTrace());
                }
            }
        });
    }

    static JSONObject ToJson(String strData) {
        JSONObject json;
        try {
            json = new JSONObject(strData);
        } catch (Exception ex) {
            json = new JSONObject();
            MCLog.e(TAG, "[ToJson]错误" + ex.getStackTrace());
        }
        return json;
    }
}