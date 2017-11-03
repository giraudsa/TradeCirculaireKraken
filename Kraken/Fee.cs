using Jayrock.Json;
using System;

namespace Kraken
{
    internal class Fee : IComparable<Fee>
    {
      
        internal double Volume { get; private set; }
        internal double Frais { get; private set; }
        internal Fee(double volume)
        {
            Volume = volume;
        }
        internal Fee(JsonArray feeJson)
        {
            Volume = ((JsonNumber)feeJson[0]).ToInt32();
            Frais = ((JsonNumber)feeJson[1]).ToDouble();
        }

        public int CompareTo(Fee other)
        {
            return this.Volume.CompareTo(other.Volume);
        }
    }
}