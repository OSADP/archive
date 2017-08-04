using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;



namespace Vesco
{
    public partial class SouthWest : Form
    {

        Boolean isPmFileLoaded = false;
        Boolean isCosFileLoaded = false;

        List<String> badOrders = new List<String>();
        List<VescoOrder> vescoOrders = new List<VescoOrder>();
        List<ExecutedOrder> swExOrders = new List<ExecutedOrder>();

        int pmRowCount;
        int pmColCount;
        int cosRowCount;
        int cosColCount;
        DateTime runDate;

        public static int EXPECTED_PM_COL_COUNT = 30;
        public static int EXPECTED_COS_COL_COUNT = 30;

        object misValue = System.Reflection.Missing.Value;

        public SouthWest()
        {
            InitializeComponent();
            this.Height = 302;
        }

        private void btnExecuted_Click(object sender, EventArgs e)
        {
            openFileDialog3 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog3.Filter =
                "Excel Files (fratis*.xlsx, fratis*.xls, fratis*.csv)|fratis*.xlsx;fratis*.xls;fratis*.csv|" +
                "All Files (*.*)|*.*";

            openFileDialog3.FilterIndex = 1;

            Excel.Application excelObj = new Excel.Application();
            Excel.Workbook executedWorkbook = null;
            Excel.Worksheet executedWorksheet = null;
            Excel.Range swExRange = null;

            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (openFileDialog3.OpenFile() != null)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        label9.Text = Path.GetFileName(openFileDialog3.FileName);
                        executedWorkbook = excelObj.Workbooks.Open(openFileDialog3.FileName);

                        executedWorksheet = (Excel.Worksheet)executedWorkbook.Sheets.Item[1];
                        swExRange = executedWorksheet.UsedRange;
                        int swExCount = swExRange.Rows.Count;

                        label6.Text = (swExCount - 1).ToString();

                        // Sort based upon the order number and then the leg number
                        swExRange.Sort(swExRange.Columns[1, Type.Missing], // the first sort key - Order Number
                            Excel.XlSortOrder.xlAscending,
                            swExRange.Columns[15, Type.Missing], // second sort key - Leg Number
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

                        ExecutedOrder swExOrder;
                        ExecutedOrder tempSwExOrder;

                        for (int i = 2; i <= swExCount; i++) // Start at the second row and skip the header
                        {
                            swExOrder = new ExecutedOrder();
                            swExOrder.DeliverTo = swExRange[i, 9].Value.ToString();
                            swExOrder.DeliveryAddress = swExRange[i, 10].Value.ToString();
                            swExOrder.DeliveryCity = swExRange[i, 11].Value.ToString();
                            swExOrder.dtDevliveryDateTime = DateTime.FromOADate(
                                Convert.ToDouble(swExRange[i, 17].Value2) +
                                Convert.ToDouble(swExRange[i, 18].Value2)
                            );
                            swExOrder.Driver = swExRange[i, 19].Value.ToString();

                            // find if this driver has been added to the list
                            tempSwExOrder = swExOrders.Find(
                                delegate(ExecutedOrder aeo)
                                {
                                    return aeo.Driver == swExOrder.Driver;
                                }
                            );

                            // add the driver if it hasn't been added to the list, yet
                            if (tempSwExOrder == null)
                            {
                                swExOrders.Add(swExOrder);
                            }
                            // remove the driver from the list if the deliveryDateTime is older than the new one
                            else if (tempSwExOrder.dtDevliveryDateTime < swExOrder.dtDevliveryDateTime)
                            {
                                swExOrders.Remove(tempSwExOrder);
                                swExOrders.Add(swExOrder);
                            }
                        }

                        executedWorkbook.Close(false, misValue, misValue);
                        Console.Out.WriteLine(swExOrders.Count);
                    }

                    if (swExOrders.Count > 0)
                    {
                        btnLoadPlannedMoves.Enabled = true;
                        btnLoadCos.Enabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read " + label9.Text + ". Original error: " + ex.Message);
                    Console.Out.WriteLine(ex.StackTrace);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    if (executedWorkbook != null)
                    {
                        Marshal.ReleaseComObject(executedWorkbook);
                    }
                    if (excelObj != null)
                    {
                        excelObj.Quit();
                        Marshal.ReleaseComObject(excelObj);
                    }
                }
            }
        }

