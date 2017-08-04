using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.WebAPI.Controllers;
using IDTO.UnitTests.Fake;
using IDTO.Service;
using IDTO.Common;
using Moq;
using NUnit.Framework;
namespace IDTO.UnitTests.Fake
{
    public class TestData
    {
        public static string ModifiedByString = "Nunit";
        public static Traveler GetTraveler_CapTrans()
        {
            Traveler travelerEntity = new Traveler();
            travelerEntity.DefaultBicycleFlag = true;
            travelerEntity.DefaultMobilityFlag = true;
            travelerEntity.DefaultPriority = "";
            travelerEntity.DefaultTimezone = "";
            travelerEntity.InformedConsentDate = DateTime.UtcNow;
            travelerEntity.InformedConsent = true;
            travelerEntity.PhoneNumber = "6145555555";
            travelerEntity.Email = "captrans1@mdt.com";
            travelerEntity.LastName = "MDT2";
            travelerEntity.MiddleName = "d";
            travelerEntity.FirstName = "m";
            travelerEntity.ModifiedBy = ModifiedByString;
            travelerEntity.ModifiedDate = DateTime.UtcNow;
            travelerEntity.CreatedDate = DateTime.UtcNow;
            
            return travelerEntity;
        }

        public static Traveler GetTraveler_GregZink()
        {
            Traveler travelerEntity = new Traveler();
            travelerEntity.DefaultBicycleFlag = true;
            travelerEntity.DefaultMobilityFlag = true;
            travelerEntity.DefaultPriority = "";
            travelerEntity.DefaultTimezone = "";
            travelerEntity.InformedConsentDate = DateTime.UtcNow;
            travelerEntity.InformedConsent = true;
            travelerEntity.PhoneNumber = "6145555555";
            travelerEntity.Email = "zinkg@battelle.org";
            travelerEntity.LastName = "MDT2";
            travelerEntity.MiddleName = "d";
            travelerEntity.FirstName = "m";
            travelerEntity.ModifiedBy = ModifiedByString;
            travelerEntity.ModifiedDate = DateTime.UtcNow;
            travelerEntity.CreatedDate = DateTime.UtcNow;
            travelerEntity.Id = 2201;

            return travelerEntity;
        }

        public static Trip GetTripForNotificationTest()
        {
            Trip tripEntity = new Trip();
            tripEntity.Origination = "Rockville";
            tripEntity.Destination = "Bouldertown";
            tripEntity.TripStartDate = DateTime.UtcNow.AddMinutes(4);
            tripEntity.TripEndDate = DateTime.UtcNow.AddMinutes(30);
            tripEntity.MobilityFlag = true;
            tripEntity.BicycleFlag = false;
            tripEntity.PriorityCode = "2";
            tripEntity.TripStartNotificationSent = false;
            tripEntity.CreatedDate = tripEntity.ModifiedDate = DateTime.UtcNow.AddMinutes(-10);
            tripEntity.Traveler = GetTraveler_GregZink();
            tripEntity.TravelerId = tripEntity.Traveler.Id;

            return tripEntity;
        }

        public static Trip GetTrip()
        {
            Trip tripEntity = new Trip();
            tripEntity.Origination = "Rockville";
            tripEntity.Destination = "Bouldertown";
            tripEntity.TripStartDate = DateTime.Parse("1/1/2014 10:02");
            tripEntity.TripEndDate = DateTime.Parse("1/1/2014 11:02");
            tripEntity.MobilityFlag = true;
            tripEntity.BicycleFlag = false;
            tripEntity.PriorityCode = "2";

            tripEntity.CreatedDate = tripEntity.ModifiedDate = DateTime.UtcNow;
            return tripEntity;
        }
        public static List<Step> GetSteps()
        {
            List<Step> steps = new List<Step>();
            int stepnumber = 1;

            Step stepEntity = new Step();
            stepEntity.StepNumber = stepnumber++;
            stepEntity.StartDate = DateTime.Parse("1/1/2014 10:02");
            stepEntity.EndDate = DateTime.Parse("1/1/2014 10:40");
            stepEntity.FromName = "DSCS Campus";
            stepEntity.FromProviderId = (int)Providers.CapTrans;
            stepEntity.FromStopCode = "1001";
            stepEntity.ModeId = (int)Modes.Bus;
            stepEntity.RouteNumber = "039";
            stepEntity.Distance = (decimal)12.2;
            stepEntity.ToName = "Broad St Gate";
            stepEntity.ToProviderId = (int)Providers.CapTrans;
            stepEntity.ToStopCode = "2002";
            steps.Add(stepEntity);

            Step stepEntity2 = new Step();
            stepEntity2.StepNumber = stepnumber++;
            stepEntity2.StartDate = DateTime.Parse("1/1/2014 10:40");
            stepEntity2.EndDate = DateTime.Parse("1/1/2014 10:50");
            stepEntity2.FromName = "Broad St Gate";
            stepEntity2.FromProviderId = null;
            stepEntity2.FromStopCode = "2002";
            stepEntity2.ModeId = (int)Modes.Walk;
            stepEntity2.RouteNumber = "";
            stepEntity2.Distance = (decimal)1.35;
            stepEntity2.ToName = "E BROAD ST & BEECHTREE RD";
            stepEntity2.ToProviderId = null;
            stepEntity2.ToStopCode = "3003";
            steps.Add(stepEntity2);

            Step stepEntity3 = new Step();
            stepEntity3.StepNumber = stepnumber++;
            stepEntity3.StartDate = DateTime.Parse("1/1/2014 10:56");
            stepEntity3.EndDate = DateTime.Parse("1/1/2014 11:02");
            stepEntity3.FromName = "E BROAD ST & BEECHTREE RD";
            stepEntity3.FromProviderId = (int)Providers.COTA;
            stepEntity3.FromStopCode = "3003";
            stepEntity3.ModeId = (int)Modes.Bus;
            stepEntity3.RouteNumber = "426";
            stepEntity3.Distance = (decimal)12.2;
            stepEntity3.ToName = "Sandstone Street";
            stepEntity3.ToProviderId = (int)Providers.COTA;
            stepEntity3.ToStopCode = "4004";
            steps.Add(stepEntity3);

            return steps;
        }
        public  static TConnectOpportunity GetTConnectOpportunity()
        {

            TConnectOpportunity TConnOpp = new TConnectOpportunity();
            TConnOpp.ModifiedBy = ModifiedByString;
            TConnOpp.ModifiedDate = DateTime.UtcNow;
            TConnOpp.CheckpointProviderId = (int)Providers.CapTrans;
            TConnOpp.CheckpointStopCode = "2002";
            TConnOpp.CheckpointRoute = "";
            TConnOpp.TConnectProviderId = (int) Providers.COTA;
            TConnOpp.TConnectStopCode = "3003";
            TConnOpp.TConnectRoute = "426";
            return TConnOpp;
        }
    }
}
