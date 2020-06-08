using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace StreamlineFrame.Web.Common
{

    /// <summary>
    /// ִ��Sql �����ͨ�÷���
    /// </summary>
    public abstract class SqlHelper
    {
        //�����ַ���
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        #region ExecuteNonQuery
        /// <summary>
        /// ִ��sql����
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText">sql���/������sql���/�洢������</param>
        /// <param name="commandParameters"></param>
        /// <returns>��Ӱ�������</returns>
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
        /// ִ��Sql Server�洢����
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="spName">�洢������</param>
        /// <param name="parameterValues"></param>
        /// <returns>��Ӱ�������</returns>
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
        ///  ִ��sql����
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns>SqlDataReader ����</returns>
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
        /// ִ��Sql ���
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <param name="spName">Sql ���/��������sql���</param>
        /// <param name="parameterValues">����</param>
        /// <returns>ִ�н������</returns>
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
        /// ִ�д洢����
        /// </summary>
        /// <param name="connectionString">�����ַ���</param>
        /// <param name="spName">�洢������</param>
        /// <param name="parameterValues">�洢���̲���</param>
        /// <returns>ִ�н������</returns>
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
        /// ����һ���ȴ�ִ�е�SqlCommand����
        /// </summary>
        /// <param name="cmd">SqlCommand ���󣬲�����ն���</param>
        /// <param name="conn">SqlConnection ���󣬲�����ն���</param>
        /// <param name="commandText">Sql ���</param>
        /// <param name="cmdParms">SqlParameters  ����,����Ϊ�ն���</param>
        private static void PrepareCommand(SqlCommand cmd, CommandType commandType, SqlConnection conn, string commandText, SqlParameter[] cmdParms)
        {
            //������
            if (conn.State != ConnectionState.Open)
                conn.Open();

            //����SqlCommand����
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
        /// ����һ���ȴ�ִ�д洢���̵�SqlCommand����
        /// </summary>
        /// <param name="cmd">SqlCommand ���󣬲�����ն���</param>
        /// <param name="conn">SqlConnection ���󣬲�����ն���</param>
        /// <param name="spName">Sql ���</param>
        /// <param name="parameterValues">���������Ĵ洢���̲���������Ϊ��</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, string spName, params object[] parameterValues)
        {
            //������
            if (conn.State != ConnectionState.Open)
                conn.Open();

            //����SqlCommand����
            cmd.Connection = conn;
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            //��ȡ�洢���̵Ĳ���
            SqlCommandBuilder.DeriveParameters(cmd);

            //�Ƴ�Return_Value ����
            cmd.Parameters.RemoveAt(0);

            //���ò���ֵ
            if (parameterValues != null)
            {
                for (int i = 0; i < cmd.Parameters.Count; i++)
                    cmd.Parameters[i].Value = parameterValues[i];
            }
        }
        #endregion
    }
}