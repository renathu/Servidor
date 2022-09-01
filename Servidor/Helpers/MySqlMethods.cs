using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servidor.Helpers
{
    public class MySqlMethods
    {
        public static MySqlParameter CreateParameter(string name, MySqlDbType type, object value)
        {
            MySqlParameter parameter = new MySqlParameter();
            parameter.ParameterName = name;
            parameter.MySqlDbType = type;
            parameter.Value = value;

            return parameter;
        }
    }
}
