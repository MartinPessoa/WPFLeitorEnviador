using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Domain
{
    internal class Resultado : IEquatable<Resultado>
    {
        public string Campeonato { get; }

        public DateTime Data { get; }

        public string Hora { get; }

        public string Minuto { get; }

        public int GolsCasa { get; }

        public int GolsVisitante { get; }

        public Resultado(string campeonato, DateTime data, int golsCasa, int golsVisitante)
        {
            this.Campeonato = campeonato;
            this.Hora = data.Hour.ToString();
            this.Minuto = data.Minute.ToString();
            this.Data = data;

            this.GolsCasa = golsCasa;
            this.GolsVisitante = golsVisitante;
        }


        public bool Equals(Resultado? other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Data.Equals(other.Data) && GolsCasa.Equals(other.GolsCasa) && GolsVisitante.Equals(other.GolsVisitante);
        }

        public override int GetHashCode()
        {
            int hashProductData = Data.GetHashCode();

            int hashProductGolsCasa = GolsCasa.GetHashCode();

            int hashProductGolsVisitante = GolsVisitante.GetHashCode();

            //Calculate the hash code
            return hashProductData ^ hashProductGolsCasa ^ hashProductGolsVisitante;

        }
    }
}
