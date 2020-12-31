using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ThirdPartAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GithubController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly GithubService _githubService;

        public GithubController(IConfiguration configuration, GithubService githubService)
        {
            _configuration = configuration;
            _githubService = githubService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps");
        }

        [HttpGet("auth")]
        public IActionResult Auth()
        {
            var clientId = _configuration["github:clientId"];
            return Redirect($"https://github.com/login/oauth/authorize?client_id={clientId}&scope=user,public_repo");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery]string code)
        {
            var accessToken = await _githubService.GetAccessToken(code);
            var info = await _githubService.GetUserInfo(accessToken.AccessToken);

            var redirectUri = _configuration["github:redirectUri"];
            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                return Redirect(redirectUri);
            }

            return Ok(info);
        }
    }

    public class GithubService
    {
        private readonly IConfiguration _configuration;

        public HttpClient Client { get; }

        public GithubService(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            Client = client;
        }

        public async Task<GithubAccessToken> GetAccessToken(string code)
        {
            var clientId = _configuration["github:clientId"];
            var clientSecret = _configuration["github:clientSecret"];
            var url = $"https://github.com/login/oauth/access_token?client_id={clientId}&client_secret={clientSecret}&code={code}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = await Client.GetStringAsync(url);
            var token = JsonConvert.DeserializeObject<GithubAccessToken>(json);
            return token;
        }

        public async Task<GithubUserInfo> GetUserInfo(string accessToken)
        {
            var url = "https://api.github.com/user";
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            Client.DefaultRequestHeaders.Add("User-Agent", "Csharp App");
            var json = await Client.GetStringAsync(url);
            var info = JsonConvert.DeserializeObject<GithubUserInfo>(json);
            return info;
        }
    }

    public class GithubAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    public class GithubUserInfo
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
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
        public bool site_admin { get; set; }
        public string name { get; set; }
        public string company { get; set; }
        public string blog { get; set; }
        public string location { get; set; }
        public string email { get; set; }
        public object hireable { get; set; }
        public string bio { get; set; }
        public object twitter_username { get; set; }
        public int public_repos { get; set; }
        public int public_gists { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int private_gists { get; set; }
        public int total_private_repos { get; set; }
        public int owned_private_repos { get; set; }
        public int disk_usage { get; set; }
        public int collaborators { get; set; }
        public bool two_factor_authentication { get; set; }
        public Plan plan { get; set; }
    }

    public class Plan
    {
        public string name { get; set; }
        public int space { get; set; }
        public int collaborators { get; set; }
        public int private_repos { get; set; }
    }
}