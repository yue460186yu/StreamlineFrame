using System;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// IP操作方法类。
    /// </summary>
    public static class IPHelper
    {
        /// <summary>
        /// 获取当前主机的IP地址。
        /// </summary>
        /// <returns>主机IP地址。</returns>
        public static IPAddress GetHostIP()
        {
            return GetHostIP(string.Empty);
        }

        /// <summary>
        /// 获取web客户端ip
        /// </summary>
        /// <returns></returns>
        public static string GetWebClientIp()
        {
            var userIP = "未获取用户IP";
            try
            {  
                var CustomerIP = string.Empty;
                if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.ServerVariables == null)
                    return CustomerIP;

                //CDN加速后取到的IP simone 090805
                CustomerIP = HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;

                CustomerIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;

                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    CustomerIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (CustomerIP == null)
                    {
                        CustomerIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
                else
                    CustomerIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                if (string.Compare(CustomerIP, "unknown", true) == 0 || String.IsNullOrEmpty(CustomerIP))
                    return HttpContext.Current.Request.UserHostAddress;
                
                return CustomerIP;
            }
            catch { }

            return userIP;

        }

        /// <summary>
        /// 获取指定主机的IP地址。
        /// </summary>
        /// <param name="hostName">要解析的主机名。</param>
        /// <returns>主机IP地址。</returns>
        public static IPAddress GetHostIP(string hostName)
        {
            var addresses = Dns.GetHostAddresses(hostName);
            var ipv4Address = IPAddress.None;
            var ipv6Address = IPAddress.None;
            foreach (var ip in addresses)
            {
                // 优先获取ipv4
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipv4Address = ip;

                if (ip.AddressFamily == AddressFamily.InterNetworkV6 && Equals(ipv6Address, IPAddress.None))
                    ipv6Address = ip;
            }

            return !Equals(ipv4Address, IPAddress.None) ? ipv4Address : ipv6Address;
        }
    }
}