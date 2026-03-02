using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AspNetHandons.Entities;

namespace AspNetHandons.Services
{
    public class JwtService
    {
        private readonly Jwt _jwtOptions;

        public JwtService(Microsoft.Extensions.Options.IOptions<Jwt> options)
        {
            _jwtOptions = options.Value;
        }

        public string GenerateJwt(User user)
        {
            var rsa = RSA.Create();
            var privateKey = File.ReadAllText(_jwtOptions.RsaPrivateKeyLocation);
            rsa.ImportFromPem(privateKey.ToCharArray());

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Add permissions array
            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}