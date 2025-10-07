using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public class CheckBindGameStatusResult
    {
        public bool IsSuccess { get; private set; }

        public string Result { get; private set; }

        public bool IsBound { get; private set; }

        public static CheckBindGameStatusResult Parse(RawResponse rawResponse)
        {
            var ret = new CheckBindGameStatusResult();
            if (rawResponse.Exception != null)
            {
                ret.Result = "-1";
                return ret;
            }

            var json = JSON.Parse(rawResponse.Data);
            ret.Result = json["result"].Value;
            ret.IsBound = json["is_bound"].AsBool;
            ret.IsSuccess = ret.Result == "0000";

            return ret;
        }
    }
}
