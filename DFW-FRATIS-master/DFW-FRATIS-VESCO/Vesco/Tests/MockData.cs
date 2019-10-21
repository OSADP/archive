using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.MockingKernel.Moq;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.CTIP.Optimization.Services;

namespace Tests
{
    public static class MockData
    {
        public static MoqMockingKernel Kernel { get; set; }

        private static JobHelper _helper = null;
        public static JobHelper Helper
        {
            get { return _helper ?? (_helper = new JobHelper()); }
        }

        private static IRandomNumberGenerator _randomNumberGenerator;
        private static IRandomNumberGenerator RandomNumberGenerator
        {
            get
            {
                return _randomNumberGenerator ?? (_randomNumberGenerator = GetService<IRandomNumberGenerator>());
            }
        }

        public static TInterface GetService<TInterface>()
        {
            try
            {
                var result = Kernel.Get<TInterface>();
                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static IList<Driver> GetMockDrivers(int count = 10, 
            Location startLocation = null, 
            double availableDrivingHours = 10, 
            double availableDutyHours = 11)
        {
            IList<Driver> result = new List<Driver>();
            if (startLocation == null)
            {
                startLocation = GetMockLocations(1, "Driver").FirstOrDefault();
            }

            for (int i = 0; i < count; i++)
            {
                RandomNumberGenerator.Reseed(i);
                var orderType = RandomNumberGenerator.Next(1, 4);
                var isShortHaul = RandomNumberGenerator.Next(1, 3) == 1;
                var isHazmat = RandomNumberGenerator.Next(1, 4) == 1;

                result.Add(new Driver()
                {
                    AvailableDrivingHours = 8.0,
                    AvailableDutyHours = 10.0,
                    DisplayName = string.Format("Driver {0}", i+1),
                    EarliestStartTime = TimeSpan.FromHours(6),
                    StartingLocation = startLocation,
                    OrderType = orderType,
                    IsShortHaulEligible = isShortHaul,
                    IsLongHaulEligible = !isShortHaul,
                    IsHazmatEligible = isHazmat
                });
            }

            return result;
        }

        private static double GetMockLongitude()
        {
            return -98.111;
            return double.Parse(string.Format("-98.{0}", RandomNumberGenerator.Next(99999)));
        }

        private static double GetMockLatitude()
        {
            return 33.111;
            return double.Parse(string.Format("33.{0}", RandomNumberGenerator.Next(99999)));
        }

        public static IList<Location> GetMockLocations(int count, string displayNamePrefix = "")
        {
            IList<Location> result = new List<Location>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new Location()
                {
                    DisplayName = string.Format("{0} Location {1}", displayNamePrefix, i+1).Trim(),
                    Latitude = GetMockLatitude(),
                    Longitude = GetMockLongitude()
                });
            }
            return result;
        }


        public static Location GetRandomLocation(IList<Location> locations)
        {
            RandomNumberGenerator.Reseed(locations.Count());
            return locations[RandomNumberGenerator.Next()];
        }

        public static IList<Location> GetRandomLocations(int count, IList<Location> locations, bool allowRepeatLocations = false)
        {
            var result = new List<Location>();
            var usedIndexes = new HashSet<int>();
            
            for (int i = 0; i < count && count <= locations.Count; )
            {
                var randomIndex = RandomNumberGenerator.Next(locations.Count);
                if (allowRepeatLocations || !usedIndexes.Contains(randomIndex))
                {
                    usedIndexes.Add(randomIndex);
                    result.Add(locations[randomIndex]);
                    i++;
                }
                else
                {
                    // do not increment, try again
                }
            }
            return result;
        }

        public static IList<Job> GetJobs(int count, IList<Location> locations, bool twoStopsPerJob = true)
        {
            IList<Job> result = new List<Job>();
            var routeSanitizer = GetService<IRouteSanitizer>();
            RandomNumberGenerator.Reseed(locations.Count());
            for (int i = 0; i < count; i++)
            {
                _randomNumberGenerator.Reseed(i);
                var orderType = RandomNumberGenerator.Next(1, 4);
                var isShortHaul = RandomNumberGenerator.Next(1, 3) == 1;
                var isHazmat = RandomNumberGenerator.Next(1, 4) == 1;

                var job = new Job()
                    {
                        Id = i + 1,
                        DisplayName = string.Format("Job {0}", i + 1),
                        IsHazmat = isHazmat,
                        IsShortHaul = isShortHaul,
                        IsLongHaul = !isShortHaul,
                        Priority = isHazmat || RandomNumberGenerator.Next(1, 4) == 1 ? 3 : RandomNumberGenerator.Next(1, 3),
                        EquipmentConfiguration =
                            new EquipmentConfiguration(
                                new Chassis(), new Container(), new ChassisOwner(), new ContainerOwner()),
                        OrderType = orderType
                                
                    };

                if (twoStopsPerJob)
                {
                    var stopLocations = GetRandomLocations(2, locations, false);
                    var rs1 = Helper.CreateRouteStop(job, 90, StopActions.PickupLoadedWithChassis,
                        stopLocations[0], Helper.GetTimeSpan(30), Helper.GetTimeSpan(0, 0), Helper.GetTimeSpan(24, 0));

                    var rs2 = Helper.CreateRouteStop(job, 90, StopActions.DropOffLoadedWithChassis,
                        stopLocations[1], Helper.GetTimeSpan(30), Helper.GetTimeSpan(12, 0), Helper.GetTimeSpan(14, 0));

                    job.RouteStops.Add(rs1);
                    job.RouteStops.Add(rs2);
                }
                else
                {
                    var rs = Helper.CreateRouteStop(job, 30, StopActions.NoAction,
                        GetRandomLocation(locations), Helper.GetTimeSpan(30), Helper.GetTimeSpan(6, 0), Helper.GetTimeSpan(8, 0));
                    job.RouteStops.Add(rs);
                }

                // route sanitization
                routeSanitizer.PrepareJob(job);

                result.Add(job);
            } 

            return result;
        }


