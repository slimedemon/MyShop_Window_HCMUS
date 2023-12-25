using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public interface IBillRepository
    {
        Task<int> Add(Bill bill);
        Task Edit(Bill bill);
        Task Remove(int id);
        Task<Bill> GetById(int id);

        Task<List<Bill>> GetAll(DateOnly? dateFrom, DateOnly? dateTo);
        Task<List<int>> GetEmptyBillId();
        Task<List<Order>> GetBillDetailById(int billId);
        Task<List<int>> GetBookIdsById(int billId);

        Task AddBillDetail(Order billDetail);
        Task EditBillDetail(Order billDetail);
        Task RemoveBillDetail(int billId, int bookId);
    }
}
