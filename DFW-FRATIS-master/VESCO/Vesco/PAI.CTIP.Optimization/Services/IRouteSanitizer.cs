using System;
using System.Linq;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public interface IRouteSanitizer
    {
        /// <summary>
        /// Returns true if route stop is valid
        /// </summary>
        /// <param name="stop"></param>
        /// <returns></returns>
        bool IsValidRouteStop(RouteStop stop);

        /// <summary>
        /// function that check if the truck configuration is valid.
        /// returns true if valid and false if not
        /// </summary>
        /// <returns></returns>
        bool IsValidTruckConfig(TruckConfiguration truckConfiguration);

        /// <summary>
        /// Prepares a job for optimization
        /// </summary>
        /// <param name="job"></param>
        void PrepareJob(Job job);
    }

    public class RouteSanitizer : IRouteSanitizer
    {
        /// <summary>
        /// Returns true if route stop is valid
        /// </summary>
        /// <param name="stop"></param>
        /// <returns></returns>
        public bool IsValidRouteStop(RouteStop stop)
        {
            if ((stop.StopAction.PreState & stop.PreTruckConfig.TruckState) != stop.PreTruckConfig.TruckState)
            {
                Console.WriteLine("\n\tInvalid Route Stop");
                Console.WriteLine(string.Format("\t\tstop.Location.DisplayName=<{0}>, stop.StopAction.PreState=<{1}>, stop.PreTruckConfig.TruckState=<{2}>", 
                    stop.Location.DisplayName, stop.StopAction.PreState, stop.PreTruckConfig.TruckState));
                return false;
            }

            if (!IsValidTruckConfig(stop.PreTruckConfig))
            {
                return false;
            }

            if (!IsValidTruckConfig(stop.PostTruckConfig))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// function that check if the truck configuration is valid.
        /// returns true if valid and false if not
        /// </summary>
        /// <returns></returns>
        public bool IsValidTruckConfig(TruckConfiguration truckConfiguration)
        {
            var t = truckConfiguration;

            if ((t.EquipmentConfiguration.Container == null && t.IsLoaded) || (t.EquipmentConfiguration.Chassis == null && t.IsLoaded))
            {
                return false;
            }

            if (t.EquipmentConfiguration.Container != null && t.EquipmentConfiguration.ContainerOwner == null || t.EquipmentConfiguration.Container == null && t.EquipmentConfiguration.ContainerOwner != null)
            {
                return false;
            }

            //if (t.EquipmentConfiguration.Chassis != null && t.EquipmentConfiguration.Container != null && !t.EquipmentConfiguration.Container.IsAllowed(t.EquipmentConfiguration.Chassis))
            //{
            //    return false;
            //}

            if ((t.TruckState == TruckState.Bobtail || t.TruckState == TruckState.Chassis) && t.IsLoaded)
            {
                return false;
            }

            if (t.TruckState == TruckState.Bobtail && t.IsLoaded)
            {
                return false;
            }

            if ((t.TruckState == TruckState.Loaded && !t.IsLoaded) || (t.TruckState != TruckState.Loaded && t.IsLoaded))
            {
                return false;
            }

            if (t.TruckState == TruckState.Empty && t.IsLoaded)
            {
                return false;
            }

            return true;
        }


        public void PrepareJob(Job job)
        {
            RouteStop previousStop = null;
            foreach (var routeStop in job.RouteStops)
            {
                PrepareRouteStop(routeStop, previousStop, job.EquipmentConfiguration, job);
                previousStop = routeStop;
            }
        }

        public void PrepareRouteStop(RouteStop result, RouteStop previousStop, EquipmentConfiguration equipment, Job job)
        {

            if (result.StopAction == StopActions.LiveLoading || result.StopAction == StopActions.LiveUnloading)
            {
                if (previousStop == null)
                {
                    Console.WriteLine("Live action cannot be the first stop of an order");
                    Console.WriteLine(string.Format("\t\tjob={0},result.Location.DisplayName={1}", job.DisplayName, result.Location.DisplayName));
//                    throw new Exception("Live action cannot be the first stop of an order");
                }

                result.PreTruckConfig = new TruckConfiguration()
                {
                    EquipmentConfiguration = new EquipmentConfiguration()
                    {
                        Chassis = previousStop.PostTruckConfig.EquipmentConfiguration.Chassis,
                        ChassisOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ChassisOwner,
                        Container = previousStop.PostTruckConfig.EquipmentConfiguration.Container,
                        ContainerOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ContainerOwner,
                    },
                    IsLoaded = previousStop.PostTruckConfig.IsLoaded
                };

                result.PostTruckConfig = new TruckConfiguration()
                {
                    EquipmentConfiguration = new EquipmentConfiguration()
                    {
                        Chassis = previousStop.PostTruckConfig.EquipmentConfiguration.Chassis,
                        ChassisOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ChassisOwner,
                        Container = previousStop.PostTruckConfig.EquipmentConfiguration.Container,
                        ContainerOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ContainerOwner,
                    },
                    IsLoaded = previousStop.PostTruckConfig.IsLoaded
                };

                if (result.StopAction == StopActions.LiveLoading)
                {
                    result.PostTruckConfig.IsLoaded = true;
                }

                if (result.StopAction == StopActions.LiveUnloading)
                {
                    result.PostTruckConfig.IsLoaded = false;

                    // mark previous live unloading stop as loaded
                    if (previousStop.StopAction == StopActions.LiveUnloading)
                    {
                        previousStop.PostTruckConfig.IsLoaded = true;
                        result.PreTruckConfig.IsLoaded = true;
                    }
                }
            }
            else
            {
                if (equipment == null || equipment.Chassis == null || equipment.Container == null)
                {
                    if (result.StopAction.PostState != TruckState.Bobtail)
                    {
                        // should have equipment, container and chassis instantiated at this point
                        if (equipment == null)
                        {
                            equipment = new EquipmentConfiguration();
                        }

                        equipment.Container = new Container();
                        equipment.Chassis = new Chassis();
                        equipment.ChassisOwner = new ChassisOwner();
                        equipment.ContainerOwner = new ContainerOwner();
                    }
                }

                result.PreTruckConfig = GetTruckConfigForAction(result.StopAction.PreState, equipment);
                result.PostTruckConfig = GetTruckConfigForAction(result.StopAction.PostState, equipment);
            }

            if (!IsValidRouteStop(result))
            {
                Console.WriteLine(string.Format("\t\t\tjob=<{0}>\n",
                    job.DisplayName));
//                throw new Exception("Invalid Route Stop");
            }
        }

/*
        public void PrepareRouteStop(RouteStop result, RouteStop previousStop, EquipmentConfiguration equipment)
        {

            if (result.StopAction == StopActions.LiveLoading || result.StopAction == StopActions.LiveUnloading)
            {
                if (previousStop == null)
                {
                    throw new Exception("Live action cannot be the first stop of an order");
                }

                result.PreTruckConfig = new TruckConfiguration()
                {
                    EquipmentConfiguration = new EquipmentConfiguration()
                    {
                        Chassis = previousStop.PostTruckConfig.EquipmentConfiguration.Chassis,
                        ChassisOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ChassisOwner,
                        Container = previousStop.PostTruckConfig.EquipmentConfiguration.Container,
                        ContainerOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ContainerOwner,
                    },
                    IsLoaded = previousStop.PostTruckConfig.IsLoaded
                };

                result.PostTruckConfig = new TruckConfiguration()
                {
                    EquipmentConfiguration = new EquipmentConfiguration()
                    {
                        Chassis = previousStop.PostTruckConfig.EquipmentConfiguration.Chassis,
                        ChassisOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ChassisOwner,
                        Container = previousStop.PostTruckConfig.EquipmentConfiguration.Container,
                        ContainerOwner = previousStop.PostTruckConfig.EquipmentConfiguration.ContainerOwner,
                    },
                    IsLoaded = previousStop.PostTruckConfig.IsLoaded
                };

                if (result.StopAction == StopActions.LiveLoading)
                {
                    result.PostTruckConfig.IsLoaded = true;
                }

                if (result.StopAction == StopActions.LiveUnloading)
                {
                    result.PostTruckConfig.IsLoaded = false;

                    // mark previous live unloading stop as loaded
                    if (previousStop.StopAction == StopActions.LiveUnloading)
                    {
                        previousStop.PostTruckConfig.IsLoaded = true;
                        result.PreTruckConfig.IsLoaded = true;
                    }
                }

            }

            else
            {
                result.PreTruckConfig = GetTruckConfigForAction(result.StopAction.PreState, equipment);
                result.PostTruckConfig = GetTruckConfigForAction(result.StopAction.PostState, equipment);
            }

            if (!IsValidRouteStop(result))
            {
                Console.WriteLine(string.Format("\t\t{0} is apparently not valid for some reason.", result.Location.DisplayName));
//                throw new Exception("Invalid Route Stop");
            }
        }
*/
        public TruckConfiguration GetTruckConfigForAction(TruckState truckState, EquipmentConfiguration equipment)
        {
            TruckConfiguration truckConfiguration = null;

            switch (truckState)
            {
                case TruckState.Chassis:
                    truckConfiguration = new TruckConfiguration()
                    {
                        EquipmentConfiguration = new EquipmentConfiguration()
                        {
                            Chassis = equipment.Chassis,
                            ChassisOwner = equipment.ChassisOwner,
                        }
                    };

                    break;

                case TruckState.Empty:

                    truckConfiguration = new TruckConfiguration()
                    {
                        EquipmentConfiguration = new EquipmentConfiguration()
                        {
                            Chassis = equipment.Chassis,
                            ChassisOwner = equipment.ChassisOwner,
                            Container = equipment.Container,
                            ContainerOwner = equipment.ContainerOwner,
                        }
                    };


                    break;

                case TruckState.Loaded:

                    truckConfiguration = new TruckConfiguration()
                    {
                        IsLoaded = true,
                        EquipmentConfiguration = new EquipmentConfiguration()
                        {
                            Chassis = equipment.Chassis,
                            ChassisOwner = equipment.ChassisOwner,
                            Container = equipment.Container,
                            ContainerOwner = equipment.ContainerOwner,
                        }
                    };

                    break;

                default:
                    truckConfiguration = new TruckConfiguration();
                    break;
            }

            return truckConfiguration;
        }
    }

}