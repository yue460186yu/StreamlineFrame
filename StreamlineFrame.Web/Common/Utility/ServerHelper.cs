using System;
using System.Web;
using System.Web.Hosting;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// 服务器帮助类。
    /// </summary>
    public static class ServerHelper
    {
        /// <summary>
        /// 使用HttpContext返回与指定虚拟路径相对应的物理路径。
        /// </summary>
        /// <param name="path">Web 应用程序中的虚拟路径。</param>
        /// <returns>对应于 path 的 Web 服务器上的物理文件路径。</returns>
        public static string HttpContextMapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        /// <summary>
        /// 使用HostingEnvironment映射路径为以内容根为基准的物理路径。
        /// </summary>
        /// <param name="path">路径。</param>
        /// <returns>以内容根为基准的物理路径。</returns>
        public static string HostingMapPath(string path)
        {
            return HostingEnvironment.MapPath(path);
        }

        /// <summary>
        /// 获取当前应用程序域所在的基目录。
        /// </summary>
        /// <returns>基目录。</returns>
        public static string GetAppDomainBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}