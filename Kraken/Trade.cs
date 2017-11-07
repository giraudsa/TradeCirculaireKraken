using Jayrock.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    //XBTUSD : Base = XBT, Quote = USD
    internal abstract class Trade
    {
        protected readonly ValeurEchange pair;
        protected readonly OrderType action; //buy or sell
        protected readonly Richesse objetRichesse;

        internal Trade(ValeurEchange pair, OrderType action, Richesse objetRichesse)
        {
            this.pair = pair;
            this.action = action;
            this.objetRichesse = objetRichesse;
        }

        internal abstract bool Execute(Site site);

    }

    internal abstract class SimpleTrade : Trade
    {
        public SimpleTrade(ValeurEchange pair, OrderType action, Richesse objetRichesse) : base(pair, action, objetRichesse) { }
    }

    internal class SimpleMarketTrade : SimpleTrade
    {
        public SimpleMarketTrade(ValeurEchange pair, OrderType action, Richesse objetRichesse) : base(pair, action, objetRichesse) { }

        internal override bool Execute(Site site)
        {
            // La crypto est la monnaie de base
            string type = action.ToString();
            decimal volume = Convert.ToDecimal(objetRichesse.Quantite);
            KrakenOrder k = new KrakenOrder
            {
                Pair = pair.IdName,
                Type = type,
                OrderType = KrakenOrderType.market.ToString(),
                Volume = volume
            };
            JsonObject json = site.AddOrder(k);
            if (((JsonArray)json["error"]).Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
