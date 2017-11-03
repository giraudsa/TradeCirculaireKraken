using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    internal abstract class Trade
    {
        private readonly Monnaie _monnaieDeBase;
        private readonly Monnaie _monnaieDeQuote;
        private readonly Action _action;
        internal double Montant { get; }

        internal Trade(Monnaie mbase, Monnaie quote, Action action, double montant)
        {
            this._monnaieDeBase = mbase;
            this._monnaieDeQuote = quote;
            this._action = action;
            this.Montant = montant;
        }

        internal abstract bool Execute();

    }

    internal abstract class SimpleTrade : Trade
    {
        public SimpleTrade(Monnaie mbase, Monnaie quote, Action action, double montant) : base(mbase, quote, action, montant) { }
    }

    internal class SimpleMarketTrade : SimpleTrade
    {
        public SimpleMarketTrade(Monnaie mbase, Monnaie quote, Action action, double montant) : base(mbase, quote, action, montant) { }

        internal override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
