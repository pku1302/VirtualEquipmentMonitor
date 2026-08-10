using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualEquipmentMonitor.Domain.Equipment
{
    public enum EquipmentState
    {
        Stopped,
        Running,
        Warning,
        Fault
    }
}
