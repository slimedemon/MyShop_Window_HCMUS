using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShop.ViewModel
{
    public class EditPromotionViewModel: ViewModelBase
    {
        // Fields
        private IPromotionRepository _promotionRepository;
        private IBookRepository _bookRepository;

        private Promotion _currentPromotion;
        private ObservableCollection<Book> _books;
        private Promotion _bindingPromotion;
        private BookInPromotion _selectedBookInPromotion;
        private Book _selectedBook;
        private List<BookInPromotion> _originalBookInPromotions;

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


        public Promotion CurrentPromotion { get => _currentPromotion; set => _currentPromotion = value; }
        public ObservableCollection<Book> Books { get => _books; set => _books = value; }
        public ObservableCollection<BookInPromotion> BookInPromotions { get; set; }
        public Promotion BindingPromotion { get => _bindingPromotion; set => _bindingPromotion = value; }
        public BookInPromotion SelectedBookInPromotion { get => _selectedBookInPromotion; set => _selectedBookInPromotion = value; }

        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
            }
        }

        public async void PageLoaded()
        {
            var task = await _bookRepository.GetAll();
            if (task == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from the database!");
                // purpose: continue flow;
                task = new List<Book>();
            }

            task.ForEach(book => Books.Add(book));

            // Get all Orders and copy to original BookInPromotions.
            var task2 = await _promotionRepository.GetAllBookPromotionsByPromotionId(CurrentPromotion.Id);
            if (task2 == null)
            {
                await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                // purpose: continue flow
                task2 = new List<BookPromotion>();
            }

            for (int i = 0; i < task2.Count; i++)
            {
                _originalBookInPromotions.Add(new BookInPromotion
                {
                    No = i + 1,
                   BookId = task2[i].BookId,
                    BookName = task2[i].BookName,
                });
                BookInPromotions[i].No = i + 1;
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
            if (BookInPromotions.Select(x => x.BookId).ToList().Contains(SelectedBook.Id))
            {
                await App.MainRoot.ShowDialog("Duplicate book", "This book already exists in the order!");
                return;
            }
            else
            {
                var newBookInPromotion = new BookInPromotion()
                {
                    No = BookInPromotions.Count + 1,
                    BookId = SelectedBook.Id,
                    BookName = SelectedBook.Title,
                };

                BookInPromotions.Add(newBookInPromotion);
            }
        }

        public async void ExecuteRemoveCommand()
        {
            // - total_price
            if (SelectedBookInPromotion == null)
            {
                await App.MainRoot.ShowDialog("No selected selected order", "Please select the order which you would like to delete!");
                return;
            }
            BookInPromotions.Remove(SelectedBookInPromotion);

            for (int i = 0; i < BookInPromotions.Count; i++)
            {
                BookInPromotions[i].No = i + 1;
            }
        }

        public async void ExecuteConfirmCommand()
        {
            try
            {
                if (CurrentPromotion.Name == null || CurrentPromotion.Name.Equals(""))
                {
                    await App.MainRoot.ShowDialog("The name of promotion is empty", "Please enter name of promotion!");
                    return;
                }

                if (CurrentPromotion.Discount <= 0 || CurrentPromotion.Discount > 100)
                {
                    await App.MainRoot.ShowDialog("Invalid discount", "Please check discount again!");
                    return;
                }

                if (BookInPromotions.Count() < 1)
                {
                    await App.MainRoot.ShowDialog("There is no any order", "Please add at least one order!");
                    return;
                }

                bool task3 = await _promotionRepository.EditPromotion(CurrentPromotion);

                if (!task3)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                    return;
                }

                // remove original bill detail
                for (int i = 0; i < _originalBookInPromotions.Count; i++)
                {
                    var flag = await _promotionRepository.RemoveBookPromotion(CurrentPromotion.Id, _originalBookInPromotions[i].BookId);
                    if (!flag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                // Add books in promotion
                for (int i = 0; i < BookInPromotions.Count; i++)
                {
                    var resultFlag = await _promotionRepository.AddBookPromotion(new BookPromotion
                    {
                        PromotionId = CurrentPromotion.Id,
                        BookId = BookInPromotions[i].BookId
                    });

                    if (!resultFlag)
                    {
                        await App.MainRoot.ShowDialog("Error", "Something is broken when system is retrieving data from database!");
                        return;
                    }
                }

                await App.MainRoot.ShowDialog("Success", "New promotion is added!");
                ExecuteBackCommand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void ExecuteBackCommand()
        {
            ParentPageNavigation.ViewModel = new PromotionManagementViewModel();
        }

        // Constructor
        public EditPromotionViewModel(Promotion promotion)
        {
            _originalBookInPromotions = new List<BookInPromotion>();
            _promotionRepository = new PromotionRepository();
            _bookRepository = new BookRepository();
            BindingPromotion = new Promotion();

            // => init NewBill
            CurrentPromotion = new Promotion()
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Discount = promotion.Discount,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
            };

            Books = new ObservableCollection<Book>();
            BookInPromotions = new ObservableCollection<BookInPromotion>();

            PageLoaded();

            //BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ConfirmCommand = new RelayCommand(ExecuteConfirmCommand);

            AddCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommand = new RelayCommand(ExecuteRemoveCommand);
        }

        public class BookInPromotion : INotifyPropertyChanged
        {
            public int No { get; set; }
            public int BookId { get; set; }
            public string BookName { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
