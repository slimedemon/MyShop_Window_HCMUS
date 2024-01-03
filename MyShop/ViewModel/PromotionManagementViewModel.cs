using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.ViewModel
{
    public class PromotionManagementViewModel : ViewModelBase
    {
        // Fields
        private DateOnly _dateFrom;
        private DateOnly _dateTo;
        private int _totalItems;
        private Dictionary<int, List<BookPromotionRow>> _bookPromotionRowDic; //int <<billId>> respective to the bill's list of <<billDetail>>

        private IPromotionRepository _promotionRepository;

        private PromotionRow _selectedPromotionRow;
        private ObservableCollection<PromotionRow> _promotionList;
        private ObservableCollection<BookPromotionRow> _selectedBookPromotionRowList;

        //-> Commands
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SearchCommand { get; }
        public int TotalItems { get => _totalItems; set => _totalItems = value; }
        public ObservableCollection<PromotionRow> PromotionRowList { get => _promotionList; set => _promotionList = value; }
        public ObservableCollection<BookPromotionRow> SelectedBookPromotionList { get => _selectedBookPromotionRowList; set => _selectedBookPromotionRowList = value; }

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

        public PromotionRow SelectedPromotionRow
        {
            get => _selectedPromotionRow;
            set
            {
                _selectedPromotionRow = value;
                SelectedBookPromotionList.Clear();

                if (value == null)
                {
                    return;
                }

                List<BookPromotionRow> bookPromotionRows;
                _bookPromotionRowDic.TryGetValue(value.Id, out bookPromotionRows);
                bookPromotionRows.ForEach(row =>
                {
                    SelectedBookPromotionList.Add(row);
                });

                OnPropertyChanged(nameof(SelectedPromotionRow));
            }
        }

        // Constructor
        public PromotionManagementViewModel()
        {
            SaveCurrentPage();

            _promotionRepository = new PromotionRepository();
            _bookPromotionRowDic = new Dictionary<int, List<BookPromotionRow>>();
            PromotionRowList = new ObservableCollection<PromotionRow>();
            SelectedBookPromotionList = new ObservableCollection<BookPromotionRow>();

            //Initial paging info
            {
                DateFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
                DateTo = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            ExecuteGetAllCommand();

            AddCommand = new RelayCommand(ExecuteCreatePromotionCommand);
            DeleteCommand = new RelayCommand(ExecuteDeletePromotionCommand);
            EditCommand = new RelayCommand(ExecuteEditPromotionCommand);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
        }

        private void SaveCurrentPage()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]))
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "PromotionManagementPage";
            }
            else
            {
                configuration.AppSettings.Settings["CurrentPage"].Value = "DashboardPage";
            }

            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void ExecuteCreatePromotionCommand()
        {
            ParentPageNavigation.ViewModel = new AddPromotionViewModel();
        }

        public async void ExecuteDeletePromotionCommand()
        {
            if (SelectedPromotionRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }
            var confirmed = await App.MainRoot.ShowYesCancelDialog("Delete this item?", "Delete", "Cancel");

            if (confirmed == true)
            {
                int key = SelectedPromotionRow.Id;

                // remove from BOOK_PROMOTION
                List<BookPromotionRow> bookPromotionRows;
                _bookPromotionRowDic.TryGetValue(key, out bookPromotionRows);
                for (int i = 0; i < bookPromotionRows.Count; i++)
                {
                    var resultFlag = await _promotionRepository.RemoveBookPromotion(key, bookPromotionRows[i].BookId);
                    if (!resultFlag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                // remove from PROMOTION
                var task = await _promotionRepository.RemovePromotion(key);
                if (!task)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                _bookPromotionRowDic.Remove(key);
                PromotionRowList.Remove(SelectedPromotionRow);

                for (int i = 0; i < PromotionRowList.Count; i++)
                {
                    PromotionRowList[i].No = i + 1;
                }

                await App.MainRoot.ShowDialog("Success", "Promotion is removed!");
            }
        }

        public async void ExecuteEditPromotionCommand()
        {
            if (SelectedPromotionRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }

            ParentPageNavigation.ViewModel = new EditPromotionViewModel(new Promotion()
            {
                Id = SelectedPromotionRow.Id,
                Name = SelectedPromotionRow.Name,
                Discount = SelectedPromotionRow.Discount,
                StartDate = SelectedPromotionRow.StartDate,
                EndDate = SelectedPromotionRow.EndDate,
            });
        }

        public async void ExecuteGetAllCommand()
        {
            PromotionRowList.Clear();
            _bookPromotionRowDic.Clear();
            TotalItems = 0;

            // get all from date to date
            var task = await _promotionRepository.GetAllPromotions(DateFrom, DateTo);
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");

                // create empty task to continue flow
                task = new List<Promotion>();
            }

            for (int i = 0; i < task.Count; i++)
            {
                List<BookPromotion> temp = await _promotionRepository.GetAllBookPromotionsByPromotionId(task[i].Id);
                if (temp == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    // purpose: continue flow
                    temp = new List<BookPromotion>();
                }

                List<BookPromotionRow> bookPromotionRows = new List<BookPromotionRow>();

                for (int j = 0; j < temp.Count; j++)
                {
                    bookPromotionRows.Add(new BookPromotionRow()
                    {
                        No = j + 1,
                        PromotionId = temp[j].PromotionId,
                        BookId = temp[j].BookId,
                        BookName = temp[j].BookName,
                    });
                }

                PromotionRowList.Add(new PromotionRow()
                {
                    No = i + 1,
                    Id = task[i].Id,
                    Name = task[i].Name,
                    Discount = task[i].Discount,
                    StartDate = task[i].StartDate,
                    EndDate = task[i].EndDate,
                });

                _bookPromotionRowDic.Add(task[i].Id, bookPromotionRows);
            }
            TotalItems = PromotionRowList.Count;
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

        public class PromotionRow : INotifyPropertyChanged
        {
            public int No { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public int Discount { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class BookPromotionRow : INotifyPropertyChanged
        {
            public int No { get; set; }
            public int PromotionId { get; set; }
            public int BookId { get; set; }
            public string BookName { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
