using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Models.BaseDadosModel;
using Model.Models.LoginModel;
using Servidor.Conexao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Servidor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/teste")]
    public class TesteController : ControllerBase
    {
        [HttpGet("testarConexao")]
        public async Task<IActionResult> TestarConexao()
        {
            var result = await Task.Run(() => "Sucesso");

            return Ok(result);
        }

        [HttpGet("testarDataBase")]
        public async Task<IActionResult> TestarDataBase([FromBody]BaseDados model)
        {
            MariaDB mariaDB = new MariaDB(model.Nome);
            var result = await Task.Run(() => mariaDB.TestarConexao());

            if(result)
            {
                return Ok("Sucesso");
            }
            else
            {
                return BadRequest("Erro");
            }
        }
    }
}
