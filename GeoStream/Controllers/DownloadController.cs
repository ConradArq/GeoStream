using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using GeoStream.Dtos.Enums;
using System.Drawing;
using GeoStream.Extensions;
using GeoStream.Services;
using GeoStream.Dtos.Application;
using GeoStream.Dtos;

namespace GeoStream.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DownloadController : ControllerBase
    {
        private IApiClient _apiClient { get; set; }

        public DownloadController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> DownloadReports(Reports reportId, string? assetCode)
        {
            var excelStream = await GetExcelFileStream(reportId, assetCode);
            if (excelStream != null)
            {
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = string.Concat(reportId.GetDisplayName().Replace(", ", "-"), ".xlsx");
                return File(excelStream, contentType, fileName);
            }
            else
            {
                return BadRequest("The type of report or the asset Code does not exist");
            }
        }

        private async Task<Stream?> GetExcelFileStream(Reports reportId, string? assetCode)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            switch (reportId)
            {
                case Reports.ScannerActivityReport:
                    var scannerActivityReportExcel = new ExcelPackage();
                    var scannerActivityReportDataSheet = scannerActivityReportExcel.Workbook.Worksheets.Add("Scanner Activity Data");

                    FillScannerActivityDataSheet(scannerActivityReportDataSheet);

                    var scannerActivityStream = new MemoryStream();
                    scannerActivityReportExcel.SaveAs(scannerActivityStream);
                    scannerActivityStream.Position = 0;
                    return scannerActivityStream;

                case Reports.AssetActivityReport:
                    if (string.IsNullOrEmpty(assetCode))
                    {
                        return null;
                    }

                    var assetActivityReportExcel = new ExcelPackage();
                    var assetActivityReportDataSheet = assetActivityReportExcel.Workbook.Worksheets.Add("Asset Activity Data");

                    var responseDto = await _apiClient.SendRequest<ApiResponseDto<IEnumerable<AssetDto>>>("ScannersApi.Asset.Search", new { Code = assetCode });

                    if (!responseDto.Succeeded)
                    {
                        return null;
                    }
                    else
                    {
                        var asset = responseDto.Data?.FirstOrDefault();

                        if (asset == null)
                        {
                            return null;
                        }

                        FillAssetActivityDataSheet(assetActivityReportDataSheet, asset);

                        var operationCostsStream = new MemoryStream();
                        assetActivityReportExcel.SaveAs(operationCostsStream);
                        operationCostsStream.Position = 0;
                        return operationCostsStream;
                    }

                default:
                    return null;
            }
        }

        private void FillAssetActivityDataSheet(ExcelWorksheet sheet, AssetDto asset)
        {
            // Header
            var range = sheet.Cells["A2:F2"];
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.Font.Color.SetColor(Color.Blue);
            range.Merge = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A2"].Value = "Asset Activity Overview";

            // Rows
            sheet.Cells["A3"].Value = "Asset Code";
            sheet.Cells["B3"].Value = asset.Code;

            sheet.Cells["A4"].Value = "Owner Document Number";
            sheet.Cells["B4"].Value = asset.OwnerDocumentNumber;
        }
 

        private void FillScannerActivityDataSheet(ExcelWorksheet sheet)
        {
            // Header
            var range = sheet.Cells["A2:F2"];
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.Font.Color.SetColor(Color.Blue);
            range.Merge = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A2"].Value = "Scanner Activity Overview";

            // Rows
            sheet.Cells["A3"].Value = "Scanner ID";
            sheet.Cells["B3"].Value = "Scanner 1";  // Example value

            sheet.Cells["A4"].Value = "Scan Frequency";
            sheet.Cells["B4"].Value = "5 scans/min";  // Example value

            sheet.Cells["A5"].Value = "Total Assets Scanned";
            sheet.Cells["B5"].Value = "1500";  // Example value

            sheet.Cells["A6"].Value = "Operational Time";
            sheet.Cells["B6"].Value = "8 hours";  // Example value
        }

    }
}
