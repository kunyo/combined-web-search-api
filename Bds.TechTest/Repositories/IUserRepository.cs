using Bds.TechTest.Models;
using System;
using System.Threading.Tasks;

namespace Bds.TechTest.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByCredentials(string username, string password);
    }
}