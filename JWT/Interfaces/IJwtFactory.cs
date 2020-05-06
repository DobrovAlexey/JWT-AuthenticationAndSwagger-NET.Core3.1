using System.Threading.Tasks;
using JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Models;

namespace JWT_AuthenticationAndSwagger_NET.Core3._1.JWT.Interfaces
{
    public interface IJwtFactory
    {
        Task<AccessToken> GenerateEncodedToken(long id, string phoneNumber, string email, string lastName,
            string firstName, string region, string city, string[] roles);

        long? ValidateToken(string token);
    }
}