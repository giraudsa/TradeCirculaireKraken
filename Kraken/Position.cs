using System;
using Jayrock.Json;
using System.Globalization;

namespace Kraken
{
    internal abstract class Position
    {
        //EURUSD : Base = EURO, Quote = USD
        internal static Func<Position, double> getVolumeBase = position => position.VolumeBase;
        internal static Func<Position, double> getVolumeQuote = position => position.VolumeQuote;
        internal static Func<Position, double, double> volumeBasePour = (position, volumeQuote) => position.VolumeBasePour(volumeQuote);
        internal static Func<Position, double, double> volumeQuotePour = (position, volumeBase) => position.VolumeQuotePour(volumeBase);

        protected Monnaie monnaieDeBase;
        protected Monnaie monnaieDeQuote;
        protected double PrixBase { get; }
        protected double PrixBaseFee
        {
            get
            {
                return CalculPrixBase(0.36);
            }
        }
        internal double VolumeBase { get; }
        private double VolumeQuote
        {
            get
            {
                return VolumeBase * PrixBase;
            }
        }
        private double VolumeQuoteFee
        {
            get
            {
                return VolumeBase * PrixBaseFee;
            }
        }
        private double PrixQuote
        {
            get
            {
                return 1 / PrixBase;
            }
        }
        private double PrixQuoteFee
        {
            get
            {
                return 1 / PrixBaseFee;
            }
        }
        protected DateTime Date { get; }

        internal abstract double CalculPrixBase(double fee);

        internal Position(JsonArray jsonAsk, Monnaie mbase, Monnaie quote)
        {
            this.monnaieDeBase = mbase;
            this.monnaieDeQuote = quote;
            PrixBase = double.Parse((string)jsonAsk[0], CultureInfo.InvariantCulture.NumberFormat);
            VolumeBase = double.Parse((string)jsonAsk[1], CultureInfo.InvariantCulture.NumberFormat);
            Date = new DateTime(((JsonNumber)jsonAsk[2]).ToInt64());
        }

        internal Position(double prixBase, double volumeBase, DateTime date, Monnaie mbase, Monnaie quote)
        {
            this.monnaieDeBase = mbase;
            this.monnaieDeQuote = quote;
            PrixBase = prixBase;
            VolumeBase = VolumeBase;
            Date = date;
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
            return "Position : " + PrixBase + monnaieDeBase + "/" + monnaieDeQuote + " volume : " + VolumeBase;
        }
    }

    internal class PositionAchatBase : Position
    {
        public PositionAchatBase(double prixBase, PositionAchatBase positionAchatBase) : base(prixBase, positionAchatBase.VolumeBase, positionAchatBase.Date, positionAchatBase.monnaieDeBase, positionAchatBase.monnaieDeQuote)
        {           
        }

        internal PositionAchatBase(JsonArray jsonAsk, Monnaie mbase, Monnaie quote) : base(jsonAsk, mbase, quote)
        {
        }

        internal override double CalculPrixBase(double fee)
        {
            return PrixBase * (1 - fee);
        }
    }
    internal class PositionVenteBase : Position
    {

        public PositionVenteBase(double prixBase, PositionVenteBase positionVenteBase) : base(prixBase, positionVenteBase.VolumeBase, positionVenteBase.Date, positionVenteBase.monnaieDeBase, positionVenteBase.monnaieDeQuote)
        {
        }

        internal PositionVenteBase(JsonArray jsonAsk, Monnaie mbase, Monnaie quote) : base(jsonAsk, mbase, quote)
        {
        }
        

        internal override double CalculPrixBase(double fee)
        {
            return PrixBase * (1 + fee);
        }
    }
    internal class PositionAchatBaseFee : PositionAchatBase
    {
        internal PositionAchatBaseFee(PositionAchatBase positionAchatBase, double fee) : base(positionAchatBase.CalculPrixBase(fee), positionAchatBase)
        {
        }

    }
    internal class PositionVenteBaseFee : PositionVenteBase
    {
        internal PositionVenteBaseFee(PositionVenteBase positionVenteBase, double fee) : base(positionVenteBase.CalculPrixBase(fee), positionVenteBase)
        {
        }

    }

}