using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ttc.Model;
using Ttc.Model.Core;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Ttc.WebApi.Utilities.Auth;

public class UserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TtcSettings _settings;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? Alias => Principal?.Identity?.Name ?? "Anonymous";

    public int? PlayerId
    {
        get
        {
            string? playerId = Principal?.Claims.FirstOrDefault(x => x.Type == "playerId")?.Value;
            if (string.IsNullOrWhiteSpace(playerId))
                return null;

            return int.Parse(playerId);
        }
    }

    public UserProvider(IHttpContextAccessor httpContextAccessor, TtcSettings settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _settings = settings;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.PlayerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Alias),
            new Claim("alias", user.Alias),
            new Claim("playerId", user.PlayerId.ToString()),
            new Claim("security", JsonConvert.SerializeObject(user.Security)),
            new Claim("teams", string.Join(",", user.Teams)),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMonths(6),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public User? ValidateToken(string token)
    {
        var validationParameters = CreateTokenParameters(_settings);
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var jwt = (JwtSecurityToken)validatedToken;

            string? alias = jwt.Claims.FirstOrDefault(x => x.Type == "alias")?.Value;
            string? playerId = jwt.Claims.FirstOrDefault(x => x.Type == "playerId")?.Value;
            string? security = jwt.Claims.FirstOrDefault(x => x.Type == "security")?.Value;
            string? teams = jwt.Claims.FirstOrDefault(x => x.Type == "teams")?.Value;
            if (alias == null || playerId == null || security == null || teams == null)
                return null;

            var userModel = new User
            {
                Alias = alias,
                PlayerId = int.Parse(playerId),
                Teams = !string.IsNullOrWhiteSpace(teams) ? teams.Split(',').Select(int.Parse).ToArray() : [],
                Security = JsonConvert.DeserializeObject<List<string>>(security) ?? [],
                Token = token,
            };
            return userModel;
        }
        catch (SecurityTokenValidationException)
        {
            return null;
        }
    }

    public static TokenValidationParameters CreateTokenParameters(TtcSettings settings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSecret))
        };
    }

    public void CleanSensitiveData<T>(ICollection<T> data)
        where T : ITtcConfidential
    {
        if (IsAuthenticated)
            return;

        foreach (var record in data)
        {
            record.Hide();
        }
    }

    public void CleanSensitiveData<T>(T data)
        where T : ITtcConfidential
    {
        if (IsAuthenticated)
            return;

        data.Hide();
    }
}
