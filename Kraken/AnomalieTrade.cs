using System;

namespace Kraken
{
    internal abstract class AnomalieTrade : IComparable<AnomalieTrade>
    {
        protected int nombreDeTrade;
        public Richesse Investissement { get; protected set; }
        public Richesse ApresTrade { get; protected set; }//l'investissement et aprestrade sont en euro
        internal Richesse Gain
        {
            get
            {
                try
                {
                    return ApresTrade - Investissement;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(this.ToString());
                    throw e;
                }
            }
        }

        internal Richesse GainFee => new Richesse(Investissement.Quantite * (PourcentageGain - 0.0025 * nombreDeTrade), Investissement.Monnaie);

        internal double PourcentageGain
        {
            get
            {
                if (Investissement.Quantite == 0)
                    return -1;
                return Gain / Investissement;
            }
        }

        protected AnomalieTrade(int nombreDeTrade)
        {
            this.nombreDeTrade = nombreDeTrade;
        }

        internal abstract void Execute(Site site);

        public int CompareTo(AnomalieTrade other)
        {
            return GainFee.CompareTo(other.GainFee);
        }
    }
}