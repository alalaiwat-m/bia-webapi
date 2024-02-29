using System.Collections.Generic;
using System.Data;
using System.Web;
using BACWebAPI.Models.Common;
using BusinessClassLibrary.EntityClass;
using Dapper;

namespace BACWebAPI.Models.Service
{
    public class clsEmailService : Connection
    {
        private DynamicParameters builder = new DynamicParameters();
        private string strQuery = "";

        public List<EmailDetails> GetEmailTemplateByCode(string strCode, bool blnIsArabic = false)
        {
            strQuery = @" SELECT emailtemplateid,code,value AS emailcontent,subject,name,ccemail,bccemail,isarabic
                        FROM 
	                        emailtemplate
                        WHERE 
                        ISNULL(flagdeleted,0) = 0 
                        AND isarabic = @isarabic
                        AND code = @code ";

            ParameterBuilder.AddParameter(builder, "code", strCode);
            ParameterBuilder.AddParameter(builder, "isarabic", blnIsArabic);

            return ExecuteCommand<EmailDetails>(strQuery, builder);
        }

        public int UpsertEmail(string code, string value, string subject, string name, string ccemail, string bccemail,
            bool isarabic)
        {
            builder = new DynamicParameters();
            strQuery = @"UpdateEmailTemplate";

            HttpContext.Current.Trace.Warn("Value ::" + value);
            ParameterBuilder.AddParameter(builder, "code", code);
            ParameterBuilder.AddParameter(builder, "isarabic", isarabic);
            ParameterBuilder.AddParameter(builder, "name", name);
            ParameterBuilder.AddParameter(builder, "value", value);
            ParameterBuilder.AddParameter(builder, "subject", subject);
            ParameterBuilder.AddParameter(builder, "ccemail", ccemail);
            ParameterBuilder.AddParameter(builder, "bccemail", bccemail);

            return ExecuteCommand<Alert>(strQuery, builder, CommandType.StoredProcedure);
        }

        public List<systemSetting> GetSystemSettings()
        {
            strQuery = @" SELECT name,[key],value,isarabic,[desc],createdon as createdDate
                          FROM 
	                        systemsetting
                        WHERE 
                        ISNULL(flagdeleted,0) = 0 
                        AND ISNULL(flagactive,1) = 1 ";

            //ParameterBuilder.AddParameter(builder, "isarabic", blnIsArabic);

            return ExecuteCommand<systemSetting>(strQuery, builder);
        }

        public int UpdateSystemSettings(string strValue, string strKey)
        {
            builder = new DynamicParameters();
            strQuery = @" 
	                            UPDATE systemsetting
	                            SET 
		                            value = @value,createdon=GETUTCDATE()
	                            WHERE  [key]=@key";

            ParameterBuilder.AddParameter(builder, "value", strValue);
            ParameterBuilder.AddParameter(builder, "key", strKey);

            return ExecuteNonQuery(strQuery, builder);
        }
    }
}