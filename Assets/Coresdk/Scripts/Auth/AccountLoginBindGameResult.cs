using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class AccountLoginBindGameResult
    {
        public bool IsSuccess { get; private set; }

        public string Result { get; private set; }

        public string Reason { get; private set; }

        public string Token { get; private set; }

        public static AccountLoginBindGameResult Parse(string url)
        {
            return new AccountLoginBindGameResult()
            {
                IsSuccess = true,
                Result = "0000",
                Reason = ""
            };
        }

        public static AccountLoginBindGameResult Parse(RawResponse rawResponse)
        {
            var ret = new AccountLoginBindGameResult();
            if (rawResponse.Exception != null)
            {
                ret.Result = "-1";
                ret.Reason = rawResponse.Exception.Message;
                return ret;
            }

            var json = JSON.Parse(rawResponse.Data);
            ret.Result = json["result"].Value;
            ret.Reason = json["reason"].Value;
            ret.Token = json["jwt"].Value;
            ret.IsSuccess = ret.Result == "0000";

            return ret;
        }
    }
}
