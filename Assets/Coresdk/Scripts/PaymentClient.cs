using UnityEngine;
using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class PaymentClient
    {
        AuthClient m_authClient;
        Config m_config;
        MainThreadDispatcher m_mainThreadDispatcher;
        Coresdk m_sdk;

        public PaymentClient(Coresdk sdk, AuthClient authClient, Config config, MainThreadDispatcher mainThreadDispatcher)
        {
            m_sdk = sdk;
            m_authClient = authClient;
            m_config = config;
            m_mainThreadDispatcher = mainThreadDispatcher;
        }

        public void Purchase()
        {
            const string endpoint = "/payment";

            var query = new Dictionary<string, string>()
            {
                { "jwt", m_authClient.Token },
                { "lang",  m_sdk.language }
            };

            var paymentUrl = string.Format("{0}/{1}?{2}", m_config.PaymentDomain, endpoint, APIUtil.GetQueryString(query));

            Application.OpenURL(paymentUrl);
        }
    }
}

