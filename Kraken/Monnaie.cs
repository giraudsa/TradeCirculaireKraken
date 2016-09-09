using Jayrock.Json;
using Jayrock.Json.Conversion.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    class Monnaie
    {

        internal string NomId { get; } //ex : XETH
        internal string Nom { get; } //ex : ETH
        internal int Decimal { get; }
        internal int DisplayDecimals { get; }

        internal Monnaie(string nomId, JsonObject jsonObject)
        {
            NomId = nomId;
            Nom = (string)jsonObject["altname"];
            Decimal = ((JsonNumber)jsonObject["decimals"]).ToInt32();
            DisplayDecimals = ((JsonNumber)jsonObject["display_decimals"]).ToInt32();
        }

        public override string ToString()
        {
            return Nom;
        }
    }
}
