using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RunAway.Controllers;

[ApiController]
[Route("strava")]
public class StravaController(IConfiguration config) : ControllerBase
{
    [HttpGet("auth")]
    public IActionResult Authorize()
    {
        var clientId = config["Strava:ClientId"];
        var redirectUri = config["Strava:RedirectUri"];

        var scopes = "read,activity:read_all";


        var url = $"https://www.strava.com/oauth/authorize" +
                  $"?client_id={clientId}" +
                  $"&redirect_uri={redirectUri}" +
                  $"&response_type=code" +
                  $"&approval_prompt=auto" +
                  $"&scope={scopes}";

        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("No code returned from Strava.");

        var clientId = config["Strava:ClientId"];
        var clientSecret = config["Strava:ClientSecret"];

        using var http = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://www.strava.com/oauth/token");

        var form = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "code", code },
            { "grant_type", "authorization_code" }
        };

        request.Content = new FormUrlEncodedContent(form);

        var response = await http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest(json);

        // TODO: Deserialize JSON → store refresh_token + access_token in DB

        return Ok(json);
    }
}