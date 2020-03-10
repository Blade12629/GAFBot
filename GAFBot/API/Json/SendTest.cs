using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Json
{
    public class SendTest
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        public SendTest( string message)
        {
            Message = message;
        }

        public SendTest()
        {

        }
    }
}
