using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorWasmAuth.Authentication;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AuthenticationType = "CustomAuth";
    private const string UserStorageKey = "user";
    private readonly BrowserStorageService _browserStorageService;

    public CustomAuthStateProvider(BrowserStorageService browserStorageService)
    {
        _browserStorageService = browserStorageService;
        AuthenticationStateChanged += CustomAuthStateProvider_AuthenticationStateChanged;
    }

    private async void CustomAuthStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var authState = await task;
        if(authState is not null)
        {
            var idStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(
                !string.IsNullOrWhiteSpace(idStr) // idStr is not null or empty
                && int.TryParse(idStr, out int id) // idStr is a valid integer
                && id > 0 // id is a gereater than zero
                )
            {
                CurrentUser = new User
                {
                    Id = id,
                    Name = authState.User.FindFirst(ClaimTypes.Name)!.Value,
                    Token = authState.User.FindFirst("Token")!.Value
                };
                return;
            }
        }
        CurrentUser = new();
    }

    public User CurrentUser { get; set; } = new();

    private AuthenticationState EmptyAuthState => new (new ClaimsPrincipal());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _browserStorageService.GetFromStorage<User?>(UserStorageKey);
        if (user is null)
        {
            // User auth state is not in browser's session/local storage
            // User is not logged in
            CurrentUser = new();
            return EmptyAuthState;
        }
        else
        {
            //  User auth state is there in the browser's session/local storage
            // User is logged in, we can fill out user details from this storage
            CurrentUser = user;

            var authState = GenerateAuthState(user);
            return authState;
        }
    }

    public async Task LoginAsync(string username, string password)
    {
        // Make Api Call with these Username and PAssword
        // and obtain the User Info from the api server
        var user = new User
        {
            Id = 1,
            Name = "Abhay Prince",
            Token = "some-random-token-value"
        };
        await _browserStorageService.SaveToStorage(UserStorageKey, user);

        var authState = GenerateAuthState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task LogoutAsync()
    {
        await _browserStorageService.RemoveFromStorage(UserStorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(EmptyAuthState));
    }

    private static AuthenticationState GenerateAuthState(User user)
    {
        Claim[] claims = [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("Token", user.Token),
        ];

        var identity = new ClaimsIdentity(claims, AuthenticationType);
        var claimsPrincial = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(claimsPrincial);
        return authState;
    }
}
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Token { get; set; }
}