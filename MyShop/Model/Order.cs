using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public sealed class Order: INotifyPropertyChanged
    {
        public int No { get; set; }
        public int BillId { get; set; }
        public string BookName { get; set; }
        public int BookId { get; set; }
        public int Price { get; set; }
        public int Number { get; set; }
        public int StockQuantity { get; set; }
        public string PromotionName { get; set; }
        public int PromotionId { get; set; }
        public int Discount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public int TotalPrice()
        {
            return Price*Number;
        }
    }
}