        public static IList<Driver> GetStaticDrivers()
        {
            var locations = GetStaticLocations();
            return new List<Driver>()
                {
                    new Driver()
                        {
                            DisplayName = "Driver 1",
                            AvailableDrivingHours = 10,
                            AvailableDutyHours = 11,
                            EarliestStartTime = new TimeSpan(6, 0, 0),
                            StartingLocation = locations[0]
                        },
                    new Driver()
                        {
                            DisplayName = "Driver 2",
                            AvailableDrivingHours = 10,
                            AvailableDutyHours = 11,
                            EarliestStartTime = new TimeSpan(6, 0, 0),
                            StartingLocation = locations[0]
                        },
                };
        } 

        public static IList<Location> GetStaticLocations()
        {
            return new List<Location>()
                {
                    new Location() { DisplayName = "Start Location", Latitude = 0.0, Longitude = 0.0 },
                    new Location() { DisplayName = "Location 1", Latitude = -2, Longitude = 1 },
                    new Location() { DisplayName = "Location 2", Latitude = 1, Longitude = 3 },
                    new Location() { DisplayName = "Location 3", Latitude = 3, Longitude = 0.5 },
                    //new Location() { DisplayName = "Location 4", Latitude = 5, Longitude = 0.5 },
                    //new Location() { DisplayName = "Location 5", Latitude = 4, Longitude = 0.5 },
                };
        }

        public static IList<Job> GetStaticJobs()
        {

            var locations = GetStaticLocations();
            var routeSanitizer = GetService<IRouteSanitizer>();

            var job1 = new Job() { DisplayName = "Job 1", Priority = 3 };
            job1.RouteStops = new List<RouteStop>()
                {
                    Helper.CreateRouteStop(
                        job1,
                        30,
                        StopActions.NoAction,
                        locations[1],
                        TimeSpan.FromMinutes(30),
                        TimeSpan.FromHours(0),
                        TimeSpan.FromHours(24))
                };
            routeSanitizer.PrepareJob(job1);

            var job2 = new Job() { DisplayName = "Job 2", Priority = 1 };
            job2.RouteStops = new List<RouteStop>()
                {
                    Helper.CreateRouteStop(
                        job2,
                        30,
                        StopActions.NoAction,
                        locations[2],
                        TimeSpan.FromMinutes(30),
                        TimeSpan.FromHours(0),
                        TimeSpan.FromHours(24))
                };
            routeSanitizer.PrepareJob(job2);


            var job3 = new Job() { DisplayName = "Job 3", Priority = 3};
            job3.RouteStops = new List<RouteStop>()
                {
                    Helper.CreateRouteStop(
                        job3,
                        30,
                        StopActions.NoAction,
                        locations[3],
                        TimeSpan.FromMinutes(30),
                        TimeSpan.FromHours(0),
                        TimeSpan.FromHours(24))
                };
            routeSanitizer.PrepareJob(job3);

            //var job4 = new Job() { DisplayName = "Job 4", };
            //job4.RouteStops = new List<RouteStop>()
            //    {
            //        Helper.CreateRouteStop(
            //            job4,
            //            30,
            //            StopActions.NoAction,
            //            locations[4],
            //            TimeSpan.FromMinutes(30),
            //            TimeSpan.FromHours(0),
            //            TimeSpan.FromHours(24))
            //    };
            //routeSanitizer.PrepareJob(job4);

            //var job5 = new Job() { DisplayName = "Job 5", Priority = 0};
            //job5.RouteStops = new List<RouteStop>()
            //    {
            //        Helper.CreateRouteStop(
            //            job5,
            //            30,
            //            StopActions.NoAction,
            //            locations[5],
            //            TimeSpan.FromMinutes(30),
            //            TimeSpan.FromHours(0),
            //            TimeSpan.FromHours(24))
            //    };
            //routeSanitizer.PrepareJob(job5);

            return new[] { job1, job2};
        }
    }
}
