using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Contracts.Messages
{
    public enum EquipmentOperatingState
    {
        Stopped,
        Running,
        Warning,
        Fault
    }

}
