using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDTO.Entity.Models;
using IDTO.Common;
using Repository;
namespace IDTO.Service
{
    /// <summary>
    /// Adds trip, steps, and possible TConnect, to the database.
    /// </summary>
    public class TripService : ITripService
    {
        private readonly int _tConnectWindowInMinutes = 1;

        public TripService() : this(1)
        { }

        public TripService(int tConnectWindowInMinutes)
        {
            _tConnectWindowInMinutes = tConnectWindowInMinutes;
        }

        /// <summary>
        /// Adds a trip and its steps to the database. Also determines
        ///  if the trip contains steps that match special criteria for a TConnect.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="steps"></param>
        /// <param name="Uow"></param>
        /// <returns></returns>
        public int SaveTrip(Trip trip, List<Step> steps, IUnitOfWork Uow)
        {
            //Save trip and steps to database
            Uow.Repository<Trip>().Insert(trip);
            Uow.Repository<TripEvent>().Insert(new TripEvent(trip.Id, "Trip Created"));
            Uow.Save();
            foreach (Step s in steps)
            {
                s.TripId = trip.Id;//assign the step to the trip
                Uow.Repository<Step>().Insert(s);
                Uow.Save();
            }

            //Use TConnectOpportunity table as the list of "special" connections.
            //Compare endpoints in table to endpoints between steps in trip.
            //Remove walking steps, because those are not relevant to this determination.
            //If given:
            //Step A : StartA StopA
            //Step B : StartB StopB
            //If TConnectOpportunity CheckPointStopCode == StopA and TConnectStopCode == StartB
            //then a TConnect should be created

            Step[] busSteps = steps.Where(t => t.ModeId != (int)Modes.Walk).ToArray();
              bool notTheEnd = true;
            var e = busSteps.GetEnumerator();
            
            
            /*
            var tostopcodesArray = busSteps.Select(k => k.ToStopCode).ToList();
            var tOppCheckpointsAsArray = Uow.Repository<TConnectOpportunity>().Query().Get().Select(asdf => asdf.CheckpointStopCode).ToList();
            var allTOpps = Uow.Repository<TConnectOpportunity>().Query().Get().AsEnumerable().ToList();
            var thecodethatmatched = tostopcodesArray.Where(code => Uow.Repository<TConnectOpportunity>().Query().Get().Any(tconn => tconn.CheckpointStopCode.Equals(code))).ToList();
            var stepsInTOppTable = busSteps.Where(code => Uow.Repository<TConnectOpportunity>().Query().Get().Any(tconn => tconn.CheckpointStopCode.Equals(code.ToStopCode))).ToList();

            var tOppInStep = Uow.Repository<TConnectOpportunity>().Query().Get().Where(code => busSteps.Any(tconn => tconn.ToStopCode.Equals(code.CheckpointStopCode))).ToList();

            foreach (TConnectOpportunity topp in tOppInStep)
            {
                var enumerable = busSteps.Select(k => k.ToStopCode.Equals(topp.CheckpointStopCode)).GetEnumerator();//get the tOpp that matched
                var current = enumerable.Current;
                enumerable.MoveNext();
                var next = enumerable.Current;

            }
          

            e.MoveNext();//move to first spot.
            while (notTheEnd)
            {
                Step theStep = (Step)e.Current;
                var tOppMatchingStep = Uow.Repository<TConnectOpportunity>().Query().Get().Where(code => code.CheckpointStopCode.Equals(theStep.ToStopCode)).ToList();
                if (tOppMatchingStep.Count > 1)
                {
                    //TODO
                    throw new Exception("we could have one checkpoint that has multiple different tconnects, yes???");
                }
                else if (tOppMatchingStep.Count == 1)
                {
                    notTheEnd = e.MoveNext();//see if the from code for the next step matches the tconnect for the match.
                    if (notTheEnd)
                    {
                        theStep = (Step)e.Current;
                        if (tOppMatchingStep[0].TConnectStopCode == theStep.FromStopCode)
                        {
                            //Add TConnect
                            var tconnect = "add new";
                        }
                    }
                }
                else
                {
                    notTheEnd = e.MoveNext();//no matches, go onto next step to check it too

                }
            }

            
            e.Reset();
           */
            e.MoveNext();//move to first step.
            while (notTheEnd)
            {
                Step theStep = (Step)e.Current;
                notTheEnd = e.MoveNext();//see if the from code for the next step matches the tconnect for the pair.
                if (notTheEnd)
                {
                    Step theNextStep = (Step)e.Current;
                    var tOppMatchingStep = Uow.Repository<TConnectOpportunity>().Query().Get()
                        .Where(code => code.CheckpointStopCode.Equals(theStep.ToStopCode)
                            && code.TConnectStopCode.Equals(theNextStep.FromStopCode)
                            && code.TConnectRoute.Equals(theNextStep.RouteNumber)).ToList();
                    if (tOppMatchingStep.Count > 1)
                    {
                        //To Stop Code and from stop code pairings in TConnectOpportunity should be unique.
                        throw new Exception("TripService: TConnectOpportunity should not have duplicate rows.");
                    }
                    else if (tOppMatchingStep.Count == 1)
                    {
                        //Add TConnect
                       
                        //Tconnect created with Status New.
                        //InboundVehicle,TConnectRequestID will all be 
                        //filled in later by the Monitor
                        TConnect newTConnect = new TConnect();
                        newTConnect.CreatedDate = DateTime.UtcNow;
                        newTConnect.ModifiedDate = DateTime.UtcNow;
                        newTConnect.ModifiedBy = "WebAPI";
                        //newTConnect.TConnectStatusId = Uow.Repository<TConnectStatus>().Query().Filter(s => s.Name.Equals("New")).Get().Select(s => s.Id).First();
                        newTConnect.TConnectStatusId = (int)TConnectStatuses.New;
                        newTConnect.InboundStepId = theStep.Id;
                        newTConnect.OutboundStepId = theNextStep.Id;


                        TimeSpan walkDuration = CalculateWalkDuration(theStep, theNextStep, steps);
                        //Start time of the departing step minus walk time would be the earliest time a TConnect would be issued.
                        //If bus left at 2:00, and walking takes 5 minutes, the arriving bus would have to arrive by 1:55 to make it.
                        newTConnect.StartWindow = theNextStep.StartDate - walkDuration;
                        //For now, we assume the bus will never wait more than 8 minutes. However,
                        //I think this may be provider and even stop-dependent.  Bus routes that have busses that
                        //leave every 10 minutes would probably not wait more than 2 minutes, but routes that are
                        //only hourly or daily may be willing to wait longer. Perhaps the max wait time should go
                        //into the tconnectopportunity table per route.
                        newTConnect.EndWindow = theNextStep.StartDate.AddMinutes(_tConnectWindowInMinutes) - walkDuration;
                        Uow.Repository<TConnect>().Insert(newTConnect);
                        Uow.Repository<TripEvent>().Insert(new TripEvent(trip.Id, "T-Connect Created"));
                        Uow.Save();
                    }
                }
            }

            return trip.Id;
        }

        /// <summary>
        /// Adds the durations of each step in between the two connecting bus stops - this is the total
        /// extra time required between buses.
        ///A simple subtraction of theNextStep.StartDate - theStep.EndDate is likely not valid,
        ///because chances are the walking step will get you to the departing bus stop early.
        /// </summary>
        /// <param name="theStep"></param>
        /// <param name="theNextStep"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        private static TimeSpan CalculateWalkDuration(Step theStep, Step theNextStep, List<Step> steps)
        {
            int i =0;
            TimeSpan walkDuration = new TimeSpan(0);
            //for example, step1 is bus, step2 is walk, step3 is bike, step 4 is bus.
            //0 index so subtract 1 from StepNumber +1
            for (i = theStep.StepNumber; i < theNextStep.StepNumber-1; i++)
            {
                walkDuration += steps[i].EndDate - steps[i].StartDate;

            }
                
                //TimeSpan walkDuration = theNextStep.StartDate - theStep.EndDate;
                return walkDuration;
        }
    }
}
