using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services.CSVReaderProcessors
{
    internal class BaseItemProcessor
    {

        public string? Campeonato { get; set; }

        public string? Hora { get; set;  }

        public string? Minuto { get; set; }

        public DateTime DataHora { get; set; }

        public static bool EhInformaçãoProcessável(string[] linhaSplitada)
        {
            if (linhaSplitada[1] != "Extrair") return false;

            return true;
        }

        protected bool AnotarMinutoEHora(string minutoEHora, DateTime data, char separator)
        {
            var splitado = minutoEHora.Split(separator);

            if (Hora != null || Minuto != null)
            {
                Debug.WriteLine("Tentando sobreescrever Hora ou Minuto");
                return false;
            }

            Hora = splitado[0];
            Minuto = splitado[1];

            var intHora = int.Parse(Hora);
            var intMinuto = int.Parse(Minuto);

            DataHora = new DateTime(data.Year, data.Month, data.Day, intHora, intMinuto, 0);

            return true;
        }
    }
}
