using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    internal class TradeCirculaire : AnomalieTrade
    {
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM1 = (investissementM1, m1m2, m2m3, m3m1) => new TradeCirculaire(investissementM1, m1m2, m2m3, m3m1);
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM2 = (etapeM2, m1m2, m2m3, m3m1) => new TradeCirculaire(m1m2, etapeM2, m2m3, m3m1);
        internal static Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurFromM3 = (etapeM3, m1m2, m2m3, m3m1) => new TradeCirculaire(m1m2, m2m3, etapeM3, m3m1);


        internal Richesse EtapeM2 { get; }
        internal Richesse EtapeM3 { get; }


        internal TradeCirculaire(Richesse richesseInitialeM1, ValeurEchange m1m2, ValeurEchange m2m3, ValeurEchange m3m1) : base(3)
        {
            Investissement = richesseInitialeM1;
            EtapeM2 = m1m2.RichesseApresTrade(richesseInitialeM1);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ApresTrade = m3m1.RichesseApresTrade(EtapeM3);
        }

        internal TradeCirculaire(ValeurEchange m1m2, Richesse etapeM2, ValeurEchange m2m3, ValeurEchange m3m1) : base(3)
        {
            Investissement = m1m2.RichesseAvantTrade(etapeM2);
            EtapeM2 = Investissement.Quantite > 0 ? etapeM2 : new Richesse(0, etapeM2.Monnaie);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ApresTrade = m3m1.RichesseApresTrade(EtapeM3);
        }

        internal TradeCirculaire(ValeurEchange m1m2, ValeurEchange m2m3, Richesse etapeM3, ValeurEchange m3m1) : base(3)
        {
            EtapeM2 = m2m3.RichesseAvantTrade(etapeM3);
            Investissement = m1m2.RichesseAvantTrade(EtapeM2);
            EtapeM2 = Investissement.Quantite > 0 ? EtapeM2 : new Richesse(0, EtapeM2.Monnaie);
            EtapeM3 = m2m3.RichesseApresTrade(EtapeM2);
            ApresTrade = m3m1.RichesseApresTrade(EtapeM3);
        }

        public override string ToString()
        {
            return Investissement + " --> " + EtapeM2 + " --> " + EtapeM3 + " --> " + ApresTrade + " gain : " + Gain+ " soit " + PourcentageGain+ " %";
        }
    }
}
