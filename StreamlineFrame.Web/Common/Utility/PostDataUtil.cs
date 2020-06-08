using System;
using System.IO;
using System.Net;
using System.Text;

namespace StreamlineFrame.Web.Common
{
    public class PostDataUtil
    {
        /// <summary>
        /// 创建HttpWebRequest对象(POST方式)
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="byteArray">发送的字节数组</param>
        /// <returns>HttpWebRequest对象</returns>
        private static HttpWebRequest CreatePostWebRequest(string url, Byte[] byteArray, int timeout = 1000 * 30)
        {
            //构造HttpWebRequest对象
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            //设置HttpWebRequest对象的属性
            request.Method = "POST";
            request.Timeout = timeout;
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 1000;

            //发出请求，并将数据发送到远程服务器
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            return request;
        }

        /// <summary>
        /// 创建HttpWebRequest对象(GET方式)
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="getData">请求的数据</param>
        /// <returns>HttpWebRequest对象</returns>
        private static HttpWebRequest CreateGetWebRequest(String url, String getData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (getData == "" ? "" : "?") + getData);

            //设置HttpWebRequest对象的属性
            request.Method = "GET";
            request.Timeout = 1000 * 30;
            request.ContentType = "text/html;charset=UTF-8";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 1000;

            return request;
        }

        /// <summary>
        /// Post数据
        /// </summary>
        /// <param name="url">目的地的Url</param>
        /// <param name="postData">发送的数据</param>
        /// <param name="compress">数据压缩枚举</param>
        /// <exception cref="ArgumentNullException">Thrown when url or postData is null or empty</exception>
        /// <exception cref="System.Text.EncoderFallbackException">System.Text.EncoderFallbackException</exception>
        /// <returns>从页面返回的数据</returns>
        public static string PostWebData(String url, String postData)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("Empty String", "url can't be null or empty.");
            if (string.IsNullOrEmpty(postData)) throw new ArgumentNullException("Empty String", "postData can't be null or empty.");

            //将字符串转化为Byte[]
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            try
            {
                //构造HttpWebRequest对象
                HttpWebRequest request = CreatePostWebRequest(url, byteArray);

                //获得请求的回应
                using (WebResponse response = request.GetResponse())
                {
                    //构造流读取对象
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        //返回获得的请求数据
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Post数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentTyup"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string PostWebData(String url, String postData, string contentType = "application/json", int timeout = 1000 * 30)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("Empty String", "url can't be null or empty.");
            if (string.IsNullOrEmpty(postData)) throw new ArgumentNullException("Empty String", "postData can't be null or empty.");

            //将字符串转化为Byte[]
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            try
            {
                //构造HttpWebRequest对象
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

                //设置HttpWebRequest对象的属性
                request.Method = "POST";
                request.Timeout = timeout;
                request.ContentLength = byteArray.Length;
                request.ContentType = contentType;
                request.ProtocolVersion = HttpVersion.Version11;
                request.ServicePoint.ConnectionLimit = 1000;

                //发出请求，并将数据发送到远程服务器
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                //获得请求的回应
                using (WebResponse response = request.GetResponse())
                {
                    //构造流读取对象
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        //返回获得的请求数据
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Post数据
        /// </summary>
        /// <param name="url">目的地的Url</param>
        /// <param name="postData">发送的数据</param>
        /// <param name="compress">数据压缩枚举</param>
        /// <exception cref="ArgumentNullException">Thrown when url or postData is null or empty</exception>
        /// <exception cref="System.Text.EncoderFallbackException">System.Text.EncoderFallbackException</exception>
        /// <returns>从页面返回的数据</returns>
        public static string PostWebData(String url, String postData, int timeout)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("Empty String", "url can't be null or empty.");
            if (string.IsNullOrEmpty(postData)) throw new ArgumentNullException("Empty String", "postData can't be null or empty.");

            //将字符串转化为Byte[]
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            try
            {
                //构造HttpWebRequest对象
                HttpWebRequest request = CreatePostWebRequest(url, byteArray, timeout);

                //获得请求的回应
                using (WebResponse response = request.GetResponse())
                {
                    //构造流读取对象
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        //返回获得的请求数据
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 通过GET方式提交数据到url
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="getData">请求的参数</param>
        /// <param name="compress">数据压缩枚举</param>
        /// <exception cref="ArgumentNullException">Thrown when url or postData is null or empty</exception>
        /// <exception cref="System.Text.EncoderFallbackException">System.Text.EncoderFallbackException</exception>
        /// <returns>从页面返回的数据</returns>
        public static string GetWebData(string url, string getData)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("Empty String", "url can't be null or empty.");

            try
            {
                //构造HttpWebRequest对象
                HttpWebRequest request = CreateGetWebRequest(url, getData);

                //获得请求的回应
                using (WebResponse response = request.GetResponse())
                {
                    //构造流读取对象
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        //返回获得的请求数据
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
