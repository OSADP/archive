using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Common;
using Repository;
using IDTO.BusScheduleInterface;
using System.Globalization;

namespace IDTO.DataProcessor.VehicleLocationMonitor
{
    public class CabsVehicleLocation : IVehicleLocation
    {
        private IBusSchedule busTimeApi;
        public CabsVehicleLocation(IBusSchedule busTimeApi)
        {
            if (busTimeApi == null)
                throw new ArgumentNullException("busTimeApi");

            this.busTimeApi = busTimeApi;
        }

        public Providers ProviderName
        {
            get
            {
                return busTimeApi.GetProviderId();
            }
        }
        public async Task<DateTime> CalculateEstimatedTimeOfArrivalAsync(TConnect tConnect, IUnitOfWork Uow)
        {
            string DATE_FORMAT = "MM/dd/yyyy h:mm:ss";
            DateTime dtParsed = DateTime.MinValue;
            try
            {
                var predictions = await busTimeApi.GetPredictionsAsync(tConnect.InboundStep.ToStopCode, tConnect.InboundStep.RouteNumber);
                var prediction = predictions.Find(p => tConnect.InboundVehicle.Equals(p.VehicleID.ToString(), StringComparison.CurrentCultureIgnoreCase));

                bool wasParsed = DateTime.TryParseExact(prediction.PredictedTimeOfArrivalOrDeparture, DATE_FORMAT, 
                    System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.None, out dtParsed);

                if (!wasParsed)
                {
                    DATE_FORMAT = "M/d/yyyy h:m:s tt";
                    wasParsed = DateTime.TryParseExact(prediction.PredictedTimeOfArrivalOrDeparture, DATE_FORMAT,
                    System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.None, out dtParsed);

                    if (!wasParsed)
                    {
                        DATE_FORMAT = "MM/dd/yyyy hh:mm:ss tt";
                        wasParsed = DateTime.TryParseExact(prediction.PredictedTimeOfArrivalOrDeparture, DATE_FORMAT,
                        System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.None, out dtParsed);
                    }
                }
                
                DateTime dtUTC = TimeZoneInfo.ConvertTime(dtParsed, TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time"), TimeZoneInfo.Utc);

                return dtUTC;

            }
            catch(Exception ex)
            {
                return DateTime.MinValue;
                // Eat this.  Logging would be cool though.                  
            }
        }
        public string GetInboundVehicleName(TConnect tConnect, IUnitOfWork Uow)
        {
            var getIdTask = Task.Factory.StartNew(async () =>
            {
                if (string.IsNullOrEmpty(tConnect.InboundStep.FromStopCode) || string.IsNullOrEmpty(tConnect.InboundStep.RouteNumber))
                    return string.Empty;
                else
                    return await this.busTimeApi.GetNextVehicleIDAsync(tConnect.InboundStep);//tConnect.InboundStep.FromStopCode, tConnect.InboundStep.RouteNumber);

            }).Unwrap();

            return getIdTask.Result;
        }
    }
}
