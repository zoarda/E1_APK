package com.external;

import android.app.Activity;

import com.mchsdk.extras.IActiveJsb;
import com.mchsdk.extras.Tracker;
import com.mchsdk.open.IGPUserObsv;
import com.mchsdk.open.MCApiFactory;
import com.mchsdk.open.OrderInfo;
import com.mchsdk.open.RoleInfo;
import com.mchsdk.open.ToastUtil;

import com.mchsdk.paysdk.bean.ChannelAndGameInfo;
import com.mchsdk.paysdk.bean.PersonalCenterModel;
import com.mchsdk.paysdk.utils.MCLog;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

public class UnityNativeInterface implements IActiveJsb {
    private static final String TAG = "UnityNativeInterface";

    private static UnityNativeInterface mInst;
    public static UnityNativeInterface getInstance() {
        if (mInst == null) {
            mInst = new UnityNativeInterface();
        }
        return mInst;
    }

    //----------------主动调用
    @Override
    public void initResult(boolean success, JSONObject data) {
        String name = "init:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void loginResult(boolean success, JSONObject data) {
        String name = "login:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void logoutResult(boolean success, JSONObject data) {
        String name = "logout:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void payResult(boolean success, JSONObject data) {
        String name = "pay:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void userResult(boolean success, JSONObject data) {
        String name = "user:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void uploadRoleResult(boolean success, JSONObject data) {
        String name = "uploadRole:" + (success ? "success" : "fail");
        call(name, data);
    }

    @Override
    public void sdkResult(boolean success, JSONObject data) {
        String name = "sdk:" + (success ? "success" : "fail");
        call(name, data);
    }

    //----------------被动调用
    // 初始化
    void _init(JSONObject data) {
        // 使用Unity的Activity
        Activity context = UnityPlayer.currentActivity;
        JSONObject config = data.optJSONObject("config");
        boolean debug = data.optBoolean("debug", false);
        if (!MCApiFactory.getMCApi().isInit()) {
            MCApiFactory.getMCApi().init(context, config, this, debug);
        }
    }

    // 登录
    void _login(JSONObject data) {
        MCLog.d(TAG, "_login:"+data.toString());
        Activity context = MCApiFactory.getMCApi().getContext();
        IGPUserObsv userObsv = MCApiFactory.getMCApi().getLoginCallback();
        MCApiFactory.getMCApi().startLogin(context, userObsv);
    }

    // 登出
    void _logout(JSONObject data) {
        MCLog.d(TAG, "_logout:"+data.toString());
        Activity context = MCApiFactory.getMCApi().getContext();
        boolean silent = data.optBoolean("silent", false);
        MCApiFactory.getMCApi().loginOutWithReason(context, silent, "[logout]调用接口退出");
    }

    // 内购
    void _pay(JSONObject data) {
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
            MCApiFactory.getMCApi().pay(order);
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    // 充值
    void _ptbpay(JSONObject data) {
        MCLog.d(TAG, "_ptbpay:"+data.toString());
        MCApiFactory.getMCApi().ptbpay(/*, order*//*, sdkPayCallback*/);
    }

    // 账单
    void _bill(JSONObject data){
        MCLog.d(TAG, "_bill:"+data.toString());
        MCApiFactory.getMCApi().bill();
    }

    // 客服
    void _chat(JSONObject data) {
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
            MCApiFactory.getMCApi().init2(context, data, this, false);
            MCApiFactory.getMCApi().chat2();
        }
    }

    // 获取余额
    void _user(JSONObject data) {
        MCLog.d(TAG, "_user:" + data.toString());
        MCApiFactory.getMCApi().user();
    }

    // 上报角色信息
    void _uploadRole(JSONObject data) {
        MCLog.d(TAG, "_uploadRole:" + data.toString());
        RoleInfo info = new RoleInfo();
        info.setServerId(data.optString("serverId",""));
        info.setServerName(data.optString("serverName",""));
        info.setRoleId(data.optString("roleId",""));
        info.setRoleName(data.optString("roleName",""));
        info.setRoleLevel(data.optString("roleLevel",""));
        info.setRoleCombat(data.optString("roleCombat",""));
        MCApiFactory.getMCApi().uploadRole(info);
    }

    // --埋点
    // 记录登录
    void _logSetUserId(JSONObject data) {
        String userId = data.optString("userId");
        Tracker.getInstance().SetUserId(userId);
    }

    // 记录登出
    void _logClearUser(JSONObject data) {
        Tracker.getInstance().ClearUser();
    }

    // 记录支付
    void _logPurchasedEvent(JSONObject data) {
        String orderId = data.optString("orderId");
        String productName = data.optString("productName");
        double amount = data.optDouble("amount");
        String currencyType = data.optString("currencyType");
        String paymentChannel = data.optString("paymentChannel");
        JSONObject properties = data.optJSONObject("properties");
        Tracker.getInstance().LogPurchasedEvent(orderId, productName, amount, currencyType, paymentChannel, properties);
    }

    // -------------- 与JS交互
    static void call(String name, JSONObject data) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    data.put("method",name);
                    String strData = data.toString();
                    UnityPlayer.UnitySendMessage("NativeSDK","onCall",strData);
                    MCLog.e(TAG, "[UnityNativeInterface::call]" + strData);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public static void onCall(String name, String strData) {
        // 全部放到UI线程里去执行
        Activity context = UnityPlayer.currentActivity;
        UnityNativeInterface inst = UnityNativeInterface.getInstance();
        context.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    JSONObject data = ToJson(strData);
                    switch (name) {
                        case "init":
                            inst._init(data);
                            break;
                        case "login":
                            inst._login(data);
                            break;
                        case "logout":
                            inst._logout(data);
                            break;
                        case "pay":
                            inst._pay(data);
                            break;
                        case "ptbpay":
                            inst._ptbpay(data);
                            break;
                        case "bill":
                            inst._bill(data);
                            break;
                        case "chat":
                            inst._chat(data);
                            break;
                        case "user":
                            inst._user(data);
                            break;
                        case "uploadRole":
                            inst._uploadRole(data);
                            break;
                        case "logSetUserId":
                            inst._logSetUserId(data);
                            break;
                        case "logClearUser":
                            inst._logClearUser(data);
                            break;
                        case "logPurchasedEvent":
                            inst._logPurchasedEvent(data);
                            break;
                    }
                } catch (Exception ex) {
                    MCLog.e(TAG, "[onCall]错误" + ex.getStackTrace());
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