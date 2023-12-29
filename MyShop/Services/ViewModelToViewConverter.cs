using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using MyShop.View;
using MyShop.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Services
{
    
    internal class ViewModelToViewConverter : IValueConverter
    {
        private static readonly Dictionary<Type, Type> pairs = new Dictionary<Type, Type>()
        {
            {typeof(LoginViewModel), typeof(LoginPage)},
            {typeof(MainNavigationViewModel), typeof(MainNavigationPage)},
            {typeof(ProductManagementViewModel), typeof(ProductManagementPage)},
            {typeof(DashboardViewModel), typeof(DashboardPage)},
            {typeof(OrderManagementViewModel), typeof(OrderManagementPage)},
            {typeof(AccountViewModel), typeof(AccountPage)},
            {typeof(SettingsViewModel), typeof(SettingsPage)},
            {typeof(StatisticsViewModel), typeof(StatisticsPage)},
            {typeof(BookManagementViewModel), typeof(BookManagementPage)},
            {typeof(GenreManagementViewModel), typeof(GenreManagementPage)},
            {typeof(DailyRevenueStatisticViewModel), typeof(DailyRevenueStatisticPage) },
            {typeof(WeeklyRevenueStatisticViewModel), typeof(WeeklyRevenueStatisticPage) },
            {typeof(MonthlyRevenueStatisticViewModel), typeof(MonthlyRevenueStatisticPage) },
            {typeof(YearlyRevenueStatisticViewModel), typeof(YearlyRevenueStatisticPage) },
            {typeof(DailyProductStatisticViewModel), typeof(DailyProductStatisticPage) },
            {typeof(WeeklyProductStatisticViewModel), typeof(WeeklyProductStatisticPage) },
            {typeof(MonthlyProductStatisticViewModel), typeof(MonthlyProductStatisticPage) },
            {typeof(YearlyProductStatisticViewModel), typeof(YearlyProductStatisticPage) },
            {typeof(AddBookViewModel), typeof(AddBookPage) },
            {typeof(EditBookViewModel), typeof(EditBookPage)},
            {typeof(AddOrderViewModel), typeof(AddOrderPage) },
            {typeof(EditOrderViewModel), typeof(EditOrderPage)},
            {typeof(DatabaseConfigurationViewModel), typeof(DatabaseConfigurationPage)},
            {typeof(SignupViewModel), typeof(SignupPage)},
            {typeof(RevenueStatisticsViewModel), typeof(RevenueStatisticsPage)},
            {typeof(ProductStatisticsViewModel), typeof(ProductStatisticsPage)},
            {typeof(BestSellerStatisticsViewModel), typeof(BestSellerStatisticsPage)}
            //add more page...
        };
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            pairs.TryGetValue(value.GetType(), out var page);
            Page x = (Page)Activator.CreateInstance(page);
            x.DataContext = value;
            return x;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
