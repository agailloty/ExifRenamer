using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExifRenamer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private FolderService _folderService;
        private int _totalImagesCount;
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
                TotalImagesCount = GetTotalImagesCount();
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
                var directory = new DirectoryInfo(selectedPath);
                if (PathFolders.All(folder => folder.FullName != directory.FullName))
                {
                    PathFolders.Add(new DirectoryInfo(selectedPath));
                    TotalImagesCount = GetTotalImagesCount();
                }
            }
        }

        public ICommand RemoveFolderCommand { get; }
        public ICommand AddFolderCommand { get; }

        public ObservableCollection<DirectoryInfo> PathFolders { get; set; }

        private int GetTotalImagesCount()
        {
            int totalImages = 0;
            foreach (var folder in PathFolders)
            {
                totalImages += _folderService.GetImageFilesCount(folder.FullName);
            }
            return totalImages;
        }

        public int TotalImagesCount
        {
            get => _totalImagesCount;
            set => SetProperty(ref _totalImagesCount, value);
        }
    }
}