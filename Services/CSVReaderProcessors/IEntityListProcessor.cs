using System;
using System.Collections.Generic;
using WPFLeitorEnviador.Domain;

namespace WPFLeitorEnviador.Services
{
    public enum TiposDeProccessor
    {
        odds,
        resultados
    }

    internal interface IGenericEntityListProcessor 
    {
        void ProcessarLinha(string[] fields, DateTime data);
        List<IEntityBase> GetItems();
    }

    internal interface IEntityListProcessor<T> :IGenericEntityListProcessor
    {
        new List<T> GetItems();
        
    }
}