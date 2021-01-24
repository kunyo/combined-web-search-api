using Bds.TechTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bds.TechTest.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> entries;

        public UserRepository()
        {
            this.entries = new List<User>()
            {
                new User { Id = Guid.NewGuid(), Username = "pRpsUKcFzxzupxGyeaPyHx2nN6NBos2iD7peRXme", Password = "Fu6fswGfmgbg9fNA68vmjzUSY2dYK5WtsoA8RyXn" },
                new User { Id = Guid.NewGuid(), Username = "bqEEg3c2HLM4jMXLvxDBboQzYGn2JuNPoL7tDSV4", Password = "vQwJgfbNGWmEj5WMxy3ZPCJKZxTYvBCCTJzekBd3" }
            }.ToDictionary(x => x.Id, x => x);
        }

        public Task<User> GetUserByCredentials(string username, string password)
        {
            return Task.FromResult(entries.Values.FirstOrDefault(x => string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase) && password == x.Password));
        }
    }
}
