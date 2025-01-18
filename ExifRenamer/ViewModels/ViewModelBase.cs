using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExifRenamer.ViewModels;

public class ViewModelBase : ObservableObject
{
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                SetProperty(ref _isBusy, value);
            }
        }
    }
}