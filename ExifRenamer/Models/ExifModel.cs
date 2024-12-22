namespace ExifRenamer.Models;

public record ExifModel(
    string Make,
    string Model,
    string ExposureTime,
    string FNumber,
    string ISO,
    string Artist,
    string Copyright,
    string ResolutionUnit,
    string FocalLength,
    string LensModel,
    string LensSerialNumber,
    string LensMake,
    string LensSpecification,
    string Software,
    string DateTime,
    string Latitude,
    string Longitude);