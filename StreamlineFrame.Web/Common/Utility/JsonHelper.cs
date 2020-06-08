using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// JSON序列化帮助类。
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 获取筛选后的JsonResult对象。
        /// </summary>
        /// <param name="value">string集合。</param>
        /// <param name="term">string集合的筛选条件。</param>
        /// <param name="count">控制返回集合的长度。</param>
        /// <returns>JsonResult对象。</returns>
        public static JsonResult GetAutoCompleteJsonAfterScreening(IEnumerable<string> value, string term, int count)
        {
            var result = new JsonResult { Data = JsonConvert.SerializeObject(value.Where(r => r.Contains(term)).Take(count)), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return result;
        }

        /// <summary>
        /// 序列化object对象。
        /// </summary>
        /// <param name="value">序列化的值。</param>
        /// <param name="isWriteDisplayName">是否输出DisplayName属性的值。</param>
        /// <returns>json字符串。</returns>
        public static string SerializeObject(object value, bool isWriteDisplayName = false)
        {
            return SerializeObject(value, null, null, isWriteDisplayName);
        }

        /// <summary>
        /// 序列化包含指定属性的值。
        /// </summary>
        /// <param name="value">序列化的值。</param>
        /// <param name="include">允许序列化的属性名称数组。</param>
        /// <param name="isWriteDisplayName">是否输出DisplayName属性的值。</param>
        /// <returns>json字符串。</returns>
        public static string SerializeIncludeProperty(object value, string[] include, bool isWriteDisplayName = false)
        {
            return SerializeObject(value, include, null, isWriteDisplayName);
        }

        /// <summary>
        /// 序列化不包含指定属性的值。
        /// </summary>
        /// <param name="value">序列化的值。</param>
        /// <param name="exclude">不允许序列化的属性名称数组。</param>
        /// <param name="isWriteDisplayName">是否输出DisplayName属性的值。</param>
        /// <returns>json字符串。</returns>
        public static string SerializeExcludeProperty(object value, string[] exclude, bool isWriteDisplayName = false)
        {
            return SerializeObject(value, null, exclude, isWriteDisplayName);
        }

        /// <summary>
        /// 序列化不包含指定属性的值。
        /// </summary>
        /// <param name="value">序列化的值。</param>
        /// <param name="include">允许序列化的属性名称数组。</param>
        /// <param name="exclude">不允许序列化的属性名称数组。</param>
        /// <param name="isWriteDisplayName">是否输出DisplayName属性的值。</param>
        /// <returns>json字符串。</returns>
        public static string SerializeObject(object value, string[] include, string[] exclude, bool isWriteDisplayName = false)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd HH:mm",
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include
                });
        }
    }
}