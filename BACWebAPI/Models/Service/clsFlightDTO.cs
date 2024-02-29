using System.Collections.Generic;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsFlightDTO : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public void InsertFlightThirdPartyRequestResponse(string strRequest, string strResponse, string strType)
        {
            builder = new DynamicParameters();
            strQuery = @" INSERT INTO flightslog(request,response,type,createdon)
                                        VALUES (@request,@response,@type,GETUTCDATE()) ";

            ParameterBuilder.AddParameter(builder, "request", strRequest);
            ParameterBuilder.AddParameter(builder, "response", strResponse);
            ParameterBuilder.AddParameter(builder, "type", strType);

            ExecuteNonQuery(strQuery, builder);
        }

        public List<DBCache> GetFlightResponse(string strType, int intTimeDiff)
        {
            strQuery =
                @" SELECT TOP 1 response AS data, (@timeDiff - DATEDIFF(minute, createdon, GETUTCDATE())) AS minTime
                         FROM flightslog
                         WHERE 
	                        ISNULL(flagdeleted,0) = 0 
	                        AND ISNULL(flagexpire,0) = 0
	                        AND DATEDIFF(minute, createdon, GETUTCDATE()) < @timeDiff
	                        AND [type] = @Type 
                            ORDER BY flightslogid DESC";

            ParameterBuilder.AddParameter(builder, "Type", strType);
            ParameterBuilder.AddParameter(builder, "timeDiff", intTimeDiff);

            return ExecuteCommand<DBCache>(strQuery, builder);
        }
    }
}