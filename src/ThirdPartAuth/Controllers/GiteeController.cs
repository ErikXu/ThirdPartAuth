using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ThirdPartAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiteeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly GiteeService _giteeService;

        public GiteeController(IConfiguration configuration, GiteeService giteeService)
        {
            _configuration = configuration;
            _giteeService = giteeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("https://gitee.com/api/v5/oauth_doc#/");
        }

        [HttpGet("auth")]
        public IActionResult Auth()
        {
            var clientId = _configuration["gitee:clientId"];
            var callbackUri = _configuration["gitee:callbackUri"];
            return Redirect($"https://gitee.com/oauth/authorize?client_id={clientId}&redirect_uri={callbackUri}&response_type=code");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery]string code)
        {
            var accessToken = await _giteeService.GetAccessToken(code);
            var info = await _giteeService.GetUserInfo(accessToken.AccessToken);

            var redirectUri = _configuration["gitee:redirectUri"];
            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                return Redirect(redirectUri);
            }

            return Ok(info);
        }
    }

    public class GiteeService
    {
        private readonly IConfiguration _configuration;

        public HttpClient Client { get; }

        public GiteeService(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            Client = client;
        }

        public async Task<GiteeAccessToken> GetAccessToken(string code)
        {
            var clientId = _configuration["gitee:clientId"];
            var clientSecret = _configuration["gitee:clientSecret"];
            var callbackUri = _configuration["gitee:callbackUri"];
            var url = $"https://gitee.com/oauth/token?grant_type=authorization_code&code={code}&client_id={clientId}&redirect_uri={callbackUri}&client_secret={clientSecret}";

            var response = await Client.PostAsync(url, new StringContent(string.Empty));
            var json = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<GiteeAccessToken>(json);
            return token;
        }

        public async Task<GiteeUserInfo> GetUserInfo(string accessToken)
        {
            var url = $"https://gitee.com/api/v5/user?access_token={accessToken}";
            var json = await Client.GetStringAsync(url);
            var info = JsonConvert.DeserializeObject<GiteeUserInfo>(json);
            return info;
        }
    }

    public class GiteeAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAt { get; set; }
    }

    public class GiteeUserInfo
    {
        public int id { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string avatar_url { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public object blog { get; set; }
        public object weibo { get; set; }
        public object bio { get; set; }
        public int public_repos { get; set; }
        public int public_gists { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public int stared { get; set; }
        public int watched { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object email { get; set; }
    }
}