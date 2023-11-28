using Microsoft.EntityFrameworkCore;
using RestWebApiApp.Models;

namespace RestWebApiApp.Data
{
    public interface IUserRep
    {
        public Task<User?> GetUserByIdAsync(int userId);

        public Task<User?> GetUserByUsernameAsync(string username);

        public Task AddUserAsync(User user);

        public Task UpdateUserAsync(User user);

        public Task DeleteUserAsync(int userId);

        public Task<User?> GetUserByRefToken(string refToken);
    }
}
