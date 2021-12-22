using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Domain
{
    internal class Odd
    {
        public string Campeonato { get; }

        public string Hora { get;  }

        public string Minuto { get; }
        public string Over15 { get; }
        public string Over25 { get; }

        public Odd(string campeonato, string hora, string minuto, string over15, string over25)
        {
            
            this.Campeonato = campeonato;
            this.Hora = hora;
            this.Minuto = minuto;

            this.Over15 = over15;
            this.Over25 = over25;
        }
    }
}
