using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vesco
{
    class AssociatedConverter
    {
        public static int export1 = 0;
        public static int export2 = 0;
        public static int export3 = 0;
        public static int export4 = 0;
        public static int export5 = 0;
        public static int export6 = 0;
        public static int export7 = 0;
        public static int export8 = 0;
        public static int export9 = 0;
        public static int owl1 = 0;
        public static int owl2 = 0;
        public static int export10 = 0;
        public static int export11 = 0;
        public static int export12 = 0;
        public static int export13 = 0;
        public static int import1 = 0;
        public static int import2 = 0;
        public static int import3 = 0;
        public static int import4 = 0;
        public static int import5 = 0;
        public static int import6 = 0;
        public static int import7 = 0;
        public static int import7a = 0;
        public static int owe1 = 0;
        public static int import8 = 0;
        public static int import9 = 0;
        public static int import10 = 0;
        public static int import11 = 0;
        public static int import13 = 0;
        public static int import14 = 0;

        public static int chassisMove = 0;

        public static int totalOrdersCount = 0;
        public static int badOrdersCount = 0;
        public static int bad1OrdersCount = 0;
        public static int bad2OrdersCount = 0;
        public static int bad3OrdersCount = 0;
        public static int bad4OrdersCount = 0;
        public static int unkownOrdersCount = 0;


        public static List<String> badOrders = new List<String>();

        public static List<VescoOrder> Convert(List<TriniumOrder> assOrders) {

//            Console.Out.WriteLine("Order => " + assOrders[0].OrderNumber + ", Order Size => " + assOrders.Count());
            List<VescoOrder> vescoOrders = new List<VescoOrder>();
            VescoOrder tempVo;
            String tempJobNumber = assOrders[0].OrderNumber;
            totalOrdersCount++;

            if (assOrders.Count() == 1) {

                //Owe 1 - One Way Empty
                if ("OWE".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    owe1++;
                }

                //owl1 - One Way Load
                else if ("OWL".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    owl1++;
                }
                // Chassis Move
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    chassisMove++;
                }
                //Export 6 - Export load, L/L complete
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) && 
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

                    export6++;
                }
                //Export 7 - Export empty, L/L complete
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    export7++;
                }
                //Export 8 - Export load no L/L
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    export8++;
                }
                //Export 5 - Export empty
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    export5++;
                }
                //Import 4 - Import LU complete
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    import4++;
                }
                //Import 5 - Import Empty
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    import5++;
                }

                //Import 2 - Loaded import
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    import2++;
                }
                //Import 7a - Import yard pull only
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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

                    import7a++;
                }
                //DEFAULT - Import with Live Unload 2
                //else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                //         "FCL".Equals(assOrders[0].LegType) &&
                //         assOrders[0].Ll)
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
                //    tempVo.StopAction = "12-Live Unloading,LU";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    defaultImportWithLu2++;
                //}
                else
                {
                    Console.Out.WriteLine("\tBad 1 Order - " + tempJobNumber);
                    badOrdersCount++;
                    bad1OrdersCount++;
                    badOrders.Add(tempJobNumber);
                }

            } 
            else if (assOrders.Count() == 2)
            {

                //Export 1 - Export with Live Load
                if ("ED".Equals(assOrders[0].DispatchCategoryCode) && 
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
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

                //Export 4 - Export with LL both legs FCL
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
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

                    export4++;
                }

                //Export 9 - Export with yard pull
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
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

                    export9++;
                }

                    
                // Import 1 - Import with Live Unload
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
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
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    import1++;
                }
                ////DEFAULT - Export
                //else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                //  "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                //  "ECL".Equals(assOrders[0].LegType) &&
                //  "FCL".Equals(assOrders[1].LegType) &&
                //    !assOrders[0].Ll && 
                //    !assOrders[1].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                //    tempVo.Location = assOrders[1].PickupName;
                //    tempVo.Address = assOrders[1].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                //    tempVo.Location = assOrders[1].DeliverName;
                //    tempVo.Address = assOrders[1].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    defaultExport++;
                //}
                //DEFAULT - Import
                //else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                //  "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                //  "FCL".Equals(assOrders[0].LegType) &&
                //  "ECL".Equals(assOrders[1].LegType) &&
                //    !assOrders[0].Ll && 
                //    !assOrders[1].Ll)
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

                //    defaultImport++;
                //}
                //Owl 2 - one way load with LU
                else if ("OWL".Equals(assOrders[0].DispatchCategoryCode) &&
                  "FCL".Equals(assOrders[0].LegType) &&
                  "ECL".Equals(assOrders[1].LegType) &&
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
                    tempVo.StopAction = "12-Live Unloading";
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
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    owl2++;
                }
                // Import 13 - Import with chassis move and LU completed
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) && 
                    "ECL".Equals(assOrders[0].LegType) &&
                    "CHS".Equals(assOrders[1].LegType) &&
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
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    import13++;
                }
                // Import 6 - Import Yard Pull
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
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

                    import6++;
                }
                // Import 14 - Full import with return empty
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                  "FCL".Equals(assOrders[0].LegType) &&
                  "ECL".Equals(assOrders[1].LegType) &&
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
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    import14++;
                }
                else
                {
                    Console.Out.WriteLine("\tBad 2 Order - " + tempJobNumber);
                    badOrdersCount++;
                    bad2OrdersCount++;
                    badOrders.Add(tempJobNumber);
                }

            } else if (assOrders.Count() == 3) {

                //Export 10 - Export with LL and chassis drop
                if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
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
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
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

                    export10++;
                }
                // Import 8 - Import with Live Unload and chassis drop
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
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

                    import8++;
                }
                ////DROPPING OFF CHASSIS AT THE END - Export
                //else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                //    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                //    "ECL".Equals(assOrders[0].LegType) &&
                //    "FCL".Equals(assOrders[1].LegType) &&
                //    "CHS".Equals(assOrders[2].LegType) &&
                //    !assOrders[0].Ll && 
                //    !assOrders[1].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "7-Pickup Empty With Chassis,PEWC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                //    tempVo.Location = assOrders[1].PickupName;
                //    tempVo.Address = assOrders[1].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "6-Drop Off Loaded,DL";
                //    tempVo.Location = assOrders[2].PickupName;
                //    tempVo.Address = assOrders[2].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "5";
                //    tempVo.StopAction = "2-Drop Off Chassis,DC";
                //    tempVo.Location = assOrders[2].DeliverName;
                //    tempVo.Address = assOrders[2].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    docateExport++;
                //}

                // Import 9 - Import with chassis drop
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
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

                    import9++;
                }

                // Export 11 - Export with chassis pickup and LL
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
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

                    export11++;
                }

                //PICKING UP CHASSIS AT THE START - Export with LU
                //else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                //    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                //    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
                //    "CHS".Equals(assOrders[0].LegType) &&
                //    "FCL".Equals(assOrders[1].LegType) &&
                //    "ECL".Equals(assOrders[2].LegType) &&
                //    assOrders[0].Ll && 
                //    assOrders[1].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "1-Pickup Chassis,PC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "5-Pickup Loaded,PL";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "12-Live Unload,LU";
                //    tempVo.Location = assOrders[2].PickupName;
                //    tempVo.Address = assOrders[2].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[2].DeliverName;
                //    tempVo.Address = assOrders[2].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    pucatsImportWithLu++;
                //}
                ////PICKING UP CHASSIS AT THE START - Export
                //else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                //    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                //    "CHS".Equals(assOrders[0].LegType) &&
                //    "ECL".Equals(assOrders[1].LegType) &&
                //    "FCL".Equals(assOrders[2].LegType) &&
                //    !assOrders[1].Ll && 
                //    !assOrders[2].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "1-Pickup Chassis,PC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "3-Pickup Empty,PE";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[1].PickupName;
                //    tempVo.Address = assOrders[1].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                //    tempVo.Location = assOrders[2].PickupName;
                //    tempVo.Address = assOrders[2].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "5";
                //    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                //    tempVo.Location = assOrders[2].DeliverName;
                //    tempVo.Address = assOrders[2].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    pucatsExport++;
                //}
                // Import 10 - Import with chassis pickup and LU
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "FCL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                    assOrders[1].Ll &&
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
                    tempVo.StopAction = "12-Live Unloading";
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

                    import10++;
                }
                //Export 3 - Export with 2 LL
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[2].DispatchCategoryCode) &&
                    "ECL".Equals(assOrders[0].LegType) &&
                    "FCL".Equals(assOrders[1].LegType) &&
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
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[2].PickupName;
                    tempVo.Address = assOrders[2].PickupAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledPickupTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledPickupTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
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

                    export3++;
                }
                //Import 3 - Import with 2 Lu
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
                    "FCL".Equals(assOrders[0].LegType) &&
                    "ECL".Equals(assOrders[1].LegType) &&
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
                    tempVo.StopAction = "12-Live Unloading";
                    tempVo.Location = assOrders[0].DeliverName;
                    tempVo.Address = assOrders[0].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "12-Live Unloading";
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

                    import3++;
                }

                //Import 7 - Import with LU and yard pull
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.StopAction = "12-Live Unloading";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
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

                    import7++;
                }

                // Import with LU 5 - Hub Stop
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.StopAction = "0-No Action,NA";
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
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);
                }
                //Export 2 - Export with LL and yard pull
                // Very Possibly this might have to get split into two seperate orders.
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[2].DispatchCategoryCode) &&
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
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[1]);
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
                else
                {
                    Console.Out.WriteLine("\tBad 3 Order - " + tempJobNumber);
                    bad3OrdersCount++;
                    badOrdersCount++;
                    badOrders.Add(tempJobNumber);
                }
            }
            else if (assOrders.Count() == 4)
            {
                // Export 12 - Export with chassis pickup, LL, chassis drop off
                if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[2].DispatchCategoryCode) &&
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

                    export13++;

                }
                // Import 11 - Import with chassis pickup, dropoff and LU
                else if ("ID".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[2].DispatchCategoryCode) &&
                    "ID".Equals(assOrders[3].DispatchCategoryCode) &&
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
                    tempVo.addCarryOvers(assOrders[0]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "3";
                    tempVo.StopAction = "12-Live Unload,LU";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.addCarryOvers(assOrders[1]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "4";
                    tempVo.StopAction = "4-Drop Off Empty,DE";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "5";
                    tempVo.StopAction = "2-Drop Off Chassis,DC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    import11++;
                }
                //// PICKING UP AND DROPPING OFF CHASSIS - Export
                //else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                //    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                //    "CHS".Equals(assOrders[0].LegType) &&
                //    "ECL".Equals(assOrders[1].LegType) &&
                //    "FCL".Equals(assOrders[2].LegType) &&
                //    "CHS".Equals(assOrders[3].LegType) &&
                //    !assOrders[1].Ll && 
                //    !assOrders[2].Ll)
                //{
                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "1";
                //    tempVo.StopAction = "1-Pickup Chassis,PC";
                //    tempVo.Location = assOrders[0].PickupName;
                //    tempVo.Address = assOrders[0].PickupAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledPickupTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledPickupTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "2";
                //    tempVo.StopAction = "3-Pickup Empty,PE";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "3";
                //    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                //    tempVo.Location = assOrders[0].DeliverName;
                //    tempVo.Address = assOrders[0].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[0].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[0].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[0]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "4";
                //    tempVo.StopAction = "9-Pickup Loaded With Chassis,PLWC";
                //    tempVo.Location = assOrders[1].DeliverName;
                //    tempVo.Address = assOrders[1].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[1]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "5";
                //    tempVo.StopAction = "6-Drop Off Loaded,DL";
                //    tempVo.Location = assOrders[2].DeliverName;
                //    tempVo.Address = assOrders[2].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[2]);
                //    vescoOrders.Add(tempVo);

                //    tempVo = new VescoOrder();
                //    tempVo.Job = tempJobNumber;
                //    tempVo.Sequence = "6";
                //    tempVo.StopAction = "2-Drop Off Chassis,DC";
                //    tempVo.Location = assOrders[3].DeliverName;
                //    tempVo.Address = assOrders[3].DeliverAddress;
                //    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                //    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                //    tempVo.addCarryOvers(assOrders[3]);
                //    vescoOrders.Add(tempVo);

                //    puadocExport++;
                //}

                // Export 12 - Export with chassis pickup, yard pull and L/L
                else if ("ED".Equals(assOrders[0].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[1].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[2].DispatchCategoryCode) &&
                    "ED".Equals(assOrders[3].DispatchCategoryCode) &&
                   "CHS".Equals(assOrders[0].LegType) &&
                   "ECL".Equals(assOrders[1].LegType) &&
                   "ECL".Equals(assOrders[2].LegType) &&
                   "FCL".Equals(assOrders[3].LegType) &&
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
                    tempVo.StopAction = "8-Drop Off Empty With Chassis,DEWC";
                    tempVo.Location = assOrders[1].DeliverName;
                    tempVo.Address = assOrders[1].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[1].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[1].ScheduledDeliverTimeTo);
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
                    tempVo.StopAction = "11-Live Loading,LL";
                    tempVo.Location = assOrders[2].DeliverName;
                    tempVo.Address = assOrders[2].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[2].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[2].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[2]);
                    vescoOrders.Add(tempVo);

                    tempVo = new VescoOrder();
                    tempVo.Job = tempJobNumber;
                    tempVo.Sequence = "6";
                    tempVo.StopAction = "10-Drop Off Loaded With Chassis,DLWC";
                    tempVo.Location = assOrders[3].DeliverName;
                    tempVo.Address = assOrders[3].DeliverAddress;
                    tempVo.WindowStart = getTime(assOrders[3].ScheduledDeliverTimeFrom);
                    tempVo.WindowEnd = getTime(assOrders[3].ScheduledDeliverTimeTo);
                    tempVo.addCarryOvers(assOrders[3]);
                    vescoOrders.Add(tempVo);

                    export12++;
                } 

                else
                {
                    Console.Out.WriteLine("\tBad 4 Order - " + tempJobNumber);
                    bad4OrdersCount++;
                    badOrdersCount++;
                    badOrders.Add(tempJobNumber);
                }
            }
            else
            {
                unkownOrdersCount++;
            }

            return vescoOrders;
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
