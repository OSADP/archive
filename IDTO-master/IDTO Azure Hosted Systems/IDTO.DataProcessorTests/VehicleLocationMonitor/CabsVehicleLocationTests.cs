using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.DataProcessor.VehicleLocationMonitor;
using Xunit;
using Microsoft.QualityTools.Testing.Fakes;
using IDTO.BusScheduleInterface;

namespace IDTO.DataProcessor.VehicleLocationMonitor.Tests
{
    public class CabsVehicleLocationTests
    {
        [Fact()]
        public void CalculateEstimatedTimeOfArrivalAsyncTest()
        {
            string DATE_FORMAT = "yyyyMMdd H:mm";
            string DATE = "20140320 18:30";
            string VEHICLE_ID = "1234";

            IDTO.Entity.Models.Fakes.StubStep step = new IDTO.Entity.Models.Fakes.StubStep();
            step.ToStopCode = "55";
            step.RouteNumber = "CLN";

            var tc = new IDTO.Entity.Models.Fakes.StubTConnect();
            tc.InboundStepGet = () => step;
            tc.InboundVehicle = VEHICLE_ID.ToString();

            using(ShimsContext.Create())
            {
                //var prediction = new IDTO.BusTime.Fakes.ShimPrediction();
                var prediction = new IDTO.BusScheduleInterface.Fakes.StubIPrediction();

                prediction.VehicleIDGet = () => VEHICLE_ID;
                prediction.PredictedTimeOfArrivalOrDepartureGet = () => DATE;

                IBusSchedule bs = new BusScheduleInterface.Fakes.StubIBusSchedule()
                {
                    GetPredictionsAsyncStringString = (x, y) =>
                    {
                        var t = Task.Factory.StartNew<List<IPrediction>>(() =>
                        {
                            var pl = new List<IPrediction>();
                            pl.Add(prediction);
                            return pl;
                        });

                        return t;
                    }
                };

                IVehicleLocation vehLoc = new CabsVehicleLocation(bs);
                var result = vehLoc.CalculateEstimatedTimeOfArrivalAsync(tc, null);
                var dt = result.Result;

                Assert.Equal(DateTime.ParseExact(DATE, DATE_FORMAT, System.Globalization.CultureInfo.CurrentCulture), dt);
            }  
        }
    }
}
