﻿namespace ServiceLib.Handler.Fmt
{
    public class SocksFmt : BaseFmt
    {
        public static ProfileItem? Resolve(string str, out string msg)
        {
            msg = ResUI.ConfigurationFormatIncorrect;
            ProfileItem? item;

            item = ResolveSocksNew(str) ?? ResolveSocks(str);
            if (item == null)
            {
                return null;
            }
            if (item.Address.Length == 0 || item.Port == 0)
            {
                return null;
            }

            item.ConfigType = EConfigType.SOCKS;

            return item;
        }

        public static string? ToUri(ProfileItem? item)
        {
            if (item == null) return null;
            string url = string.Empty;

            string remark = string.Empty;
            if (Utils.IsNotEmpty(item.Remarks))
            {
                remark = "#" + Utils.UrlEncode(item.Remarks);
            }
            //url = string.Format("{0}:{1}@{2}:{3}",
            //    item.security,
            //    item.id,
            //    item.address,
            //    item.port);
            //url = Utile.Base64Encode(url);
            //new
            var pw = Utils.Base64Encode($"{item.Security}:{item.Id}");
            return ToUri(EConfigType.SOCKS, item.Address, item.Port, pw, null, remark);
        }

        private static ProfileItem? ResolveSocks(string result)
        {
            ProfileItem item = new()
            {
                ConfigType = EConfigType.SOCKS
            };
            result = result[Global.ProtocolShares[EConfigType.SOCKS].Length..];
            //remark
            int indexRemark = result.IndexOf("#");
            if (indexRemark > 0)
            {
                try
                {
                    item.Remarks = Utils.UrlDecode(result.Substring(indexRemark + 1, result.Length - indexRemark - 1));
                }
                catch { }
                result = result[..indexRemark];
            }
            //part decode
            int indexS = result.IndexOf("@");
            if (indexS > 0)
            {
            }
            else
            {
                result = Utils.Base64Decode(result);
            }

            string[] arr1 = result.Split('@');
            if (arr1.Length != 2)
            {
                return null;
            }
            string[] arr21 = arr1[0].Split(':');
            //string[] arr22 = arr1[1].Split(':');
            int indexPort = arr1[1].LastIndexOf(":");
            if (arr21.Length != 2 || indexPort < 0)
            {
                return null;
            }
            item.Address = arr1[1][..indexPort];
            item.Port = Utils.ToInt(arr1[1][(indexPort + 1)..]);
            item.Security = arr21[0];
            item.Id = arr21[1];

            return item;
        }

        private static ProfileItem? ResolveSocksNew(string result)
        {
            var parsedUrl = Utils.TryUri(result);
            if (parsedUrl == null) return null;

            ProfileItem item = new()
            {
                Remarks = parsedUrl.GetComponents(UriComponents.Fragment, UriFormat.Unescaped),
                Address = parsedUrl.IdnHost,
                Port = parsedUrl.Port,
            };

            // parse base64 UserInfo
            var rawUserInfo = Utils.UrlDecode(parsedUrl.UserInfo);
            var userInfo = Utils.Base64Decode(rawUserInfo);
            var userInfoParts = userInfo.Split(new[] { ':' }, 2);
            if (userInfoParts.Length == 2)
            {
                item.Security = userInfoParts[0];
                item.Id = userInfoParts[1];
            }

            return item;
        }
    }
}