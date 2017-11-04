using System;

namespace Kraken
{
    internal abstract class AnomalieTrade
    {
        protected int nombreDeTrade;
        public Richesse Investissement { get; protected set; }
        public Richesse ApresTrade { get; protected set; }
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
    }
}