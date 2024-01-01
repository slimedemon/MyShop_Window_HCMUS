using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public interface IPromotionRepository
    {
        Task<int> Add(Promotion promotion);
        Task<bool> RemovePromotion(int id);
        Task<bool> EditPromotion(Promotion promotion);
        Task<List<Promotion>> GetAllPromotions(DateOnly? dateFrom, DateOnly? dateTo);
        Task<Promotion> GetPromotionById(int promotionId);
        Task<List<BookPromotion>> GetAllBookPromotionsByPromotionId(int promotionId);
        Task<bool> AddBookPromotion(BookPromotion bookPromotion);
        Task<bool> RemoveBookPromotion(int promotionId, int bookId);
    }
}
