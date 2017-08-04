using INCZONE.Interfaces;
using INCZONE.Model;

namespace INCZONE.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<DGPSConfiguration> DGPSConfigurations { get; }
        IRepository<CapWINConfiguration> CapWINConfigurations { get; }
        IRepository<DSRCConfiguration> DSRCConfigurations { get; }
        IRepository<EventType> EventTypes { get;  }
        IRepository<EventLog> EventLogs { get; }
        IRepository<LogLevel> LogLevels { get; }
        IRepository<MapLink> MapLinks { get; }
        IRepository<MapNode> MapNodes { get; }
        IRepository<MapSet> MapSets { get; }
        IRepository<AudibleVisualAlarm> AudibleVisualAlarms { get; }
        IRepository<AlarmConfiguration> AlarmConfigurations { get; }
        IRepository<AlarmLevel> AlarmLevels { get; }
        IRepository<VehicleAlarm> VehicleAlarms { get; }
        IRepository<BluetoothConfig> BluetoothConfigs { get; }

        void Commit();
    }
}
