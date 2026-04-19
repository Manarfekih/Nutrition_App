using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Recipe_Nutrition_App.Components.Auth;


public sealed class HttpContextAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        return Task.FromResult(new AuthenticationState(user));
    }
}
