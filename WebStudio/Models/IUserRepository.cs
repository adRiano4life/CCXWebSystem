using System.Collections.Generic;

namespace WebStudio.Models
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User Get(string id);
        void Create(User user);
    }
}