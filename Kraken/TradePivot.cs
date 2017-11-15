using System;

namespace Kraken
{
    internal abstract class TradePivot : AnomalieTrade
    {
        internal static Func<Richesse, ValeurEchange, ValeurEchange, TradePivot> newTradeEurUsd = (Richesse richesseEurToTrad, ValeurEchange veEURO, ValeurEchange veUSD) =>  new TradeEurUsd(richesseEurToTrad, veEURO, veUSD); 
        internal static Func<Richesse, ValeurEchange, ValeurEchange, TradePivot> newTradeUsdEur = (Richesse richesseEurToTrad, ValeurEchange veEURO, ValeurEchange veUSD) =>  new TradeUsdEur(richesseEurToTrad, veEURO, veUSD);

        private readonly Monnaie monnaiePivot;
        protected Richesse EtapePivot { get; set; }
        protected readonly ValeurEchange veEuro;
        protected readonly ValeurEchange veUSD;

        protected abstract ValeurEchange GetValeurEchangeBuyPivot();
        protected abstract ValeurEchange GetValeurEchangeSellPivot();


        internal TradePivot(ValeurEchange veEURO, ValeurEchange veUSD) : base(2)
        {
            this.veEuro = veEURO;
            this.veUSD = veUSD;
            monnaiePivot = veEURO.MonnaieDeQuote == Monnaie.EURO ? veEURO.MonnaieDeBase : veEURO.MonnaieDeQuote;
        }

        internal override void Execute(Site site)
        {
            Richesse pivotAvecMarge = EtapePivot * (1 - 0.0025);
            SimpleMarketTrade tradeBuyPivot = new SimpleMarketTrade(GetValeurEchangeBuyPivot(), OrderType.buy, pivotAvecMarge);
            SimpleMarketTrade tradeSellPivot = new SimpleMarketTrade(GetValeurEchangeSellPivot(), OrderType.sell, pivotAvecMarge);
            bool ok1, ok2 = false;
            try
            {
                ok1 = tradeBuyPivot.Execute(site);
                int compteur = 0;
                while (ok1 && !ok2 || compteur++ < 5)
                    ok2 = tradeSellPivot.Execute(site);
                if (ok1 && !ok2)
                {
                    Portefeuille.EnvoyerMail(new System.Net.Mail.MailAddress("giraudsa@hotmail.com"), "probleme dans le deuxieme trades Kraken", "http://www.kraken.com/login on est coincé avec " + pivotAvecMarge.ToString());
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Portefeuille.EnvoyerMail(new System.Net.Mail.MailAddress("giraudsa@hotmail.com"), "probleme dans les trades Kraken", "http://www.kraken.com/login je ne sais pas ou on en est");
                throw;
            }
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
            return richesseUsdToTrade + " correspondant à " + Investissement + " --> " + EtapePivot + " --> " + ApresTrade + " gain : " + Gain + " soit " + PourcentageGain * 100 + " %";
        }

        protected override ValeurEchange GetValeurEchangeBuyPivot()
        {
            return veUSD;
        }

        protected override ValeurEchange GetValeurEchangeSellPivot()
        {
            return veEuro;
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
            return  Investissement + " --> " + EtapePivot + " --> " + richesseUsdApresTrade + " correspondant à " + ApresTrade + " gain : " + Gain + " soit " + PourcentageGain * 100 + " %";
        }

        protected override ValeurEchange GetValeurEchangeBuyPivot()
        {
            return veEuro;
        }

        protected override ValeurEchange GetValeurEchangeSellPivot()
        {
            return veUSD;
        }
    }
}
