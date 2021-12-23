using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;
using WPFLeitorEnviador.Services.DTO;

namespace WPFLeitorEnviador.Services
{
    internal class OddCSVReader : OddCSVReaderBase1
    {
        //protected new IEntityListProcessor<Odd> _oddProccessor;
        protected new List<Odd> _itensGerando = new();

        public OddCSVReader(string pasta, string campeonato, IProgress<string>? progress) :base(pasta, campeonato, new OddListProcessor(campeonato), progress)
        {

        }

        ~OddCSVReader()
        {
            _informarResultado = null;
        }


        public new List<Odd> Read()
        {
            var lista =  base.Read();
            return _itensGerando.Distinct().OrderBy(o => o.Data).ToList();
        }
    }


}
