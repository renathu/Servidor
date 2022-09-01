using MySql.Data.MySqlClient;
using Servidor.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Servidor.Conexao
{
    public class MariaDB
    {
        public string StrConexao { get; set; }

        delegate T ObjectActivator<T>();        

        public MariaDB(string baseDados)
        {
            StrConexao = $"Data Source = 127.0.0.1;Port = 3307;User Id = root;Password = NETSOF1234;Database = {baseDados};Default Command Timeout = 600;Connection Timeout = 5;Connection Lifetime = 30;Keep Alive = 10;Logging = False;Pooling = True;Maximum Pool Size = 100;Minimum Pool Size = 5;";
        }

        public MySqlConnection AbrirBanco()
        {
            MySqlConnection cnn = new MySqlConnection(this.StrConexao);
            cnn.Open();

            return cnn;
        }

        public void FecharBanco(MySqlConnection cnn)
        {
            if (cnn != null && cnn.State == System.Data.ConnectionState.Open)
            {
                cnn.Close();
            }
        }

        public bool TestarConexao()
        {
            try
            {
                MySqlConnection cnn = AbrirBanco();
                FecharBanco(cnn);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private T CreateInstance<T>()
        {
            Type[] types = Type.EmptyTypes;

            ConstructorInfo ctor = typeof(T).GetConstructor(types);

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp);

            //compile it
            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();

            //create an instance:
            return compiled();
        }

        /// <summary>
        /// Retorna valor do tipo especificado
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="sql">Sql</param>        
        /// <returns></returns>
        public T RetornaValor<T>(string sql)
        {
            return RetornaValor<T>(sql, null, null);
        }

        /// <summary>
        /// Retorna valor do tipo especificado
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="sql">Sql</param>
        /// <param name="lstParams">Lista de parametros</param>
        /// <returns></returns>
        public T RetornaValor<T>(string sql, List<MySqlParameter> lstParams)
        {
            return RetornaValor<T>(sql, lstParams, null);
        }

        /// <summary>
        /// Retorna valor do tipo especificado
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="sql">Sql</param>
        /// <param name="lstParams">Lista de parametros</param>
        /// <param name="transacao">Transação</param>
        /// <returns></returns>
        public T RetornaValor<T>(string sql, List<MySqlParameter> lstParams, MySqlTransaction transacao)
        {
            T retorno = default(T);

            if (typeof(T) == typeof(String))
            {
                retorno = (T)Convert.ChangeType(String.Empty, typeof(T));
            }

            using (MySqlCommand cmd = new MySqlCommand(sql))
            {
                if (transacao != null)
                {
                    cmd.Connection = transacao.Connection;
                    cmd.Transaction = transacao;
                }
                else
                {
                    cmd.Connection = AbrirBanco();
                }
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;

                if (lstParams != null)
                {
                    foreach (MySqlParameter parametro in lstParams)
                    {
                        //Adicionando o parâmetro
                        cmd.Parameters.Add(parametro);
                    }
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    object valor = null;

                    if (reader == null)
                        return retorno;

                    if (reader.Read())
                    {
                        valor = reader.GetValue(0);

                        if (valor != null && valor.GetType() != typeof(DBNull))
                        {
                            if (valor.GetType() != typeof(T))
                            {
                                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    retorno = (T)Convert.ChangeType(valor, Nullable.GetUnderlyingType(typeof(T)));
                                }
                                else
                                {
                                    retorno = (T)Convert.ChangeType(valor, typeof(T));
                                }
                            }
                            else
                            {
                                retorno = (T)valor;
                            }
                        }
                    }
                }
            }

            return retorno;
        }

        /// <summary>
        /// Retorna o objeto
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="sql">Sql</param>
        /// <returns></returns>
        public List<T> RetornaObjetosT<T>(String sql)
        {
            return RetornaObjetosT<T>(sql, null, null);
        }

        /// <summary>
        /// Retorna o objeto
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="sql">Sql</param>
        /// <param name="lstParams">Lista de parametros</param>
        /// <returns></returns>
        public List<T> RetornaObjetosT<T>(String sql, List<MySqlParameter> lstParams)
        {
            return RetornaObjetosT<T>(sql, lstParams, null);
        }

        /// <summary>
        /// Retorna o objeto
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="sql">Sql</param>
        /// <param name="lstParams">Lista de parametros</param>
        /// <param name="transacao">Transação</param>
        /// <returns></returns>
        public List<T> RetornaObjetosT<T>(String sql, List<MySqlParameter> lstParams, MySqlTransaction transacao)
        {
            List<T> ret = CreateInstance<List<T>>();
            PropertyInfo[] propriedadesObjeto = typeof(T).GetProperties().Where(I => I.CanWrite).ToArray();

            MySqlCommand cmd = new MySqlCommand(sql);
            MySqlDataReader dataReader = null;

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (transacao != null)
            {
                cmd.Connection = transacao.Connection;
                cmd.Transaction = transacao;
            }

            if (lstParams != null)
            {
                foreach (MySqlParameter parametro in lstParams)
                {
                    //Adicionando o parâmetro
                    cmd.Parameters.Add(parametro);
                }
            }

            if (transacao == null)
            {
                cmd.Connection = AbrirBanco();
            }

            using (dataReader = cmd.ExecuteReader(transacao == null ? CommandBehavior.CloseConnection : CommandBehavior.Default))
            {
                Dictionary<Int32, PropertyInfo> dictProp = new Dictionary<Int32, PropertyInfo>();
                bool flag = false;

                while (dataReader.Read())
                {
                    //T objAtual = (T)Activator.CreateInstance(typeof(T));

                    T objAtual = CreateInstance<T>();

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        PropertyInfo propriedadeAtual = null;

                        if (flag == true)
                        {
                            if (dictProp.TryGetValue(i, out propriedadeAtual) == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            propriedadeAtual = propriedadesObjeto.FirstOrDefault(f => f.Name.ToUpper() == dataReader.GetName(i).ToUpper());
                            if (propriedadeAtual != null)
                            {
                                dictProp.Add(i, propriedadeAtual);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (dataReader.IsDBNull(i) == false)
                        {
                            Type TipoCampo = propriedadeAtual.PropertyType.IsNullable() ? Nullable.GetUnderlyingType(propriedadeAtual.PropertyType) : propriedadeAtual.PropertyType;
                            var valorDB = dataReader[i];

                            object valor = null;

                            if (TipoCampo.IsEnum)
                            {
                                valor = Enum.Parse(TipoCampo, valorDB.ToString());
                            }
                            else
                            {
                                if (TipoCampo == typeof(DateTime) && dataReader[i].GetType() == typeof(TimeSpan))
                                {
                                    valor = Convert.ChangeType(valorDB.ToString(), TipoCampo);
                                }
                                else
                                {
                                    valor = Convert.ChangeType(valorDB, TipoCampo);
                                }
                            }

                            propriedadeAtual.SetValue(objAtual, valor, null);
                        }

                    }

                    flag = true;

                    ret.Add(objAtual);
                }

                if (transacao == null)
                {
                    dataReader.Close();
                    FecharBanco(cmd.Connection);
                }
            }

            return ret;
        }
    }
}
