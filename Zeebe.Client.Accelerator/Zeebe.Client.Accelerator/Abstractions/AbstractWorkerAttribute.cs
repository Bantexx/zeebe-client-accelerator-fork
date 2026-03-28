using System;

namespace Zeebe.Client.Accelerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]    
    public abstract class AbstractWorkerAttribute : Attribute 
    { }    
}