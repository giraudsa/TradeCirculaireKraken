using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Gmail.v1.UsersResource;

using Google.Apis.Gmail.v1.Data;
using System.Net.Mail;

namespace Kraken
{
    internal class Portefeuille
    {
        private static readonly string USD_EUR_URL = "https://www.revolut.com/api/quote/internal/USDEUR";
        private static readonly DateTime JSON_ORIGINE_DES_DATES = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        internal readonly Site Site;
        private readonly IDictionary<Monnaie, Richesse> _richesses = new Dictionary<Monnaie, Richesse>();
        private readonly Timer _metAJour;
        private readonly TimeSpan _refreshPooling = new TimeSpan(0, 2, 0);//2 min
        public static double USD_EUR;
        private static DateTime dateMiseAJourTauxUsdEur;

        private Richesse GetRichesse(Monnaie monnaie)
        {
            return _richesses[monnaie];
        }

        internal Portefeuille()
        {
            Site = new Site();
            _metAJour = new Timer(MeilleurEchange, true, new TimeSpan(0), _refreshPooling);
        }

        internal void StopTrading()
        {
            _metAJour.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void MetAJour()
        {
            MetAJourTauxEuroUsd();
            MetAJourRichesses();
            MetAJourSite();
        }

        private void MetAJourSite()
        {
            Site.MetAJour();
        }

        private void MetAJourRichesses()
        {
            _richesses.Clear();
            JsonObject json = Site.client.GetBalance();
            if (((JsonArray)json["error"]).Count != 0)
                throw new Exception("erreur a la récupération des richesses du portefeuille");
            var balances = (JsonObject)json["result"];
            foreach (string key in balances.Names)
            {
                Monnaie monnaie = Monnaie.GetMonnaie(key);
                double qtte = double.Parse((string)balances[key]);
                _richesses.Add(monnaie, new Richesse(qtte, monnaie));
            }
        }

        private void MetAJourTauxEuroUsd()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(USD_EUR_URL);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    String rep = reader.ReadToEnd();
                    JsonObject json = JsonConvert.Import<JsonObject>(rep);
                    dateMiseAJourTauxUsdEur = JSON_ORIGINE_DES_DATES.AddMilliseconds((double)(JsonNumber)json["timestamp"]);
                    USD_EUR = (double)(JsonNumber)json["rate"];
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    Console.WriteLine(errorText);
                }
                throw;
            }
        }

        private void MeilleurEchange(object boolMetAJour)
        {
            try
            {
                MetAJour();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            var bestTrades = Site.TrouveMeilleurEchangeEURUSD(GetRichesse(Monnaie.EURO), GetRichesse(Monnaie.USD));
            foreach (AnomalieTrade t in bestTrades)
                Console.WriteLine(t.GainFee + " : " + t.ToString());
            try
            {
                if(bestTrades.Count > 0)
                    Site.ExecuteTrade(bestTrades.FindMax());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                StopTrading();
            }
        }

        /*
         * 
         * PAS DE PRISE EN COMPTE DE SPREAD SUR LE TAUX EURO DOLLAR (REVOLUT)
       
        /**
         * combien il aurait fallu fournir de richesse en euro pour avoir cette richesse  USD
        /
        internal static Richesse ConvertFromEuroToUsd(Richesse richesseEnUsd)
        {
            double euros = richesseEnUsd.Quantite * USD_EUR_ASK;
            return new Richesse(euros, Monnaie.EURO);
        }

        /**
         * combien on peut avoir en euro a partir d'une richesse en USD
        /
        internal static Richesse ConvertFromUsdToEuro(Richesse richesseEnUsd)
        {
            double euros = richesseEnUsd.Quantite * USD_EUR_BID;
            return new Richesse(euros, Monnaie.EURO);
        }

        */

        internal static Richesse ConvertUsdEnEuro(Richesse richesseEnUsd)
        {
            return new Richesse(richesseEnUsd.Quantite * USD_EUR, Monnaie.EURO);
        }

        internal static double ConvertUsdEnEuro(double qtteUsd)
        {
            return qtteUsd * USD_EUR;
        }


        #region mail
        private static string[] Scopes = { GmailService.Scope.GmailSend};
        private static string ApplicationName = "bot de trade";
        private static UserCredential Credential { get; } = InitialiseCredential();
        private static GmailService MailService { get; } = InitialiseMailService();

        /**
         * Create Gmail API service.
         */
        private static GmailService InitialiseMailService()
        {
            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });
        }

        private static UserCredential InitialiseCredential()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/botDeTrade.json");

                var ret = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
                return ret;
            }
        }

        internal static void EnvoyerMail(MailAddress destinataire, string sujet, string corps)
        {
            var fromAddress = new MailAddress("giraudsa@gmail.com", "the best");


            var msg = new AE.Net.Mail.MailMessage
            {
                Subject = sujet,
                Body = corps,
                From = fromAddress
            };
            msg.To.Add(destinataire);
            var msgStr = new StringWriter();
            msg.Save(msgStr);

            var result = MailService.Users.Messages.Send(new Message
            {
                Raw = Base64UrlEncode(msgStr.ToString())
            }, "me").Execute();
            Console.WriteLine("Message ID {0} envoyé.", result.Id);
        }
        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
        #endregion

    }
}
