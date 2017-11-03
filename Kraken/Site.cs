using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;
using System.Threading;
using System.Runtime.CompilerServices;
using static Kraken.Monnaie;
using C5;

namespace Kraken
{
    class Site
    {

        internal static KrakenClient client = new KrakenClient();
        private InterchangableBiKeyDictionnary<Monnaie, ValeurEchange> BaseEtQuoteToVe { get; }
        private System.Collections.Generic.HashSet<Monnaie> MonnaieTradable { get; } = new System.Collections.Generic.HashSet<Monnaie>();
        private System.Collections.Generic.HashSet<Monnaie> MonnaieTradableEURUSD { get; } = new System.Collections.Generic.HashSet<Monnaie>();
        internal List<ValeurEchange> Pairs { get; } = new List<ValeurEchange>();

        private Dictionary<Monnaie, Richesse> gains = new Dictionary<Monnaie, Richesse>();
        private Dictionary<Monnaie, Richesse> gainsFee = new Dictionary<Monnaie, Richesse>();
        private List<AnomalieTrade> transactions = new List<AnomalieTrade>();
        private List<AnomalieTrade> transactionsFee = new List<AnomalieTrade>();

        internal void WriteGains()
        {
            lock (gains)
            {
                if (gains.Count == 0) Console.WriteLine("Rien de gagné jusqu'à présent :(");
                foreach (Richesse richesse in gains.Values)
                {
                    Console.WriteLine(richesse);
                }
            }
        }

        internal void WriteTransactions()
        {
            lock (transactions)
            {
                if (transactions.Count == 0) Console.WriteLine("pas de transaction jusqu'à présent :(");
                foreach (AnomalieTrade trade in transactions)
                {
                    Console.WriteLine(trade);
                }
            }
        }

        internal void WriteTransactionsFee()
        {
            lock (transactionsFee)
            {
                if (transactionsFee.Count == 0) Console.WriteLine("pas de transaction jusqu'à présent :(");
                foreach (AnomalieTrade trade in transactionsFee)
                {
                    Console.WriteLine(trade);
                }
            }
        }

        internal void WriteGainsWithFee()
        {
            lock (gainsFee)
            {
                if (gainsFee.Count == 0) Console.WriteLine("Rien de gagné jusqu'à présent :(");
                foreach (Richesse richesse in gainsFee.Values)
                {
                    Console.WriteLine(richesse);
                }
            }
        }

