using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using PeriodTracker.ApiGateways.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PeriodTracker.Services.Framework
{
    public interface IJwtService
    {
        Ticket CurrentIdentity();
    }

    public class JwtService(IHttpContextAccessor _httpContextAccessor) : IJwtService
    {
        /// <summary>
        /// Create JwToken
        /// </summary>
        /// <param name="expiresIn">In hours, default: 24 hours</param>
        /// <returns></returns>
        public static string CreateToken(int expiresIn = 24, IDictionary<string, string>? claims = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfigs.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            if (claims != null)
            {
                userClaims.AddRange(claims.Select(claim => new Claim(claim.Key, claim.Value ?? string.Empty)));
            }
            var token = new JwtSecurityToken(
                JwtConfigs.Issuer,
                JwtConfigs.Audience,
                userClaims,
                notBefore: DateTime.UtcNow,
expires: DateTime.UtcNow.AddHours(expiresIn),
                signingCredentials: credentials
              );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /// <summary>
        /// Get currrent identity
        /// </summary>
        /// <returns></returns>
        public Ticket CurrentIdentity()
        {
            Ticket ticket = new();
            var claims = _httpContextAccessor.HttpContext.User.Claims;
            if (claims != null && claims.Any())
            {
                ticket.UserId = int.Parse(claims.FirstOrDefault(x => x.Type == "UserId")!.Value);
                ticket.UserName = claims.FirstOrDefault(x => x.Type == "UserName")?.Value;
                ticket.UserDisplayName = claims.FirstOrDefault(x => x.Type == "UserDisplayName")?.Value;
                ticket.Role = claims.FirstOrDefault(x => x.Type == "Role")?.Value;
            }
            return ticket;
        }
    }
}
