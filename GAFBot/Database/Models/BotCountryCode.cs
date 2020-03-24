using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Models
{
    public class BotCountryCode
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }

        public BotCountryCode()
        {

        }

        public BotCountryCode(string countryCode, string country)
        {
            CountryCode = countryCode;
            Country = country;
        }
    }
}
