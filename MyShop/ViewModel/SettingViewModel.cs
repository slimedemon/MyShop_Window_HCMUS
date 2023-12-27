using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Model;
using MyShop.Repository;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Preview;

namespace MyShop.ViewModel
{
    public class SettingViewModel: ViewModelBase
    {
        // Fields
        private bool _toggleSwitchIsOn;
        private int _itemsPerPage;
        private IBookRepository _bookRepository;

        // getter, setter
        public bool ToggleSwitchIsOn 
        { 
            get => _toggleSwitchIsOn;
            set
            {
                _toggleSwitchIsOn = value;
                ExecuteSaveRememberPageCommand();
                OnPropertyChanged(nameof(ToggleSwitchIsOn));
            }
        }
        public int ItemsPerPage 
        { 
            get => _itemsPerPage;
            set => _itemsPerPage = value;
        }


        public RelayCommand SaveSettingCommand { get => _saveSettingCommand; set => _saveSettingCommand = value; }
        public RelayCommand ImportByExcelCommand { get => _importByExcelCommand; set => _importByExcelCommand = value; }
        public RelayCommand ImportByAccessCommand { get => _importByAccessCommand; set => _importByAccessCommand = value; }

        //-> Commands
        private RelayCommand<object> _saveRememberPageCommand;
        private RelayCommand _saveSettingCommand;
        private RelayCommand _importByExcelCommand;
        private RelayCommand _importByAccessCommand;

        // Constructor
        public SettingViewModel()
        {
            ToggleSwitchIsOn = false;
            _bookRepository = new BookRepository();
            SaveSettingCommand = new RelayCommand(ExecuteSaveSettingCommand);
            PageLoaded();
            ImportByExcelCommand = new RelayCommand(ExecuteImportByExcelCommand);
            ImportByAccessCommand = new RelayCommand(ExecuteImportByAccessCommand);
        }

        private void PageLoaded()
        {
            try
            {
                ItemsPerPage = Convert.ToInt32(ConfigurationManager.AppSettings["ItemsPerPage"]);
                ToggleSwitchIsOn = Convert.ToBoolean(ConfigurationManager.AppSettings["RememberPage"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void ExecuteSaveSettingCommand()
        {
            try
            {
                var sysconfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                sysconfig.AppSettings.Settings["ItemsPerPage"].Value = ItemsPerPage.ToString();
                sysconfig.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");

                await App.MainRoot.ShowDialog("Success", "Your changes have been successfully saved and applied.");
            }
            catch(Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }

        }

        private void ExecuteSaveRememberPageCommand()
        {
            try
            {
                var sysconfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                sysconfig.AppSettings.Settings["RememberPage"].Value = ToggleSwitchIsOn.ToString();
                sysconfig.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ExecuteImportByAccessCommand()
        {
            await App.MainRoot.ShowDialog("Warning", "This action refresh all the records from database");
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".accdb");
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                //Read data from Access file
                var genres = await _bookRepository.ReadBookGenreFromAccessFile(file);
                if (genres == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when genres are being imported!");
                    return;
                }

                var books = await _bookRepository.ReadBookDataFromAccessFile(file);
                if (books == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when books are being imported!");
                    return;
                }

                try
                {
                    _bookRepository.Refresh(books, genres);
                    await App.MainRoot.ShowDialog("NOTIFICATION", "Restore successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system refreshes tables");
                }
            }
        }

        private async void ExecuteImportByExcelCommand()
        {
            await App.MainRoot.ShowDialog("Warning", "This action can replace all the record form the table from database");
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".xlsx");
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                //Read data from Excel file
                var genres = await _bookRepository.ReadBookGenreFromExcelFile(file);
                if (genres == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when genres are being imported!");
                    return;
                }

                var books = await _bookRepository.ReadBookDataFromExcelFile(file);
                if (books == null)
                {
                    await App.MainRoot.ShowDialog("Error", "Something is broken when books are being imported!");
                    return;
                }

                try
                {
                    _bookRepository.Refresh(books, genres);
                    await App.MainRoot.ShowDialog("NOTIFICATION", "Restore successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    await App.MainRoot.ShowDialog("Error", "Something is broken when system refreshes tables");
                }
            }

        }
    }
}
