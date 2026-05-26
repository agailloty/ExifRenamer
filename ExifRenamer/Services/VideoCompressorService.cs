using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExifRenamer.Models;

namespace ExifRenamer.Services;

public class VideoCompressionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long InputSize { get; set; }
    public long OutputSize { get; set; }
}

public class VideoCompressorService
{
    private static readonly string[] VideoExtensions =
        { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".m4v", ".webm" };

    public string[] GetVideoFiles(IEnumerable<string> folderPaths)
    {
        var files = new List<string>();
        foreach (var folder in folderPaths)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
            {
                if (VideoExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    files.Add(file);
            }
        }
        return files.ToArray();
    }

    public async Task<VideoCompressionResult> CompressAsync(
        string inputPath,
        string outputPath,
        VideoCompressionPreset preset,
        string ffmpegPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var inputFile = new FileInfo(inputPath);
        if (!inputFile.Exists)
            return new VideoCompressionResult { Success = false, ErrorMessage = "Source file not found." };

        var inputSize = inputFile.Length;
        var args = BuildArguments(inputPath, outputPath, preset);

        var processInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processInfo)
                ?? throw new InvalidOperationException("Failed to start ffmpeg.");

            // Read stderr for progress lines without blocking
            _ = Task.Run(async () =>
            {
                string? line;
                while ((line = await process.StandardError.ReadLineAsync(cancellationToken)) != null)
                    progress?.Report(line);
            }, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (File.Exists(outputPath))
            {
                var outputFile = new FileInfo(outputPath);
                // Preserve original file system dates
                outputFile.CreationTime = inputFile.CreationTime;
                outputFile.LastWriteTime = inputFile.LastWriteTime;

                return new VideoCompressionResult
                {
                    Success = true,
                    InputSize = inputSize,
                    OutputSize = outputFile.Length
                };
            }

            return new VideoCompressionResult
            {
                Success = false,
                ErrorMessage = "Output file was not created."
            };
        }
        catch (OperationCanceledException)
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            throw;
        }
        catch (Exception ex)
        {
            return new VideoCompressionResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static string BuildArguments(string inputPath, string outputPath, VideoCompressionPreset preset)
    {
        var parts = new List<string>
        {
            "-y",
            "-i", $"\"{inputPath}\"",
            "-vcodec", "libx264",
            "-crf", preset.Crf.ToString(),
            "-preset", preset.FfmpegPreset,
            "-acodec", "copy",
            "-threads", "4",
            "-loglevel", "error",
            // Preserve all metadata: global tags, chapter markers, and per-stream tags
            "-map_metadata", "0",
            "-map_chapters", "0",
            "-map_metadata:s:v", "0:s:v",
            "-map_metadata:s:a", "0:s:a"
        };

        if (!string.IsNullOrEmpty(preset.ScaleFilter))
        {
            parts.Add("-vf");
            parts.Add($"scale={preset.ScaleFilter}");
        }

        parts.Add($"\"{outputPath}\"");
        return string.Join(" ", parts);
    }
}
