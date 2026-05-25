using ExifRenamer.Models;

namespace ExifRenamer.ViewModels;

public class VideoCompressionJobViewModel : ViewModelBase
{
    private VideoCompressionJobStatus _status;
    private string _statusText = "⏳ En attente";
    private long _inputSize;
    private long _outputSize;
    private string? _errorMessage;

    public required string InputFilename { get; set; }
    public required string OutputFilename { get; set; }
    public required string InputPath { get; set; }
    public required string OutputPath { get; set; }

    public VideoCompressionJobStatus Status
    {
        get => _status;
        set
        {
            if (!SetProperty(ref _status, value)) return;
            StatusText = value switch
            {
                VideoCompressionJobStatus.Queued      => "⏳ En attente",
                VideoCompressionJobStatus.Processing  => "⟳ En cours...",
                VideoCompressionJobStatus.Done        => $"✓ {ReductionText}",
                VideoCompressionJobStatus.Failed      => $"✗ Erreur",
                VideoCompressionJobStatus.Skipped     => "— Ignoré",
                VideoCompressionJobStatus.Cancelled   => "✕ Annulé",
                _                                     => string.Empty
            };
            OnPropertyChanged(nameof(IsProcessing));
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public long InputSize
    {
        get => _inputSize;
        set => SetProperty(ref _inputSize, value);
    }

    public long OutputSize
    {
        get => _outputSize;
        set
        {
            SetProperty(ref _outputSize, value);
            OnPropertyChanged(nameof(ReductionText));
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsProcessing => Status == VideoCompressionJobStatus.Processing;

    public string ReductionText
    {
        get
        {
            if (InputSize <= 0 || OutputSize <= 0) return string.Empty;
            var pct = (1.0 - (double)OutputSize / InputSize) * 100;
            return $"-{pct:F1}% ({FormatBytes(InputSize)} → {FormatBytes(OutputSize)})";
        }
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        >= 1_000_000_000 => $"{bytes / 1_000_000_000.0:F1} GB",
        >= 1_000_000     => $"{bytes / 1_000_000.0:F1} MB",
        >= 1_000         => $"{bytes / 1_000.0:F1} KB",
        _                => $"{bytes} B"
    };
}
