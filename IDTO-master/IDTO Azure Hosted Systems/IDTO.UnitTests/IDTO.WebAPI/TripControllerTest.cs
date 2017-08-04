using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using Moq;
using NUnit.Framework;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using IDTO.Common;
namespace IDTO.UnitTests.IDTO.WebAPI
{
    [TestFixture]
    public class TripControllerTest
    {
         
        private static void SetupControllerForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Trip");
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Trip" } });

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.Request = request;
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
        }
        [Test]
        public void PostTripAdd_Passes()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                unitOfWork.Repository<Traveler>().Insert(new Traveler { Id = 1, FirstName = "TestFN", LastName = "TestLN", ObjectState = ObjectState.Added });
                unitOfWork.Save();
                var controller = new TripController(idtoFakeContext);
                SetupControllerForTests(controller);

                TripModel m = new TripModel
                {
                    TravelerId = 1,
                    Origination = "Neverland",
                    Destination = "Montana",
                    TripStartDate = DateTime.Parse("1/2/2014"),
                    TripEndDate = DateTime.Parse("1/3/2014"),
                    MobilityFlag = true,
                    BicycleFlag = true,
                    PriorityCode = "1"
                };
                List<StepModel> steps = new List<StepModel>();
                StepModel stepmodel = new StepModel();
                stepmodel.StartDate = DateTime.Parse("1/1/2014 10:02");
                stepmodel.EndDate = DateTime.Parse("1/1/2014 10:40");
                stepmodel.FromName = "Quartz Street";
                stepmodel.FromProviderId = (int)Providers.COTA;
                stepmodel.FromStopCode = "1001";
                stepmodel.ModeId = (int)Modes.Bus;
                stepmodel.RouteNumber = "039";
                stepmodel.Distance = (decimal)12.2;
                stepmodel.ToName = "Slate Run Road";
                stepmodel.ToProviderId = (int)Providers.COTA;
                stepmodel.ToStopCode = "2002";
                steps.Add(stepmodel);
                m.Steps = steps;
                try
                {
                    var actionResult = controller.PostTrip(m);
                    var response = actionResult as CreatedAtRouteNegotiatedContentResult<TripModel>;
                    Assert.IsNotNull(response);
                    var trips = response.Content;
                    Assert.AreEqual("Neverland",trips.Origination);
                }
                catch (HttpResponseException ex)
                {
                    Assert.Fail();
                }
            }
        }
        [Test]
        public void PostTripAddBadDate_Fails()
        {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                unitOfWork.Repository<Traveler>().Insert(new Traveler { Id = 1, FirstName = "TestFN", LastName = "TestLN", ObjectState = ObjectState.Added });
                unitOfWork.Save();
                var controller = new TripController(idtoFakeContext);
                SetupControllerForTests(controller);

                TripModel m = new TripModel
                {
                    TravelerId = 1,
                    Origination = "Neverland",
                    Destination = "Montana",
                    TripStartDate = DateTime.Parse("10/2/2013"),
                    TripEndDate = DateTime.Parse("10/2/2012"),
                    MobilityFlag = true,
                    BicycleFlag = true,
                    PriorityCode = "1"
                };
                List<StepModel> steps = new List<StepModel>();
                StepModel stepmodel = new StepModel();
                stepmodel.StartDate = DateTime.Parse("1/1/2014 10:02");
                stepmodel.EndDate = DateTime.Parse("1/1/2014 10:40");
                stepmodel.FromName = "Quartz Street";
                stepmodel.FromProviderId = (int)Providers.COTA;
                stepmodel.FromStopCode = "1001";
                stepmodel.ModeId = (int)Modes.Bus;
                stepmodel.RouteNumber = "039";
                stepmodel.Distance = (decimal)12.2;
                stepmodel.ToName = "Slate Run Road";
                stepmodel.ToProviderId = (int)Providers.COTA;
                stepmodel.ToStopCode = "2002";
                steps.Add(stepmodel);
                m.Steps = steps;
                try
                {
                    var actionResult = controller.PostTrip(m);
                    var response = actionResult as CreatedAtRouteNegotiatedContentResult<TripModel>;
                    Assert.AreEqual(true, false, "Test should have thrown an exception and not executed this line.");
                  
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, ex.Response.StatusCode);
                }
            }
        }
         [Test]
         public void PostTripAddInvalidTraveler_Fails()
         {
            using (IDbContext idtoFakeContext = new IDTOFakeContext())
            using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
            {
                unitOfWork.Repository<Traveler>().Insert(new Traveler { Id = 1, FirstName = "TestFN", LastName = "TestLN", ObjectState = ObjectState.Added });
                unitOfWork.Save();
                var controller = new TripController(idtoFakeContext);
                  SetupControllerForTests(controller);

                TripModel m = new TripModel
                {
                    TravelerId = 50,
                    Origination = "Neverland",
                    Destination = "Montana",
                    TripStartDate = DateTime.Parse("10/2/2012"),
                    TripEndDate = DateTime.Parse("10/2/2013"),
                    MobilityFlag = true,
                    BicycleFlag = true,
                    PriorityCode = "1"
                };
                List<StepModel> steps = new List<StepModel>();
                StepModel stepmodel = new StepModel();
                stepmodel.StartDate = DateTime.Parse("1/1/2014 10:02");
                stepmodel.EndDate = DateTime.Parse("1/1/2014 10:40");
                stepmodel.FromName = "Quartz Street";
                stepmodel.FromProviderId = (int)Providers.COTA;
                stepmodel.FromStopCode = "1001";
                stepmodel.ModeId = (int)Modes.Bus;
                stepmodel.RouteNumber = "039";
                stepmodel.Distance = (decimal)12.2;
                stepmodel.ToName = "Slate Run Road";
                stepmodel.ToProviderId = (int)Providers.COTA;
                stepmodel.ToStopCode = "2002";
                steps.Add(stepmodel);
                m.Steps = steps;
                try
                {
                    var actionResult = controller.PostTrip(m);
                    Assert.AreEqual(true, false, "Test should have thrown an exception and not executed this line.");
                }
                catch(HttpResponseException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
                }

  //               var response = actionResult as OkNegotiatedContentResult<IEnumerable<Trip>>;
  //     Assert.IsNotNull(response);
  //    var books = response.Content;
  //Assert.AreEqual(5, books.Count());


  //      var response = actionResult as NotFoundResult;
   //             Assert.IsNotNull(response);

    
            }
         }

         [Test]
         public void PostTripLoadLikeGSON_Passes()
         {
             using (IDbContext idtoFakeContext = new IDTOFakeContext())
             using (IUnitOfWork unitOfWork = new UnitOfWork(idtoFakeContext))
             {
                 unitOfWork.Repository<Traveler>().Insert(new Traveler { Id = 1003, FirstName = "TestFN", LastName = "TestLN", ObjectState = ObjectState.Added });
                 unitOfWork.Save();
                 var controller = new TripController(idtoFakeContext);
                 SetupControllerForTests(controller);

                 //TripModel m = new TripModel
                 //{
                 //    TravelerId = 1,
                 //    Origination = "Neverland",
                 //    Destination = "Montana",
                 //    TripStartDate = DateTime.Parse("1/2/2014"),
                 //    TripEndDate = DateTime.Parse("1/3/2014"),
                 //    MobilityFlag = true,
                 //    BicycleFlag = true,
                 //    PriorityCode = "1"
                 //};

                 //StepModel


                 // convert string to stream
                 byte[] byteArray = Encoding.UTF8.GetBytes(GSONcontents);
                 //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
                 MemoryStream jsonStream = new MemoryStream(byteArray);

                 DataContractJsonSerializer serializer =
    new DataContractJsonSerializer(typeof(TripModel));

                // TripModel trip = (TripModel)serializer.ReadObject(jsonStream);
                 TripModel trip = Deserialize<TripModel>(GSONcontents);
                 try
                 {
                     var actionResult = controller.PostTrip(trip);
                     var response = actionResult as CreatedAtRouteNegotiatedContentResult<TripModel>;
                     Assert.IsNotNull(response);
                     var trips = response.Content;
                     Assert.AreEqual("DCSC", trips.Origination);
                 }
                 catch (HttpResponseException ex)
                 {
                     Assert.Fail();
                 }
             }
         }
         public static T Deserialize<T>(string json)
         {
             return new JavaScriptSerializer().Deserialize<T>(json);
         }
   
         private string GSONcontents = @"{
  ""TripStartDate"": ""12/16/2013 20:11:30"",
  ""Destination"": ""UNK"",
  ""TripEndDate"": ""12/16/2013 20:33:43"",
  ""Steps"": [
    {
      ""ToStopCode"": ""DCSCMN1"",
      ""EndDate"": ""12/16/2013 20:14:30"",
      ""FromName"": ""DSCS Campus"",
      ""ToName"": ""DCSCMAIN"",
      ""StartDate"": ""12/16/2013 20:11:30"",
      ""ModeId"": 2,
      ""Distance"": 1,
      ""FromProviderId"": 4,
      ""ToProviderId"": 0,
      ""Id"": 0,
      ""TripId"": 0
    },
    {
      ""ToStopCode"": ""BROBEETW"",
      ""EndDate"": ""12/16/2013 20:16:30"",
      ""FromName"": ""DCSCMAIN"",
      ""ToName"": ""BROBEETW"",
      ""FromStopCode"": ""DCSCMN1"",
      ""StartDate"": ""12/16/2013 20:14:30"",
      ""ModeId"": 1,
      ""Distance"": 0.1,
      ""FromProviderId"": 0,
      ""ToProviderId"": 1,
      ""Id"": 0,
      ""TripId"": 0
    },
    {
      ""ToStopCode"": ""UNK"",
      ""EndDate"": ""12/16/2013 20:33:43"",
      ""FromName"": """",
      ""ToName"": ""Unknown"",
      ""FromStopCode"": ""BROBEETW"",
      ""StartDate"": ""12/16/2013 20:23:43"",
      ""ModeId"": 2,
      ""Distance"": 1,
      ""FromProviderId"": 1,
      ""ToProviderId"": 0,
      ""Id"": 0,
      ""TripId"": 0
    }
  ],
  ""Origination"": ""DCSC"",
  ""PriorityCode"": ""1"",
  ""MobilityFlag"": false,
  ""TravelerId"": 1003,
  ""Id"": 0,
  ""BicycleFlag"": false
}
";
    }
}
