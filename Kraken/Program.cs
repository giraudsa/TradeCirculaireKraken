using Jayrock.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kraken
{
    class Program
    {
        private static void Main(string[] args)
        {
            //TestsAsync();
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

        private static void TestsAsync()
        {
            Portefeuille.EnvoyerMail(new System.Net.Mail.MailAddress("giraudsa@hotmail.com"), "coucou", "voila");
            var client = new KrakenClient();
            var tr = client.GetOpenOrders(true, "-1338747254");
            if (((JsonArray)tr["error"]).Count > 0)
                Console.WriteLine("erreur");
            else if (((JsonObject)((JsonObject)tr["result"])["open"]).Names.Count == 0)
                Console.WriteLine("pas passé");
            else
                Console.WriteLine("oK");
            List<string> orders = Order(client);
            foreach(string order in orders)
                CancelOrder(client, order);
        }

        private static void CancelOrder(KrakenClient client, string order)
        {
            JsonObject ret = client.CancelOrder(order);
        }

        private static List<string> Order(KrakenClient client)
        {
            var userref = (int)DateTime.UtcNow.Ticks; //user reference id.  32 - bit signed number.  (optional)
            var k = new KrakenOrder
            {
                Pair = "XETHZUSD",
                Type = OrderType.sell.ToString(),
                OrderType = KrakenOrderType.limit.ToString(),
                Price = 1606,
                Volume = 0.09m,
                Userref = userref.ToString()
            };
            //k.OFlags = OFlag.viqc.ToString(); //volume in quote currency
            JsonObject ret = client.AddOrder(k);

            if (((JsonArray)ret["error"]).Count == 0)
            {
                List<string> rettour = new List<string>();
                foreach(string s in (JsonArray)((JsonObject)ret["result"])["txid"])
                {
                    rettour.Add(s);
                }
                return rettour;
            }
            throw new Exception();
        }
    }
}
