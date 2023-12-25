using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public interface IAccountRepository
    {
        Task<string> AuthenticateAccount(NetworkCredential credential);
        Task<bool> Add(Account account);
        Task<bool> UpdateProfile(Account account);
        Task<bool> ChangePassword(Account account);
        Task<Account> GetById(int id);
        void Remove(int id);
        Task<Account> GetByUsername(string username);
    }
}
