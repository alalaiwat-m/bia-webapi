using System.Collections.Generic;
using System.Web;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsUserService : Connection
    {
        private readonly DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public List<users> GetUserByUsernamePwd(string strUserName, string strPassword)
        {
            strQuery = @" SELECT userid,name,username
                        FROM 
	                        users
                        WHERE 
                        ISNULL(flagdeleted,0) = 0 
                        AND username = @userName
                        AND password = @password
                        AND ISNULL(flagactive,1) = 1 ";

            ParameterBuilder.AddParameter(builder, "userName", strUserName);
            ParameterBuilder.AddParameter(builder, "password", strPassword);
            HttpContext.Current.Trace.Warn("strUserName :: " + strUserName + " ::: pwd :: " + strPassword);

            return ExecuteCommand<users>(strQuery, builder);
        }
    }
}