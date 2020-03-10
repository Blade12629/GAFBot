using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Json
{
    public class SendAuthorize
    {
        [JsonProperty("usernameOrEmail")]
        public string UsernameOrEmail { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }

        public SendAuthorize(string usernameOrEmail, string password)
        {
            UsernameOrEmail = usernameOrEmail;
            Password = password;
        }

        public SendAuthorize()
        {

        }
    }
}
