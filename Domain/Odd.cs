using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFLeitorEnviador.Domain
{
    internal class Odd : IEquatable<Odd>, IEntityBase
    {
        public string Campeonato { get; }

        public DateTime Data {  get; }

        public string Hora { get;  }

        public string Minuto { get; }
        public string Over15 { get; }
        public string Over25 { get; }

        public Odd(string campeonato, string over15, string over25, DateTime data)
        {
            
            this.Campeonato = campeonato;
            this.Hora = data.Hour.ToString();
            this.Minuto = data.Minute.ToString();
            this.Data = data;

            this.Over15 = over15;
            this.Over25 = over25;
        }

        public bool Equals(Odd? other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Data.Equals(other.Data) && Over15.Equals(other.Over15) && Over25.Equals(other.Over25);
        }

        public override int GetHashCode()
        {
            int hashProductData = Data.GetHashCode();
  
            int hashProductOver15 = Over15.GetHashCode();

            int hashProductOver25 = Over25.GetHashCode();

            //Calculate the hash code
            return hashProductData ^ hashProductOver15 ^ hashProductOver25;

        }
    }
}
