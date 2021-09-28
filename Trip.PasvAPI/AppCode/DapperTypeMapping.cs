using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Newtonsoft.Json;

public class ObjectJsonMapper : SqlMapper.ITypeHandler
{
    public object Parse(Type destinationType, object value)
    {
        return JsonConvert.DeserializeObject(value.ToString(), destinationType);
    }

    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.Value = (value == null) ? (object)DBNull.Value : JsonConvert.SerializeObject(value);
        parameter.DbType = DbType.String;
    }
}
