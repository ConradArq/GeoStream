using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Core.Commands
{
    public abstract class EmitterCommand : Command
    {
        public string Code { get; set; } = string.Empty;
    }
}
