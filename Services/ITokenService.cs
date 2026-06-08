using proiectMTP.Models;

namespace proiectMTP.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Profesor professor);
}
