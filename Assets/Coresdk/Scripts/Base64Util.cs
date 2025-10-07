using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Games.Coresdk.Unity
{
    public sealed class Base64Util
    {
        public static string ToBase64(string value)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(value);

            //編成 Base64 字串
            return System.Convert.ToBase64String(bytes);
        }
    }

}