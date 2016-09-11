using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    internal class TradeCirculaire
    {
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM1 = (investissementM1, m1m2, m2m3, m3m1) => new TradeCirculaire(investissementM1, m1m2, m2m3, m3m1);
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM2 = (etapeM2, m1m2, m2m3, m3m1) => new TradeCirculaire(m1m2, etapeM2, m2m3, m3m1);
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM3 = (etapeM3, m1m2, m2m3, m3m1) => new TradeCirculaire(m1m2, m2m3, etapeM3, m3m1);

        internal Richesse InvestissementM1 { get; }
        internal Richesse EtapeM2 { get; }
        internal Richesse EtapeM3 { get; }
        internal Richesse ObjectifM1 { get; }
        internal Richesse Gain
        {
            get
            {
                try
                {
                    return ObjectifM1 - InvestissementM1;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(this.ToString());
                    throw e;
                }
            }
        }

        internal Richesse GainFee
        {
            get
            {
                return new Richesse(InvestissementM1.Quantite * (PourcentageGain - 0.75) / 100, InvestissementM1.Monnaie);
            }
        }
        internal double PourcentageGain
        {
            get
            {
                if (InvestissementM1.Quantite == 0)
                    return -1;
                return Gain / InvestissementM1;
            }
        }

        internal TradeCirculaire(Richesse richesseInitialeM1, ValeurEchange m1m2, ValeurEchange m2m3, ValeurEchange m3m1)
        {
            InvestissementM1 = richesseInitialeM1;
            EtapeM2 = m1m2.RichesseApresTrade(richesseInitialeM1);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ObjectifM1 = m3m1.RichesseApresTrade(EtapeM3);
        }

        internal TradeCirculaire(ValeurEchange m1m2, Richesse etapeM2, ValeurEchange m2m3, ValeurEchange m3m1)
        {
            InvestissementM1 = m1m2.RichesseAvantTrade(etapeM2);
            EtapeM2 = InvestissementM1.Quantite > 0 ? etapeM2 : new Richesse(0, etapeM2.Monnaie);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ObjectifM1 = m3m1.RichesseApresTrade(EtapeM3);
        }

        internal TradeCirculaire(ValeurEchange m1m2, ValeurEchange m2m3, Richesse etapeM3, ValeurEchange m3m1)
        {
            EtapeM2 = m2m3.RichesseAvantTrade(etapeM3);
            InvestissementM1 = m1m2.RichesseAvantTrade(EtapeM2);
            EtapeM2 = InvestissementM1.Quantite > 0 ? EtapeM2 : new Richesse(0, EtapeM2.Monnaie);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ObjectifM1 = m3m1.RichesseApresTrade(EtapeM3);
        }

        public override string ToString()
        {
            return InvestissementM1 + " --> " + EtapeM2 + " --> " + EtapeM3 + " --> " + ObjectifM1 + " gain : " + Gain + " soit " + PourcentageGain + " %";
        }
    }
}
