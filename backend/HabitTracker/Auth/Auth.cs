using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using HabitTracker.DTOs.User;
using HabitTracker.Exceptions;
using HabitTracker.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HabitTracker.Authentication;

public class LocalAuthenticationOptions : AuthenticationSchemeOptions { }
public class LocalAuthenticationHandler(
            IOptionsMonitor<LocalAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserService service)
            : AuthenticationHandler<LocalAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var a = Request.Headers.Authorization;
        var auth = a.FirstOrDefault("");
        if (auth is null or "")
        {
            return AuthenticateResult.NoResult();
        }
        var words = auth.Split(" ");
        if (words is null || words.Length != 2 || words[0] != "SessionToken") {
            return AuthenticateResult.Fail("invalid authentication format");
        }
        var token = words[1];
        try
        {
            var user = service.validateToken(token);
            Request.HttpContext.Items["user"] = new IdOnly(user.Id);
            var identity = new ClaimsIdentity(null, Scheme.Name);
            var principal = new GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (InvalidTokenException)
        {
            return AuthenticateResult.Fail($"invalid token {token}");
        }
    }
}
