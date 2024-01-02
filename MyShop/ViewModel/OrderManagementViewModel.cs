using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Services.Store;


namespace MyShop.ViewModel
{
    class OrderManagementViewModel : ViewModelBase
    {
        // Fields
        private DateOnly _dateFrom;
        private DateOnly _dateTo;
        private int _totalItems;
        private Dictionary<int, List<BillDetailRow>> _billDetailRowDic; //int <<billId>> respective to the bill's list of <<billDetail>>

        private IBillRepository _billRepository;
        private ICustomerRepository _customerRepository;

        private BillRow _selectedBillRow;
        private ObservableCollection<BillRow> _billRowList;
        private ObservableCollection<BillDetailRow> _selectedBillDetailRowList;

        //-> Commands
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SearchCommand { get; }
        public int TotalItems { get => _totalItems; set => _totalItems = value; }
        public ObservableCollection<BillRow> BillRowList { get => _billRowList; set => _billRowList = value; }
        public ObservableCollection<BillDetailRow> SelectedBillDetailList { get => _selectedBillDetailRowList; set => _selectedBillDetailRowList = value; }

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

        public BillRow SelectedBillRow
        {
            get => _selectedBillRow;
            set
            {
                _selectedBillRow = value;
                SelectedBillDetailList.Clear();

                if (value == null)
                {
                    return;
                }

                List<BillDetailRow> billDetailRows;
                _billDetailRowDic.TryGetValue(value.BillId, out billDetailRows);
                billDetailRows.ForEach(row =>
                {
                    SelectedBillDetailList.Add(row);
                });

                OnPropertyChanged(nameof(SelectedBillRow));
            }
        }

        // Constructor
        public OrderManagementViewModel() {
            SaveCurrentPage();

            _billRepository = new BillRepository();
            _customerRepository = new CustomerRepository();
            _billDetailRowDic = new Dictionary<int, List<BillDetailRow>>();
            BillRowList = new ObservableCollection<BillRow>();
            SelectedBillDetailList = new ObservableCollection<BillDetailRow>();

            //Initial paging info
            {
                DateFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                DateTo = DateOnly.FromDateTime(DateTime.Now);
            }
            ExecuteGetAllCommand();

            AddCommand = new RelayCommand(ExecuteCreateOrderCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteOrderCommand);
            EditCommand = new RelayCommand(ExecuteEditOrderCommand);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "OrderManagementPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void ExecuteCreateOrderCommand()
        {
            ParentPageNavigation.ViewModel = new AddOrderViewModel();
        }

        public async void ExecuteDeleteOrderCommand()
        {
            if (SelectedBillRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }
            var confirmed = await App.MainRoot.ShowYesCancelDialog("Delete this item?","Delete","Cancel");

            if (confirmed == true)
            {
                int key = SelectedBillRow.BillId;

                // remove from DETAILTED_BILL
                List<BillDetailRow> billDetailRows;
                _billDetailRowDic.TryGetValue(key, out billDetailRows);
                for (int i = 0;i < billDetailRows.Count; i++)
                {
                    var resultFlag = await _billRepository.RemoveBillDetail(key, billDetailRows[i].BookId);
                    if (!resultFlag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                _billDetailRowDic.Remove(key);
                BillRowList.Remove(SelectedBillRow);
                for (int i = 0; i < BillRowList.Count; i++)
                {
                    BillRowList[i].No = i + 1;
                }

                // remove from BILL
                var task = await _billRepository.Remove(key);
                if (!task) 
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }
                    
                await App.MainRoot.ShowDialog("Success", "Order is removed!");
            }
        }

        public async void ExecuteEditOrderCommand()
        {
            if (SelectedBillRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }

            ParentPageNavigation.ViewModel = new EditOrderViewModel(new Bill() 
            { 
                 Id = SelectedBillRow.BillId,
                 CustomerId = SelectedBillRow.CustomerId,
                 TransactionDate = SelectedBillRow.TransactionDate,
                 TotalPrice = SelectedBillRow.TotalPrice,
            });
        }

        public async void ExecuteGetAllCommand()
        {
            BillRowList.Clear();
            _billDetailRowDic.Clear();
            SelectedBillRow = null;
            TotalItems = 0;

            // get all from date to date
            var task = await _billRepository.GetAll(DateFrom, DateTo);
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                
                // create empty task to continue flow
                task = new List<Bill>();
            }

            for (int i = 0; i < task.Count; i++)
            {
                List<Order> temp = await _billRepository.GetBillDetailById(task[i].Id);
                if (temp == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    // purpose: continue flow
                    temp = new List<Order>();
                }

                Customer customer = await _customerRepository.GetById(task[i].CustomerId);
                if(customer == null) 
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    // purpose: continue flow
                    customer = new Customer();
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

                BillRowList.Add(new BillRow()
                {
                    No = i + 1,
                    BillId = task[i].Id,
                    CustomerId = task[i].CustomerId,
                    TotalPrice = task[i].TotalPrice,
                    TransactionDate = task[i].TransactionDate,
                    CustomerName = customer.Name,
                });

                _billDetailRowDic.Add(task[i].Id, billDetailRows);
            }
            TotalItems = BillRowList.Count;
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

        public class BillRow : INotifyPropertyChanged
        {
            public int No { get; set; }
            public string CustomerName { get; set; }
            public int TotalPrice { get; set; }
            public DateOnly? TransactionDate { get; set; }
            public int BillId { get; set; }
            public int CustomerId { get; set; }

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
