using System;
using Newtonsoft.Json;

namespace MyJunkYourStuff.Models
{
    public static class Extensions
    {
        public static int ToEpoch(this DateTime date)
        {
            if (date == null) return int.MinValue;
            DateTime epoch = new DateTime(1970, 1, 1);
            TimeSpan epochTimeSpan = date - epoch;
            return (int)epochTimeSpan.TotalSeconds;
        }
    }

    public class DateEpoch
    {
        // See http://blogs.msdn.com/b/documentdb/archive/2014/11/18/working-with-dates-in-azure-documentdb.aspx

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "epoch")]
        public int Epoch
        {
            get
            {
                return (this.Date.Equals(null) || this.Date.Equals(DateTime.MinValue))
                    ? int.MinValue
                    : this.Date.ToEpoch();
            }
        }
    }
}