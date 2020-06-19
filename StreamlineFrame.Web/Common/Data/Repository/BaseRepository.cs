using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        private const string SqlInsert = @"INSERT {0} ({1}) VALUES ({2})\n";

        //删
        private const string SqlDelete = @"DELETE FROM {0} WHERE {1}\n";

        //改
        private const string SqlUpdate = @"UPDATE {0} SET {1} WHERE {2}\n";

        //查
        private const string SqlQuery = @"SELECT {0} FROM {1} WHERE {2}\n";

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
            var sql = string.Format(SqlQuery, "*", this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
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
        /// <returns>实体</returns>
        public TModel Get(Expression<Func<TModel, bool>> exp)
        {
            var sql = string.Format(SqlQuery, "*", this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
            return this.Get(sql, null);
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>实体</returns>
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
        /// 获取集合
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>实体数据集合</returns>
        public IEnumerable<TModel> GetList(string sql = null, SqlParameter[] para = null)
        {
            var list = new List<TModel>();
            if (string.IsNullOrWhiteSpace(sql))
                sql = string.Format(SqlQuery, "*", this.GetDBName(typeof(TModel)), "0=0");

            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, null))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        list.Add(this.ModelFactory(reader));

                    return list;
                }

                return null;
            }
        }

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns>实体数据集合</returns>
        public IEnumerable<TModel> GetList(Expression<Func<TModel, bool>> exp)
        {
            var list = new List<TModel>();
            var sql = string.Format(SqlQuery, "*", this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));

            using (var reader = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, null))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        list.Add(this.ModelFactory(reader));

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
            if (model == null)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.InsertSqlFactory(model, para);

            return this.Insert(sql, para.ToArray());
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns>添加成功行数</returns>
        public int BatchInsert(IEnumerable<TModel> list)
        {
            if (!list?.Any() ?? true)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.BatchInsertSqlFactory(list, para);

            return this.Insert(sql, para.ToArray());
        }

        #endregion

        #region u

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加修改行数</returns>
        public int Update(string sql, SqlParameter[] para)
        {
            return SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql, para);
        }

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name="model">sql语句</param>
        /// <returns>添加修改行数</returns>
        public int Update(TModel model)
        {
            if (model == null)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.UpdateSqlFactory(model, para);

            return this.Update(sql, para.ToArray());
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns>修改修改行数</returns>
        public int BatchUpdate(IEnumerable<TModel> list)
        {
            if (!list?.Any() ?? true)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.BatchUpdateSqlFactory(list, para);

            return this.Update(sql, para.ToArray());
        }

        #endregion

        #region d

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="para">sql注入参数</param>
        /// <returns>添加删除行数</returns>
        public int Delete(string sql, SqlParameter[] para)
        {
            return SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql, para);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="expr">Lambda表达式</param>
        /// <returns></returns>
        public int BatchDelete(Expression<Func<TModel, bool>> exp)
        {
            var sql = string.Format(SqlDelete, this.GetDBName(typeof(TModel)), ExpressionHelper.ExpressionToSql(exp));
            return this.Delete(sql, null);
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns>添加删除行数</returns>
        public int Delete(TModel model)
        {
            if (model == null)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.DeleteSqlFactory(model, para);

            return this.Delete(sql, para.ToArray());
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns>添加删除行数</returns>
        public int BatchDelete(IEnumerable<TModel> list)
        {
            if (!list?.Any() ?? true)
                return 0;

            var para = new List<SqlParameter>();
            var sql = this.BatchDeleteSqlFactory(list, para);

            return this.Delete(sql, para.ToArray());
        }

        #endregion

        #region SQL工厂

        public virtual string BatchInsertSqlFactory(IEnumerable<TModel> list, List<SqlParameter> para)
        {
            var index = 0;
            var sql = string.Empty;

            foreach (var model in list)
                sql += this.InsertSqlFactory(model, para, index++);

            return sql;
        }

        public virtual string InsertSqlFactory(TModel model, List<SqlParameter> para, int index = 0)
        {
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
                    var parastring = $"@{property.Name}{index}";
                    values.Add(parastring);
                    para.Add(new SqlParameter(parastring, value));
                }
            }

            return string.Format(SqlInsert, this.GetDBName(typeof(TModel)), string.Join(", ", columns), string.Join(", ", values));
        }

        public virtual string BatchUpdateSqlFactory(IEnumerable<TModel> list, List<SqlParameter> para)
        {
            var index = 0;
            var sql = string.Empty;

            foreach (var model in list)
                sql += this.UpdateSqlFactory(model, para, index++);

            return sql;
        }

        public virtual string UpdateSqlFactory(TModel model, List<SqlParameter> para, int index = 0)
        {
            var set = new List<string>();
            var where = new List<string>();
            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                if (value != null)
                {
                    var parastring = $"@{property.Name}{index}";
                    if (this.IsKey(property))
                    {
                        where.Add($"{this.GetDBName(property)} = {parastring}");
                        para.Add(new SqlParameter(parastring, value));
                    }
                    else
                    {
                        set.Add($"{this.GetDBName(property)} = {parastring}");
                        para.Add(new SqlParameter(parastring, value));
                    }
                }
            }

            return string.Format(SqlUpdate, this.GetDBName(typeof(TModel)), string.Join(", ", set), string.Join(" and ", where));
        }

        public virtual string BatchDeleteSqlFactory(IEnumerable<TModel> list, List<SqlParameter> para)
        {
            var index = 0;
            var sql = string.Empty;

            foreach (var model in list)
                sql += this.DeleteSqlFactory(model, para, index++);

            return sql;
        }

        public virtual string DeleteSqlFactory(TModel model, List<SqlParameter> para, int index = 0)
        {
            var where = new List<string>();
            var properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                if (value != null)
                {
                    var parastring = $"@{property.Name}{index}";
                    if (this.IsKey(property))
                    {
                        where.Add($"{this.GetDBName(property)} = {parastring}");
                        para.Add(new SqlParameter(parastring, property.GetValue(model)));
                    }
                }
            }

            return string.Format(SqlDelete, this.GetDBName(typeof(TModel)), string.Join(" and ", where));
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