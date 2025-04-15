using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Common;

namespace ExifRenamer.ViewModels;

public class ExifMetadataExplorerDialogViewModel
{
    public event EventHandler<ExifMetadataDialogResult?>? RequestClose;
    ExifInput _parameter;
    
    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }
    
    public ExifMetadataDialogResult? Result { get; private set; }
    public ExifMetadataExplorerDialogViewModel(ExifInput parameter)
    {
        _parameter = parameter;

        OkCommand = new RelayCommand(OnOkCommand);
        CancelCommand = new RelayCommand(OnCancelCommand);
        
        ExifTags = parameter.ExifTags;
    }
    
    public List<string> ExifTags { get; set; }
    
    private void OnOkCommand()
    {
        Result = new ExifMetadataDialogResult
        {
            ClosingResult = ClosingResult.Ok
        };
        
        RequestClose?.Invoke(this, Result);
    }
    private void OnCancelCommand()
    {
        Result = new ExifMetadataDialogResult
        {
            ClosingResult = ClosingResult.Cancel
        };
        
        RequestClose?.Invoke(this, Result);
    }
}