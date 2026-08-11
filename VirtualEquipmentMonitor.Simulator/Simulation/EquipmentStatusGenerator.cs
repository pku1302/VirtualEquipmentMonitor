using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Contracts.Messages;

namespace VirtualEquipmentMonitor.Simulator.Simulation
{
    public sealed class EquipmentStatusGenerator
    {
        private const string DeviceId = "EQ-001";

        private double _temperature = 25.0;
        private int _rpm;
        private double _vibration = 0.1;
        public EquipmentStatusMessage Generate()
        {
            UpdateSimulationValues();

            return new EquipmentStatusMessage(
                DeviceId,
                DateTimeOffset.UtcNow,
                DetermineState(),
                Math.Round(_temperature, 2),
                _rpm,
                Math.Round(_vibration, 2));
        }

        private void UpdateSimulationValues()
        {
            _rpm = Random.Shared.Next(1200, 1801);

            _temperature += Random.Shared.NextDouble() * 2.0 - 0.7;
            _temperature = Math.Clamp(_temperature, 20.0, 100.0);

            _vibration = 0.3 + Random.Shared.NextDouble() * 5.0;
        }
        private EquipmentOperatingState DetermineState()
        {
            if (_temperature >= 90 || _vibration >= 4.5)
            {
                return EquipmentOperatingState.Fault;
            }

            if (_temperature >= 75 || _vibration >= 3.5)
            {
                return EquipmentOperatingState.Warning;
            }

            return EquipmentOperatingState.Running;
        }
    }
}
