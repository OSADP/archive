using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using PAI.CTIP.Optimization.Model.Orders;
using Vesco;

namespace VescoConsole
{

    public class Associated
    {

        int assRowCount;
        int assColCount;
        DateTime runDate;
        public Boolean isExecuted;
        String executedFileName;

        public static int EXPECTED_ASS_COL_COUNT = 30;

        List<Job> jobs = new List<Job>();
        List<String> badOrders = new List<String>();

        List<VescoOrder> vescoOrders = new List<VescoOrder>();
        List<List<TriniumOrder>> triniumOrderCollection = new List<List<TriniumOrder>>();
        List<ExecutedOrder> assExOrders = new List<ExecutedOrder>();

        private static JobHelper _helper = null;

        public static JobHelper Helper
        {
            get { return _helper ?? (_helper = new JobHelper()); }
        }

        public void processExecuted(string _fileName)
        {

            Excel.Application excelObj = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range assRange = null;
            object misValue = System.Reflection.Missing.Value;

            isExecuted = true;
            executedFileName = _fileName;

            try
            {
                workbook = excelObj.Workbooks.Open(_fileName);

                worksheet = (Excel.Worksheet)workbook.Sheets.Item[1];
                assRange = worksheet.UsedRange;
                assRowCount = assRange.Rows.Count;
                assColCount = assRange.Columns.Count;

                // Sort based upon the order number and then the leg number
                assRange.Sort(assRange.Columns[1, Type.Missing], // the first sort key - Order Number
                    Excel.XlSortOrder.xlAscending,
                    assRange.Columns[15, Type.Missing], // second sort key - Leg Number
                    Type.Missing, Excel.XlSortOrder.xlAscending,
                    Type.Missing, Excel.XlSortOrder.xlAscending, // this would be the third key
                    Excel.XlYesNoGuess.xlYes, // ignore the header
                    Type.Missing,
                    Type.Missing,
                    Excel.XlSortOrientation.xlSortColumns,
                    Excel.XlSortMethod.xlPinYin,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal);

                ExecutedOrder assExOrder;
                ExecutedOrder tempAssExOrder;

                for (int i = 2; i <= assRowCount; i++) // Start at the second row and skip the header
                {
                    assExOrder = new ExecutedOrder();
                    assExOrder.DeliverTo = assRange[i, 9].Value.ToString();
                    assExOrder.DeliveryAddress = assRange[i, 10].Value.ToString();
                    assExOrder.DeliveryCity = assRange[i, 11].Value.ToString();
                    assExOrder.dtDevliveryDateTime = DateTime.FromOADate(
                        Convert.ToDouble(assRange[i, 17].Value2) +
                        Convert.ToDouble(assRange[i, 18].Value2)
                    );
                    assExOrder.Driver = assRange[i, 19].Value.ToString();

                    // find if this driver has been added to the list
                    tempAssExOrder = assExOrders.Find(
                        delegate(ExecutedOrder aeo)
                        {
                            return aeo.Driver == assExOrder.Driver;
                        }
                    );

                    // add the driver if it hasn't been added to the list, yet
                    if (tempAssExOrder == null)
                    {
                        assExOrders.Add(assExOrder);
                    }
                    // remove the driver from the list if the deliveryDateTime is older than the new one
                    else if (tempAssExOrder.dtDevliveryDateTime < assExOrder.dtDevliveryDateTime)
                    {
                        assExOrders.Remove(tempAssExOrder);
                        assExOrders.Add(assExOrder);
                    }
                }

                Console.Out.WriteLine(assExOrders.Count);
                workbook.Close(false, misValue, misValue);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (workbook != null)
                {
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelObj != null)
                {
                    excelObj.Quit();
                    Marshal.ReleaseComObject(excelObj);
                }
            }
        }

