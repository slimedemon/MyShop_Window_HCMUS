using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Gaming.Input.Custom;
using Windows.UI.WebUI;

namespace MyShop.ViewModel
{
    public class AddOrderViewModel : ViewModelBase
    {
        // Fields
        private IBillRepository _billRepository;
        private IBookRepository _bookRepository;
        private ICustomerRepository _customerRepository;
        private IPromotionRepository _promotionRepository;

        private Bill _newBill;
        private ObservableCollection<Book> _books;
        private Customer _bindingCustomer;
        private Order _bindingOrder;
        private Order _selectedOrder;

        private int _currentTotalPrice;
        private Book _selectedBook;
        private Promotion _selectedPromotion;

        //-> Commands
        private RelayCommand _backCommand;
        private RelayCommand _confirmCommand;
        private RelayCommand _addCommand;
        private RelayCommand _removeCommand;
        private RelayCommand _refreshCommand;

        // getter, setter
        public RelayCommand BackCommand { get => _backCommand; set => _backCommand = value; }
        public RelayCommand ConfirmCommand { get => _confirmCommand; set => _confirmCommand = value; }
        public RelayCommand AddCommand { get => _addCommand; set => _addCommand = value; }
        public RelayCommand RemoveCommand { get => _removeCommand; set => _removeCommand = value; }
        public RelayCommand RefreshCommand { get => _refreshCommand; set => _refreshCommand = value; }

