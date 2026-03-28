using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes;

public class StreamEnabledAttribute : AbstractWorkerAttribute
{
    public StreamEnabledAttribute(bool streamEnabled)
    {
        StreamEnabled = streamEnabled;
    }
    
    public bool StreamEnabled { get; set; }
}

