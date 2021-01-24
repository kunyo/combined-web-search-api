using Bds.TechTest.Dtos;

namespace Bds.TechTest.Services
{
    public interface IUserService
    {
        System.Threading.Tasks.Task<UserInfoDto> GetUserByCredentials(string username, string password);
    }
}