using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.View;

namespace MyShop.ViewModel
{
    public class CustomerManagementViewModel : ViewModelBase
    {
        // Fields
        private DateOnly _dateFrom;
        private DateOnly _dateTo;
        private int _totalItems;
        private Dictionary<int, List<BillDetailRow>> _billDetailRowDic; //int <<billId>> respective to the bill's list of <<billDetail>>

        private IBillRepository _billRepository;
        private ICustomerRepository _customerRepository;

        private CustomerRow _selectedCustomerRow;
        private ObservableCollection<CustomerRow> _customerRowList;
        private ObservableCollection<BillDetailRow> _selectedBillDetailRowList;

        //-> Commands
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SearchCommand { get; }
        public int TotalItems { get => _totalItems; set => _totalItems = value; }
        public ObservableCollection<CustomerRow> CustomerRowList { get => _customerRowList; set => _customerRowList = value; }
        public ObservableCollection<BillDetailRow> SelectedBillDetailList { get => _selectedBillDetailRowList; set => _selectedBillDetailRowList = value; }
        public string CustomerName { get; set; }

        // getter, setter
        public DateOnly DateFrom
        {
            get => _dateFrom;
            set
            {
                //SetProperty(ref _date, value);
                _dateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
            }
        }

        public DateOnly DateTo
        {
            get => _dateTo;
            set
            {
                //SetProperty(ref _date, value);
                _dateTo = value;
                OnPropertyChanged(nameof(DateTo));
            }
        }

        public CustomerRow SelectedCustomerRow
        {
            get => _selectedCustomerRow;
            set
            {
                _selectedCustomerRow = value;
                SelectedBillDetailList.Clear();

                if (value == null)
                {
                    CustomerName = "";
                    return;
                }
                CustomerName = value.Name;
                List<BillDetailRow> billDetailRows;
                _billDetailRowDic.TryGetValue(value.BillId, out billDetailRows);
                billDetailRows.ForEach(row =>
                {
                    SelectedBillDetailList.Add(row);
                });

                OnPropertyChanged(nameof(SelectedCustomerRow));
            }
        }

