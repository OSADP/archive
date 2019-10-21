using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vesco
{
    class SouthwestConverter
    {
        public static int totalOrdersCount = 0;
        public static int goodOrdersCount = 0;
        public static int bad1OrdersCount = 0;
        public static int bad2OrdersCount = 0;
        public static int bad3OrdersCount = 0;
        public static int bad4OrdersCount = 0;
        public static int unkownOrdersCount = 0;

        public static int export1 = 0;
        public static int export2 = 0;
        public static int export2a = 0;
        public static int export3 = 0;
        public static int export3a = 0;
        public static int import1 = 0;
        public static int import2 = 0;
        public static int import2a = 0;
        public static int import3 = 0;
        public static int import3a = 0;
        public static int import4 = 0;
        public static int cityEmptyRepo = 0;
        public static int cityLoadedRepo = 0;
        public static int export4 = 0;
        public static int export5 = 0;
        public static int export6 = 0;
        public static int import5 = 0;
        public static int import6 = 0;
        public static int import7 = 0;
        public static int export7 = 0;
        public static int import8 = 0;
        public static int export8 = 0;
        public static int import9 = 0;
        public static int export9 = 0;
        public static int import10 = 0;
        public static int export10 = 0;
        public static int import11 = 0;
        public static int chassis1 = 0;
        public static int lude = 0;
        public static int export11 = 0;
        public static int export12 = 0;

        public static List<String> badOrders = new List<String>();

        public static List<VescoOrder> Convert(List<TriniumOrder> assOrders) {

//            Console.Out.WriteLine("Order => " + assOrders[0].OrderNumber + ", Order Size => " + assOrders.Count());
            List<VescoOrder> vescoOrders = new List<VescoOrder>();
            VescoOrder tempVo;
            String tempJobNumber = assOrders[0].OrderNumber;
            totalOrdersCount++;

            if (assOrders.Count() == 1) {

                //cityEmptyRepo - Empty container repositioning
                if ("CER".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    cityEmptyRepo++;
                }

                //cityLoadedRepo - Loaded container repositioning
                else if ("CLR".Equals(assOrders[0].DispatchCategoryCode) &&
                  "FCL".Equals(assOrders[0].LegType) &&
                   !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    cityLoadedRepo++;
                }
                //Chassis 1 - Chassis Repositioning
                else if ("CER".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CHS".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    chassis1++;
                }
                // Export 12 - Empty export
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                     !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    export12++;
                }
                // Export 3a - Full export
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                     !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    export3a++;
                }
                // Import 3a - Full import yard pull only
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    import3a++;
                }
                //Export 2a - Export pickup empty with live load tomorrow
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    export2a++;
                }
                //Import 2a - Import yard pull only L/L tomorrow
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    import2a++;
                }

                // LUDE - Live Unload Drop Empty
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    lude++;
                }
                // Import 4 - Empty Import
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    !assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    import4++;
                }

                // Export 11 - Export live load drop load
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    assOrders[0].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    export11++;
                }

