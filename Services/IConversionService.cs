using System.Collections.Generic;
using System.Threading.Tasks;
using FlacOff.Models;

namespace FlacOff.Services;

public interface IConversionService
{
    Task<ConversionResult> ConvertAsync(
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks);
}