        // Constructor
        public CustomerManagementViewModel()
        {
            SaveCurrentPage();

            _billRepository = new BillRepository();
            _customerRepository = new CustomerRepository();
            _billDetailRowDic = new Dictionary<int, List<BillDetailRow>>();
            CustomerRowList = new ObservableCollection<CustomerRow>();
            SelectedBillDetailList = new ObservableCollection<BillDetailRow>();

            //Initial paging info
            {
                DateFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                DateTo = DateOnly.FromDateTime(DateTime.Now);
            }
            ExecuteGetAllCommand();

            AddCommand = new RelayCommand(ExecuteCreateCustomerCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCustomerCommand);
            EditCommand = new RelayCommand(ExecuteEditCustomerCommand);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "CustomerMangagementPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public async void ExecuteCreateCustomerCommand()
        {
            var confirmed = await App.MainRoot.ShowYesCancelDialog("You only create new customer by adding a new bill, do you want navigate to \"Create a new bill\" page?", "Yes", "Cancel");
            if (confirmed == true) 
            {
                int index;
                for (index = 0; index < MainNavigationPage.NVMain.MenuItems.Count; index++)
                {
                    if (((NavigationViewItem)MainNavigationPage.NVMain.MenuItems[index]).Content.Equals("Order Management"))
                    {
                        MainNavigationPage.NVMain.SelectedItem = MainNavigationPage.NVMain.MenuItems[index];
                        break;
                    }
                }
                ParentPageNavigation.ViewModel = new AddOrderViewModel();
            }
        }

        public async void ExecuteDeleteCustomerCommand()
        {
            if (SelectedCustomerRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }
            var confirmed = await App.MainRoot.ShowYesCancelDialog("Delete this item which may delete relative bill and orders?", "Delete", "Cancel");

            if (confirmed == true)
            {
                int key = SelectedCustomerRow.BillId;
                int customerId = SelectedCustomerRow.Id;

                // remove from DETAILTED_BILL
                List<BillDetailRow> billDetailRows;
                _billDetailRowDic.TryGetValue(key, out billDetailRows);
                for (int i = 0; i < billDetailRows.Count; i++)
                {
                    var resultFlag = await _billRepository.RemoveBillDetail(key, billDetailRows[i].BookId);
                    if (!resultFlag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                // remove from BILL
                var task1 = await _billRepository.Remove(key);
                if (!task1)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                // remove from CUSTOMER
                var _customerRepository = new CustomerRepository();
                var task2 = await _customerRepository.Remove(customerId);
                if (!task2)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                _billDetailRowDic.Remove(key);
                CustomerRowList.Remove(SelectedCustomerRow);
                for (int i = 0; i < CustomerRowList.Count; i++)
                {
                    CustomerRowList[i].No = i + 1;
                }

                await App.MainRoot.ShowDialog("Success", "The customer is removed!");
            }
        }

        public async void ExecuteEditCustomerCommand()
        {
            if (SelectedCustomerRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }

            var confirmed = await App.MainRoot.ShowYesCancelDialog("You only edit the customer by editing properly corresponding bill, do you want navigate to \"Edit the bill\" page?", "Yes", "Cancel");
            if (confirmed == true)
            {
                int index;
                for (index = 0; index < MainNavigationPage.NVMain.MenuItems.Count; index++)
                {
                    if (((NavigationViewItem)MainNavigationPage.NVMain.MenuItems[index]).Content.Equals("Order Management"))
                    {
                        MainNavigationPage.NVMain.SelectedItem = MainNavigationPage.NVMain.MenuItems[index];
                        break;
                    }
                }

                ParentPageNavigation.ViewModel = new EditOrderViewModel(new Bill()
                {
                    Id = SelectedCustomerRow.BillId,
                    CustomerId = SelectedCustomerRow.Id,
                    TransactionDate = SelectedCustomerRow.TransactionDate,
                    TotalPrice = SelectedCustomerRow.TotalPrice,
                });
            }
        }
            

        public async void ExecuteGetAllCommand()
        {
            CustomerRowList.Clear();
            _billDetailRowDic.Clear();
            TotalItems = 0;

            // get all from date to date
            var task = await _customerRepository.GetAll(DateFrom, DateTo);
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");

                // create empty task to continue flow
                task = new List<Customer>();
            }

            for (int i = 0; i < task.Count; i++)
            {
                Bill bill = await _billRepository.GetByCustomerId(task[i].Id);
                if (bill == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    // purpose: continue flow
                    bill = new Bill();
                }

                List<Order> temp = await _billRepository.GetBillDetailById(bill.Id);
                if (temp == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    // purpose: continue flow
                    temp = new List<Order>();
                }

                List<BillDetailRow> billDetailRows = new List<BillDetailRow>();

                for (int j = 0; j < temp.Count; j++)
                {
                    billDetailRows.Add(new BillDetailRow()
                    {
                        No = j + 1,
                        BillId = temp[j].BillId,
                        BookId = temp[j].BookId,
                        BookName = temp[j].BookName,
                        Number = temp[j].Number,
                        Price = temp[j].Price,
                        PromotionId = temp[j].PromotionId,
                        PromotionName = temp[j].PromotionName,
                        Discount = temp[j].Discount
                    });
                }

                CustomerRowList.Add(new CustomerRow()
                {
                    No = i + 1,
                    Id = task[i].Id,
                    Name = task[i].Name,
                    Phone = task[i].PhoneNumber,
                    Address = task[i].Address,
                    BillId = bill.Id,
                    TotalPrice = bill.TotalPrice,
                    TransactionDate = bill.TransactionDate,
                });

                _billDetailRowDic.TryAdd(bill.Id, billDetailRows);
            }
            TotalItems = CustomerRowList.Count;
        }

        private async void ExecuteSearchCommand()
        {
            if (DateFrom <= DateTo)
            {
                ExecuteGetAllCommand();
            }
            else
            {
                await App.MainRoot.ShowDialog("Please select correct dates", "Start date must be earlier or equal to end date!");
            }
        }

        public class CustomerRow : INotifyPropertyChanged
        {
            public int No { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public int BillId { get; set; }
            public DateOnly? TransactionDate { get; set; }
            public int TotalPrice { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class BillDetailRow : INotifyPropertyChanged
        {
            public int No { get; set; }
            public string BookName { get; set; }
            public int Number { get; set; }
            public int Price { get; set; }
            public int BookId { get; set; }
            public int BillId { get; set; }
            public int PromotionId { get; set; }
            public string PromotionName { get; set; }
            public int Discount { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
