using System.Data.Entity.Core.Objects;
using INCZONE.Interfaces;
using INCZONE.Model;

namespace INCZONE.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(string connectionString)
        {
            _context = new ObjectContext(connectionString);
            _context.ContextOptions.LazyLoadingEnabled = true;

            string justDatabaseConnString = connectionString.Substring(connectionString.IndexOf("connection string") + 19).Replace("\"", "");
            _INCZONEMainContext = new IncZoneEntities(justDatabaseConnString);
        }

        public IRepository<DGPSConfiguration> DGPSConfigurations
        {
            get { return _DGPSConfigurations ?? (_DGPSConfigurations = new CommonRepository<DGPSConfiguration>(_context)); }
        }

        public IRepository<CapWINConfiguration> CapWINConfigurations
        {
            get { return _CapWINConfigurations ?? (_CapWINConfigurations = new CommonRepository<CapWINConfiguration>(_context)); }
        }

        public IRepository<EventType> EventTypes
        {
            get { return _EventTypes ?? (_EventTypes = new CommonRepository<EventType>(_context)); }
        }

        public IRepository<EventLog> EventLogs
        {
            get { return _EventLogs ?? (_EventLogs = new CommonRepository<EventLog>(_context)); }
        }

        public IRepository<LogLevel> LogLevels
        {
            get { return _LogLevels ?? (_LogLevels = new CommonRepository<LogLevel>(_context)); }
        }

        public IRepository<DSRCConfiguration> DSRCConfigurations
        {
            get { return _DSRCConfigurations ?? (_DSRCConfigurations = new CommonRepository<DSRCConfiguration>(_context)); }
        }

        public IRepository<MapLink> MapLinks
        {
            get { return _MapLinks ?? (_MapLinks = new CommonRepository<MapLink>(_context)); }
        }

        public IRepository<MapNode> MapNodes
        {
            get { return _MapNodes ?? (_MapNodes = new CommonRepository<MapNode>(_context)); }
        }

        public IRepository<MapSet> MapSets
        {
            get { return _MapSets ?? (_MapSets = new CommonRepository<MapSet>(_context)); }
        }

        public IRepository<AudibleVisualAlarm> AudibleVisualAlarms
        {
            get { return _AudibleVisualAlarms ?? (_AudibleVisualAlarms = new CommonRepository<AudibleVisualAlarm>(_context)); }
        }

        public IRepository<AlarmConfiguration> AlarmConfigurations
        {
            get { return _AlarmConfigurations ?? (_AlarmConfigurations = new CommonRepository<AlarmConfiguration>(_context)); }
        }

        public IRepository<AlarmLevel> AlarmLevels
        {
            get { return _AlarmLevels ?? (_AlarmLevels = new CommonRepository<AlarmLevel>(_context)); }
        }

        public IRepository<VehicleAlarm> VehicleAlarms
        {
            get { return _VehicleAlarms ?? (_VehicleAlarms = new CommonRepository<VehicleAlarm>(_context)); }
        }

        public IRepository<BluetoothConfig> BluetoothConfigs
        {
            get { return _BluetoothConfigs ?? (_BluetoothConfigs = new CommonRepository<BluetoothConfig>(_context)); }
        }

        public void Commit()
        {
            _context.SaveChanges();
            _INCZONEMainContext.SaveChanges();
        }

        CommonRepository<DGPSConfiguration> _DGPSConfigurations;
        CommonRepository<CapWINConfiguration> _CapWINConfigurations;
        CommonRepository<DSRCConfiguration> _DSRCConfigurations;
        CommonRepository<EventType> _EventTypes;
        CommonRepository<EventLog> _EventLogs;
        CommonRepository<LogLevel> _LogLevels;
        CommonRepository<MapLink> _MapLinks;
        CommonRepository<MapNode> _MapNodes;
        CommonRepository<MapSet> _MapSets;
        CommonRepository<AudibleVisualAlarm> _AudibleVisualAlarms;
        CommonRepository<AlarmConfiguration> _AlarmConfigurations;
        CommonRepository<AlarmLevel> _AlarmLevels;
        CommonRepository<VehicleAlarm> _VehicleAlarms;
        CommonRepository<BluetoothConfig> _BluetoothConfigs;

        private readonly ObjectContext _context;
        private readonly IncZoneEntities _INCZONEMainContext;        
    }
}

