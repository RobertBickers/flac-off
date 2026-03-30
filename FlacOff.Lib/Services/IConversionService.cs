using FlacOff.Lib.Models;

namespace FlacOff.Lib.Services;

public interface IConversionService
{
    Task<ConversionResult> ConvertAsync(
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks);
}
