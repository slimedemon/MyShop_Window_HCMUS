using CommunityToolkit.Mvvm.Input;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using Windows.UI.WebUI;

namespace MyShop.ViewModel
{
    public class BookTypeViewModel : ViewModelBase
    {
        private List<Genre> _genres;
        private ObservableCollection<GenreRow> _displayGenreRowsCollection;
        private IBookRepository _bookRepository;
        private GenreRow _selectedGenreRow;

        private RelayCommand _addCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _saveCommand;
        private RelayCommand<object> _gotFocusCommand;
        public List<Genre> Genres { get => _genres; set => _genres = value; }
        public ObservableCollection<GenreRow> DisplayGenreRowsCollection { get => _displayGenreRowsCollection; set => _displayGenreRowsCollection = value; }
        public IBookRepository BookRepository { get => _bookRepository; set => _bookRepository = value; }
        public GenreRow SelectedGenreRow { get => _selectedGenreRow; set => _selectedGenreRow = value; }
        public RelayCommand AddCommand { get => _addCommand; set => _addCommand = value; }
        public RelayCommand DeleteCommand { get => _deleteCommand; set => _deleteCommand = value; }
        public RelayCommand SaveCommand { get => _saveCommand; set => _saveCommand = value; }
        public RelayCommand<object> GotFocusCommand { get => _gotFocusCommand; set => _gotFocusCommand = value; }

        public BookTypeViewModel()
        {
            _bookRepository = new BookRepository();
            DisplayGenreRowsCollection = new ObservableCollection<GenreRow>();
            PageLoaded();

            AddCommand = new RelayCommand(ExecuteAddCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            GotFocusCommand = new RelayCommand<object>(ExecuteGotFocusCommand);
        }
        private void ExecuteGotFocusCommand(object sender)
        {
            TextBox textBox = sender as TextBox;
        }

        private async void ExecuteSaveCommand()
        {
            if (SelectedGenreRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }

            var task = await _bookRepository.EditGenre(new Genre() { Id = SelectedGenreRow.Id, Name = SelectedGenreRow.Name});
            UpdateDataSource();
        }

        private async void ExecuteDeleteCommand()
        {
            if (SelectedGenreRow == null)
            {
                await App.MainRoot.ShowDialog("No selected item", "Please select an item first!");
                return;
            }

            var task = await _bookRepository.RemoveGenre(SelectedGenreRow.Id);
            UpdateDataSource();
        }

        private async void ExecuteAddCommand()
        {
            var task = await _bookRepository.AddGenre(new Genre { Name = "New Genre" }) ;
            UpdateDataSource();
        }
        public void PageLoaded()
        {
            UpdateDataSource();
        }

        public async void UpdateDataSource()
        {
            Genres = await _bookRepository.GetGenres();
            DisplayGenreRowsCollection.Clear();

            for (int i = 0; i < Genres.Count; i++)
            {
                DisplayGenreRowsCollection.Add(new GenreRow()
                {
                    No = i + 1,
                    Id = Genres[i].Id,
                    Name = Genres[i].Name,
                });
            }
        }

        public class GenreRow: INotifyPropertyChanged
        {
            public int No { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}