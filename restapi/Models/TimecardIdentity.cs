using System;

namespace restapi.Models
{
    public class TimecardIdentity
    {
        public TimecardIdentity(string area, string number) 
        { 
            this.Area = area;
            this.Number = number;
        }

        public TimecardIdentity(string number) : this("dat", number) { }

        public TimecardIdentity() : this( GetSerialNumber().ToString() ) { }

        public string Area { get; set; }

        public string Number { get; set; }

        public string Value { get => $"{Area}-{Number}"; }

        private static long GetSerialNumber()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}