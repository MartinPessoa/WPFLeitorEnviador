using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services.CSVReaderProcessors
{
    internal static class EntityProccessorFactory
    {
     
        public static IGenericEntityListProcessor Get(TiposDeProccessor tipo, string campeonato)
        {
            switch (tipo)
            {
                case TiposDeProccessor.odds:
                    return new OddListProcessor(campeonato);
                case TiposDeProccessor.resultados:
                    break;
                default:
                    break;
            }

            throw new Exception("ERRO: não foi definido o tipo de proccessor.");
        }

        public static Type GetType(TiposDeProccessor tipo)
        {
            switch (tipo)
            {
                case TiposDeProccessor.odds:
                    return typeof(IEntityListProcessor<Odd>);
                case TiposDeProccessor.resultados:
                    break;
                default:
                    break;
            }

            throw new Exception("ERRO: não foi definido o tipo de proccessor.");
        }

    }
}
