using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageService.ActionFilters
{
    public class JsonWebToken
    {
        private static Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>()
    {
      {
        JwtHashAlgorithm.HS256,
        (Func<byte[], byte[], byte[]>) ((key, value) =>
        {
          using (HMACSHA256 hmacshA256 = new HMACSHA256(key))
            return hmacshA256.ComputeHash(value);
        })
      },
      {
        JwtHashAlgorithm.HS384,
        (Func<byte[], byte[], byte[]>) ((key, value) =>
        {
          using (HMACSHA384 hmacshA384 = new HMACSHA384(key))
            return hmacshA384.ComputeHash(value);
        })
      },
      {
        JwtHashAlgorithm.HS512,
        (Func<byte[], byte[], byte[]>) ((key, value) =>
        {
          using (HMACSHA512 hmacshA512 = new HMACSHA512(key))
            return hmacshA512.ComputeHash(value);
        })
      }
    };

        public static string Encode(object payload, string key, JwtHashAlgorithm algorithm)
        {
            List<string> stringList = new List<string>();
            var data = new
            {
                typ = "JWT",
                alg = algorithm.ToString()
            };
            byte[] bytes1 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)data, Formatting.None));
            byte[] bytes2 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));
            stringList.Add(JsonWebToken.Base64UrlEncode(bytes1));
            stringList.Add(JsonWebToken.Base64UrlEncode(bytes2));
            byte[] bytes3 = Encoding.UTF8.GetBytes(string.Join(".", stringList.ToArray()));
            byte[] bytes4 = Encoding.UTF8.GetBytes(key);
            byte[] input = JsonWebToken.HashAlgorithms[algorithm](bytes4, bytes3);
            stringList.Add(JsonWebToken.Base64UrlEncode(input));
            return string.Join(".", stringList.ToArray());
        }

        public static string Decode(string token, string key)
        {
            return JsonWebToken.Decode(token, key, true);
        }

        public static string Decode(string token, string key, bool verify)
        {
            string[] strArray = token.Split('.');
            string input1 = strArray[0];
            string input2 = strArray[1];
            byte[] inArray1 = JsonWebToken.Base64UrlDecode(strArray[2]);
            JObject jobject1 = JObject.Parse(Encoding.UTF8.GetString(JsonWebToken.Base64UrlDecode(input1)));
            JObject jobject2 = JObject.Parse(Encoding.UTF8.GetString(JsonWebToken.Base64UrlDecode(input2)));
            if (verify)
            {
                byte[] bytes1 = Encoding.UTF8.GetBytes(input1 + "." + input2);
                byte[] bytes2 = Encoding.UTF8.GetBytes(key);
                string algorithm = (string)jobject1["alg"];
                byte[] inArray2 = JsonWebToken.HashAlgorithms[JsonWebToken.GetHashAlgorithm(algorithm)](bytes2, bytes1);
                if (Convert.ToBase64String(inArray1) != Convert.ToBase64String(inArray2))
                    throw new ApplicationException(string.Format("Invalid signature."));
            }
            return jobject2.ToString();
        }

        private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
        {
            if (algorithm == "HS256")
                return JwtHashAlgorithm.HS256;
            if (algorithm == "HS384")
                return JwtHashAlgorithm.HS384;
            if (algorithm == "HS512")
                return JwtHashAlgorithm.HS512;
            throw new InvalidOperationException("Algorithm not supported.");
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).Split('=')[0].Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 0:
                    return Convert.FromBase64String(s);
                case 2:
                    s += "==";
                    goto case 0;
                case 3:
                    s += "=";
                    goto case 0;
                default:
                    throw new Exception("Illegal base64url string!");
            }
        }
    }

    public enum JwtHashAlgorithm
    {
        HS256,
        HS384,
        HS512,
    }
}
