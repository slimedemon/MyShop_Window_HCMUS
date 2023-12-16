using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public interface IStatisticRepository
    {
        Task<List<Tuple<DateTime, int>>> GetDailyStatistic(DateTime startDate, DateTime endDate);

        Task<List<Tuple<int, DateTime>>> GetListOfWeeks();

        Task<List<Tuple<DateTime, int>>> GetWeeklyStatistic(DateTime startDate, DateTime endDate);

        Task<List<Tuple<DateTime, int>>> GetMonthlyStatistic(DateTime startDate, DateTime endDate);

        Task<List<Tuple<DateTime, int>>> GetYearlyStatistic(DateTime startDate, DateTime endDate);

        Task<List<Tuple<string, int>>> GetProductStatistic(DateTime startDate, DateTime endDate);

        Task<int> GetCurrentMonthlyNumberOfSoldBookStatistic();
        Task<int> GetNumberOfBooks();
        Task<List<Tuple<string, int>>> GetTop5ProductStatistic(DateTime startDate, DateTime endDate);
        Task<List<Tuple<string, int>>> GetProductQuantityStatistic();
        Task<List<Tuple<DateTime, int>>> GetMonthRevenuesOfYear();
    }
}
