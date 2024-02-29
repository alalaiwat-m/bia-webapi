using System.Collections.Generic;
using System.Data;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsNotificationService : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public List<Alert> getNotificationByFlightDetails(string flightNumber, string strScheduleDate, string strValue,
            string strType, string strAirlineCode, string strSource)
        {
            strQuery =
                @" SELECT airlinecode,flightnumber,currentstatus,value,type,source,isarabic,scheduledate,createdon 
                          FROM flightnotification 
                          WHERE ISNULL(flagdeleted,0) = 0 AND ISNULL(flagexpire,0) = 0 AND flightnumber = @flightnumber AND airlinecode=@airlinecode  AND scheduledate = @scheduleDate AND value = @value AND type = @type AND source=@source ";

            ParameterBuilder.AddParameter(builder, "flightnumber", flightNumber);
            ParameterBuilder.AddParameter(builder, "scheduleDate", strScheduleDate);
            ParameterBuilder.AddParameter(builder, "value", strValue);
            ParameterBuilder.AddParameter(builder, "type", strType);
            ParameterBuilder.AddParameter(builder, "airlinecode", strAirlineCode);
            ParameterBuilder.AddParameter(builder, "source", strSource);

            return ExecuteCommand<Alert>(strQuery, builder);
        }

        public int InsertFlightNotification(string airlineCode, string flightNumber, string currentStatus,
            string strScheduleDate, string strValue, string strType, string source, bool blnIsArabic = false)
        {
            builder = new DynamicParameters();
            strQuery =
                @" IF NOT EXISTS(SELECT 1 FROM flightnotification WHERE ISNULL(flagdeleted,0) = 0 AND ISNULL(flagexpire,0) = 0 AND flightnumber = @flightnumber AND airlinecode=@airlinecode AND scheduledate = @scheduledate AND value = @value AND type = @type AND source=@source)
                            BEGIN
                            INSERT INTO flightnotification(airlinecode,flightnumber,currentstatus,value,type,source,isarabic,scheduledate,createdon)
                                                                        VALUES (@airlinecode,@flightnumber,@currentstatus,@value,@type,@source,@isarabic,@scheduledate,GETUTCDATE())
                            END
                            ELSE 
                            BEGIN
	                            UPDATE flightnotification
	                            SET 
		                            createdon = GETUTCDATE()
	                            WHERE 
		                            ISNULL(flagdeleted,0) = 0 AND ISNULL(flagexpire,0) = 0 AND flightnumber = @flightnumber AND airlinecode=@airlinecode AND scheduledate = @scheduledate AND value = @value AND type = @type AND isarabic = @isarabic
                            END ";

            ParameterBuilder.AddParameter(builder, "airlinecode", airlineCode);
            ParameterBuilder.AddParameter(builder, "flightnumber", flightNumber);
            ParameterBuilder.AddParameter(builder, "scheduledate", strScheduleDate);
            ParameterBuilder.AddParameter(builder, "currentstatus", currentStatus);
            ParameterBuilder.AddParameter(builder, "value", strValue);
            ParameterBuilder.AddParameter(builder, "type", strType);
            ParameterBuilder.AddParameter(builder, "isarabic", blnIsArabic);
            ParameterBuilder.AddParameter(builder, "source", source);

            return ExecuteNonQuery(strQuery, builder);
        }

        public List<Alert> getAllAlerts()
        {
            builder = new DynamicParameters();
            strQuery = @"getAlertsDetails";

            return ExecuteCommand<Alert>(strQuery, builder, CommandType.StoredProcedure);
        }

        public int InsertFlightNotificationHistory(long intFlightNotificationId, string currentStatus,
            bool blnNotified = false)
        {
            builder = new DynamicParameters();
            strQuery =
                @" INSERT INTO flightnotificationhistory(flightnotificationid,currentstatus,flagnotified,createdon)
                                            VALUES (@flightnotificationid,@currentstatus,@flagnotified,GETUTCDATE()) ";

            ParameterBuilder.AddParameter(builder, "flightnotificationid", intFlightNotificationId);
            ParameterBuilder.AddParameter(builder, "currentstatus", currentStatus);
            ParameterBuilder.AddParameter(builder, "flagnotified", blnNotified);

            return ExecuteNonQuery(strQuery, builder);
        }

        public List<Alert> getAllNotification()
        {
            strQuery = @" SELECT airlinecode,flightnumber,currentstatus,value,type,source,isarabic
                        FROM 
	                        flightnotification
                        WHERE 
                        ISNULL(flagdeleted,0) = 0 
                        AND ISNULL(flagexpire,0) = 0
                        AND createdon <= DATEADD(hour,24,GETUTCDATE())
                        AND ISNULL(flagactive,1) = 1 ";

            return ExecuteCommand<Alert>(strQuery, builder);
        }
    }
}