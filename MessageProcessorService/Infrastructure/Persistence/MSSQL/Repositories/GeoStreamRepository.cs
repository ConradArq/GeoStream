using MessageProcessorService.Domain.Interfaces;
using MessageProcessorService.Domain.Models.Entities;

namespace MessageProcessorService.Infrastructure.Persistence.MSSQL.Repositories
{
    public class GeoStreamRepository : IGeoStreamRepository
    {
        private GeoStreamDbContext _context;

        public GeoStreamRepository(GeoStreamDbContext context)
        {
            _context = context;
        }

        public Scanner? GetScannerBy(string scannerCode)
        {
            var scanner = (from sc in _context.Scanners
                           where sc.Code == scannerCode
                           select sc).FirstOrDefault();
            return scanner;
        }

        public Hub? GetHubBy(string scannerCode)
        {
            var hub = (from scanner in _context.Scanners
                           join est in _context.Hubs on scanner.HubId equals est.Id
                           where scanner.Code == scannerCode
                           select est).FirstOrDefault();

            return hub;
        }

        public Tuple<long, string>? GetAssetIdAndCodeBy(string label)
        {
            var assetIdAndCode = (from veh in _context.Assets
                                                 join vehEmitter in _context.AssetEmitters on veh.Id equals vehEmitter.AssetId
                                                 where vehEmitter.Emitter == label && vehEmitter.IsActive == true
                                                 select new Tuple<long, string>(veh.Id, veh.LicenseCode)).FirstOrDefault();

            return assetIdAndCode;
        }
    }
}
