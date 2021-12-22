using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Services.DTO
{
    internal class DateTimeConverterModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringData"></param>
        /// <returns>DateTime</returns>
        /// <exception cref="Exception">Se não foi possível converter em DateTime</exception>
        public static DateTime Converter(string stringData)
        {
            // "2021-12-20 01:14:26"

            var splits = stringData.Split(' ');

            int[] data = Array.Empty<int>();
            int[] horas = Array.Empty<int>();

            try
            {
                data = ToIntArray(splits[0].Split('-')).ToArray();
                horas = ToIntArray(splits[1].Split(':')).ToArray();
            } catch (Exception ex)
            {
                throw new Exception("Não foi possível converter em Data: " + stringData, ex);
            }



            return new DateTime(data[0], data[1], data[2], horas[0], horas[1], horas[2]);
        }

        private static IEnumerable<int> ToIntArray(string[] stringArray)
        {
            IEnumerable<int> retorno = Array.Empty<int>();

            foreach(var item in stringArray)
            {
                if(!int.TryParse(item, out int convertido))
                {
                    throw new Exception("Não foi possível converter em Inteiro: " + String.Join(" - ", stringArray));
                }

                retorno = retorno.Append(convertido);
            }

            return retorno;
        }


    }
}
