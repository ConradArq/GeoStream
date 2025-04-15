namespace TelemetryService.Core.Commands
{
    public abstract class EmitterCommand : Command
    {
        public string Code { get; protected set; } = string.Empty;
    }
}
