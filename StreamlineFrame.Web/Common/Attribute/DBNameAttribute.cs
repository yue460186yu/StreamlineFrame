using System;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// 数据库真名特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DBNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public DBNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}