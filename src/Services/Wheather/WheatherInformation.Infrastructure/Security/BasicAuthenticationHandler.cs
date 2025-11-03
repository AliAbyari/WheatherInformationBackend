using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _config;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration config)
        : base(options, logger, encoder, clock)
    {
        _config = config;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

            if (authHeader.Scheme != "Basic")
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authentication Scheme"));

            var credentialsBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(':', 2);

            if (credentials.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic authentication format"));

            var username = credentials[0];
            var password = credentials[1];

            var expectedUsername = _config["SecurityOptions:Username"];
            var expectedPassword = _config["SecurityOptions:Password"];

            if (username != expectedUsername || password != expectedPassword)
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 format in Authorization Header"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
        }
    }
}