        internal Site()
        {
            BaseEtQuoteToVe = new InterchangableBiKeyDictionnary<Monnaie, ValeurEchange>();
            var requeteMonnaies = client.GetActiveAssets();
            var listeMonnaies = (JsonObject)requeteMonnaies["result"];
            foreach (string key in listeMonnaies.Names)
            {
                new Monnaie(key, (JsonObject)listeMonnaies[key]);
            }
            var requeteValeursEchanges = client.GetAssetPairs();
            var valeursEchanges = (JsonObject)requeteValeursEchanges["result"];
            Parallel.ForEach(valeursEchanges.Names.Cast<string>(), (key) =>//foreach (string key in valeursEchanges.Names)
            {
                if (!key.Contains(".d"))
                {
                    var valeurEchange = (JsonObject)valeursEchanges[key];
                    Monnaie monnaieDeBase = GetMonnaie((string)valeurEchange["base"]);
                    Monnaie monnaieDeQuote = GetMonnaie((string)valeurEchange["quote"]);
                    Monnaie feeVolumeMonnaie = GetMonnaie((string)valeurEchange["fee_volume_currency"]);
                    var ve = new ValeurEchange(key, monnaieDeBase, monnaieDeQuote, feeVolumeMonnaie, valeurEchange);
                    if (!ve.FeesMaker.IsEmpty)
                    {
                        AddDictionnaire(monnaieDeBase, monnaieDeQuote, ve);
                    }
                }
            });
            Parallel.ForEach(MonnaieTradable, (monnaie) =>
            {
                if(BaseEtQuoteToVe.ContainsKeys(monnaie, EURO) && BaseEtQuoteToVe.ContainsKeys(monnaie, USD))
                {
                    MonnaieTradableEURUSD.Add(monnaie);
                }
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddDictionnaire(Monnaie mbase, Monnaie quote, ValeurEchange ve)
        {
            BaseEtQuoteToVe.Add(mbase, quote, ve);
            Pairs.Add(ve);
            MonnaieTradable.Add(mbase);
            MonnaieTradable.Add(quote);
        }

        internal void MetAJour()
        {
            Parallel.ForEach(Pairs, (pair) =>
            {
                if(pair.MonnaieDeQuote == EURO || pair.MonnaieDeQuote == USD)
                    pair.MetAJour();
            });
        }

        internal Monnaie GetMonnaie(string idName)
        {
            return Monnaie.GetMonnaie(idName);
        }

        internal List<TradePivot> TrouveMeilleurEchangeEURUSD(Richesse dispoEuro, Richesse dispoUSD)
        {
            List<TradePivot> ret = new List<TradePivot>();
            foreach(Monnaie monnaiePivot in MonnaieTradableEURUSD)
            {
                if (BaseEtQuoteToVe.TryGetValue(EURO, monnaiePivot, out ValeurEchange veEURO) && BaseEtQuoteToVe.TryGetValue(USD, monnaiePivot, out ValeurEchange veUSD))
                {
                    TradePivot bestTrade = MeilleurEchangeDepuis(dispoEuro, veEURO, veUSD, monnaiePivot);
                    if (bestTrade == null) bestTrade = MeilleurEchangeDepuis(dispoUSD, veEURO, veUSD, monnaiePivot);
                    if (bestTrade != null) ret.Add(bestTrade);
                }
            }
            return ret;
        }

        private static TradePivot MeilleurEchangeDepuis( Richesse dispo, ValeurEchange veEURO, ValeurEchange veUSD, Monnaie monnaiePivot)
        {
            Monnaie monnaieInitiale = dispo.Monnaie; // EURO ou USD
            var newTradePivot = monnaieInitiale == EURO ? TradePivot.newTradeEurUsd : TradePivot.newTradeUsdEur;
            ValeurEchange valeurEchangeInitiale = monnaieInitiale == EURO ? veEURO : veUSD;
            ValeurEchange valeurEchangeFinale = monnaieInitiale == EURO ? veUSD : veEURO;

            TradePivot tmp = null;
            Richesse richesseToTrade = new Richesse(0, monnaieInitiale);
            Richesse etape;
            TradePivot bestTrade = newTradePivot(richesseToTrade, veEURO, veUSD);
            int i = 0, j = 0;
            while (tmp == null || tmp.Gain > bestTrade.Gain)
            {
                if (tmp != null)
                    bestTrade = tmp;
                etape = TrouveProchaineEtape(monnaieInitiale, monnaiePivot, valeurEchangeInitiale, valeurEchangeFinale, ref i, ref j);
                richesseToTrade = etape < dispo ? etape : dispo;
                tmp = newTradePivot(richesseToTrade, veEURO, veUSD);
            }
            if (bestTrade.Gain.Quantite > 0)
                return bestTrade;
            return null;
        }

        private static Richesse TrouveProchaineEtape(Monnaie monnaieInitiale, Monnaie monnaiePivot, ValeurEchange valeurEchangeInitiale, ValeurEchange valeurEchangeFinale, ref int i, ref int j)
        {
            Richesse etapeSensNaturelle = valeurEchangeInitiale.GetRichesseToTrade(monnaieInitiale, i);
            Richesse etapeSensInverse = valeurEchangeInitiale.RichesseAvantTrade(valeurEchangeFinale.GetRichesseToTrade(monnaiePivot, j));
            if(etapeSensNaturelle < etapeSensInverse)
            {
                ++i;
                return etapeSensNaturelle;
            }
            ++j;
            return etapeSensInverse;
        }

        internal void TrouveMeilleurEchangeCirculaire(object boolMetAJour)
        {
            try
            {
                CycliqueTripleKeyDictionary<Monnaie, TradeCirculaire> dicoCyclesPossibles = new CycliqueTripleKeyDictionary<Monnaie, TradeCirculaire>();
                if ((bool)boolMetAJour)
                    MetAJour();
                foreach (var m1 in MonnaieTradable)
                {
                    foreach (var m2 in MonnaieTradable)
                    {
                        if (m2 == m1)
                            continue;
                        foreach (var m3 in MonnaieTradable)
                        {
                            if (m3 == m1 || m3 == m2)
                                continue;
                            if (dicoCyclesPossibles.TryGetValue(m1, m2, m3, out TradeCirculaire tmp)) //deja calculé
                                continue;
                            tmp = CalculTradeCirculaire(m1, m2, m3);
                            dicoCyclesPossibles.Add(m1, m2, m3, tmp);
                        }
                    }
                }
                ExecuteTrade(dicoCyclesPossibles.Values);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("probleme reseau" + e);
            }
        }

        internal void ExecuteTrade(IEnumerable<AnomalieTrade> trades)
        {
            Console.Out.WriteLine("Mise à jour de " + DateTime.UtcNow.ToShortTimeString());
            foreach (AnomalieTrade trade in trades)
            {
                if (trade != null && trade.Gain.Quantite > 0)
                {
                    AjouteGain(trade.Gain, gains);
                    AjouteTrade(trade, transactions);
                    if (trade.GainFee.Quantite > 0)
                    {
                        AjouteGain(trade.GainFee, gainsFee);
                        AjouteTrade(trade, transactionsFee);
                    }
                }
            }
        }

        private void AjouteTrade(AnomalieTrade trade, List<AnomalieTrade> liste)
        {
            lock (liste)
            {
                liste.Add(trade);
            }
        }

        private void AjouteGain(Richesse gain, Dictionary<Monnaie, Richesse> dico)
        {
            lock (dico)
            {
                if (!dico.TryGetValue(gain.Monnaie, out Richesse  dejaLa))
                    dico.Add(gain.Monnaie, gain);
                else dico[gain.Monnaie] = dejaLa + gain;
            }
        }

        private TradeCirculaire CalculTradeCirculaire(Monnaie m1, Monnaie m2, Monnaie m3)
        {
            if (!BaseEtQuoteToVe.TryGetValue(m1, m2, out ValeurEchange ve12)
                || !BaseEtQuoteToVe.TryGetValue(m2, m3, out ValeurEchange ve23)
                || !BaseEtQuoteToVe.TryGetValue(m3, m1, out ValeurEchange ve31))
                return null;
            TradeCirculaire partantDeM1 = CalculTradeCirculaireEnPartantDe(m1, ve12, ve12, ve23, ve31, TradeCirculaire.constructeurFromM1);
            TradeCirculaire partantDeM2 = CalculTradeCirculaireEnPartantDe(m2, ve23, ve12, ve23, ve31, TradeCirculaire.constructeurFromM2);
            TradeCirculaire partantDeM3 = CalculTradeCirculaireEnPartantDe(m3, ve31, ve12, ve23, ve31, TradeCirculaire.constructeurFromM3);

            TradeCirculaire best = partantDeM1;
            if (best.Gain< partantDeM2.Gain)
                best = partantDeM2;
            if (best.Gain< partantDeM3.Gain)
                best = partantDeM3;
            return best;
        }

        private TradeCirculaire CalculTradeCirculaireEnPartantDe(Monnaie m, ValeurEchange veInitiale, ValeurEchange ve12, ValeurEchange ve23, ValeurEchange ve31,
            Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurDepuisEtape)
        {
            TradeCirculaire tmp = null;
            TradeCirculaire bestTrade = constructeurDepuisEtape(new Richesse(0, m), ve12, ve23, ve31);
            int i = 0;
            while (tmp == null || tmp.Gain> bestTrade.Gain)
            {
                if (tmp != null)
                    bestTrade = tmp;
                Richesse etape = veInitiale.GetRichesseToTrade(m, i++);
                tmp = constructeurDepuisEtape(etape, ve12, ve23, ve31);
            }
            return bestTrade;
        }
    }
}
