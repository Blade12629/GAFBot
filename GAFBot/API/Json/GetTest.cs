using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API.Json
{
    public class GetTest
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public GetTest(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public GetTest()
        {

        }
    }
}
