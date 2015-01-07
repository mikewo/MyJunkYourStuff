using System;
using Newtonsoft.Json;

namespace MyJunkYourStuff.Models
{
    public class Location
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public DateTime StartTime { get; set; }
        
        public string MainImageName { get; set; }

        public string JunkerName { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }
    }
}