package com.external;

import com.mchsdk.extras.IActiveJsb;
import org.json.JSONObject;

public class UnityActiveJsb implements IActiveJsb {
    @Override
    public void initResult(boolean success, JSONObject data) {
        UnityNativeInterface.initResult(success, data);
    }

    @Override
    public void loginResult(boolean success, JSONObject data) {
        UnityNativeInterface.loginResult(success, data);
    }

    @Override
    public void logoutResult(boolean success, JSONObject data) {
        UnityNativeInterface.logoutResult(success, data);
    }

    @Override
    public void payResult(boolean success, JSONObject data) {
        UnityNativeInterface.payResult(success, data);
    }

    @Override
    public void userResult(boolean success, JSONObject data) {
        UnityNativeInterface.userResult(success, data);
    }

    @Override
    public void sdkResult(boolean success, JSONObject data) {
        UnityNativeInterface.sdkResult(success, data);
    }
}