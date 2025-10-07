using System;
using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class LoginResult
    {
        public string Token { get; private set; }

        public Exception Exception { get; private set; }

        public static LoginResult Parse(RawResponse rawResponse)
        {
            var ret = new LoginResult();
            if (rawResponse.Exception != null)
            {
                ret.Exception = rawResponse.Exception;
                return ret;
            }

            try
            {
                var json = JSON.Parse(rawResponse.Data);
                ret.Token = json["jwt"].Value;
                return ret;
            }
            catch (Exception ex)
            {
                ret.Exception = ex;
                return ret;
            }
        }
    }
}
