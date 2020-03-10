using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GAFBot.API.Json;
using Newtonsoft.Json;

namespace GAFBot.API
{
    public class HTTP
    {
        private HttpClient _http;
        private APIToken _token;
        private string _address;
        private readonly object _httpLock = new object();

        public HTTP(string address)
        {
            _address = address;
            _http = new HttpClient();
        }

        public async Task<bool> Auth(string user, string pass)
        {
            Logger.Log("Authenticating at Web API", LogLevel.Trace);

            const string route = "/api/auth/signin";
            SendAuthorize authS = new SendAuthorize(user, pass);
            string authData = JsonConvert.SerializeObject(authS);

            string response = await Post(authData, route, true);

            GetAuthorize authG;
            try
            {
                authG = JsonConvert.DeserializeObject<GetAuthorize>(response);
            }
            catch (Exception)
            {
                Logger.Log("Could not authenticate at Web API", LogLevel.Trace);
                return false;
            }

            _token = APIToken.Create(authG);

            if (!_token.IsValid || _token.IsExpired)
            {
                Logger.Log("Could not authenticate at Web API", LogLevel.Trace);
                return false;
            }

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);

            Logger.Log("Authenticated at Web API", LogLevel.Trace);

            return true;
        }

        public async Task<string> SendResults(Osu.results.AnalyzerResult ar)
        {
            const string route = "/api/result/new";

            if (ar == null)
            {
                Logger.Log("Analyzerresult is empty", LogLevel.Trace);
                return "";
            }

            SendOsuResult result = JsonResultConverter.ConvertSendOsuResult(ar);
            string json = JsonConvert.SerializeObject(result);
            string jsonInd = JsonConvert.SerializeObject(result, Formatting.Indented);
            Logger.Log("Sending Json: " + jsonInd, LogLevel.Trace);

            string r = await Post(json, route);
            return r;
        }
        
        public async Task<bool> TestMessage(string message)
        {
            const string route = "/api/dev/json";

            SendTest testS = new SendTest(message);
            string json = JsonConvert.SerializeObject(testS);
            string response = await Post(json, route);
            GetTest testG = null;

            try
            {
                testG = JsonConvert.DeserializeObject<GetTest>(response);
            }
            catch (Exception)
            {
                return false;
            }

            return testG.Success;
        }
        
        private async Task<string> Post(string data, string route, bool auth = false)
        {
            if (!auth && (_token.IsExpired || !_token.IsValid))
            {
                Logger.Log("Token not valid, requesting new one", LogLevel.WARNING);
                if (!await Auth(Program.Config.WebsiteUser, Program.DecryptString(Program.Config.WebsitePassEncrypted)))
                {
                    Logger.Log("Could not get valid token, aborting post request");

                    return "";
                }
            }
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;

            lock (_httpLock)
            {
                response = _http.PostAsync(_address + route, content).Result;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
    
    public class APIToken : GetAuthorize
    {
        public DateTime CreationDate { get; set; }
        public bool IsExpired
        {
            get
            {
                return DateTime.UtcNow.Ticks >= CreationDate.AddDays(6.5).Ticks;
            }
        }
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(TokenType);
            }
        }

        private APIToken()
        {

        }

        public static APIToken Create(GetAuthorize token)
        {
            APIToken t = new APIToken()
            {
                CreationDate = DateTime.UtcNow,
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };

            return t;
        }
    }
}