/*                //PICKING UP AND DROPPING OFF CHASSIS - Chassis Move
                else if ("CER".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CHS".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

//                    puadocChassisMove++;
                }
                // ??
                //Default Export One  --  Delete ??
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                //    defaultExportOne++;
                }
                //Default Export Two  --  Delete ??
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                 //   defaultExportTwo++;
                }
                //Default Import One  --  Delete ??
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

 //                   defaultImportOne++;
                }
                //Default Import Two  --  Delete ??
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType))
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

   //                 defaultImportTwo++;
                } */
                else
                {
                    Console.Out.WriteLine("\tBad 1 Order - " + tempJobNumber);
//                    badOrdersCount++;
                    bad1OrdersCount++;
                    badOrders.Add(tempJobNumber);
                }
            } 
            
            
            else if (assOrders.Count() == 2)
            {
                //Export 1 - Export with Live Load
                if ("CEX".Equals(assOrders[0].DispatchCategoryCode) && 
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
                   assOrders[0].Ll &&
                   assOrders[1].Ll)
                {

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    export1++;
                }
                //Import 1 - Import with Live Unload
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                  "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                  "FCL".Equals(assOrders[0].LegType) &&
                  "ECL".Equals(assOrders[1].LegType) &&
                   assOrders[0].Ll &&
                   assOrders[1].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DWEC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    import1++;
                }
                //Export 3 - Export with yard pull
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                   "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                  "FCL".Equals(assOrders[0].LegType) &&
                  "FCL".Equals(assOrders[1].LegType) &&
                   !assOrders[0].Ll &&
                   !assOrders[1].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    export3++;
                }
                //Import 4 - ??DELETE??
                //else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                //   "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                //  "FCL".Equals(assOrders[0].LegType) &&
                //  "ECL".Equals(assOrders[1].LegType) &&
                //   !assOrders[0].Ll &&
                //   !assOrders[1].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                //    tempVo.Location = assOrders[1].PickupName;
                //    tempVo.Address = assOrders[1].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[1].DeliverName;
                //    tempVo.Address = assOrders[1].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    import4++;
                //}
                //Import 3 - Import with hub stop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
                    !assOrders[0].Ll &&
                    !assOrders[1].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    import3++;
                }
                else
                {
                    Console.Out.WriteLine("\tBad 2 Order - " + tempJobNumber);
//                    badOrdersCount++;
                    bad2OrdersCount++;
                    badOrders.Add(tempJobNumber);
                }

            } else if (assOrders.Count() == 3) {

                //Export 4 - Export with Live Load and Chassis drop
                if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
                    "CHS".Equals(assOrders[2].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export4++;
                }
                // Import 5 - Import with LU and Chassis drop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "FCL".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "CHS".Equals(assOrders[2].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import5++;
                }
                // Import 7 - Import with yard pull and chassis drop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "FCL".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "CHS".Equals(assOrders[2].LegType) &&
                    !assOrders[0].Ll &&
                    !assOrders[1].Ll &&
                    !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import7++;
                }
                //Export 7 - Export with LL
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[2].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "FCL".Equals(assOrders[2].LegType) &&
                     assOrders[0].Ll &&
                     assOrders[1].Ll &&
                     assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "3-Pickup Empty,PE";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export7++;
                }
                // Import 8 - Import with LU and chassis drop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "5-Pickup Loaded,PL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import8++;
                }
                //Export 8 - Export with chassis pickup
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CHS".Equals(assOrders[0].LegType) &&
                    "ECL".Equals(assOrders[1].LegType) &&
                    "FCL".Equals(assOrders[2].LegType) &&
                   !assOrders[0].Ll &&
                   !assOrders[1].Ll &&
                   !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "3-Pickup Empty,PE";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export8++;
                }
                //Import 9 - Import with chassis pickup
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                    !assOrders[1].Ll &&
                    !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "5-Pickup Loaded,PL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import9++;
                }
                //Export 2 - Export with LL and yard pull
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[2].DispatchCategoryCode) &&
                   "ECL".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "FCL".Equals(assOrders[2].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export2++;
                }
                //Import 2 - Import with LU Hub Stop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "FCL".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import2++;
                }
                //Export 6 - Export with yard pull and chassis drop
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                   "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CEX".Equals(assOrders[2].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
                    "CHS".Equals(assOrders[2].LegType) &&
                   !assOrders[0].Ll &&
                   !assOrders[1].Ll &&
                   !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export6++;
                }
                else
                {
                    Console.Out.WriteLine("\tBad 3 Order - " + tempJobNumber);
                    bad3OrdersCount++;
//                    badOrdersCount++;
                    badOrders.Add(tempJobNumber);
                }
            }

            else if (assOrders.Count() == 4)
            {
                // Export 8 - Export with chassis pickup and drop off + LL
                if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[2].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "FCL".Equals(assOrders[2].LegType) &&
                   "CHS".Equals(assOrders[3].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll &&
                    assOrders[3].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "3-Pickup Empty,PE";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    export9++;

                }
                //Import 10 - Import with chassis pickup and dropoff +  LU
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                   "CHS".Equals(assOrders[3].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll &&
                    assOrders[3].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "5-Pickup Loaded,PL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    import10++;
                }
                // Export 10 - Export with chassis pickup and dropoff
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "CHS".Equals(assOrders[2].LegType) &&
                    !assOrders[0].Ll &&
                    !assOrders[1].Ll &&
                    !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "5-Pickup Loaded,PL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    export10++;
                }
                // Import 11 - Import with chassis pickup and dropoff
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "CHS".Equals(assOrders[2].LegType) &&
                    !assOrders[0].Ll &&
                    !assOrders[1].Ll &&
                    !assOrders[2].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "1-Pickup Chassis,PC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "5-Pickup Loaded,PL";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    import11++;
                }
                // Export 5 - Export with LL, yard pull, and chassis drop
                else if ("CEX".Equals(assOrders[0].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[1].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[2].DispatchCategoryCode) &&
                    "CEX".Equals(assOrders[3].DispatchCategoryCode) &&
                        "ECL".Equals(assOrders[0].LegType) &&
                        "ECL".Equals(assOrders[1].LegType) &&
                        "FCL".Equals(assOrders[2].LegType) &&
                        "CHS".Equals(assOrders[3].LegType) &&
                    assOrders[0].Ll &&
                    assOrders[1].Ll &&
                    assOrders[2].Ll &&
                    assOrders[3].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "6-Drop Off Loaded,DL";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "6";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    export5++;
                }
                //Import 6 - Import with Chassis at End, LU and Hub Stop
                else if ("CIM".Equals(assOrders[0].DispatchCategoryCode) &&
                   "CIM".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CIM".Equals(assOrders[2].DispatchCategoryCode) &&
                   "CIM".Equals(assOrders[3].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
                    "ECL".Equals(assOrders[2].LegType) &&
                    "CHS".Equals(assOrders[3].LegType) &&
                   assOrders[0].Ll &&
                   assOrders[1].Ll &&
                   assOrders[2].Ll &&
                   assOrders[3].Ll)
                {
                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "1";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[0].PickupName;
                    tempVo.Address = assOrders[0].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "2";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                    tempVo.Location = assOrders[1].PickupName;
                    tempVo.Address = assOrders[1].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "12-Live Unloading,LU";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "6";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    import6++;
                }
                else
                {
                    Console.Out.WriteLine("\tBad 4 Order - " + tempJobNumber);
                    bad4OrdersCount++;
//                    badOrdersCount++;
                    badOrders.Add(tempJobNumber);
                }
            }
            else
            {
                unkownOrdersCount++;
            }

            return vescoOrders;
        }

        public static int getBadOrdersCount()
        {
            return bad1OrdersCount + 
                bad2OrdersCount + 
                bad3OrdersCount + 
                bad4OrdersCount + 
                unkownOrdersCount;

        }

        public static int getGoodVescoOrdersCount()
        {
            return SouthwestConverter.export1 +
                SouthwestConverter.export2 +
                SouthwestConverter.export2a +
                SouthwestConverter.export3 +
                SouthwestConverter.export3a +
                SouthwestConverter.import1 +
                SouthwestConverter.import2 +
                SouthwestConverter.import2a +
                SouthwestConverter.import3 +
                SouthwestConverter.import3a +
                SouthwestConverter.import4 +
                SouthwestConverter.cityEmptyRepo +
                SouthwestConverter.cityLoadedRepo +
                SouthwestConverter.export4 +
                SouthwestConverter.export5 +
                SouthwestConverter.export6 +
                SouthwestConverter.import5 +
                SouthwestConverter.import6 +
                SouthwestConverter.import7 +
                SouthwestConverter.export7 +
                SouthwestConverter.import8 +
                SouthwestConverter.export8 +
                SouthwestConverter.import9 +
                SouthwestConverter.export9 +
                SouthwestConverter.import10 +
                SouthwestConverter.export10 +
                SouthwestConverter.import11 +
                SouthwestConverter.chassis1 +
                SouthwestConverter.lude +
                SouthwestConverter.export11 +
                SouthwestConverter.export12;
        }


        private static string getTime(Double time)
        {
            if (time != 0)
            {
                DateTime conv = DateTime.FromOADate(time);
                return conv.TimeOfDay.ToString();
            }

            return null;
        }
    }
}
