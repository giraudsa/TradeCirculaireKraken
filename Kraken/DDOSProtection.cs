using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Kraken
{
    /// <summary>
    /// We have safeguards in place to protect against abuse/DoS attacks as well as order book manipulation caused by the rapid placing and canceling of orders.
    /// Every user of our API has a "call counter" which starts at 0.
    /// Ledger/trade history calls increase the counter by 2.
    /// Place/cancel order calls do not affect the counter.
    /// All other API calls increase the counter by 1.
    /// The user's counter is reduced every couple of seconds, and if the counter exceeds the user's maximum API access is suspended for 15 minutes.Tier 2 users have a maximum of 15 and their count gets reduced by 1 every 3 seconds.Tier 3 and 4 users have a maximum of 20; the count is reduced by 1 every 2 seconds for tier 3 users, and is reduced by 1 every 1 second for tier 4 users.
    /// Although placing and cancelling orders does not increase the counter, there are separate limits in place to prevent order book manipulation.Only placing orders you intend to fill and keeping the rate down to 1 per second is generally enough to not hit this limit.
    /// </summary>
    internal class DDOSProtection
    {
        private ReaderWriterLockSlim lockCompteur = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly NiveauDeVerification tier;
        private readonly ushort compteurMax;
        private readonly TimeSpan tempsReductionCompteur;
        private ushort _compteur;
        private ushort compteur
        {
            get
            {
                try
                {
                    lockCompteur.EnterReadLock();
                    return _compteur;
                }
                finally
                {
                    lockCompteur.ExitReadLock();
                }
            }
            set { _compteur = value; }
        }
        private DateTime lastCall;
        private Timer diminueCompteur;
        private AutoResetEvent notifieDiminutionCompteur = new AutoResetEvent(false);

        internal DDOSProtection(NiveauDeVerification tier)
        {
            diminueCompteur = new Timer(DiminueCompteur, null, tempsReductionCompteur, tempsReductionCompteur);
            lastCall = DateTime.UtcNow;
            this.tier = tier;
            switch (tier)
            {
                case NiveauDeVerification.Tier2:
                    compteurMax = 15;
                    tempsReductionCompteur = new TimeSpan(0, 0, 3);//3 secondes
                    break;
                case NiveauDeVerification.Tier3:
                case NiveauDeVerification.Tier4:
                    compteurMax = 20;
                    tempsReductionCompteur = new TimeSpan(0, 0, 1);
                    break;
            }
        }

        internal void WaitToProceed(ushort poids)
        {
            if (poids == 0)
            {
                Strategie1();
                return;
            }
            Strategie2(poids);

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Strategie2(ushort poids)
        {
            while (compteur + poids > compteurMax)
            {
                notifieDiminutionCompteur.WaitOne();
            }
            AugmenteCompteur(poids);
        }

        /// <summary>
        /// Although placing and cancelling orders does not increase the counter, 
        /// there are separate limits in place to prevent order book manipulation.
        /// Only placing orders you intend to fill and keeping the rate down to 1 per second 
        /// is generally enough to not hit this limit.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Strategie1()
        {
            long tempsAAttendre = TimeSpan.TicksPerSecond - DateTime.UtcNow.Subtract(lastCall).Ticks;
            if (tempsAAttendre > 0)
                Thread.Sleep(new TimeSpan(tempsAAttendre));
            lastCall = DateTime.UtcNow;
        }

        private void DiminueCompteur(object dummy)
        {
            lockCompteur.EnterWriteLock();
            if(compteur>0)
                --compteur;
            lockCompteur.ExitWriteLock();
            notifieDiminutionCompteur.Set();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AugmenteCompteur(ushort nombre)
        {
            lockCompteur.EnterWriteLock();
            if (compteur > 0)
                compteur = (ushort)(compteur + nombre);
            lockCompteur.ExitWriteLock();
        }
    }

    internal enum NiveauDeVerification
    {
        Tier2,
        Tier3,
        Tier4
    }
}