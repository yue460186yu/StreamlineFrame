using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace StreamlineFrame.Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            // 配置返回的时间类型数据格式
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(
                new Newtonsoft.Json.Converters.IsoDateTimeConverter()
                {
                    DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
                }
            );
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// 跨域设置
        /// </summary>
        protected void Application_BeginRequest()
        {
            //OPTIONS请求方法的主要作用：
            //1、获取服务器支持的HTTP请求方法；也是黑客经常使用的方法。
            //2、用来检查服务器的性能。如：AJAX进行跨域请求时的预检，需要向另外一个域名的资源发送一个HTTP OPTIONS请求头，用以判断实际发送的请求是否安全。
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                //表示对输出的内容进行缓冲，执行page.Response.Flush()时，会等所有内容缓冲完毕，将内容发送到客户端。
                //这样就不会出错，造成页面卡死状态，让用户无限制的等下去
                Response.StatusCode = 200;
                Response.SubStatusCode = 200;
                Response.Flush();
            }
        }
    }
}