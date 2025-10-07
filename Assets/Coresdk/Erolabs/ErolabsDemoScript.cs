using UnityEngine;
using System.Collections;
using Erolabs.Sdk.Unity;

public class ErolabsDemoScript : MonoBehaviour
{
    string game_id = "1";
    string game_account = "game_001";
    CoresdkDemoUI ui;

    private IEnumerator Start()
    {
        ui = GetComponent<CoresdkDemoUI>();

        ui.loginButton.onClick.AddListener(Login);
        ui.logoutButton.onClick.AddListener(Logout);
        ui.bindButton.onClick.AddListener(Bind);
        ui.purchaseButton.onClick.AddListener(Purchase);
        ui.postBindButton.onClick.AddListener(PostBind);

        yield return StartCoroutine(ErolabsSDK.Initialize(_ => Debug.Log("is initialized:" + _)));
    }

    private void UpdateView(string text)
    {
        ui.viewText.text = text;
    }

    private void Login()
    {
        ErolabsSDK.OpenLogin(game_id, OnLoginCallback);
    }

    private void OnLoginCallback(Games.Coresdk.Unity.ProfileResult result)
    {
        System.Exception exception = result.Exception;

        if (exception == null)
        {
            UpdateView("user_id:\n" + result.Data.user_info.user_id);
        }
        else
        {
            UpdateView("Exception:\n" + exception.ToString());
        }
    }

    private void Logout()
    {
        ErolabsSDK.OpenLogout(OnLogoutCallback);
    }

    private void Bind()
    {
        ErolabsSDK.OpenAccountBindGame(game_id, game_account, OnAccountLoginBindGameCallback);
    }

    private void Purchase()
    {
        ErolabsSDK.OpenPayment();
    }

    private void PostBind()
    {
        ErolabsSDK.PostAccountBindGame(game_id, game_account, OnAccountBindGameCallback);
    }

    private void OnLogoutCallback(Games.Coresdk.Unity.LogoutResult result)
    {
        System.Exception exception = result.Exception;

        if (exception == null)
        {
            UpdateView("logout");
        }
        else
        {
            UpdateView("Exception:\n" + exception.ToString());
        }
    }

    private void OnAccountLoginBindGameCallback(Games.Coresdk.Unity.BindProfileResult result)
    {
        System.Exception exception = result.Exception;

        if (exception == null)
        {
            UpdateView(string.Format("bind result:{0}\nuser info:\naccount:{1}", result.IsSuccess, result.Data.user_info.account));
        }
        else
        {
            UpdateView("Exception:\n" + exception.ToString());
        }
    }

    private void OnAccountBindGameCallback(Games.Coresdk.Unity.AccountBindGameResult result)
    {
        UpdateView(string.Format("bind result:{0}\nreason:{1}", result.Result, result.Reason));
    }
}

