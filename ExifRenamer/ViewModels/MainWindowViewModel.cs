using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Models;
using ExifRenamer.Services;

namespace ExifRenamer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly FolderService _folderService;
    private readonly ExifService _exifService;
    private int _totalImagesCount;
    private readonly RenamerService _renamerService;
    private bool _isSelectExifVisible;
    private RenamerPatternModel _selectedBuiltInRenamerPattern;

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _folderService = new FolderService();
        AddFolderCommand = new RelayCommand(async () => await AddFolder());
        PathFolders = new ObservableCollection<DirectoryInfo>();
        RemoveFolderCommand = new RelayCommand<DirectoryInfo>(RemoveFolder);
        SelectExifMetadataCommand = new RelayCommand(OpenExifMetadataDialog);
        _exifService = new ExifService();
        _renamerService = new RenamerService();
        BuiltInRenamerPatterns = _renamerService.GetBuiltInRenamerPatterns().AsReadOnly();
    }

    public ICommand RemoveFolderCommand { get; }
    public ICommand AddFolderCommand { get; }

    public ObservableCollection<DirectoryInfo> PathFolders { get; set; }

    public int TotalImagesCount
    {
        get => _totalImagesCount;
        set => SetProperty(ref _totalImagesCount, value);
    }

    public ICommand SelectExifMetadataCommand { get; }
    public List<ExifModel> ExifMetadata { get; }
    public ICommand OKCommand { get; }
    public ReadOnlyCollection<RenamerPatternModel> BuiltInRenamerPatterns { get; }

    public RenamerPatternModel SelectedBuiltInRenamerPattern
    {
        get => _selectedBuiltInRenamerPattern;
        set
        {
            if (SetProperty(ref _selectedBuiltInRenamerPattern, value))
            {
                IsSelectExifVisible = value.Name == "Custom";
            }
        }
    }

    public bool IsSelectExifVisible
    {
        get => _isSelectExifVisible;
        set => SetProperty(ref _isSelectExifVisible, value);
    }

    private void RemoveFolder(DirectoryInfo? folder)
    {
        if (folder != null && PathFolders.Contains(folder))
        {
            PathFolders.Remove(folder);
            TotalImagesCount = GetTotalImagesCount();
        }
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
    
    private async void OpenExifMetadataDialog()
    {
        if (PathFolders.Any())
        {
            string path = PathFolders.First().FullName;
            var files = _folderService.GetImageFiles(path);
            if (files.Any())
            {
                var exifMetadata = _exifService.ExtractExifData(files.First());
                await _dialogService.ShowExifMetadataDialogAsync();
            }
        }
    }
}