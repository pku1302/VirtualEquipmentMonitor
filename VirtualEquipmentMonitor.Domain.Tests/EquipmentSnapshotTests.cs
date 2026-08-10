using System;
using System.Collections.Generic;
using System.Text;
using VirtualEquipmentMonitor.Domain.Equipment;

namespace VirtualEquipmentMonitor.Domain.Tests
{
    public sealed class EquipmentSnapshotTests
    {
        [Fact]
        public void Constructor_WithValidValues_CreatesSnapshot()
        {
            var timestamp = DateTimeOffset.UtcNow;

            var snapshot = new EquipmentSnapshot(
                "EQ-001",
                timestamp,
                EquipmentState.Running,
                42.5,
                1500,
                1.25);

            Assert.Equal("EQ-001", snapshot.DeviceId);
            Assert.Equal(timestamp, snapshot.Timestamp);
            Assert.Equal(EquipmentState.Running, snapshot.State);
            Assert.Equal(42.5, snapshot.Temperature);
            Assert.Equal(1500, snapshot.Rpm);
            Assert.Equal(1.25, snapshot.Vibration);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void EquipmentId_Should_Reject_Invalid_Value(string deviceId)
        {
            var action = () => new EquipmentSnapshot(
                deviceId,
                DateTimeOffset.UtcNow,
                EquipmentState.Stopped,
                20,
                0,
                0);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Constructor_WithNegativeRpm_ThrowsException()
        {
            var action = () => new EquipmentSnapshot(
                "EQ-001",
                DateTimeOffset.UtcNow,
                EquipmentState.Running,
                20,
                -1,
                0);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        [Fact]
        public void Constructor_WithNegativeVibration_ThrowsException()
        {
            var action = () => new EquipmentSnapshot(
                "EQ-001",
                DateTimeOffset.UtcNow,
                EquipmentState.Running,
                20,
                1000,
                -0.1);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

    }
}
