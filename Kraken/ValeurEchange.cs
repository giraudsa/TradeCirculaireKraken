using C5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;

namespace Kraken
{
    class ValeurEchange
    {

        //EURUSD : Base = EURO, Quote = USD
        internal string IdName { get; }
        internal string Name { get; }
        internal Monnaie MonnaieDeBase { get; }
        internal Monnaie MonnaieDeQuote { get; }
        internal int PairDecimals { get; private set; }
        internal int LotDecimals { get; private set; }
        internal int LotMultiplier { get; private set; }
        internal TreeSet<Fee> Fees { get; private set; }
        internal TreeSet<Fee> FeesMaker { get; private set; }
        internal Monnaie FeeVolumeMonnaie { get; private set; }
        internal int MarginCall { get; private set; }
        internal int MarginStop { get; private set; }
        private Profondeur Profondeur { get; }

        internal ValeurEchange(string idName, Monnaie monnaieDeBase, Monnaie monnaieDeQuote, Monnaie feeVolumeMonnaie, JsonObject valeurEchange)
        {
            IdName = idName;
            MonnaieDeBase = monnaieDeBase;
            MonnaieDeQuote = monnaieDeQuote;
            FeeVolumeMonnaie = feeVolumeMonnaie;
            Name = (string)valeurEchange["altname"];
            LotDecimals = ((JsonNumber)valeurEchange["lot_decimals"]).ToInt32();
            PairDecimals = ((JsonNumber)valeurEchange["pair_decimals"]).ToInt32();
            LotMultiplier = ((JsonNumber)valeurEchange["lot_multiplier"]).ToInt32();
            Fees = new TreeSet<Fee>();
            foreach (JsonArray feeJson in (JsonArray)valeurEchange["fees"])
            {
                Fees.Add(new Fee(feeJson));
            }
            FeesMaker = new TreeSet<Fee>();
            var feesMakerJson = (JsonArray)valeurEchange["fees_maker"];
            if (feesMakerJson == null)
                return;
            foreach (JsonArray feeJson in feesMakerJson)
            {
                FeesMaker.Add(new Fee(feeJson));
            }
            MarginCall = ((JsonNumber)valeurEchange["margin_call"]).ToInt32();
            MarginStop = ((JsonNumber)valeurEchange["margin_stop"]).ToInt32();
            Profondeur = new Profondeur(MonnaieDeBase, MonnaieDeQuote);
            MetAJourProfondeur();
        }

        private void MetAJourProfondeur()
        {
            Profondeur.MetAJour(IdName, this);
        }

        internal Fee GetFee(double volume)
        {
            return Fees.WeakPredecessor(new Fee(volume));
        }

        private Fee GetFeeMaker(int volume)
        {
            return FeesMaker.WeakPredecessor(new Fee(volume));
        }
        public override string ToString()
        {
            return base.ToString() + " " + Name;
        }
        internal Richesse GetRichesseToTrade(Monnaie m1, int i)
        {
            List<Position> l = Profondeur.PositionsVente(m1);
            Func<Position, double> volume = m1 == MonnaieDeBase ? Position.getVolumeBase : Position.getVolumeQuote;
            double qtte = 0;
            for (int j = 0; j <= i && j < l.Count - 1; j++)
            {
                qtte += volume(l[j]);
            }
            return new Richesse(qtte, m1);
        }
        internal void MetAJour()
        {
            Profondeur.MetAJour(IdName, this);
        }

        internal Richesse RichesseApresTrade(Richesse richesseAvantTrade)
        {
            return Profondeur.RichesseApresTrade(richesseAvantTrade);
        }

        internal Richesse RichesseAvantTrade(Richesse richesseApresTrade)
        {
            return Profondeur.RichesseAvantTrade(richesseApresTrade);
        }
    }
}
