using System.Collections.Generic;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsEventsDTO : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public void InsertEventsThirdPartyRequestResponse(string strRequest, string strResponse, string strType,
            string strKey)
        {
            builder = new DynamicParameters();
            strQuery = @" INSERT INTO eventslog(request,response,createdon,type,[key])
                                        VALUES (@request,@response,GETUTCDATE(),@type,@Key) ";

            ParameterBuilder.AddParameter(builder, "request", strRequest);
            ParameterBuilder.AddParameter(builder, "response", strResponse);
            ParameterBuilder.AddParameter(builder, "type", strType);
            ParameterBuilder.AddParameter(builder, "Key", strKey);

            var LastInsertedId = ExecuteNonQuery(strQuery, builder);
        }

        public List<DBCache> GetEventsResponse(string strType, int intTimeDiff)
        {
            strQuery =
                @" SELECT TOP 1 response AS data, (@timeDiff - DATEDIFF(minute, createdon, GETUTCDATE())) AS minTime
                         FROM eventslog
                         WHERE 
	                        ISNULL(flagdeleted,0) = 0 
	                        AND ISNULL(flagexpire,0) = 0
	                        AND DATEDIFF(minute, createdon, GETUTCDATE()) < @timeDiff
                            AND [type] = @Type
                            ORDER BY eventslogid DESC";

            ParameterBuilder.AddParameter(builder, "Type", strType);
            ParameterBuilder.AddParameter(builder, "timeDiff", intTimeDiff);

            return ExecuteCommand<DBCache>(strQuery, builder);
        }

        public List<DBCache> GetEventDetailsResponse(string strType, string strKey, int intTimeDiff)
        {
            strQuery =
                @" SELECT TOP 1 response AS data, (@timeDiff - DATEDIFF(minute, createdon, GETUTCDATE())) AS minTime
                         FROM eventslog
                         WHERE 
	                        ISNULL(flagdeleted,0) = 0 
	                        AND ISNULL(flagexpire,0) = 0
	                        AND DATEDIFF(minute, createdon, GETUTCDATE()) < @timeDiff
                            AND [type] = @Type
                            AND [key] = @Key
                            ORDER BY eventslogid DESC";

            ParameterBuilder.AddParameter(builder, "Type", strType);
            ParameterBuilder.AddParameter(builder, "Key", strKey);
            ParameterBuilder.AddParameter(builder, "timeDiff", intTimeDiff);

            return ExecuteCommand<DBCache>(strQuery, builder);
        }
    }
}