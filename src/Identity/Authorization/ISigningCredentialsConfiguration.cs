using Microsoft.IdentityModel.Tokens;

namespace Identity.Authorization
{
    public interface ISigningCredentialsConfiguration
    {
        SymmetricSecurityKey Key { get; }
        SigningCredentials SigningCredentials { get; }
    }
}