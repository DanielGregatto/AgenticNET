using Identity.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Authorization
{
    public class SigningCredentialsConfiguration : ISigningCredentialsConfiguration
    {
        private readonly JWTConfig _jwtConfig;
        public SymmetricSecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningCredentialsConfiguration(IOptions<JWTConfig> jwtConfig)
        {
            this._jwtConfig = jwtConfig.Value;
            this.Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Secret));
            this.SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
        }
    }
}