using Identity.Authorization;
using Identity.Model;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Services.Infrastructure
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISigningCredentialsConfiguration _signingCredentialsConfiguration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly JWTConfig _jwtConfig;

        public JwtTokenGenerator(
            UserManager<ApplicationUser> userManager,
            ISigningCredentialsConfiguration signingCredentialsConfiguration,
            IRefreshTokenService refreshTokenService,
            IOptions<JWTConfig> jwtConfig)
        {
            _userManager = userManager;
            _signingCredentialsConfiguration = signingCredentialsConfiguration;
            _refreshTokenService = refreshTokenService;
            _jwtConfig = jwtConfig.Value;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var expires = DateTime.Now.AddMinutes(_jwtConfig.MinutesValid);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", user.FullName ?? ""),
                new Claim("FirstName", !string.IsNullOrEmpty(user.FullName) ? user.FullName.Split(" ")[0] : "")
            };

            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null && roles.Any())
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity();
            identity.AddClaims(claims);

            var handler = new JwtSecurityTokenHandler();

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = _signingCredentialsConfiguration.SigningCredentials,
                Subject = identity,
                NotBefore = DateTime.Now,
                Expires = expires
            };

            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            var result = await _refreshTokenService.CreateAsync(user);
            return result?.Token;
        }
    }
}
