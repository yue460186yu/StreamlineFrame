using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;

namespace StreamlineFrame.Web.Common
{
    /// <summary>
    /// 通用数据库仓储
    /// </summary>
    /// <typeparam name="TModel">实体类型</typeparam>
    public class BaseRepository<TModel>
         where TModel : class, new()
    {
        //链接字符串
        private readonly string ConnectionString;

        public BaseRepository(string db)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[db].ConnectionString;
        }

        #region r

        /// <summary>
        /// 包涵实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>是否包涵</returns>
        public bool Has(string sql, SqlParameter[] para)
        {
            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, para))
                return reader.HasRows;
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns></returns>
        public TModel Get(Expression<Func<TModel, bool>> exp)
        {
            //todo:解析exp转sql
            var sql = $"SELECT * FROM {this.GetDBName(typeof(TModel))} WHERE ";
            sql += ExpressionHelper.ExpressionToSql(exp.Body);
            return this.Get(sql, null);
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns></returns>
        public TModel Get(string sql, SqlParameter[] para)
        {
            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, para))
            {
                if (reader.Read())
                    return this.ModelFactory(reader);

                return null;
            }
        }

        /// <summary>
        /// 获取某字段
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        public string GetField(string sql, SqlParameter[] para, string name)
        {
            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, para))
                return reader.Read() ? reader[name].ToString() : null;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>该实体数据集合</returns>
        public IEnumerable<TModel> GetAll(string sql = null, SqlParameter[] para = null)
        {
            var list = new List<TModel>();
            if (string.IsNullOrWhiteSpace(sql))
                sql = $"select * from {this.GetDBName(typeof(TModel))}";

            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, null))
            {
                if (reader.HasRows)
                {
                    TModel model;
                    while (reader.Read())
                    {
                        model = this.ModelFactory(reader);
                        list.Add(model);
                    }

                    return list;
                }

                return null;
            }
        }

        /// <summary>
        /// 实体工厂(可重写)
        /// </summary>
        /// <param name="reader">数据集</param>
        public virtual TModel ModelFactory(SqlDataReader reader)
        {
            var model = new TModel();
            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
                if (reader[this.GetDBName(property)] != DBNull.Value)
                    property.SetValue(model, reader[this.GetDBName(property)], property.GetIndexParameters());

            return model;
        }

        #endregion

        #region c

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Insert(IEnumerable<TModel> list)
        {
            var count = 0;
            foreach (var model in list)
                count += this.Insert(model);

            return count;
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Insert(TModel model)
        {
            var para = new List<SqlParameter>();
            var columns = new List<string>();
            var values = new List<string>();

            var properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (this.IsSelfIncreasing(property))
                    continue;

                var value = property.GetValue(model);
                if (value != null)
                {
                    columns.Add(this.GetDBName(property));
                    values.Add($"@{property.Name}");
                    para.Add(new SqlParameter($"@{property.Name}", value));
                }
            }

            var sql = $@"INSERT {this.GetDBName(typeof(TModel))} ({string.Join(", ", columns)}) " +
                $"VALUES ({string.Join(", ", values)})";

            return this.Insert(sql, para.ToArray());
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Insert(string sql, SqlParameter[] para)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, CommandType.Text, sql, para));
        }

        #endregion

        #region u

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Update(string sql, SqlParameter[] para)
        {
            return SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql, para);
        }

        #endregion

        #region d

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Delete(TModel model)
        {
            var para = new List<SqlParameter>();
            var where = new List<string>();

            var properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (this.IsKey(property))
                {
                    where.Add($"this.GetDBName(property) = @{property.Name}");
                    para.Add(new SqlParameter($"@{property.Name}", property.GetValue(model)));
                }
            }

            var sql = $@"DELETE FROM {this.GetDBName(typeof(TModel))} WHERE {string.Join(" and ", where)}";
            return this.Delete(sql, para.ToArray());
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加成功行数</returns>
        public int Delete(string sql, SqlParameter[] para)
        {
            return SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql, para);
        }

        #endregion

        /// <summary>
        /// 获取数据库真名
        /// </summary>
        /// <param name="mi">成员信息</param>
        /// <returns>数据库真名</returns>
        private string GetDBName(MemberInfo mi)
        {
            return mi.IsDefined(typeof(DBNameAttribute), true) ? mi.GetCustomAttribute<DBNameAttribute>(false).Name : mi.Name;
        }

        /// <summary>
        /// 是否自增
        /// </summary>
        /// <param name="pi">字段信息</param>
        /// <returns>是否自增</returns>
        private bool IsSelfIncreasing(PropertyInfo pi)
        {
            return this.IsKey(pi) ? pi.GetCustomAttribute<DBKeyAttribute>(false).IsSelfIncreasing : false;
        }

        /// <summary>
        /// 是否是主键
        /// </summary>
        /// <param name="pi">字段信息</param>
        /// <returns>是否是主键</returns>
        private bool IsKey(PropertyInfo pi)
        {
            return pi.IsDefined(typeof(DBKeyAttribute), true);
        }
    }
}