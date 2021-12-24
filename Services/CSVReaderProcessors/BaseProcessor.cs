using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services.CSVReaderProcessors
{
    internal class BaseListProcessor
    {
       protected readonly string _campeonato = "";

        public BaseListProcessor(string campeonato)
        {
            this._campeonato=campeonato;
        }

        public string GetCampeonato()
        {
            return _campeonato;
        }

        protected int PegarInt(string v)
        {
            if (int.TryParse(v.Substring(1), out int valor))
            {
                return valor;
            }

            return -1;
        }

    }
}
