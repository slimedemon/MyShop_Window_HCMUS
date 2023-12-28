using CommunityToolkit.Mvvm.Input;
using Microsoft.Windows.ApplicationModel.Resources;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShop.ViewModel
{
    class EditOrderViewModel : ViewModelBase
    {
        // Fields
        private IBillRepository _billRepository;
        private IBookRepository _bookRepository;
        private ICustomerRepository _customerRepository;

        private Bill _currentBill;

        private ObservableCollection<Book> _books;

        private Customer _bindingCustomer;
        private Order _bindingOrder;
        private Order _selectedOrder;

        private int _currentTotalPrice;
        private Book _selectedBook;
        private List<Order> _originalOrders;

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


        public Bill CurrentBill { get => _currentBill; set => _currentBill = value; }
        public ObservableCollection<Book> Books { get => _books; set => _books = value; }
        public ObservableCollection<Order> Orders { get; set; }
        public Order BindingOrder { get => _bindingOrder; set => _bindingOrder = value; }
        public Order SelectedOrder { get => _selectedOrder; set => _selectedOrder = value; }
        public Customer BindingCustomer { get => _bindingCustomer; set => _bindingCustomer = value; }

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
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

        // Constructor
        public EditOrderViewModel(Bill currentBill)
        {
            _billRepository = new BillRepository();
            _bookRepository = new BookRepository();
            _customerRepository = new CustomerRepository();
            _originalOrders = new List<Order>();
            BindingCustomer = new Customer();
            BindingOrder = new Order()
            {
                Number = 1,
            };

            // => init CurrentBill
            CurrentBill = currentBill;

            Books = new ObservableCollection<Book>();
            Orders = new ObservableCollection<Order>();
            CurrentTotalPrice = CurrentBill.TotalPrice;

            PageLoaded();

            //BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ConfirmCommand = new RelayCommand(ExecuteConfirmCommand);

            AddCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommand = new RelayCommand(ExecuteRemoveCommand);
            RefreshCommand = new RelayCommand(ExecuteRefreshCommand);
        }

        public async void PageLoaded()
        {
            // Get all books
            var task1 = await _bookRepository.GetAll();
            if (task1 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from the database!");
                // purpose: to continue flow;
                task1 = new List<Book>();
            }
            task1.ForEach(book => Books.Add(book));

            // Get customer
            var task2 = await _customerRepository.GetById(CurrentBill.CustomerId);
            if(task2 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                return;
            }

            BindingCustomer = task2;

            // Get all Orders
            var task3 = await _billRepository.GetBillDetailById(CurrentBill.Id);
            if (task3 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                task3 = new List<Order>();
            }

            //update current stock for all orders and restore quantity for each book before edit (when record is saved, stock is updated)
            for (int i = 0; i < task3.Count(); i++)
            {
                // save all original order
                _originalOrders.Add(new Order
                {
                    Number = task3[i].Number,
                    BillId = task3[i].BillId,
                    BookId = task3[i].BookId,
                    BookName = task3[i].BookName,
                    No = task3[i].No,
                    Price = task3[i].Price,
                    StockQuantity = task3[i].StockQuantity,
                });

                var temp = Books.Where(book => book.Id == task3[i].BookId).First();
                if (temp != null)
                {
                    temp.Quantity = temp.Quantity + task3[i].Number;
                }
                
                task3[i].StockQuantity = temp.Quantity;
                await _bookRepository.EditBookQuantity(task3[i].BookId, task3[i].StockQuantity);

                // Update No.
                task3[i].No = i + 1;
            }

            task3.ForEach(order => Orders.Add(order));
            ExecuteRefreshCommand();
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
            if (BindingOrder.Number > SelectedBook.Quantity)
            {
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
                    StockQuantity = SelectedBook.Quantity
                };

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
                CurrentTotalPrice += Orders[i].TotalPrice();
            }

            for (int i = 0; i < Orders.Count; i++)
            {
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

                // Edit current customer
                var resultFlag = await _customerRepository.Edit(BindingCustomer);
                if (resultFlag)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                // add bill values + update total price in real-time + update quantity
                ExecuteRefreshCommand();
                CurrentBill.TotalPrice = CurrentTotalPrice;
                var task = await _billRepository.Edit(CurrentBill);

                if (!task)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                // remove original bill detail
                for (int i = 0; i < _originalOrders.Count; i++)
                {
                    var flag = await _billRepository.RemoveBillDetail(_originalOrders[i].BillId, _originalOrders[i].BookId);
                    if (!flag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                // add bill detail
                for (int i = 0; i < Orders.Count; i++)
                {
                    Orders[i].BillId = CurrentBill.Id;
                    var flag = await _billRepository.AddBillDetail(Orders[i]);
                    if (!flag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }

                    await _bookRepository.EditBookQuantity(Orders[i].BookId, Orders[i].StockQuantity - Orders[i].Number);
                }

                await App.MainRoot.ShowDialog("Success", "Current bill is saved!");

                // Back to parent page
                ParentPageNavigation.ViewModel = new OrderManagementViewModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public async void ExecuteBackCommand()
        {
            // recover quantity property of Book
            for (int i = 0; i < _originalOrders.Count(); i++)
            {
                var temp = Books.Where(book => book.Id == _originalOrders[i].BookId).First();
                await _bookRepository.EditBookQuantity(_originalOrders[i].BookId, temp.Quantity - _originalOrders[i].Number);
            }

            ParentPageNavigation.ViewModel = new OrderManagementViewModel();
        }
    }
}