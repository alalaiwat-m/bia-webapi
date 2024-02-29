using System.Data;
using Dapper;

namespace BACWebAPI.Models.Common
{
    public class ParameterBuilder : IParameterBuilder
    {
        public static void AddParameter(DynamicParameters builder, string strParameterName, object value,
            DbType? dbType = null, ParameterDirection? direction = ParameterDirection.Input)
        {
            builder.Add(strParameterName, value, dbType, direction);
        }

        //public DynamicParameters getParameters()
        //{
        //    return this.builder;
        //}
    }
}