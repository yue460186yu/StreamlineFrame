using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace StreamlineFrame.Web.Common
{

    /// <summary>
    /// 执行Sql 命令的通用方法
    /// </summary>
    public abstract class SqlHelper
    {
        //链接字符串
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        #region ExecuteNonQuery
        /// <summary>
        /// 执行sql命令
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="commandParameters"></param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行Sql Server存储过程
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues"></param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, conn, spName, parameterValues);
                return cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region ExecuteReader
        /// <summary>
        ///  执行sql命令
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns>SqlDataReader 对象</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {

            var conn = new SqlConnection(connectionString);
            try
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, conn, spName, parameterValues);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                conn.Close();
                throw;
            }
        }
        #endregion

        #region ExecuteDataset
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, conn, spName, parameterValues);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }
        #endregion

        #region ExecuteScalar
        /// <summary>
        /// 执行Sql 语句
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="spName">Sql 语句/参数化的sql语句</param>
        /// <param name="parameterValues">参数</param>
        /// <returns>执行结果对象</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">存储过程参数</param>
        /// <returns>执行结果对象</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, conn, spName, parameterValues);
                return cmd.ExecuteScalar();
            }
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 设置一个等待执行的SqlCommand对象
        /// </summary>
        /// <param name="cmd">SqlCommand 对象，不允许空对象</param>
        /// <param name="conn">SqlConnection 对象，不允许空对象</param>
        /// <param name="commandText">Sql 语句</param>
        /// <param name="cmdParms">SqlParameters  对象,允许为空对象</param>
        private static void PrepareCommand(SqlCommand cmd, CommandType commandType, SqlConnection conn, string commandText, SqlParameter[] cmdParms)
        {
            //打开连接
            if (conn.State != ConnectionState.Open)
                conn.Open();

            //设置SqlCommand对象
            cmd.Connection = conn;
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// 设置一个等待执行存储过程的SqlCommand对象
        /// </summary>
        /// <param name="cmd">SqlCommand 对象，不允许空对象</param>
        /// <param name="conn">SqlConnection 对象，不允许空对象</param>
        /// <param name="spName">Sql 语句</param>
        /// <param name="parameterValues">不定个数的存储过程参数，允许为空</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, string spName, params object[] parameterValues)
        {
            //打开连接
            if (conn.State != ConnectionState.Open)
                conn.Open();

            //设置SqlCommand对象
            cmd.Connection = conn;
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            //获取存储过程的参数
            SqlCommandBuilder.DeriveParameters(cmd);

            //移除Return_Value 参数
            cmd.Parameters.RemoveAt(0);

            //设置参数值
            if (parameterValues != null)
            {
                for (int i = 0; i < cmd.Parameters.Count; i++)
                    cmd.Parameters[i].Value = parameterValues[i];
            }
        }
        #endregion
    }
}