        public Bill NewBill { get => _newBill; set => _newBill = value; }
        public ObservableCollection<Book> Books { get => _books; set => _books = value; }
        public ObservableCollection<Order> Orders { get; set; }
        public Order BindingOrder { get => _bindingOrder; set => _bindingOrder = value; }
        public Order SelectedOrder { get => _selectedOrder; set => _selectedOrder = value; }
        public Customer BindingCustomer { get => _bindingCustomer; set => _bindingCustomer = value; }
        public Promotion SelectedPromotion { get => _selectedPromotion; set => _selectedPromotion = value; }
        public List<Promotion> Promotions { get; set; }
        public ObservableCollection<Promotion> DisplayPromotion { get; set; }
        public Dictionary<int, List<BookPromotion>> PromotionDic { get; set; }

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                DisplayPromotion.Clear();
                for (int i = 0; i < Promotions.Count; i++)
                {
                    if (!PromotionDic.TryGetValue(Promotions[i].Id, out var bookPromotions))
                    {
                        bookPromotions = new List<BookPromotion>();
                    }

                    for (int j = 0; j < bookPromotions.Count; j++)
                    {
                        if (bookPromotions[j].BookId == value.Id)
                        {
                            DisplayPromotion.Add(Promotions[i]);
                            break;
                        }
                    }
                }

                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
            }
        }

        public int CurrentTotalPrice
        {
            get => _currentTotalPrice;
            set 
            { 
                _currentTotalPrice = value;
                OnPropertyChanged("CurrentTotalPrice");
            }
        }


        public async void PageLoaded()
        {
            var task1 = await _bookRepository.GetAll();
            if (task1 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from the database!");
                // purpose: continue flow;
                task1 = new List<Book>();
            }

            task1.ForEach(book => Books.Add(book));

            var task2 = await _promotionRepository.GetAllPromotions(DateOnly.MinValue, DateOnly.MaxValue);
            if (task2 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from the database!");
                // purpose: continue flow;
                task2 = new List<Promotion>();
            }

            for (int i = 0; i < task2.Count; i++)
            {
                PromotionDic.Clear();
                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
                if (task2[i].StartDate <= currentDate && task2[i].EndDate >= currentDate)
                {
                    Promotions.Add(task2[i]);

                    var bookPromotions = await _promotionRepository.GetAllBookPromotionsByPromotionId(task2[i].Id);
                    if (bookPromotions == null)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from the database!");
                        // purpose: continue flow;
                        bookPromotions = new List<BookPromotion>();
                    }

                    PromotionDic.Add(task2[i].Id, bookPromotions);
                }
            }
        }

        // Edit bill details
        public async void ExecuteAddCommand()
        {
            if (SelectedBook == null)
            {
                await App.MainRoot.ShowDialog("No selected book", "Please select the book you would like to add to the order!");
                return;
            }
            if (Orders.Select(x => x.BookId).ToList().Contains(SelectedBook.Id))
            {
                await App.MainRoot.ShowDialog("Duplicate book", "This book already exists in the order!");
                return;

            }
            if (BindingOrder.Number > SelectedBook.Quantity) { 
                await App.MainRoot.ShowDialog("Not enough stock", "Out of stock or insufficient quantity for sale! Please restock the goods and update the system accordingly!");
                return;
            }
            else
            {

                var newOrder = new Order()
                {
                    No = Orders.Count + 1,
                    BookId = SelectedBook.Id,
                    BookName = SelectedBook.Title,
                    Number = BindingOrder.Number,
                    Price = SelectedBook.Price,
                    StockQuantity = SelectedBook.Quantity,
                };

                if (SelectedPromotion == null)
                {
                    newOrder.PromotionId = 0;
                    newOrder.PromotionName = "";
                    newOrder.Discount = 0;
                }
                else
                {
                    newOrder.PromotionId = SelectedPromotion.Id;
                    newOrder.PromotionName = SelectedPromotion.Name;
                    newOrder.Discount = SelectedPromotion.Discount;
                }

                Orders.Add(newOrder);
                ExecuteRefreshCommand();
            }
        }

        public async void ExecuteRemoveCommand()
        {
            // - total_price
            if (SelectedOrder == null)
            {
                await App.MainRoot.ShowDialog("No selected selected order", "Please select the order which you would like to delete!");
                return;
            }
            Orders.Remove(SelectedOrder);
            ExecuteRefreshCommand();
        }

        public void ExecuteRefreshCommand()
        {
            CurrentTotalPrice = 0;

            for (int i = 0; i < Orders.Count; i++)
            {
                CurrentTotalPrice += (int)(Orders[i].TotalPrice()* (100 - Orders[i].Discount) / 100);
                Orders[i].No = i + 1;
            }
        }

        public async void ExecuteConfirmCommand()
        {
            try
            {
                if (BindingCustomer.Name == null || BindingCustomer.Name.Equals(""))
                {
                    await App.MainRoot.ShowDialog("Customer name is empty", "Please enter customer name!");
                    return;
                }

                if (Orders.Count() < 1)
                {
                    await App.MainRoot.ShowDialog("There is no any order", "Please add at least one order!");
                    return;
                }

                if (BindingCustomer == null)
                {
                    await App.MainRoot.ShowDialog("No selected customer", "Please select the owner of this bill!");
                    return;
                }

                // add new customer
                int customerId = await _customerRepository.Add(BindingCustomer);

                if (customerId == -1)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                if (customerId < 1)
                {
                    await App.MainRoot.ShowDialog("Add new customer failed", "Please check information of customer!");
                    return;
                }

                BindingCustomer.Id = customerId;

                // add bill values + update total price in real-time + update quantity
                NewBill.CustomerId = BindingCustomer.Id;
                ExecuteRefreshCommand();
                NewBill.TotalPrice = CurrentTotalPrice;
                int newId = await _billRepository.Add(NewBill);

                if (newId == -1)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                if (newId < 1)
                {
                    await App.MainRoot.ShowDialog("Add new bill failed", "Please check information of bill!");
                    return;
                }

                NewBill.Id = newId;

                //var task = await _billRepository.Edit(NewBill);
                //if (!task)
                //{
                //    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                //    return;
                //}

                // add bill detail
                for (int i = 0; i < Orders.Count; i++)
                {
                    Orders[i].BillId = NewBill.Id;
                    var resultFlag = await _billRepository.AddBillDetail(Orders[i]);
                    if (!resultFlag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }

                    await _bookRepository.EditBookQuantity(Orders[i].BookId, Orders[i].StockQuantity - Orders[i].Number);
                }

                await App.MainRoot.ShowDialog("Success", "New bill is added!");
                ExecuteBackCommand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }

        public void ExecuteBackCommand()
        {
            ParentPageNavigation.ViewModel = new OrderManagementViewModel();
        }

        // Constructor
        public AddOrderViewModel()
        {
            _billRepository = new BillRepository();
            _bookRepository = new BookRepository();
            _customerRepository = new CustomerRepository();
            _promotionRepository = new PromotionRepository();
            BindingCustomer = new Customer();
            PromotionDic = new Dictionary<int, List<BookPromotion>>();
            DisplayPromotion = new ObservableCollection<Promotion>();
            BindingOrder = new Order()
            {
                Number = 1,
            };

            // => init NewBill
            NewBill = new Bill() 
            {
                CustomerId = 0,
                TotalPrice = 0,
                TransactionDate =  DateOnly.FromDateTime(DateTime.Now),
            };

            Books = new ObservableCollection<Book>();
            Orders = new ObservableCollection<Order>();
            Promotions = new List<Promotion>();
            CurrentTotalPrice = 0;

            PageLoaded();

            //BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ConfirmCommand = new RelayCommand(ExecuteConfirmCommand);   
            
            AddCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommand = new RelayCommand(ExecuteRemoveCommand);
            RefreshCommand = new RelayCommand(ExecuteRefreshCommand);
        }
    }
}