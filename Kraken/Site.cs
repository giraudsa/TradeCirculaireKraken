using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jayrock.Json;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Kraken
{
    class Site
    {
        internal static KrakenClient client = new KrakenClient();

        private Dictionary<Monnaie, Richesse> gains = new Dictionary<Monnaie, Richesse>();
        private Dictionary<Monnaie, Richesse> gainsFee = new Dictionary<Monnaie, Richesse>();
        private List<TradeCirculaire> transactions = new List<TradeCirculaire>();
        private List<TradeCirculaire> transactionsFee = new List<TradeCirculaire>();
        private InterchangableBiKeyDictionnary<Monnaie, ValeurEchange> BaseEtQuoteToVe { get; }
        private HashSet<Monnaie> monnaieTradable { get; } = new HashSet<Monnaie>();
        internal List<ValeurEchange> pairs { get; } = new List<ValeurEchange>();
        internal Dictionary<string, Monnaie> monnaies { get; }

        internal void WriteGains()
        {
            lock (gains)
            {
                if (gains.Count == 0) Console.WriteLine("Rien de gagné jusqu'à présent :(");
                foreach(Richesse richesse in gains.Values)
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
                foreach (TradeCirculaire trade in transactions)
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
                foreach (TradeCirculaire trade in transactionsFee)
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

        private Timer metAJour;
        private TimeSpan refreshPooling = new TimeSpan(0, 2, 0);//2 min

        internal Site()
        {
            BaseEtQuoteToVe = new InterchangableBiKeyDictionnary<Monnaie, ValeurEchange>();
            var requeteMonnaies = client.GetActiveAssets();
            var listeMonnaies = (JsonObject)requeteMonnaies["result"];
            monnaies = new Dictionary<string, Monnaie>();
            foreach (string key in listeMonnaies.Names)
            {
                monnaies.Add(key, new Monnaie(key, (JsonObject)listeMonnaies[key]));
            }
            var requeteValeursEchanges = client.GetAssetPairs();
            var valeursEchanges = (JsonObject)requeteValeursEchanges["result"];
            Parallel.ForEach(valeursEchanges.Names.Cast<string>(), (key) =>//foreach (string key in valeursEchanges.Names)
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
            });
            metAJour = new Timer(TrouveMeilleurEchange, true, new TimeSpan(0), refreshPooling);

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddDictionnaire(Monnaie mbase, Monnaie quote, ValeurEchange ve)
        {
            BaseEtQuoteToVe.Add(mbase, quote, ve);
            pairs.Add(ve);
            monnaieTradable.Add(mbase);
            monnaieTradable.Add(quote);
        }

        internal void MetAJour()
        {
            Parallel.ForEach(pairs, (pair) =>
            {
                pair.MetAJour();
            });
        }

        internal Monnaie GetMonnaie(string idName)
        {
            Monnaie ret = null;
            monnaies.TryGetValue(idName, out ret);
            return ret;
        }

        internal void TrouveMeilleurEchange(object boolMetAJour)
        {
            try
            {
                CycliqueTripleKeyDictionary<Monnaie, TradeCirculaire> dicoCyclesPossibles = new CycliqueTripleKeyDictionary<Monnaie, TradeCirculaire>();
                if ((bool)boolMetAJour)
                    MetAJour();
                foreach (var m1 in monnaieTradable)
                {
                    foreach (var m2 in monnaieTradable)
                    {
                        if (m2 == m1)
                            continue;
                        foreach (var m3 in monnaieTradable)
                        {
                            if (m3 == m1 || m3 == m2)
                                continue;
                            TradeCirculaire tmp;
                            if (dicoCyclesPossibles.TryGetValue(m1, m2, m3, out tmp)) //deja calculé
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

        private void ExecuteTrade(IEnumerable<TradeCirculaire> trades)
        {
            Console.Out.WriteLine("Mise à jour de " + DateTime.UtcNow.ToShortTimeString());
            foreach (TradeCirculaire trade in trades)
            {
                if (trade != null && trade.Gain.Quantite > 0)
                {
                    AjouteGain(trade.Gain, gains);
                    AjouteTrade(trade, transactions);
                    if(trade.GainFee.Quantite > 0)
                    {
                        AjouteGain(trade.GainFee, gainsFee);
                        AjouteTrade(trade, transactionsFee);
                    }
                }
            }
        }

        private void AjouteTrade(TradeCirculaire trade, List<TradeCirculaire> liste)
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
                Richesse dejaLa = null;
                if (!dico.TryGetValue(gain.Monnaie, out dejaLa))
                    dico.Add(gain.Monnaie, gain);
                else dico[gain.Monnaie] = dejaLa + gain;
            }
        }

        private TradeCirculaire CalculTradeCirculaire(Monnaie m1, Monnaie m2, Monnaie m3)
        {
            ValeurEchange ve1, ve2, ve3;
            if (!BaseEtQuoteToVe.TryGetValue(m1, m2, out ve1)
                || !BaseEtQuoteToVe.TryGetValue(m2, m3, out ve2)
                || !BaseEtQuoteToVe.TryGetValue(m3, m1, out ve3))
                return null;
            TradeCirculaire partantDeM1 = CalculTradeCirculaireEnPartantDe(m1, ve1, ve2, ve3, TradeCirculaire.constructeurFromM1);
            TradeCirculaire partantDeM2 = CalculTradeCirculaireEnPartantDe(m2, ve1, ve2, ve3, TradeCirculaire.constructeurFromM2);
            TradeCirculaire partantDeM3 = CalculTradeCirculaireEnPartantDe(m3, ve1, ve2, ve3, TradeCirculaire.constructeurFromM3);

            TradeCirculaire best = partantDeM1;
            if (best.Gain < partantDeM2.Gain)
                best = partantDeM2;
            if (best.Gain < partantDeM3.Gain)
                best = partantDeM3;
            return best;
        }

        private TradeCirculaire CalculTradeCirculaireEnPartantDe(Monnaie m, ValeurEchange ve1, ValeurEchange ve2, ValeurEchange ve3,
            Func<Richesse, ValeurEchange, ValeurEchange, ValeurEchange, TradeCirculaire> constructeurDepuisEtape)
        {
            TradeCirculaire tmp = null;
            TradeCirculaire bestTrade = constructeurDepuisEtape(new Richesse(0, m), ve1, ve2, ve3);
            int i = 0;
            while (tmp == null || tmp.Gain > bestTrade.Gain)
            {
                if (tmp != null)
                    bestTrade = tmp;
                Richesse etape = ve1.GetRichesseToTrade(m, i++);//TODO c'est pas forcement ve1
                tmp = constructeurDepuisEtape(etape, ve1, ve2, ve3);
            }
            return bestTrade;
        }
    }
}
