using System.Collections.Generic;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsInstagramDTO : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public void InsertInstagramThirdPartyRequestResponse(string strRequest, string strResponse, string strKey)
        {
            builder = new DynamicParameters();
            strQuery =
                @" INSERT INTO instagramlog(request,response,createdon,[key]) VALUES (@request,@response,GETUTCDATE(),@culturekey)";

            ParameterBuilder.AddParameter(builder, "request", strRequest);
            ParameterBuilder.AddParameter(builder, "response", strResponse);
            ParameterBuilder.AddParameter(builder, "culturekey", strKey);

            var LastInsertedId = ExecuteNonQuery(strQuery, builder);
        }

        public List<DBCache> GetInstagramResponse(int intTimeDiff, string strKey)
        {
            strQuery =
                @" SELECT TOP 1 response AS data, (@timeDiff - DATEDIFF(minute, createdon, GETUTCDATE())) AS minTime
                         FROM instagramlog
                         WHERE 
	                        ISNULL(flagdeleted,0) = 0 
	                        AND ISNULL(flagexpire,0) = 0
	                        AND DATEDIFF(minute, createdon, GETUTCDATE()) < @timeDiff 
                            AND [key] = @Key
                            ORDER BY instagramlogid DESC";

            ParameterBuilder.AddParameter(builder, "timeDiff", intTimeDiff);
            ParameterBuilder.AddParameter(builder, "key", strKey);

            return ExecuteCommand<DBCache>(strQuery, builder);
        }
    }
}