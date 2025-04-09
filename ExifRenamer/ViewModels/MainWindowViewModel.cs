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
    private readonly ExifService _exifService;
    private readonly FolderService _folderService;
    private readonly RenamerService _renamerService;
    private bool _isSelectExifVisible;
    private ObservableCollection<PreviewModel> _renamePreviews;
    private RenamerPatternModel _selectedDateRenamerPattern;
    private int _totalImagesCount;
    private bool _isRenameEnabled;
    private bool _hasImages;
    private RenamerDateType _selectedRenamerDateType;
    private string _customFormat;

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        _folderService = new FolderService();
        AddFolderCommand = new RelayCommand(async () => await AddFolder());
        PathFolders = new ObservableCollection<DirectoryInfo>();
        RemoveFolderCommand = new RelayCommand<DirectoryInfo>(RemoveFolder);
        SelectExifMetadataCommand = new RelayCommand(OpenExifMetadataDialog);
        ValidateCustomFormatCommand = new AsyncRelayCommand(UpdateImageCount);
        _exifService = new ExifService();
        _renamerService = new RenamerService();
        BuiltInRenamerPatterns = _renamerService.GetBuiltInRenamerPatterns().AsReadOnly();
        SelectedDateRenamerPattern = BuiltInRenamerPatterns.First();
        RenameCommand = new RelayCommand(RenameImages);
        RenamerDateTypes = new ObservableCollection<RenamerDateType>
        {
            new("Creation date", DateType.Creation),
            new("Photo taken date", DateType.PhotoTaken),
            new("Modification date", DateType.Modification),
        };
        SelectedRenamerDateType = RenamerDateTypes[1];
    }

    public ICommand RemoveFolderCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand ValidateCustomFormatCommand { get; }

    public ObservableCollection<DirectoryInfo> PathFolders { get; set; }

    public int TotalImagesCount
    {
        get => _totalImagesCount;
        set 
        {
            if (SetProperty(ref _totalImagesCount, value))
            {
                HasImages = value > 0;
            } 
        }
    }

    public bool HasImages
    {
        get => _hasImages;
        set => SetProperty(ref _hasImages, value);
    }

    public ICommand SelectExifMetadataCommand { get; }
    public ICommand OKCommand { get; }
    public ReadOnlyCollection<RenamerPatternModel> BuiltInRenamerPatterns { get; }

    public RenamerPatternModel SelectedDateRenamerPattern
    {
        get => _selectedDateRenamerPattern;
        set
        {
            if (SetProperty(ref _selectedDateRenamerPattern, value))
            {
                if (value != null)
                {
                    IsSelectExifVisible = value.Name == "Custom";
                }
            }
            UpdateImageCount();
        }
    }

    public bool IsSelectExifVisible
    {
        get => _isSelectExifVisible;
        set => SetProperty(ref _isSelectExifVisible, value);
    }

    public ObservableCollection<PreviewModel> RenamePreviews
    {
        get => _renamePreviews;
        set => SetProperty(ref _renamePreviews, value);
    }

    public ICommand RenameCommand { get; }

    public bool IsRenameEnabled
    {
        get => _isRenameEnabled;
        set => SetProperty(ref _isRenameEnabled, value);
    }
    
    public ObservableCollection<RenamerDateType> RenamerDateTypes { get; set; }

    public RenamerDateType SelectedRenamerDateType
    {
        get => _selectedRenamerDateType;
        set
        {
            if (SetProperty(ref _selectedRenamerDateType, value))
            {
                SelectedDateRenamerPattern = _selectedDateRenamerPattern;
            }
        }
    }

    public string CustomFormat
    {
        get => _customFormat;
        set
        {
            SetProperty(ref _customFormat, value);
        }
    }

    private void RemoveFolder(DirectoryInfo? folder)
    {
        if (folder == null || !PathFolders.Contains(folder)) return;
        PathFolders.Remove(folder);
        UpdateImageCount();
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
                UpdateImageCount();
            }
        }
    }

    private async Task<PreviewModel[]> GetImagePreviews()
    {
        List<string[]> previews = new();
        foreach (var folder in PathFolders) previews.Add(_folderService.GetImageFiles(folder.FullName));

        var files = previews.SelectMany(preview => preview).ToArray();
        var dateRenamerPattern = SelectedDateRenamerPattern;
        if (SelectedDateRenamerPattern.Name == "Custom" && !string.IsNullOrEmpty(CustomFormat))
        {
            dateRenamerPattern = new RenamerPatternModel
            {
                Name = CustomFormat,
                Description = "Custom format",
            };
        }
        var previewResults = await _renamerService.GetRenamePreviews(files, dateRenamerPattern, SelectedRenamerDateType.DateType);
        return previewResults;
    }

    private async void OpenExifMetadataDialog()
    {
        if (!PathFolders.Any()) return;
        var path = PathFolders.First().FullName;
        var files = _folderService.GetImageFiles(path);
        if (files.Any())
        {
            var exifMetadata = _exifService.ExtractExifData(files.First());
            await _dialogService.ShowExifMetadataDialogAsync();
        }
    }

    private async Task UpdateImageCount()
    {
        IsBusy = true;
        var imagePreviews = await GetImagePreviews();
        RenamePreviews = new ObservableCollection<PreviewModel>(imagePreviews);
        TotalImagesCount = RenamePreviews.Count;
        IsRenameEnabled = TotalImagesCount > 0;
        IsBusy = false;
    }
    
    private void RenameImages()
    {
        var previews = RenamePreviews;
        foreach (var preview in previews)
        {
            var oldPath = Path.Join(preview.FolderPath, preview.OldFilename);
            var newPath = Path.Join(preview.FolderPath, preview.NewNameWithExtension);
            File.Move(oldPath, newPath, overwrite:true);
        }
        UpdateImageCount();
    }
}