using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Json
{
    public class GetAuthorize
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }

        public GetAuthorize(string accessToken, string tokenType)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
        }

        public GetAuthorize()
        {

        }
    }
}
