using Bds.TechTest.Dtos;
using Bds.TechTest.Repositories;
using System.Threading.Tasks;

namespace Bds.TechTest.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<UserInfoDto> GetUserByCredentials(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var user = await userRepository.GetUserByCredentials(username, password);
                if (user != null)
                {
                    return new UserInfoDto { Id = user.Id };
                }
            }

            return null;
        }
    }
}
