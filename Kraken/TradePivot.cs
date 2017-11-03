using System;

namespace Kraken
{
    internal abstract class TradePivot : AnomalieTrade
    {
        internal static Func<Richesse, ValeurEchange, ValeurEchange, TradePivot> newTradeEurUsd = (Richesse richesseEurToTrad, ValeurEchange veEURO, ValeurEchange veUSD) =>  new TradeEurUsd(richesseEurToTrad, veEURO, veUSD); 
        internal static Func<Richesse, ValeurEchange, ValeurEchange, TradePivot> newTradeUsdEur = (Richesse richesseEurToTrad, ValeurEchange veEURO, ValeurEchange veUSD) =>  new TradeUsdEur(richesseEurToTrad, veEURO, veUSD);

        private readonly Monnaie monnaiePivot;
        protected Richesse EtapePivot { get; set; }

        /*private SimpleMarketTrade tradeBuy; 
        private SimpleMarketTrade tradeSell;

        private Richesse richesseConsommee;
        private Richesse richesseProduite;

        private Richesse _richesseConsommeeEnEuro;
        private Richesse _richesseProduiteEnEuro;
        private Richesse _augmentationDeRichesseEnEuro;
        private object lockPourcentageDeGain = new object();
        private bool pourcentageDeGainCalcule = false;
        private double _pourcentageDeGain;
        private Richesse RichesseConsommeeEnEuro
        {
            get
            {
                lock (_richesseConsommeeEnEuro)
                {
                    if (_richesseConsommeeEnEuro == null)
                    {
                        _richesseConsommeeEnEuro = richesseConsommee.Monnaie == Monnaie.EURO ? richesseConsommee : Portefeuille.ConvertUsdEnEuro(richesseConsommee);
                    }
                }
                return _richesseConsommeeEnEuro;
                
            }
        }
        private Richesse RichesseProduiteEnEuro
        {
            get
            {
                lock (_richesseProduiteEnEuro)
                {
                    if (_richesseProduiteEnEuro == null)
                    {
                        _richesseProduiteEnEuro = richesseProduite.Monnaie == Monnaie.EURO ? richesseProduite : Portefeuille.ConvertUsdEnEuro(richesseProduite);
                    }
                }
                return _richesseConsommeeEnEuro;

            }
        }
        private Richesse AugmentationDeRichesseEnEuro
        {
            get
            {
                lock (_augmentationDeRichesseEnEuro)
                {
                    if (_augmentationDeRichesseEnEuro == null)
                    {
                        _augmentationDeRichesseEnEuro = RichesseProduiteEnEuro - RichesseConsommeeEnEuro;
                    }
                }
                return _augmentationDeRichesseEnEuro;

            }
        }

        public double PourcentageDeGain
        {
            get
            {
                lock (lockPourcentageDeGain)
                {
                    if(!pourcentageDeGainCalcule)
                    {
                        _pourcentageDeGain = RichesseConsommeeEnEuro != null ? AugmentationDeRichesseEnEuro / RichesseConsommeeEnEuro : 0;
                        pourcentageDeGainCalcule = true;
                    }
                }
                return _pourcentageDeGain;
            }
        }
        */
        internal TradePivot(ValeurEchange veEURO, ValeurEchange veUSD) : base(2)
        {
            monnaiePivot = veEURO.MonnaieDeQuote == Monnaie.EURO ? veEURO.MonnaieDeBase : veEURO.MonnaieDeQuote;
        }

    }

    internal class TradeUsdEur : TradePivot
    {
        private Richesse richesseUsdToTrade;
        internal TradeUsdEur(Richesse richesseUsdToTrade, ValeurEchange veEURO, ValeurEchange veUSD) : base(veEURO, veUSD)
        {
            Investissement = Portefeuille.ConvertUsdEnEuro(richesseUsdToTrade);
            this.richesseUsdToTrade = richesseUsdToTrade;
            EtapePivot = veUSD.RichesseApresTrade(richesseUsdToTrade);
            ApresTrade = veEURO.RichesseApresTrade(EtapePivot);
        }

        public override string ToString()
        {
            return richesseUsdToTrade + " correspondant à " + Investissement + " --> " + EtapePivot + " --> " + ApresTrade + " gain : " + Gain + " soit " + PourcentageGain + " %";
        }
    }

    internal class TradeEurUsd : TradePivot
    {
        private Richesse richesseUsdApresTrade;
        internal TradeEurUsd(Richesse richesseEurToTrade, ValeurEchange veEURO, ValeurEchange veUSD) : base(veEURO, veUSD)
        {
            Investissement = richesseEurToTrade;
            EtapePivot = veEURO.RichesseApresTrade(richesseEurToTrade);
            richesseUsdApresTrade = veUSD.RichesseApresTrade(EtapePivot);
            ApresTrade = Portefeuille.ConvertUsdEnEuro(richesseUsdApresTrade);
        }

        public override string ToString()
        {
            return  Investissement + " --> " + EtapePivot + " --> " + richesseUsdApresTrade + " correspondant à " + ApresTrade + " gain : " + Gain + " soit " + PourcentageGain + " %";
        }


    }
}