        private void btnLoadPlannedMoves_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Excel Files (saic optimization*.xlsx, saic optimization*.xls, saic optimization*.csv)|" + 
                "saic optimization*.xlsx;saic optimization*.xls;saic optimization*.csv";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                Excel.Application excelObj = new Excel.Application();
                Excel.Workbook workbook = null;

                try
                {
                    if (openFileDialog1.OpenFile() != null)
                    {
                        this.Cursor = Cursors.WaitCursor;

                        label7.Text = Path.GetFileName(openFileDialog1.FileName);
                        workbook = excelObj.Workbooks.Open(openFileDialog1.FileName);

                        loadPlannedMoves(workbook);

                        isPmFileLoaded = true;
                        this.Cursor = Cursors.Default;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read " + label7.Text + ". Original error: " + ex.Message);
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

            shouldContinue();
        }

        private void loadPlannedMoves(Excel.Workbook workbook)
        {
            Excel.Worksheet worksheet = null;
            Excel.Range pmRange = null;

            int pmOrdersCount = 0;

            try
            {
                worksheet = (Excel.Worksheet)workbook.Sheets.Item[1];
                pmRange = worksheet.UsedRange;
                pmRowCount = pmRange.Rows.Count;
                pmColCount = pmRange.Columns.Count;


                if (pmColCount != EXPECTED_PM_COL_COUNT)
                {
                    throw new Exception("File does not contain " + EXPECTED_PM_COL_COUNT + " columns and can't be imported");
                }
                label5.Text = (pmRowCount - 1).ToString();
                runDate = getRunDate();
                
                // Sort based upon the order number and then the leg number
                pmRange.Sort(pmRange.Columns[1, Type.Missing], // the first sort key - Order Number
                    Excel.XlSortOrder.xlAscending,
                    pmRange.Columns[15, Type.Missing], // second sort key - Leg Number
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
                List<TriniumOrder> pmOrders = new List<TriniumOrder>();
                TriniumOrder pmOrd;

                Double dblPickupDate;
                DateTime dtPickupDate;
                Double dblDeliveryDate;
                DateTime dtDeliveryDate;

                for (int i = 2; i <= pmRowCount; i++) // Start at the second row and skip the header
                {
                    pmOrd = new TriniumOrder();
                    pmOrd.OrderNumber = pmRange[i, 1].Value.ToString();
                    pmOrd.DispatchCategoryCode = pmOrd.OrderNumber.Substring(0, 3);  //the first 3 digits of the order number
                    pmOrd.Ssl = pmRange[i, 22].Value.ToString();
                    pmOrd.LegType = pmRange[i, 14].Value.ToString();
                    pmOrd.Haz = string.Equals(pmRange[i, 26].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    pmOrd.LegNumber = pmRange[i, 15].Value.ToString();
                    pmOrd.PickupName = pmRange[i, 4].Value.ToString();
                    pmOrd.PickupAddress = pmRange[i, 5].Value.ToString();
                    pmOrd.PickupCity = pmRange[i, 6].Value.ToString();
                    pmOrd.PickupState = pmRange[i, 7].Value.ToString();
                    pmOrd.PickupZip = pmRange[i, 8].Value.ToString();
                    pmOrd.DeliverName = pmRange[i, 9].Value.ToString();
                    pmOrd.DeliverAddress = pmRange[i, 10].Value.ToString();
                    pmOrd.DeliverCity = pmRange[i, 11].Value.ToString();
                    pmOrd.DeliverState = pmRange[i, 12].Value.ToString();
                    pmOrd.DeliverZip = pmRange[i, 13].Value.ToString();
                    pmOrd.Ll = string.Equals(pmRange[i, 25].Value, "yes", StringComparison.OrdinalIgnoreCase);

                    dblPickupDate = Convert.ToDouble(pmRange[i, 16].Value2);
                    if (dblPickupDate != 0.0)
                    {
                        dtPickupDate = DateTime.FromOADate(dblPickupDate);
                        if (dtPickupDate.CompareTo(runDate) == 0 || dtPickupDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            pmOrd.ScheduledPickupDate = dblPickupDate;
                            pmOrd.ScheduledPickupTimeFrom = Convert.ToDouble(pmRange[i, 17].Value2);
                            pmOrd.ScheduledPickupTimeTo = Convert.ToDouble(pmRange[i, 18].Value2);
                        }
                    }

                    dblDeliveryDate = Convert.ToDouble(pmRange[i, 19].Value2);
                    if (dblDeliveryDate != 0.0)
                    {
                        dtDeliveryDate = DateTime.FromOADate(dblDeliveryDate);
                        if (dtDeliveryDate.CompareTo(runDate) == 0 || dtDeliveryDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            pmOrd.ScheduledDeliverDate = dblDeliveryDate;
                            pmOrd.ScheduledDeliverTimeFrom = Convert.ToDouble(pmRange[i, 20].Value2);
                            pmOrd.ScheduledDeliverTimeTo = Convert.ToDouble(pmRange[i, 21].Value2);
                        }
                    }

                    pmOrd.DispatchSequence = pmRange[i, 28].value.ToString();
                    pmOrd.Overweight = string.Equals(pmRange[i, 29].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    pmOrd.ContainerNumber = (pmRange[i, 30].Value != null ? pmRange[i, 30].Value.ToString() : "");

                    pmOrders.Add(pmOrd);

                    // A new order number is coming up or this is the last record in the collection, add this job to the jobs list
                    if (i == pmRowCount || pmRange[i, 1].Value2.ToString() != pmRange[i + 1, 1].Value2.ToString())
                    {
                        pmOrdersCount++;

                        //Convert the Associated Order to a Vesco Order
                        tempVescoOrders = SouthwestConverter.Convert(pmOrders);

                        if (tempVescoOrders.Count() == 0)
                        {
                            badOrders.Add(pmOrd.OrderNumber);
                        }

                        //Add the converted orders
                        foreach (VescoOrder vo in tempVescoOrders)
                        {
                            vescoOrders.Add(vo);
                        }

                        pmOrders.Clear();
                    }
                }

                workbook.Close(false, misValue, misValue);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Invalid Southwest Planned Moves file, " + label7.Text,
		            MessageBoxButtons.OK,
		            MessageBoxIcon.Exclamation,
		            MessageBoxDefaultButton.Button1
                );
                Console.Out.WriteLine(ex.StackTrace);
                throw new Exception("File does not contain " + EXPECTED_PM_COL_COUNT + " columns and can't be imported");
            }
            finally
            {
                if (pmRange != null)
                {
                    Marshal.ReleaseComObject(pmRange);
                }
                if (worksheet != null)
                {
                    Marshal.ReleaseComObject(worksheet);
                }
            }

            Console.Out.WriteLine(Environment.NewLine);
            Console.Out.WriteLine("Total Number Planned Moves Rows Imported => " + (pmRowCount - 1).ToString());
            Console.Out.WriteLine("Total Number of Planned Moves => " + pmOrdersCount.ToString());
            Console.Out.WriteLine("Total Number of Good Southwest Orders => " + SouthwestConverter.getGoodVescoOrdersCount());
            Console.Out.WriteLine("Total Number of Bad Southwest Orders => " + SouthwestConverter.getBadOrdersCount());
        }

        private void btnLoadCos_Click(object sender, EventArgs e)
        {
            openFileDialog2 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog2.Filter = "Excel Files (cos cty fratis*.xlsx, cos cty fratis*.xls, cos cty fratis*.csv)|cos cty fratis*.xlsx;cos cty fratis*.xls;cos cty fratis*.csv;";
            openFileDialog2.FilterIndex = 1;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {


                Excel.Application excelObj = new Excel.Application();
                Excel.Workbook workbook = null;

                try
                {
                    if (openFileDialog2.OpenFile() != null)
                    {
                        this.Cursor = Cursors.WaitCursor;

                        label1.Text = Path.GetFileName(openFileDialog2.FileName);
                        workbook = excelObj.Workbooks.Open(openFileDialog2.FileName);

                        loadCosFile(workbook);

                        isCosFileLoaded = true;
                        this.Cursor = Cursors.Default;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read " + label1.Text + ". Original error: " + ex.Message);
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

            shouldContinue();
        }

        private void loadCosFile(Excel.Workbook workbook)
        {
            Excel.Worksheet worksheet = null;
            Excel.Range cosRange = null;

            int cosOrdersCount = 0;

            try
            {
                worksheet = (Excel.Worksheet)workbook.Sheets.Item[1];
                cosRange = worksheet.UsedRange;
                cosRowCount = cosRange.Rows.Count;
                cosColCount = cosRange.Columns.Count;

                if (cosColCount != EXPECTED_COS_COL_COUNT)
                {
                    throw new Exception("File does not contain " + EXPECTED_COS_COL_COUNT + " columns and can't be imported");
                }

                label3.Text = (cosRowCount - 1).ToString();
//                runDate = getRunDate();

                // Sort based upon the order number and then the leg number
                cosRange.Sort(cosRange.Columns[1, Type.Missing], // the first sort key - Order Number
                    Excel.XlSortOrder.xlAscending,
                    cosRange.Columns[15, Type.Missing], // second sort key - Leg Number
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
                List<TriniumOrder> cosOrders = new List<TriniumOrder>();
                TriniumOrder cosOrd;

                Double dblPickupDate;
                DateTime dtPickupDate;
                Double dblDeliveryDate;
                DateTime dtDeliveryDate;

                for (int i = 2; i <= cosRowCount; i++) // Start at the second row and skip the header
                {
                    cosOrd = new TriniumOrder();
                    cosOrd.OrderNumber = cosRange[i, 1].Value.ToString();
                    cosOrd.DispatchCategoryCode = cosOrd.OrderNumber.Substring(0, 3);
                    cosOrd.Ssl = cosRange[i, 22].Value.ToString();
                    cosOrd.LegType = cosRange[i, 14].Value.ToString();
                    cosOrd.Haz = string.Equals(cosRange[i, 26].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    cosOrd.LegNumber = cosRange[i, 15].Value.ToString();
                    cosOrd.PickupName = cosRange[i, 4].Value.ToString();
                    cosOrd.PickupAddress = cosRange[i, 5].Value.ToString();
                    cosOrd.PickupCity = cosRange[i, 6].Value.ToString();
                    cosOrd.PickupState = cosRange[i, 7].Value.ToString();
                    cosOrd.PickupZip = cosRange[i, 8].Value.ToString();
                    cosOrd.DeliverName = cosRange[i, 9].Value.ToString();
                    cosOrd.DeliverAddress = cosRange[i, 10].Value.ToString();
                    cosOrd.DeliverCity = cosRange[i, 11].Value.ToString();
                    cosOrd.DeliverState = cosRange[i, 12].Value.ToString();
                    cosOrd.DeliverZip = cosRange[i, 13].Value.ToString();
                    cosOrd.Ll = string.Equals(cosRange[i, 25].Value, "yes", StringComparison.OrdinalIgnoreCase);

                    dblPickupDate = Convert.ToDouble(cosRange[i, 16].Value2);
                    if (dblPickupDate != 0.0)
                    {
                        dtPickupDate = DateTime.FromOADate(dblPickupDate);
                        if (dtPickupDate.CompareTo(runDate) == 0 || dtPickupDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            cosOrd.ScheduledPickupDate = dblPickupDate;
                            cosOrd.ScheduledPickupTimeFrom = Convert.ToDouble(cosRange[i, 17].Value2);
                            cosOrd.ScheduledPickupTimeTo = Convert.ToDouble(cosRange[i, 18].Value2);
                        }
                    }

                    dblDeliveryDate = Convert.ToDouble(cosRange[i, 19].Value2);
                    if (dblDeliveryDate != 0.0)
                    {
                        dtDeliveryDate = DateTime.FromOADate(dblDeliveryDate);
                        if (dtDeliveryDate.CompareTo(runDate) == 0 || dtDeliveryDate.CompareTo(runDate.AddDays(1)) == 0)
                        {
                            cosOrd.ScheduledDeliverDate = dblDeliveryDate;
                            cosOrd.ScheduledDeliverTimeFrom = Convert.ToDouble(cosRange[i, 20].Value2);
                            cosOrd.ScheduledDeliverTimeTo = Convert.ToDouble(cosRange[i, 21].Value2);
                        }
                    }

                    cosOrd.DispatchSequence = cosRange[i, 28].Value.ToString();
                    cosOrd.Overweight = string.Equals(cosRange[i, 29].Value, "yes", StringComparison.OrdinalIgnoreCase);
                    cosOrd.ContainerNumber = (cosRange[i, 30].Value != null ? cosRange[i, 30].Value.ToString() : false);

                    cosOrders.Add(cosOrd);

                    // A new order number is coming up or this is the last record in the collection, add this job to the jobs list
                    if (i == cosRowCount || cosRange[i, 1].Value2.ToString() != cosRange[i + 1, 1].Value2.ToString())
                    {
                        cosOrdersCount++;

                        //Convert the Southwest to a Vesco Order
                        tempVescoOrders = SouthwestConverter.Convert(cosOrders);

                        if (tempVescoOrders.Count() == 0)
                        {
                            badOrders.Add(cosOrd.OrderNumber);
                        }

                        //Add the converted orders
                        foreach (VescoOrder vo in tempVescoOrders)
                        {
                            vescoOrders.Add(vo);
                        }

                        cosOrders.Clear();
                    }

                }

                workbook.Close(false, misValue, misValue);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
                MessageBox.Show(
                    "Error has occured with the one of the fields in the COS file." + 
                    Environment.NewLine + ex.StackTrace,
                    "Invalid Southwest COS file, " + label1.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1
                );
            }
            finally
            {
                if (cosRange != null)
                {
                    Marshal.ReleaseComObject(cosRange);
                }
                if (worksheet != null)
                {
                    Marshal.ReleaseComObject(worksheet);
                }
            }

            Console.Out.WriteLine(Environment.NewLine);
            Console.Out.WriteLine("Total Number Cos Rows Imported => " + (cosRowCount - 1).ToString());
            Console.Out.WriteLine("Total Number of Cos Orders => " + cosOrdersCount.ToString());
            Console.Out.WriteLine("Total Number of Good Southwest Orders => " + SouthwestConverter.getGoodVescoOrdersCount());
            Console.Out.WriteLine("Total Number of Bad Southwest Orders => " + SouthwestConverter.getBadOrdersCount());
        }

        private void shouldContinue()
        {
            if (isPmFileLoaded && isCosFileLoaded)
            {
                this.Cursor = Cursors.WaitCursor;

                Console.Out.WriteLine("File Name" + openFileDialog1.FileName);

                Excel.Application excelObj2 = new Excel.Application();
                Excel.Workbook vescoExcelWorkbook = null;
                Excel.Worksheet vescoExcelWorksheet = null;

                Excel.Worksheet driverExcelWorksheet = null;
                Excel.Range driverRange = null;
                ExecutedOrder tempSwExOrder;

                try
                {
                    Console.Out.WriteLine(vescoOrders.Count());
                    List<String> badSouthwestOrders = SouthwestConverter.badOrders;
                    foreach (String b in badSouthwestOrders)
                    {
                        Console.Out.WriteLine("Bad Southwest Order#," + b);
                    }
                    vescoExcelWorkbook = excelObj2.Workbooks.Open(Application.StartupPath + "\\Templates\\Southwest Template.xlsx");

                    // Re-Optimize
                    if (checkBox1.Checked)
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
                            tempSwExOrder = swExOrders.Find(
                                delegate(ExecutedOrder aeo)
                                {
                                    return aeo.Driver == tempDriverId;
                                }
                            );
                            if (tempSwExOrder != null) {

                                tempDateTime = DateTime.FromOADate(driverRange[i, 4].Value2);
                                earliestStartTime = tempSwExOrder.dtDevliveryDateTime.Date.AddHours(tempDateTime.Hour);
                                dayEndsAt = earliestStartTime.AddHours(driverRange[i, 3].Value2);
                                hoursElapsed = tempSwExOrder.dtDevliveryDateTime.Subtract(earliestStartTime).TotalHours;
                                newDrivingHours = Convert.ToDouble(driverRange[i, 2].Value2) - hoursElapsed;
                                newDrivingHours = newDrivingHours > 0 ? newDrivingHours : 0.0;
                                newDutyHours = Convert.ToDouble(driverRange[i, 3].Value2) - hoursElapsed;
                                newDutyHours = newDutyHours > 0 ? newDutyHours : 0.0;

                                Console.WriteLine("Earliest Start Time -> " + earliestStartTime);
                                Console.WriteLine("Available Driving Hours -> " + driverRange[i, 2].Value2);
                                Console.WriteLine("Available Duty Hours -> " + driverRange[i, 3].Value2);
                                Console.WriteLine("Last Delivery Time -> " + tempSwExOrder.dtDevliveryDateTime);
                                Console.WriteLine("\tHours Elapsed -> " + hoursElapsed);
                                Console.WriteLine("\tNew Earliest Start Time -> " + tempSwExOrder.dtDevliveryDateTime);
                                Console.WriteLine("\tNew Driving Hours -> " + newDrivingHours);
                                Console.WriteLine("\tNew Duty Hours -> " + newDutyHours);
                                Console.WriteLine();

                                driverExcelWorksheet.Cells[i, 2] = newDrivingHours;
                                driverExcelWorksheet.Cells[i, 3] = newDutyHours;
                                driverExcelWorksheet.Cells[i, 4] = tempSwExOrder.dtDevliveryDateTime;
                                driverExcelWorksheet.Cells[i, 5] = tempSwExOrder.DeliverTo;
                                driverCount++;
                                swExOrders.Remove(tempSwExOrder);
                            }
                        }
                    }
                    
                    vescoExcelWorksheet = (Excel.Worksheet)vescoExcelWorkbook.Sheets.Item[1];

                    int row = 2;
                    String stopActionCode;
//                    String windowEnd = null;
                    String windowStart = null;
                    //Add the converted orders
                    foreach (VescoOrder vo in vescoOrders)
                    {
                        vescoExcelWorksheet.Cells[row, 1] = vo.Job;
                        vescoExcelWorksheet.Cells[row, 2] = vo.Sequence;
                        vescoExcelWorksheet.Cells[row, 3] = vo.StopAction;
                        vescoExcelWorksheet.Cells[row, 4] = vo.Location;
                        vescoExcelWorksheet.Cells[row, 5] = vo.Address;
                        
                        stopActionCode = vo.StopAction.Split('-')[0];
                        if ("11".Equals(stopActionCode) || "12".Equals(stopActionCode))
                        {
                            vescoExcelWorksheet.Cells[row, 6] = "60";
                        }
                        else
                        {
                            vescoExcelWorksheet.Cells[row, 6] = vo.StopDelay;
                        }

                        if (vo.WindowStart != null && vo.WindowEnd != null)
                        {
                            if (vo.WindowStart.Equals(vo.WindowEnd))
                            {
                                // Both the window start and end times are the same.  Increment the window end time by an hour for a more accomodating window.
                                DateTime dt = DateTime.Parse(vo.WindowStart, System.Globalization.CultureInfo.CurrentCulture);
                                windowStart = dt.AddHours(-1).ToString("HH:mm:ss");
                            }
                            else
                            {
                                windowStart = vo.WindowStart;
                            }
                        }
                        else
                        {
                            windowStart = vo.WindowStart;
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

                    // Delete the file first since I can't figure out how to get rid of the confirmation dialog
                    File.Delete(Application.StartupPath + "\\Templates\\Vesco.xlsx");
                    vescoExcelWorkbook.SaveAs(Application.StartupPath + "\\Templates\\Vesco.xlsx",
                        Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                        false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                        Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read " + label7.Text + ". Original error: " + ex.Message);
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

                Vesco vesco = new Vesco(checkBox1.Checked, vescoOrders, badOrders, label7.Text + "," + label1.Text, "SouthWest", runDate);
                vesco.Show();
                vesco.SouthWestForm = this;

                this.Hide();

                vesco.performSteps();
            }
        }

        private DateTime getRunDate()
        {
            try
            {
                string[] splits = openFileDialog1.FileName.Split('_');
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

                var tempDate = new DateTime(year, month, day);

                return tempDate.AddDays(1.0);  //oh, my next day, will be just like my last day
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Invalid file name." + ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
                return DateTime.Today;
            }
        }

        private void SouthWest_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            Properties["carrier"] = "0";
            Util.saveProperties(Properties);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                groupBox3.Visible = true;
                this.Height = 420;
                btnLoadPlannedMoves.Enabled = false;
                btnLoadCos.Enabled = false;
            }
            else
            {
                groupBox3.Visible = false;
                this.Height = 302;
                btnLoadPlannedMoves.Enabled = true;
                btnLoadCos.Enabled = true;
            }
        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            Properties["skip"] = "false";
            Util.saveProperties(Properties);
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void statsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statsToolStripMenuItem.Checked = !statsToolStripMenuItem.Checked;

            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            Properties["stats"] = statsToolStripMenuItem.Checked.ToString();
            Util.saveProperties(Properties);
        }
    }
}
