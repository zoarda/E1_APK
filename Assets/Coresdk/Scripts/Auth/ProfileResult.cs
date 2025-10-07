using UnityEngine;

namespace Games.Coresdk.Unity
{
    public class ProfileResult
    {
        [System.Serializable]
        public class ProfileData
        {
            public string result;

            public string server_time;

            public user_info user_info;
        }

        [System.Serializable]
        public class user_info
        {
            public string birthday;
            public string country;
            public string free_coins;
            public string gender;
            public string user_id;
            public string hobbies;
            public string coins;
            public string nickname;
            public string account_status;
            public string account;
        }

        public System.Exception Exception { get; private set; }

        public ProfileData Data { get; private set; }

        public bool IsSuccess { get; private set; }

        public static ProfileResult Parse(RawResponse rawResponse)
        {
            var ret = new ProfileResult();
            if (rawResponse.Exception != null)
            {
                ret.Exception = rawResponse.Exception;
                return ret;
            }

            var data = JsonUtility.FromJson<ProfileData>(rawResponse.Data);
            ret.Data = data;
            ret.IsSuccess = ret.Exception == null && data != null && data.result != null && data.result == "0000";
            return ret;
        }
    }
}
