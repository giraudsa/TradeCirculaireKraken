using System;
using Jayrock.Json;
using System.Globalization;

namespace Kraken
{
    internal class Position 
    {

        internal static Func<Position, double> getVolumeBase = position => position.VolumeBase;
        internal static Func<Position, double> getVolumeQuote = position => position.VolumeQuote;
        internal static Func<Position, double, double> volumeBasePour = (position, volumeQuote) => position.VolumeBasePour(volumeQuote);
        internal static Func<Position, double, double> volumeQuotePour = (position, volumeBase) => position.VolumeQuotePour(volumeBase);

        private Monnaie mbase;
        private Monnaie quote;
        private double PrixBase { get; }
        private double VolumeBase { get; }
        private double VolumeQuote
        {
            get
            {
                return VolumeBase * PrixBase;
            }
        }
        private double PrixQuote
        {
            get
            {
                return 1 / PrixBase;
            }
        }
        private DateTime Date { get; }

        internal Position(JsonArray jsonAsk, Monnaie mbase, Monnaie quote)
        {
            this.mbase = mbase;
            this.quote = quote;
            PrixBase = double.Parse((string)jsonAsk[0], CultureInfo.InvariantCulture.NumberFormat);
            VolumeBase = double.Parse((string)jsonAsk[1], CultureInfo.InvariantCulture.NumberFormat);
            Date = new DateTime(((JsonNumber)jsonAsk[2]).ToInt64());
        }

        internal double VolumeQuotePour(double volumeBase)
        {
            return volumeBase * PrixBase;
        }

        internal double VolumeBasePour(double volumeQuote)
        {
            return volumeQuote * PrixQuote;
        }

        public override string ToString()
        {
            return "Position : " + PrixBase + mbase + "/" + quote + " volume : " + VolumeBase;
        }
    }
}