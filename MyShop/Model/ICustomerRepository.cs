using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public interface ICustomerRepository
    {
        Task<int> Add(Customer customer);
        Task<bool> Edit(Customer customer);
        Task<List<Customer>> GetAll(DateOnly? dateFrom, DateOnly? dateTo);
        Task<Customer> GetById(int id);
        Task<bool> Remove(int id);
    }
}