        public void processAssDrop(string _fullPath)
        {
            VescoLog.LogEvent("processAssDrop, _fullPath=" + _fullPath);

            Boolean shouldContinue = false;

            Excel.Application excelObj = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range assRange = null;
            object misValue = System.Reflection.Missing.Value;

            System.Threading.Thread.Sleep(5000);

            try
            {
                workbook = excelObj.Workbooks.Open(_fullPath);

                int assOrdersCount = 0;

                worksheet = (Excel.Worksheet)workbook.Sheets.Item[1];
                assRange = worksheet.UsedRange;
                assRowCount = assRange.Rows.Count;
                assColCount = assRange.Columns.Count;

                if (assColCount != EXPECTED_ASS_COL_COUNT)
                {
                    String errMessage = _fullPath + " does not contain " + EXPECTED_ASS_COL_COUNT + 
                        " columns and can't be imported.  The most likely cause is an invlaid comma in the data.";
                    EmailHelper.sendGmail("Import Error", errMessage, false);
                    throw new Exception(errMessage);
                }

                runDate = getRunDate(_fullPath);

                // Sort based upon the order number and then the leg number
                assRange.Sort(assRange.Columns[1, Type.Missing], // the first sort key - Order Number
                    Excel.XlSortOrder.xlAscending,
                    assRange.Columns[15, Type.Missing], // second sort key - Leg Number
                    Type.Missing, Excel.XlSortOrder.xlAscending,
                    Type.Missing, Excel.XlSortOrder.xlAscending, // this would be the third key
                    Excel.XlYesNoGuess.xlYes, // ignore the header
                    Type.Missing,
                    Type.Missing,
                    Excel.XlSortOrientation.xlSortColumns,
                    Excel.XlSortMethod.xlPinYin,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal);

                List<VescoOrder> tempVescoOrders;
                List<TriniumOrder> assOrders = new List<TriniumOrder>();
                TriniumOrder assOrd;

                Double dblPickupDate;
                DateTime dtPickupDate;
                Double dblDeliveryDate;
                DateTime dtDeliveryDate;

                for (int i = 2; i <= assRowCount; i++) // Start at the second row and skip the header
                {
                    assOrd = new TriniumOrder();
                    assOrd.OrderNumber = assRange[i, 1].Value.ToString();
                    assOrd.DispatchCategoryCode = assRange[i, 2].Value.ToString();
                    assOrd.Sts = assRange[i, 3].Value.ToString();
                    assOrd.PickupName = assRange[i, 4].Value.ToString();
                    assOrd.PickupAddress = assRange[i, 5].Value.ToString();
                    assOrd.PickupCity = assRange[i, 6].Value.ToString();
                    assOrd.PickupState = assRange[i, 7].Value.ToString();
                    assOrd.PickupZip = (assRange[i, 8].Value != null ? assRange[i, 8].Value.ToString() : "");
                    assOrd.DeliverName = assRange[i, 9].Value.ToString();
                    assOrd.DeliverAddress = assRange[i, 10].Value.ToString();
                    assOrd.DeliverCity = assRange[i, 11].Value.ToString();
                    assOrd.DeliverState = assRange[i, 12].Value.ToString();
                    assOrd.DeliverZip = (assRange[i, 13].Value != null ? assRange[i, 13].Value.ToString() : "");
                    assOrd.LegType = assRange[i, 14].Value.ToString();
                    assOrd.LegNumber = assRange[i, 15].Value.ToString();

                    dblPickupDate = Convert.ToDouble(assRange[i, 16].Value2);
                    if (dblPickupDate != 0.0)
                    {
                        dtPickupDate = DateTime.FromOADate(dblPickupDate);
                        if (dtPickupDate.CompareTo(runDate) == 0 || dtPickupDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            assOrd.ScheduledPickupDate = dblPickupDate;
                            assOrd.ScheduledPickupTimeFrom = Convert.ToDouble(assRange[i, 17].Value2);
                            assOrd.ScheduledPickupTimeTo = Convert.ToDouble(assRange[i, 18].Value2);
                        }
                    }

                    dblDeliveryDate = Convert.ToDouble(assRange[i, 19].Value2);
                    if (dblDeliveryDate != 0.0)
                    {
                        dtDeliveryDate = DateTime.FromOADate(dblDeliveryDate);
                        if (dtDeliveryDate.CompareTo(runDate) == 0 || dtDeliveryDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            assOrd.ScheduledDeliverDate = dblDeliveryDate;
                            assOrd.ScheduledDeliverTimeFrom = Convert.ToDouble(assRange[i, 20].Value2);
                            assOrd.ScheduledDeliverTimeTo = Convert.ToDouble(assRange[i, 21].Value2);
                        }
                    }

                    assOrd.Ssl = (assRange[i, 22].Value != null ? assRange[i, 22].Value.ToString() : "");
                    assOrd.Size = assRange[i, 23].Value.ToString();
                    assOrd.Type = assRange[i, 24].Value.ToString();
                    assOrd.Ll = string.Equals(assRange[i, 25].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    assOrd.Haz = (assRange[i, 26].Value == "ü" || string.Equals(assRange[i, 26].Value, "yes", StringComparison.OrdinalIgnoreCase));
                    assOrd.SupplierCode = assRange[i, 27].Value.ToString();
                    assOrd.DispatchSequence = assRange[i, 28].Value.ToString();
                    assOrd.Overweight = string.Equals(assRange[i, 29].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    assOrd.ContainerNumber = (assRange[i, 30].Value != null ? assRange[i, 30].Value.ToString() : "");

                    assOrders.Add(assOrd);

                    // A new order number is coming up or this is the last record in the collection, add this job to the jobs list
                    if (i == assRowCount || assRange[i, 1].Value2.ToString() != assRange[i + 1, 1].Value2.ToString())
                    {
                        assOrdersCount++;

                        //Convert the Associated Order to a Vesco Order
                        tempVescoOrders = AssociatedConverter.Convert(assOrders);

                        if (tempVescoOrders.Count() == 0)
                        {
                            badOrders.Add(assOrd.OrderNumber);
                        }

                        //Add the converted orders
                        foreach (VescoOrder vo in tempVescoOrders)
                        {
                            vescoOrders.Add(vo);
                        }
                        triniumOrderCollection.Add(assOrders);
                        assOrders.Clear();
                    }
                }
                workbook.Close(false, misValue, misValue);
                shouldContinue = true;
            }
            catch (Exception ex)
            {
                VescoLog.LogEvent(ex.Message);
                VescoLog.LogEvent(ex.StackTrace);
            }
            finally
            {
                if (assRange != null)
                {
                    Marshal.ReleaseComObject(assRange);
                }
                if (worksheet != null)
                {
                    Marshal.ReleaseComObject(worksheet);
                }
                if (workbook != null)
                {
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelObj != null)
                {
                    excelObj.Quit();
                    Marshal.ReleaseComObject(excelObj);
                }
            }

            if (shouldContinue)
            {
                generateVescoSpreadSheet(_fullPath);
            }
        }

        private void generateVescoSpreadSheet(string _fullPath)
        {
            Excel.Application excelObj2 = new Excel.Application();
            Excel.Workbook vescoExcelWorkbook = null;
            Excel.Worksheet vescoExcelWorksheet = null;

            Excel.Worksheet locationWorkSheet = null;
            Excel.Range locationRange = null;

            Excel.Worksheet driverExcelWorksheet = null;
            Excel.Range driverRange = null;
            ExecutedOrder tempAssExOrder;

            try
            {
                vescoExcelWorkbook = excelObj2.Workbooks.Open(Properties.templateDir + "Associated Template.xlsx");

                if (assExOrders.Count > 0)
                {
                    driverExcelWorksheet = (Excel.Worksheet)vescoExcelWorkbook.Sheets.get_Item("Driver");
                    driverRange = driverExcelWorksheet.UsedRange;
                    int driverRowCount = driverRange.Rows.Count;
                    int driverCount = 0;
                    String tempDriverId;
                    DateTime tempDateTime;
                    DateTime earliestStartTime;
                    DateTime dayEndsAt;
                    double hoursElapsed;
                    double newDrivingHours;
                    double newDutyHours;

                    // loop through all the drivers
                    for (int i = 2; i <= driverRowCount; i++)  // Start at the second row and skip the header
                    {
                        tempDriverId = driverRange[i, 1].Value2.ToString();
                        // find if this driver has been added to the list
                        tempAssExOrder = assExOrders.Find(
                            delegate(ExecutedOrder aeo)
                            {
                                return aeo.Driver == tempDriverId;
                            }
                        );
                        if (tempAssExOrder != null)
                        {
                            tempDateTime = DateTime.FromOADate(driverRange[i, 4].Value2);
                            earliestStartTime = tempAssExOrder.dtDevliveryDateTime.Date.AddHours(tempDateTime.Hour);
                            dayEndsAt = earliestStartTime.AddHours(driverRange[i, 3].Value2);
                            hoursElapsed = tempAssExOrder.dtDevliveryDateTime.Subtract(earliestStartTime).TotalHours;
                            newDrivingHours = Convert.ToDouble(driverRange[i, 2].Value2) - hoursElapsed;
                            newDrivingHours = newDrivingHours > 0 ? newDrivingHours : 0.0;
                            newDutyHours = Convert.ToDouble(driverRange[i, 3].Value2) - hoursElapsed;
                            newDutyHours = newDutyHours > 0 ? newDutyHours : 0.0;

                            Console.WriteLine("Earliest Start Time -> " + earliestStartTime);
                            Console.WriteLine("Available Driving Hours -> " + driverRange[i, 2].Value2);
                            Console.WriteLine("Available Duty Hours -> " + driverRange[i, 3].Value2);
                            Console.WriteLine("Last Delivery Time -> " + tempAssExOrder.dtDevliveryDateTime);
                            Console.WriteLine("\tHours Elapsed -> " + hoursElapsed);
                            Console.WriteLine("\tNew Earliest Start Time -> " + tempAssExOrder.dtDevliveryDateTime);
                            Console.WriteLine("\tNew Driving Hours -> " + newDrivingHours);
                            Console.WriteLine("\tNew Duty Hours -> " + newDutyHours);
                            Console.WriteLine();

                            driverExcelWorksheet.Cells[i, 2] = newDrivingHours;
                            driverExcelWorksheet.Cells[i, 3] = newDutyHours;
                            driverExcelWorksheet.Cells[i, 4] = tempAssExOrder.dtDevliveryDateTime;
                            driverExcelWorksheet.Cells[i, 5] = tempAssExOrder.DeliverTo;
                            driverCount++;
                            assExOrders.Remove(tempAssExOrder);
                        }
                    }
                }

                VescoLog.LogEvent(vescoOrders.Count().ToString());
                List<String> badAssociatedOrders = AssociatedConverter.badOrders;
                foreach (String b in badAssociatedOrders)
                {
                    Console.Out.WriteLine("Bad Associated Order#," + b);
                }
                VescoLog.LogEvent(AppDomain.CurrentDomain.BaseDirectory);

                vescoExcelWorksheet = (Excel.Worksheet)vescoExcelWorkbook.Sheets.get_Item("RouteStop");

                //Location based stop delay
                locationWorkSheet = (Excel.Worksheet)vescoExcelWorkbook.Sheets.get_Item("Location");
                locationRange = locationWorkSheet.UsedRange;
                int locationRowCount = locationRange.Rows.Count;
                List<SpecialLocation> specialLocations = new List<SpecialLocation>();
                SpecialLocation specialLocation;

                for (int i = 2; i <= locationRowCount; i++)  // Start at the second row and skip the header
                {
                    if (locationRange[i, 6].Value2 != null)
                    {
                        specialLocation = new SpecialLocation();
                        specialLocation.Name = locationRange[i, 1].Value2.ToString();
                        specialLocation.Address = locationRange[i, 2].Value2.ToString();
                        specialLocation.StopDelay = locationRange[i, 6].Value2.ToString();
                        specialLocations.Add(specialLocation);
                    }
                }

                specialLocation = new SpecialLocation();
                String stopActionCode;
                String windowStart = null;

                int row = 2;
                //Add the converted orders
                foreach (VescoOrder vo in vescoOrders)
                {
                    vescoExcelWorksheet.Cells[row, 1] = vo.Job;
                    vescoExcelWorksheet.Cells[row, 2] = vo.Sequence;
                    vescoExcelWorksheet.Cells[row, 3] = vo.StopAction;
                    vescoExcelWorksheet.Cells[row, 4] = vo.Location;
                    vescoExcelWorksheet.Cells[row, 5] = vo.Address;
                    specialLocation = specialLocations.Find(item => item.Name == vo.Location && item.Address == vo.Address);
                    stopActionCode = vo.StopAction.Split('-')[0];

                    if ("11".Equals(stopActionCode) || "12".Equals(stopActionCode))
                    {
                        if (specialLocation != null)
                        {
                            vescoExcelWorksheet.Cells[row, 6] = specialLocation.StopDelay;
                        }
                        else
                        {
                            vescoExcelWorksheet.Cells[row, 6] = "60";
                        }
                    }
                    else
                    {
                        vescoExcelWorksheet.Cells[row, 6] = vo.StopDelay;
                    }

                    if (vo.WindowStart != null)
                    {
                        windowStart = (vo.WindowStart.Equals(vo.WindowEnd) ? null : vo.WindowStart);
                    }
                    else
                    {
                        windowStart = null;
                    }

                    vescoExcelWorksheet.Cells[row, 7] = windowStart;
                    vescoExcelWorksheet.Cells[row, 8] = vo.WindowEnd;
                    vescoExcelWorksheet.Cells[row, 9] = vo.Hazardous;
                    vescoExcelWorksheet.Cells[row, 10] = vo.Overweight;
                    vescoExcelWorksheet.Cells[row, 11] = vo.OriginalLegNumber;
                    vescoExcelWorksheet.Cells[row, 12] = vo.ContainerNumber;
                    vescoExcelWorksheet.Cells[row, 13] = vo.SteamshipLine;
                    vescoExcelWorksheet.Cells[row, 14] = vo.LiveLoad;
                    vescoExcelWorksheet.Cells[row, 15] = vo.OriginalLegType;
                    vescoExcelWorksheet.Cells[row, 16] = vo.DispatcherSequence;
                    row++;
                }

                // Delete the Vesco file first since I can't figure out how to get rid of the confirmation dialog
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\Templates\\Vesco.xlsx");
                vescoExcelWorkbook.SaveAs(
                    AppDomain.CurrentDomain.BaseDirectory +
                    "Templates\\Vesco.xlsx",
                    Excel.XlFileFormat.xlWorkbookDefault, Type.Missing,
                    Type.Missing, false, false,
                    Excel.XlSaveAsAccessMode.xlNoChange,
                    Excel.XlSaveConflictResolution.xlLocalSessionChanges,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            catch (Exception ex)
            {
                VescoLog.LogEvent(ex.StackTrace);
            }
            finally
            {
                if (vescoExcelWorksheet != null)
                {
                    Marshal.ReleaseComObject(vescoExcelWorksheet);
                }
                if (vescoExcelWorkbook != null)
                {
                    Marshal.ReleaseComObject(vescoExcelWorkbook);
                }
                if (excelObj2 != null)
                {
                    excelObj2.Quit();
                    Marshal.ReleaseComObject(excelObj2);
                }
            }

            String path = _fullPath + (isExecuted ? "," + executedFileName : "");
            Vesco vesco = new Vesco(isExecuted, vescoOrders, badOrders, path, "Associated");

            vesco.performSteps();
        }

        private DateTime getRunDate(string _fullPath)
        {
            string[] splits = _fullPath.Split('_');
            string sDate = "";
            foreach (string s in splits)
            {
                if (s.Count() == 8)
                {
                    sDate = s;
                    break;
                }
            }

            Int32 year = Convert.ToInt32(sDate.Substring(0, 4));
            Int32 month = Convert.ToInt32(sDate.Substring(4, 2));
            Int32 day = Convert.ToInt32(sDate.Substring(6, 2));

            return new DateTime(year, month, day);
        }
    }
}
