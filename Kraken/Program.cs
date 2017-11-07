using Jayrock.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kraken
{
    class Program
    {



        static void Main(string[] args)
        {
            Tests();
            Console.WriteLine("écrivez \"stop\" ou \"gains\" ou \"gainsFee\" ou \"transactions\" ou \"transactionsFee\" et appuyez sur entrer pour avoir l'affichage des gains depuis le début ou arrêter");
            Console.WriteLine("appuyez sur entrer pour continuer");
            Console.ReadLine();
            Console.WriteLine("c'est parti !");
            var portefeuille = new Portefeuille();
            var site = portefeuille.Site;
            string temp = "";
            while (!"stop".Equals(temp)) {
                switch (temp)
                {
                    case "gains":
                        site.WriteGains();
                        break;
                    case "gainsFee":
                        site.WriteGainsWithFee();
                        break;
                    case "transactions":
                        site.WriteTransactions();
                        break;
                    case "transactionsFee":
                        site.WriteTransactionsFee();
                        break;
                }
                temp = Console.ReadLine();
            }
        }

        private static void Tests()
        {
            var client = new KrakenClient();
            string order = Order(client);
            CancelOrder(client, order);
        }

        private static void CancelOrder(KrakenClient client, string order)
        {
            JsonObject ret = client.CancelOrder(order);
        }

        private static string Order(KrakenClient client)
        {
            KrakenOrder k = new KrakenOrder
            {
                Pair = "XETHZUSD",
                Type = OrderType.sell.ToString(),
                OrderType = KrakenOrderType.limit.ToString(),
                Price = 1606,
                Volume = 0.09m
            };
            //k.OFlags = OFlag.viqc.ToString(); //volume in quote currency
            JsonObject ret = client.AddOrder(k);

            if (((JsonArray)ret["error"]).Count == 0)
            {
                return (string)((JsonObject)ret["result"])["txid"];
            }
            throw new Exception();
        }
    }
}
