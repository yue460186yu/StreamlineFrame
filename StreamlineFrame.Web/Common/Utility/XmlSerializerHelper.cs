using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// Xml序列化助手。
    /// </summary>
    public static class XmlSerializerHelper
    {
        /// <summary>
        /// 转换对象为Xml字符串。
        /// </summary>
        /// <param name="obj">需要序列化的对象。</param>
        /// <returns>返回Xml字符串。</returns>
        public static string ConvertObjectToXmlString(object obj)
        {
            var xmlString = new StringBuilder();
            var serializer = new XmlSerializer(obj.GetType());
            using (TextWriter writer = new StringWriter(xmlString))
            {
                serializer.Serialize(writer, obj);
            }

            return xmlString.ToString();
        }

        /// <summary>
        /// 转换对象为Xml字符串。
        /// </summary>
        /// <param name="obj">需要序列化的对象。</param>
        /// <param name="type">对象类型。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <returns>返回Xml字符串。</returns>
        public static string ConvertObjectToXmlString(object obj, Type type, XmlSerializerNamespaces nameSpace = null)
        {
            var xmlString = new StringBuilder();
            var serializer = (type == null) ? new XmlSerializer(obj.GetType()) : new XmlSerializer(type);
            using (TextWriter writer = new StringWriter(xmlString))
            {
                if (nameSpace == null)
                {
                    serializer.Serialize(writer, obj);
                }
                else
                {
                    serializer.Serialize(writer, obj, nameSpace);
                }
            }

            return xmlString.ToString();
        }

        /// <summary>
        /// 转换对象为Xml纯节点字符串。
        /// </summary>
        /// <param name="obj">需要序列化的对象。</param>
        /// <returns>返回Xml字符串。</returns>
        public static string ConvertObjectToXmlNodeString(object obj)
        {
            var xmlString = new StringBuilder();
            var serializer = new XmlSerializer(obj.GetType());
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            using (var stream = new StringWriter(xmlString))
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, obj, emptyNamespaces);
                }
            }

            return xmlString.ToString();
        }

        /// <summary>
        /// 转换Xml字符串为对象。
        /// </summary>
        /// <param name="xmlString">xml字符串。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>返回对象实例。</returns>
        public static object ConvertXmlStringToObject(string xmlString, Type type)
        {
            using (var stringReader = new StringReader(xmlString))
            {
                var xmlSerializer = new XmlSerializer(type);
                return xmlSerializer.Deserialize(stringReader);
            }
        }
    }
}