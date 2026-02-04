using BitacoraAlfipac.Models;
using BitacoraAlfipac.Models.Entidades;

namespace BitacoraAlfipac.Services.Interfaces
{
    public interface IAuthService
    {
        (Usuario? user, string? error) ValidateUser(string username, string password);
    }
}
