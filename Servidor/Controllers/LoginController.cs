using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Models.LoginModel;
using MySql.Data.MySqlClient;
using Servidor.Conexao;
using Servidor.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Servidor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        [HttpGet("verificarUsuario")]
        public async Task<IActionResult> VerificarUsuario([FromBody]Login model)
        {
            if ((model.Usuario == "9999" || model.Usuario == "TÉCNICO") && Util.ValidarSenhaTecnica(model.Senha))
            {
                return Ok("Sucesso");
            }

            string sql = "SELECT ID_USUARIO " +
                        $"FROM {model.DataBase}.USUARIOS " +
                        $"WHERE NOME = @NOME AND SENHA = @SENHA LIMIT 1";

            List<MySqlParameter> listParametros = new List<MySqlParameter>();
            listParametros.Add(MySqlMethods.CreateParameter("@NOME", MySqlDbType.String, model.Usuario));
            listParametros.Add(MySqlMethods.CreateParameter("@SENHA", MySqlDbType.String, model.Senha));

            try
            {
                MariaDB mariaDB = new MariaDB(model.DataBase);
                var result = await Task.Run(
                                                () => mariaDB.RetornaValor<int>(sql, listParametros)
                                            );

                if (result != 0)
                {
                    return Ok("Sucesso");
                }
                else
                {
                    return BadRequest("Erro");
                }
            }
            catch
            {
                return BadRequest("Erro");
            }
        }

        [HttpGet("retornaUsuarios")]
        public async Task<IActionResult> RetornaUsuarios()
        {
            DataTable dtDados = new DataTable();
            dtDados.Columns.Add("CODIGO", typeof(int));
            dtDados.Columns.Add("DESCRICAO", typeof(String));

            await Task.Run(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    dtDados.Rows.Add(i + 1, "teste" + i);
                }
            });

            var jsonString = JsonSerializer.Serialize<DataTable>(dtDados);

            return Ok(jsonString);
        }
    }
}
