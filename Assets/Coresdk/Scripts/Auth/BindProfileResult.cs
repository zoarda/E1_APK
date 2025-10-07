using UnityEngine;

using ProfileData = Games.Coresdk.Unity.ProfileResult.ProfileData;

namespace Games.Coresdk.Unity
{
    public class BindProfileResult
    {
        public System.Exception Exception { get; private set; }

        public ProfileData Data { get; private set; }

        public bool IsSuccess { get; private set; }

        public static BindProfileResult Parse(ProfileResult profileResult, bool bindResult)
        {
            var ret = new BindProfileResult();
            ret.Data = profileResult.Data;
            ret.Exception = profileResult.Exception;
            ret.IsSuccess = bindResult && profileResult.IsSuccess;
            return ret;
        }
    }
}
