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
            Console.WriteLine("écrivez \"stop\" ou \"gains\" et appuyez sur entrer pour avoir l'affichage des gains depuis le début ou arrêter");
            Console.WriteLine("appuyez sur entrer pour continuer");
            Console.ReadLine();
            Console.WriteLine("c'est parti !");
            Site site = new Site();
            string temp = "";
            while (!"stop".Equals(temp)) {
                if ("gains".Equals(temp))
                {
                    site.WriteGains();
                }
                temp = Console.ReadLine();
            }
        }
    }
}
