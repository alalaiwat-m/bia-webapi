using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;

namespace BACWebAPI.Models.Common
{
    public class Connection
    {
        private readonly IDbConnection objConn;

        public Connection()
        {
            var strServerName = ConfigurationManager.AppSettings.Get("servername");
            var strUserName = ConfigurationManager.AppSettings.Get("username");
            var strPassword = ConfigurationManager.AppSettings.Get("password");
            var strDB = ConfigurationManager.AppSettings.Get("db");
            var strConn = "";

            var IsDatabasemirrored = false;
            IsDatabasemirrored = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsDatabasemirrored"));

            if (IsDatabasemirrored)
            {
                var strFailoverPartner = "";
                strFailoverPartner = Convert.ToString(ConfigurationManager.AppSettings.Get("FailoverPartnerDB"));
                strConn = "Data Source=" + strServerName + ";Failover Partner=" + strFailoverPartner +
                          ";Initial Catalog=" + strDB + ";User Id=" + strUserName + ";Password=" + strPassword;
            }
            else
            {
                strConn = "Data Source=" + strServerName + ";Initial Catalog=" + strDB + ";User Id=" + strUserName +
                          ";Password=" + strPassword;
            }

            objConn = new SqlConnection(strConn);
        }

        public string ExecuteScalar(string Query, DynamicParameters parameter,
            CommandType commandType = CommandType.Text)
        {
            if (objConn.State == ConnectionState.Closed) objConn.Open();
            var Result = objConn.ExecuteScalar<string>(Query, parameter, commandType: commandType);
            if (objConn.State == ConnectionState.Open) objConn.Close();
            return Result;
        }

        public dynamic ExecuteCommand<T>(string Query, DynamicParameters parameter,
            CommandType commandType = CommandType.Text)
        {
            try
            {
                if (objConn.State == ConnectionState.Closed) objConn.Open();
                //var Result = objConn.Query<T>(Query, param: parameter, commandType: commandType);
                var Result = objConn.Query<T>(Query, parameter, commandType: commandType);
                HttpContext.Current.Trace.Warn("ExecuteCommand<T> Result:: " + Result);

                if (objConn.State == ConnectionState.Open) objConn.Close();
                return Result;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn("ExecuteCommand :: " + ex.Message);

                return null;
            }
        }

        public int ExecuteNonQuery(string Query, DynamicParameters parameter,
            CommandType commandType = CommandType.Text, char lastId = 'Y')
        {
            var Result = 0;
            try
            {
                if (commandType == CommandType.Text)
                    if (lastId == 'Y')
                        Query += " SELECT CAST(SCOPE_IDENTITY() AS int)";
                if (objConn.State == ConnectionState.Closed) objConn.Open();

                Result = objConn.Query<int>(Query, parameter, commandType: commandType).FirstOrDefault();

                if (objConn.State == ConnectionState.Open) objConn.Close();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Trace.Warn("ExecuteNonQuery :: " + ex.Message);

                if (objConn.State == ConnectionState.Open) objConn.Close();
            }

            return Result;
        }
    }
}