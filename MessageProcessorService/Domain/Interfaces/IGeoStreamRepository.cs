using MessageProcessorService.Domain.Models.Entities;

namespace MessageProcessorService.Domain.Interfaces
{
    public interface IGeoStreamRepository
    {
        Scanner? GetScannerBy(string scannerCode);
        Hub? GetHubBy(string scannerCode);
        Tuple<long, string>? GetAssetIdAndCodeBy(string label);
    }
}