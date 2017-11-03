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
    }
}
