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
        private Action<string> _progress;

        public CSVWriter(string pasta, Action<string> progress)
        {
            this._pasta = pasta;
            this._progress = progress;
        }
        ~CSVWriter()
        {
            _progress = null;
        }

        public async Task Write(List<Odd> lista)
        {
            //escolher arquivo

            //escrever

        }
    }
}
