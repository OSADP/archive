using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.RouteAggregationLibrary.OpenTripPlanner;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;
using Moq;
using NUnit.Framework;
using RestSharp;

namespace IDTO.UnitTests.IDTO.RouteAggregationLibrary
{
     [TestFixture]
    public class OpenTripPlannerAdapterTests
    {
         [Test]
         public void GetServersVersion_Passes()
         {
             //Arrange
             RestResponse<ServerInfo> response = new RestResponse<ServerInfo>();
             response.Data = new ServerInfo();
             response.Data.serverVersion.version = "0.9.1-SNAPSHOT";

             var mock = new Mock<IRestClient>();
             mock.Setup(foo => foo.Execute<ServerInfo>(It.IsAny<IRestRequest>())).Returns(response);

             //Act
             OpenTripPlannerAdapter cut = new OpenTripPlannerAdapter(mock.Object);

             var results = cut.GetServerInfo();

             Assert.AreEqual("0.9.1-SNAPSHOT", results.serverVersion.version);
         }

         [Test]
         public void GetStopsNearPoint_Passes()
         {
             //Arrange
             RestResponse<StopList> response = new RestResponse<StopList>();
             
             Stop sampleStop1 = new Stop
             {
                 stopCode = "BROROBE",
                 stopName = "E BROAD ST & ROBINWOOD AVE",
                 stopLat = 39.973759,
                 stopLon = -82.89616,
                 id = new Id() {agencyId = "COTA", id = "2501"}
             };

             Stop sampleStop2 = new Stop
             {
                 stopCode = "BROBEETE",
                 stopName = "E BROAD ST & BEECHTREE RD",
                 stopLat = 39.973919,
                 stopLon = -82.894701,
                 id = new Id() {agencyId = "COTA", id = "2502"}
             };

             response.Data = new StopList();
             response.Data.stops.Add(sampleStop1);
             response.Data.stops.Add(sampleStop2);

             var mock = new Mock<IRestClient>();
             mock.Setup(foo => foo.Execute<StopList>(It.IsAny<IRestRequest>())).Returns(response);

             //Act
             OpenTripPlannerAdapter cut = new OpenTripPlannerAdapter(mock.Object);

             var results = cut.FindStopsNearPoint(39.974639f, -82.894460f, 500);

             Assert.AreEqual(2, results.stops.Count);
         }

         [Test]
         public void GetStopTimes_Passes()
         {
             //Arrange
             RestResponse<StopTimesList> response = new RestResponse<StopTimesList>();
             response.Data = new StopTimesList();
             StopTime sampleDepartureStopTime = new StopTime
             {
                 phase = "departure",
                 time = 1386078245,
                 trip = new Trip()
                 {
                     tripShortName = "Trips Short Name",
                     tripHeadsign = "10E EAST BROAD TO MC NAUGHTEN AND MOUNT CARMEL HOSPITAL",
                     id = new Id() {agencyId = "COTA", id = "466817"}
                 }
             };
             response.Data.stopTimes.Add(sampleDepartureStopTime);

             StopTime sampleArrivalStopTime = new StopTime
             {
                 phase = "arrival",
                 time = 1386078245,
                 trip = new Trip()
                 {
                     tripShortName = "Trips Short Name",
                     tripHeadsign = "10E EAST BROAD TO MC NAUGHTEN AND MOUNT CARMEL HOSPITAL",
                     id = new Id() {agencyId = "COTA", id = "466817"}
                 }
             };
             response.Data.stopTimes.Add(sampleArrivalStopTime);
             
             var mock = new Mock<IRestClient>();
             mock.Setup(foo => foo.Execute<StopTimesList>(It.IsAny<IRestRequest>())).Returns(response);

             //Act
             OpenTripPlannerAdapter cut = new OpenTripPlannerAdapter(mock.Object);

             var results = cut.FindStopTimes("COTA", "2501", DateTime.UtcNow.AddHours(-5), DateTime.UtcNow);

             Assert.AreEqual(1, results.stopTimes.Count);
             Assert.AreEqual("departure", results.stopTimes.First().phase);

         }

     }
}
