
using System.Reflection.Emit;
using TelemetryService.Core.Commands;

namespace TelemetryService.Application.Commands
{
    public class EmitterReadCommand : EmitterCommand
    {
        public string ScannerCode { get; set; }

        public EmitterReadCommand(string code, string scannerCode) 
        {
            Code = code;
            ScannerCode = scannerCode;
        }   
    }
}
