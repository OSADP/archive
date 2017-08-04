using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Repositories;
using INCZONE.VITAL;
using log4net;
using Phidgets;
using Phidgets.Events;
using System.Media;

namespace INCZONE.Managers
{
    public class AlarmManager
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        public AlarmManager()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);
        }

        internal void GenerateVitalAlarm(int Persistance, VITALModule vitalModule)
        {
            bool notDone = true;

            try
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                //vitalModule.ActivateAlarms();
                while (notDone)
                {
                    if (sw.ElapsedMilliseconds >= Persistance)
                    {
                        //vitalModule.DeactivateAlarms();
                        notDone = false;
                    }
                }
                sw.Stop();
            }
            catch (Exception ex)
            {
                LogEventsManager.LogEvent("Error Generating Vital Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                log.Error("Error Generating Vital Alarm", ex);
            }
        }

        internal void GenerateVitalAlarm(AlarmLevelTypes alarmLevelTypes, AlarmConfig _AlarmConfig, VITALModule vitalModule)
        {
            //IncZoneMDIParent.vitalModual.ActivateAlarms();
            int Persistance = 0;

            try
            {
                switch (alarmLevelTypes)
                {
                    case AlarmLevelTypes.Level_0:
                        Persistance = _AlarmConfig.AudibleVisualConfigs[0].Persistance;
                        break;
                    case AlarmLevelTypes.Level_1:
                        Persistance = _AlarmConfig.AudibleVisualConfigs[1].Persistance;
                        break;
                    case AlarmLevelTypes.Level_2:
                        Persistance = _AlarmConfig.AudibleVisualConfigs[2].Persistance;
                        break;
                    case AlarmLevelTypes.Level_3:
                        Persistance = _AlarmConfig.AudibleVisualConfigs[3].Persistance;
                        break;
                    case AlarmLevelTypes.Level_4:
                        Persistance = _AlarmConfig.AudibleVisualConfigs[4].Persistance;
                        break;
                }

                //GenerateVitalAlarm(Persistance, vitalModule);
            }
            catch (Exception ex)
            {
                LogEventsManager.LogEvent("Error Generating Vital Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                log.Error("Error Generating Vital Alarm", ex);

            }
        }

        internal Task GenerateAudioAlarm(int Duration, int Persistance, int Frequency)
        {
            log.Debug("Generating alarm Frequency:" + Frequency + " Duration:" + Duration + " Persistance:" + Persistance);
            Console.Out.WriteLine("Generating alarm Frequency:" + Frequency + " Duration:" + Duration + " Persistance:" + Persistance);
            Task task = Task.Run(() =>
            {
                try
                {
                    bool notDone = true;

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    Console.Out.WriteLine("Starting Alarm");
                    
                    while (notDone)
                    {
                        Console.Beep(Frequency, Duration);
                        Console.Out.WriteLine("Frequency:" + Frequency + " Duration:" + Duration);
                        if (sw.ElapsedMilliseconds >= Persistance)
                        {
                            notDone = false;
                        }
                        Thread.Sleep(150);
                    }
                    
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    LogEventsManager.LogEvent("Error Generating Audio Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                    log.Error("Error Generating Audio Alarm", ex);
                }
            }
            );

            return task;
        }

        internal void GenerateAudioAlarm(AlarmLevelTypes alarmLevelTypes, AlarmConfig _AlarmConfig)
        {
            int Duration = 0;
            int Frequency = 0;
            int Persistance = 0;

            try
            {
                switch (alarmLevelTypes)
                {
                    case AlarmLevelTypes.Level_0:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[0].Duration);
                        Frequency = int.Parse(_AlarmConfig.AudibleVisualConfigs[0].Frequency);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[0].Persistance;
                        break;
                    case AlarmLevelTypes.Level_1:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[1].Duration);
                        Frequency = int.Parse(_AlarmConfig.AudibleVisualConfigs[1].Frequency);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[1].Persistance;
                        break;
                    case AlarmLevelTypes.Level_2:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[2].Duration);
                        Frequency = int.Parse(_AlarmConfig.AudibleVisualConfigs[2].Frequency);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[2].Persistance;
                        break;
                    case AlarmLevelTypes.Level_3:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[3].Duration);
                        Frequency = int.Parse(_AlarmConfig.AudibleVisualConfigs[3].Frequency);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[3].Persistance;
                        break;
                    case AlarmLevelTypes.Level_4:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[4].Duration);
                        Frequency = int.Parse(_AlarmConfig.AudibleVisualConfigs[4].Frequency);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[4].Persistance;
                        break;
                }

                GenerateAudioAlarm(Duration, Persistance, Frequency);
            }
            catch (Exception ex)
            {
                LogEventsManager.LogEvent("Error Generating Audio Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                log.Error("Error Generating Audio Alarm", ex);
            }
        }

        internal Task GenerateRadioAlarm(int Duration, int Persistance, InterfaceKit ifKit, bool fiKitBypass)
        {
            Task task = Task.Run(() =>
            {
                if (!fiKitBypass)
                {
                    try
                    {
                        bool notDone = true;
                        Stopwatch sw = new Stopwatch();
                        ifKit.outputs[0] = true;
                        ifKit.outputs[1] = true;
                        sw.Start();
                        while (notDone)
                        {
                            if (sw.ElapsedMilliseconds >= Persistance)
                            {
                                notDone = false;
                            }
                        }

                        ifKit.outputs[1] = false;
                        ifKit.outputs[0] = false;
                        sw.Stop();
                    }
                    catch (Exception ex)
                    {
                        LogEventsManager.LogEvent("Error Generating Radio Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                        log.Error("Error Generating Radio Alarm", ex);
                    }
                }
            }
        );
        return task;
        }

        internal void GenerateRadioAlarm(AlarmLevelTypes alarmLevelTypes, AlarmConfig _AlarmConfig, InterfaceKit ifKit, bool ifKitBypass)
        {

            try
            {
                int Duration = 0;
                int Persistance = 0;

                switch (alarmLevelTypes)
                {
                    case AlarmLevelTypes.Level_0:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[0].Duration);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[0].Persistance;
                        break;
                    case AlarmLevelTypes.Level_1:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[1].Duration);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[1].Persistance;
                        break;
                    case AlarmLevelTypes.Level_2:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[2].Duration);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[2].Persistance;
                        break;
                    case AlarmLevelTypes.Level_3:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[3].Duration);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[3].Persistance;
                        break;
                    case AlarmLevelTypes.Level_4:
                        Duration = int.Parse(_AlarmConfig.AudibleVisualConfigs[4].Duration);
                        Persistance = _AlarmConfig.AudibleVisualConfigs[4].Persistance;
                        break;
                }

                GenerateRadioAlarm(Duration, Persistance, ifKit, ifKitBypass);
            }
            catch (Exception ex)
            {
                LogEventsManager.LogEvent("Error Generating Radio Audio Alarm - " + ex.Message, LogEventTypes.ALARM_CONFIG, LogLevelTypes.ERROR);
                log.Error("GenerateAudioAlarm Exception", ex);
            }
        }

        public static Alarm _GetHighestAlarm(List<Alarm> alarmList)
        {
            Alarm returnAlarm = null;

            try
            {
                foreach (Alarm alarm in alarmList)
                {
                    if (alarm.AlarmLevel == AlarmLevelTypes.Level_4)
                    {
                        returnAlarm = alarm;
                        break;
                    }

                    if (returnAlarm == null)
                    {
                        returnAlarm = alarm;
                    }
                    else
                    {
                        if (returnAlarm.AlarmLevel < alarm.AlarmLevel)
                        {
                            returnAlarm = alarm;
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("_GetHighestAlarm", ex);
            }

            return returnAlarm;
        }

    }
}
