using System;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// 数据库真名特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DBKeyAttribute : Attribute
    {
        public bool IsKey { get; private set; } = true;

        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsSelfIncreasing { get; private set; }

        public DBKeyAttribute()
            : this(true)
        {
        }

        public DBKeyAttribute(bool isSelfIncreasing)
        {
            this.IsSelfIncreasing = isSelfIncreasing;
        }
    }
}