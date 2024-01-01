using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Model
{
    public class BookPromotion : INotifyPropertyChanged
    {
        public int PromotionId { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
