using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExifRenamer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private FolderService _folderService;
        private const byte maxFolders = 10;

        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _folderService = new FolderService();
            AddFolderCommand = new RelayCommand(async () => await AddFolder(), CanAddFolder);
            PathFolders = new ObservableCollection<DirectoryInfo>();
            RemoveFolderCommand = new RelayCommand<DirectoryInfo>(RemoveFolder);
        }

        private void RemoveFolder(DirectoryInfo? folder)
        {
            if (folder != null && PathFolders.Contains(folder))
            {
                PathFolders.Remove(folder);
            }
        }

        private bool CanAddFolder()
        {
            return PathFolders.Count < maxFolders;
        }

        private async Task AddFolder()
        {
            var selectedPath = await _dialogService.ShowFolderBrowserDialogAsync();
            if (selectedPath != null)
            {
                PathFolders.Add(new DirectoryInfo(selectedPath));
                TotalImagesCount = GetTotalImagesCount();
            }
        }

        public ICommand RemoveFolderCommand { get; }
        public ICommand AddFolderCommand { get; }

        public ObservableCollection<DirectoryInfo> PathFolders { get; set; }
        
        public int GetTotalImagesCount()
        {
            int totalImages = 0;
            foreach (var folder in PathFolders)
            {
                totalImages += _folderService.GetImageFilesCount(folder.FullName);
            }
            return totalImages;
        }
        
        public int TotalImagesCount { get; set; }
    }
}