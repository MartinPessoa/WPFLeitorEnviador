using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services.CSVReaderProcessors
{
    internal class ResultadoListProcessor : BaseListProcessor, IListProcessor
    {
        private List<ResultadoItemProcessor> _lista = new();
        private readonly List<Resultado> _acumulados;

        public ResultadoListProcessor(string campeonato) : base(campeonato)
        {
            _acumulados = new List<Resultado>();
        }

        public List<Resultado> Acumulados
        {
            get { return _acumulados.Distinct().OrderBy(o => o.Data).ToList();  }
        }

        public void AcumularEZerar()
        {
            _acumulados.AddRange(GetResultados());
            _lista = new();
        }

        internal List<Resultado> GetResultados()
        {
            List<Resultado> resultados = new();

            foreach (var resultado in _lista)
            {
                if (resultado == null) continue;

                var r = resultado.getItem();

                if(r != null) resultados.Add(r);

            }

            return resultados.Distinct().OrderBy(o => o.Data).ToList();
        }

        public void ProcessarLinha(string[] fields, DateTime data)
        {
            int n = PegarInt(fields[0]);
            if (n < 0) { return; }

            if (!ResultadoItemProcessor.EhInformaçãoProcessável(fields)) return;

            var r = new ResultadoItemProcessor();
            var processou = r.ProcessarLinha(fields, n, data);

            if (processou)
            {
                _lista.Add(r);
            }
        }


        public string GetStrings()
        {
            var csv = new StringBuilder();

            Acumulados.ForEach(o =>
            {
                var resultado = o.GolsVisitante == -1 ? "5+x?" : $"{o.GolsCasa}x{o.GolsVisitante}";

                csv.Append($"{o.Campeonato}, {o.Hora}, {o.Minuto}, {resultado}\n");
            });

            return csv.ToString();

        }

        public List<object> GetForJsons()
        {
            var retorno = new List<object>();

            Acumulados.ForEach(i => { retorno.Add(i); });

            return retorno;
        }
    }
}
