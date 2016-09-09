using C5;
using Jayrock.Json;
using System;
using System.Collections.Generic;

namespace Kraken
{
    internal class Profondeur
    {
        private Monnaie monnaieDeBase;
        private Monnaie monnaieDeQuote;


        private List<Position> PositionsAchatBase { get; } = new List<Position>();
        private List<Position> PositionsVenteBase { get; } = new List<Position>();
        internal Profondeur(Monnaie monnaieDeBase, Monnaie monnaieDeQuote)
        {
            this.monnaieDeBase = monnaieDeBase;
            this.monnaieDeQuote = monnaieDeQuote;
        }

        internal void MetAJour(string idName)
        {
            var json = Site.client.GetOrderBook(idName);
            var jsonResult =(JsonObject)((JsonObject)json["result"])[idName];
            var jsonAsks = (JsonArray)jsonResult["asks"];
            var jsonBids = (JsonArray)jsonResult["bids"];
            PositionsAchatBase.Clear();
            PositionsVenteBase.Clear();
            foreach (JsonArray jsonAsk in jsonAsks)
            {
                PositionsAchatBase.Add(new Position(jsonAsk, monnaieDeBase, monnaieDeQuote));
            }
            foreach (JsonArray jsonBid in jsonBids)
            {
                PositionsVenteBase.Add(new Position(jsonBid, monnaieDeBase, monnaieDeQuote));
            }
        }

        public override string ToString()
        {
            return "Profondeur entre " + PositionsAchatBase[0] + " et " + PositionsVenteBase[0];
        }


        internal Richesse RichesseApresTrade(Richesse richesseAvantTrade)
        {
            List<Position> positions = PositionsVente(richesseAvantTrade.Monnaie);
            Func<Position, double> volumeMaxVendue = richesseAvantTrade.Monnaie == monnaieDeBase ? Position.getVolumeBase : Position.getVolumeQuote;
            Func<Position, double, double> volumeAchetePour = richesseAvantTrade.Monnaie == monnaieDeBase ? Position.volumeQuotePour : Position.volumeBasePour;
            Monnaie monnaieAAcheter = richesseAvantTrade.Monnaie == monnaieDeBase ? monnaieDeQuote : monnaieDeBase;
            double qtteAVendre = richesseAvantTrade.Quantite;
            double qtteAchetee = 0;
            int i = 0;
            while (qtteAVendre > 0 && i < positions.Count)
            {
                Position positionEnCours = positions[i++];
                double volumeVendu = Math.Min(volumeMaxVendue(positionEnCours), qtteAVendre);
                qtteAVendre -= volumeVendu;
                qtteAchetee += volumeAchetePour(positionEnCours, volumeVendu);
            }
            return new Richesse(qtteAchetee, monnaieAAcheter);
        }

        internal Richesse RichesseAvantTrade(Richesse richesseApresTrade)
        {
            List<Position> positions = PositionsAchat(richesseApresTrade.Monnaie);
            Func<Position, double> volumeMaxAchetee = richesseApresTrade.Monnaie == monnaieDeBase ? Position.getVolumeBase : Position.getVolumeQuote;
            Func<Position, double, double> volumeVenduPour = richesseApresTrade.Monnaie == monnaieDeBase ? Position.volumeQuotePour : Position.volumeBasePour;
            Monnaie monnaieVendue = richesseApresTrade.Monnaie == monnaieDeBase ? monnaieDeQuote : monnaieDeBase;
            double qtteVendue = 0;
            double qtteAchetee = richesseApresTrade.Quantite;
            int i = 0;
            while (qtteAchetee > 0 && i < positions.Count)
            {
                Position positionEnCours = positions[i++];
                double volumeAchete = Math.Min(volumeMaxAchetee(positionEnCours), qtteAchetee);
                qtteAchetee -= volumeAchete;
                qtteVendue += volumeVenduPour(positionEnCours, volumeAchete);
            }
            if (qtteAchetee > 0) qtteVendue = 0; //le trade n'aurait pas été possible
            return new Richesse(qtteVendue, monnaieVendue);
        }

        internal List<Position> PositionsVente(Monnaie monnaieAVendre)
        {
            return monnaieAVendre == monnaieDeBase ? PositionsVenteBase : PositionsAchatBase;
        }
        internal List<Position> PositionsAchat(Monnaie monnaieAchetee)
        {
            Monnaie monnaieAVendre = monnaieAchetee == monnaieDeBase ? monnaieDeQuote : monnaieDeBase;
            return PositionsVente(monnaieAVendre);
        }
    }
}