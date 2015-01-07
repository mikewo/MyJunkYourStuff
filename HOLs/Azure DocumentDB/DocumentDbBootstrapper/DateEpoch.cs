using System;
using Newtonsoft.Json;

namespace DocumentDbBootstrapper
{
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