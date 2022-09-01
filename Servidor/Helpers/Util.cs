using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servidor.Helpers
{
    public class Util
    {
        public static Boolean ValidarSenhaTecnica(String SenhaInserida)
        {
            char[] letrasSenha = new char[10] { 'N', 'E', 'T', 'S', 'O', 'F', 'T', 'S', 'Y', 'S' };
            string dataAtual = DateTime.Now.Date.ToString("ddMMyyyy");
            string senha = string.Empty;
            for (int i = 0; i < dataAtual.Length; i++)
            {
                senha += letrasSenha[int.Parse(dataAtual[i].ToString())];
            }

            if (SenhaInserida.ToUpper() == senha)
                return true;

            return false;
        }
    }
}
