using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    internal class Richesse
    {

        public static Richesse operator +(Richesse r1, Richesse r2)
        {
            if (r1.Monnaie != r2.Monnaie)
                throw new NotSupportedException("tocard !");
            return new Richesse(r1.Quantite + r2.Quantite, r1.Monnaie);
        }
        public static Richesse operator -(Richesse r1, Richesse r2)
        {
            if (r1.Monnaie != r2.Monnaie)
                throw new NotSupportedException("tocard !");
            return new Richesse(r1.Quantite - r2.Quantite, r1.Monnaie);
        }
        public static double operator /(Richesse r1, Richesse r2)
        {
            if(r1.Monnaie != r2.Monnaie)
                throw new NotSupportedException("tocard !");
            if (r2.Quantite == 0)
                throw new DivideByZeroException("imbécile !");
            return r1.Quantite / r2.Quantite;
        }
        public static bool operator >(Richesse r1, Richesse r2)
        {
            if (r1.Monnaie != r2.Monnaie)
                throw new NotSupportedException("tocard !");
            
            return r1.Quantite > r2.Quantite;
        }
        public static bool operator <(Richesse r1, Richesse r2)
        {
            if (r1.Monnaie != r2.Monnaie)
                throw new NotSupportedException("tocard !");

            return r1.Quantite < r2.Quantite;
        }

        internal double Quantite { get; }
        internal Monnaie Monnaie { get; }
        internal Richesse(double qtte, Monnaie monnaie)
        {
            Quantite = qtte;
            Monnaie = monnaie;
        }

        internal double PourcentageDeGain(Richesse other)
        {
            if (other.Monnaie != Monnaie)
                throw new NotSupportedException(string.Format("impossible de comparer des {0} avec des {1}", Monnaie.Nom, other.Monnaie.Nom));
            return (Quantite / other.Quantite) * 100 - 100;
        }

        public override string ToString()
        {
            return Quantite + " " + Monnaie.Nom;
        }
    }
}
