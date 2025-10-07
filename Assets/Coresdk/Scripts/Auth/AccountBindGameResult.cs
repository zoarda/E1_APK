
namespace Games.Coresdk.Unity
{
    public class AccountBindGameResult
    {
        public bool IsSuccess { get; private set; }

        public string Result { get; private set; }

        public string Reason { get; private set; }

        public static AccountBindGameResult Parse(string url)
        {
            return new AccountBindGameResult()
            {
                IsSuccess = true,
                Result = "0000",
                Reason = ""
            };
        }

        public static AccountBindGameResult Parse(RawResponse rawResponse)
        {
            var ret = new AccountBindGameResult();
            if (rawResponse.Exception != null)
            {
                ret.Result = "-1";
                ret.Reason = rawResponse.Exception.Message;
                return ret;
            }

            var json = JSON.Parse(rawResponse.Data);
            ret.Result = json["result"].Value;
            ret.Reason = json["reason"].Value;
            ret.IsSuccess = ret.Result == "0000";

            return ret;
        }
    }
}
