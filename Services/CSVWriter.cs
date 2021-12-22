using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services
{
    internal class CSVWriter
    {
        private string _pasta;
        public CSVWriter(string pasta)
        {
            this._pasta = pasta;
        }

        public async Task Write(List<Odd> lista)
        {
            //escolher arquivo

            //escrever

        }
    }
}
