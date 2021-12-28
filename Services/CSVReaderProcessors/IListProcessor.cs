using System;
using System.Collections.Generic;

namespace WPFLeitorEnviador.Services
{
    internal interface IListProcessor
    {
        void ProcessarLinha(string[] fields, DateTime data);
        
        /// <summary>
        /// Salva os itens lidos até agora e zera a lista temporária interna, para poder iniciar leitura de novo arquivo
        /// </summary>
        void AcumularEZerar();

        string GetStrings();

        List<object> GetForJsons();

        string GetCampeonato();
    }
}