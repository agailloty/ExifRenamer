using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Services;

namespace ExifRenamer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const byte maxFolders = 10;
    private readonly IDialogService _dialogService;
    private readonly FolderService _folderService;
    private int _totalImagesCount;

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _folderService = new FolderService();
        AddFolderCommand = new RelayCommand(async () => await AddFolder(), CanAddFolder);
        PathFolders = new ObservableCollection<DirectoryInfo>();
        RemoveFolderCommand = new RelayCommand<DirectoryInfo>(RemoveFolder);
    }

    public ICommand RemoveFolderCommand { get; }
    public ICommand AddFolderCommand { get; }

    public ObservableCollection<DirectoryInfo> PathFolders { get; set; }

    public int TotalImagesCount
    {
        get => _totalImagesCount;
        set => SetProperty(ref _totalImagesCount, value);
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

    private int GetTotalImagesCount()
    {
        var totalImages = 0;
        foreach (var folder in PathFolders) totalImages += _folderService.GetImageFilesCount(folder.FullName);
        return totalImages;
    }
}