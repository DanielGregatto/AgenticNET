using System;
using System.Text;
using Util.Interfaces;

namespace Util
{
    public class HandlerBase64String : IHandlerBase64String
    {
        public string EncodeToBase64(string texto)
        {
            try
            {
                byte[] textoAsBytes = Encoding.ASCII.GetBytes(texto);
                string resultado = Convert.ToBase64String(textoAsBytes);
                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string DecodeFrom64(string dados)
        {
            try
            {
                byte[] dadosAsBytes = Convert.FromBase64String(dados);
                string resultado = ASCIIEncoding.ASCII.GetString(dadosAsBytes);
                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
