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

        //增
        private readonly string SqlInsert = @"INSERT {0} ({1}) VALUES ({2})\n";

        //删
        private readonly string SqlDelete = @"DELETE FROM {0} WHERE {1}\n";

        //改
        private readonly string SqlUpdate = @"UPDATE {0} SET {1} WHERE {2}";

        //查
        private readonly string SqlQuery = @"SELECT * FROM {0} WHERE {1}\n";

        public BaseRepository(string db)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[db].ConnectionString;
        }

        #region r
        /// <summary>
        /// 包涵实体
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns>是否包涵</returns>
        public bool Has(Expression<Func<TModel, bool>> exp)
        {
            var sql = string.Format(this.SqlQuery, this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, null))
                return reader.HasRows;
        }

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
        /// <param name="exp">Lambda表达式</param>
        /// <returns></returns>
        public TModel Get(Expression<Func<TModel, bool>> exp)
        {
            var sql = string.Format(this.SqlQuery, this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
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
        /// 获取集合
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>该实体数据集合</returns>
        public IEnumerable<TModel> GetList(string sql = null, SqlParameter[] para = null)
        {
            var list = new List<TModel>();
            if (string.IsNullOrWhiteSpace(sql))
                sql = string.Format(this.SqlQuery, this.GetDBName(typeof(TModel)), "0=0");

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
        /// 获取集合
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns>该实体数据集合</returns>
        public IEnumerable<TModel> GetList(Expression<Func<TModel, bool>> exp)
        {
            var list = new List<TModel>();
            var sql = string.Format(this.SqlQuery, this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));

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
        public int Insert(string sql, SqlParameter[] para)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.ConnectionString, CommandType.Text, sql, para));
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="TModel">实体</param>
        /// <returns>添加成功行数</returns>
        public int Insert(TModel model)
        {
            var para = new List<SqlParameter>();
            var columns = new List<string>();
            var values = new List<string>();

            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
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

            var sql = string.Format(this.SqlInsert, this.GetDBName(typeof(TModel)), string.Join(", ", columns), string.Join(", ", values));

            return this.Insert(sql, para.ToArray());
        }

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns>添加成功行数</returns>
        public int BatchInsert(IEnumerable<TModel> list)
        {
            var columns = new List<string>();
            var values = new List<string>();
            var sql = string.Empty;
            foreach (var model in list)
            {
                var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var property in properties)
                {
                    if (this.IsSelfIncreasing(property))
                        continue;

                    var value = property.GetValue(model);
                    if (value != null)
                    {
                        columns.Add(this.GetDBName(property));
                        values.Add($"{value}");
                    }
                }

                sql += string.Format(this.SqlInsert, this.GetDBName(typeof(TModel)), string.Join(", ", columns), string.Join(", ", values));
            }

            return this.Insert(sql, null);
        }

        #endregion

        #region u

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name="model">sql语句</param>
        /// <returns>添加成功行数</returns>
        public int Update(TModel model)
        {
            var set = new List<string>();
            var where = new List<string>();

            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (this.IsKey(property))
                    where.Add($"{this.GetDBName(property)} = {property.GetValue(model)}");
                else
                    if (property.GetValue(model) != null) set.Add($"{this.GetDBName(property)} = {property.GetValue(model)}");
            }

            var sql = string.Format(this.SqlUpdate, this.GetDBName(typeof(TModel)), string.Join(", ", set), string.Join(" and ", where));
            return this.Update(sql, null);
        }

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
        public int Delete(string sql, SqlParameter[] para)
        {
            return SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql, para);
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model">实体</param>
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
                    where.Add($"{this.GetDBName(property)} = @{property.Name}");
                    para.Add(new SqlParameter($"@{property.Name}", property.GetValue(model)));
                }
            }

            var sql = string.Format(this.SqlDelete, this.GetDBName(typeof(TModel)), string.Join(" and ", where));
            return this.Delete(sql, para.ToArray());
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns></returns>
        public int BatchDelete(IEnumerable<TModel> list)
        {
            var where = new List<string>();
            var sql = string.Empty;
            foreach (var model in list)
            {
                var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var property in properties)
                {
                    if (this.IsKey(property))
                    {
                        where.Add($"{this.GetDBName(property)} = {property.GetValue(model)}");
                    }
                }

                sql += string.Format(this.SqlDelete, this.GetDBName(typeof(TModel)), string.Join(" and ", where));
            }
            return this.Delete(sql, null);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="expr">Lambda表达式</param>
        /// <returns></returns>
        public int BatchDelete(Expression<Func<TModel, bool>> exp)
        {
            var sql = string.Format(this.SqlDelete, this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
            return this.Delete(sql, null);
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