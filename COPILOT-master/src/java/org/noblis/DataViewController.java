/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package org.noblis;

import java.io.File;
import java.io.FileOutputStream;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.io.InputStream;
import java.io.PrintWriter;
import javax.servlet.ServletException;
import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.sql.Connection;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Random;
import java.util.logging.Level;
import java.util.logging.Logger;
import java.util.regex.Pattern;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.io.OutputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.text.SimpleDateFormat;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import jxl.*;
import jxl.format.UnderlineStyle;
import jxl.format.Colour;
import jxl.write.*;
import org.apache.batik.apps.rasterizer.DestinationType;
import org.apache.batik.apps.rasterizer.SVGConverter;
import org.apache.batik.apps.rasterizer.SVGConverterException;
import org.apache.batik.dom.svg.SAXSVGDocumentFactory;
import org.apache.batik.util.XMLResourceDescriptor;
import org.apache.commons.io.FileUtils;
import org.apache.commons.io.IOUtils;
import org.apache.commons.lang3.StringUtils;
import org.w3c.dom.Document;

/**
 *
 * @author M29660
 */
public class DataViewController extends HttpServlet {

    //CHANGE THIS FOR EXTERNAL PUSHES
    private final String PATH_JSON_DB = "/org/noblis/resources/db_external.json";
    private final int MonteCarloLoops = 10000;

    protected void processRequest(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException, JSONException {
        String action = request.getParameter("action");
        if (action.equals("getPage1Tables")) {
            doGetPage1Data(request, response);
        } else if (action.equals("getPage2Table")) {
            doGetPage2Data(request, response);
        } else if (action.equals("getPage3Table")) {
            doGetPage3Data(request, response);
        } else if (action.equals("doRunSimulation")) {
            doRunSimulation(request, response);
        } else if (action.equals("doExcelSheet")) {
            doExcelSheet(request, response);
        } else if (action.equals("exportSVG")) {
            doExportSVG(request, response);
        } else if (action.equals("submitIP")) {
            doSubmitIP(request, response);
        } else if (action.equals("loadSpreadsheet")) {
            doLoadSpreadsheet(request, response);
        } else {
            doDefault(request, response);
        }
    }

    @Override
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        try {
            processRequest(request, response);
        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        try {
            processRequest(request, response);
        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    private void doGetPage1Data(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="getPage1Data">
        DatabaseUtils du = new DatabaseUtils();
        Connection conn = du.getConnection(PATH_JSON_DB);
        String db_host = System.getProperty("dbhost");
        System.out.println("SYSTEM HOST: " + db_host);
        try {
            //Create response object
            JSONObject objRS = new JSONObject();
            //Get Data for applications (main table page1)
            ArrayList<HashMap<String, String>> resultSet = du.selectAllRecords(conn, "cvp_cet", "applications");
            JSONObject apps = new JSONObject();
            for (HashMap<String, String> row : resultSet) {
                apps.append(row.get("category"), row);
            }
            objRS.put("apps", apps);

            //Get Data for building blocks (bottom table page1)
            ArrayList<HashMap<String, String>> sortSet = new ArrayList<HashMap<String, String>>();
            HashMap<String, String> sort = du.getSortItem("bb_name", "ASC");
            sortSet.add(sort);
            resultSet = du.selectRecords(conn, "cvp_cet", "building_blocks", null, sortSet);
            JSONObject blocks = new JSONObject();
            int blockCount = 1;
            for (HashMap<String, String> row : resultSet) {
                blocks.put(Integer.toString(blockCount), row.get("bb_name"));
                blockCount++;
            }
            objRS.put("blocks", blocks);
            objRS.put("blockCount", blockCount - 1);

            resultSet = du.selectAllRecords(conn, "cvp_cet", "v_app_to_bb");
            JSONObject app_to_bb = new JSONObject();
            JSONArray blockArr;
            for (HashMap<String, String> row : resultSet) {
                String app_name = row.get("app_name");
                String bb_name = row.get("bb_name");
                if (app_to_bb.has(app_name)) {
                    blockArr = app_to_bb.getJSONArray(app_name);
                    blockArr.put(bb_name);
                    app_to_bb.put(app_name, blockArr);
                } else {
                    blockArr = new JSONArray();
                    blockArr.put(bb_name);
                    app_to_bb.put(app_name, blockArr);
                }
            }
            objRS.put("app_to_bb", app_to_bb);

            response.setContentType("application/json");
            response.setHeader("Cache-Control", "nocache");
            response.setCharacterEncoding("utf-8");
            try {
                PrintWriter out = response.getWriter();
                out.print(objRS.toString(4));
                out.flush();
            } catch (IOException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            } catch (JSONException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            }
        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            du.closeConnection(conn);
        }
        // </editor-fold>
    }

    private void doGetPage2Data(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="getPage2Data">
        DatabaseUtils du = new DatabaseUtils();
        Connection conn = du.getConnection(PATH_JSON_DB);

        try {
            //Create response object
            JSONObject objRS = new JSONObject();

            //Get Data for applications (main table page2)
            HashMap<String, String> sort = du.getSortItem("bb_name", "ASC");
            ArrayList<HashMap<String, String>> sortArray = new ArrayList<HashMap<String, String>>();
            sortArray.add(sort);
            ArrayList<HashMap<String, String>> resultSet = du.selectRecords(conn, "cvp_cet", "v_app_to_bb", null, sortArray);

            String curBB_Name = "";
            JSONObject appObj = new JSONObject();
            for (HashMap<String, String> row : resultSet) {
                String bb_name = row.get("bb_name");
                if (!curBB_Name.equals(bb_name)) {
                    appObj = new JSONObject();
                    appObj.put(row.get("app_name"), row.get("category"));
                } else {
                    appObj.put(row.get("app_name"), row.get("category"));
                }
                objRS.put(row.get("bb_name"), appObj);
                curBB_Name = bb_name;
            }

            response.setContentType("application/json");
            response.setHeader("Cache-Control", "nocache");
            response.setCharacterEncoding("utf-8");
            try {
                PrintWriter out = response.getWriter();
                out.print(objRS.toString(4));
                out.flush();
            } catch (IOException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            } catch (JSONException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            }
        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            du.closeConnection(conn);
        }
        // </editor-fold>
    }

    private void doGetPage3Data(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="getPage3Data">
        DatabaseUtils du = new DatabaseUtils();
        Connection conn = du.getConnection(PATH_JSON_DB);

        try {
            //Create response object
            JSONObject objRS = new JSONObject();

            //Get Data for Blocks/Apps to Components (main table page3)
            HashMap<String, String> sort1 = du.getSortItem("bb_name", "ASC");
            HashMap<String, String> sort2 = du.getSortItem("app_name", "ASC");
            ArrayList<HashMap<String, String>> sortArray = new ArrayList<HashMap<String, String>>();
            sortArray.add(sort1);
            sortArray.add(sort2);
            ArrayList<HashMap<String, String>> resultSet = du.selectRecords(conn, "cvp_cet", "v_bb_app_to_comp", null, sortArray);

            String curBB_Name = "";
            String curApp_Name = "";
            JSONObject appObj = new JSONObject();
            JSONObject compObj = new JSONObject();
            JSONObject detailsObj;
            for (HashMap<String, String> row : resultSet) {
                String bb_name = row.get("bb_name");
                if (bb_name.isEmpty() || bb_name.equals("")) {
                    bb_name = "Software Development";
                }
                String app_name = row.get("app_name");
                String comp_name = row.get("comp_name");
                detailsObj = new JSONObject();
                detailsObj.put("cost", row.get("ave_cost"));
                detailsObj.put("quantity", row.get("quantity"));
                if (!curApp_Name.equals(app_name)) {
                    compObj = new JSONObject();
                    compObj.put(comp_name, detailsObj);
                    curApp_Name = app_name;
                } else {
                    compObj.put(comp_name, detailsObj);
                }
                if (!curBB_Name.equals(bb_name)) {
                    appObj = new JSONObject();
                    appObj.put(app_name, compObj);
                    curBB_Name = bb_name;
                } else {
                    appObj.put(app_name, compObj);
                }
                objRS.put(bb_name, appObj);
            }

            response.setContentType("application/json");
            response.setHeader("Cache-Control", "nocache");
            response.setCharacterEncoding("utf-8");
            try {
                PrintWriter out = response.getWriter();
                out.print(objRS.toString(4));
                out.flush();
            } catch (IOException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            } catch (JSONException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            }
        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            du.closeConnection(conn);
        }
        // </editor-fold>
    }

    private void doExcelSheet(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="doExcelSheet">
        //Set up excel spreadsheet for writing
        response.setContentType("application/vnd.ms-excel");
        response.setHeader("Content-Disposition", "attachment; filename=CostBreakdown.xls");
        OutputStream out = null;
        WritableWorkbook w = Workbook.createWorkbook(response.getOutputStream());
        WritableSheet sheet = w.createSheet("Cost Breakdown", 0);
        WritableSheet page1Sheet = w.createSheet("Page1 Sheet", 1);
        WritableSheet page2Single = w.createSheet("Page2 Singles", 2);
        WritableSheet page2Dupe = w.createSheet("Page2 Dupes", 3);
        WritableFont arial11font = new WritableFont(WritableFont.ARIAL, 11, WritableFont.BOLD, true);
        WritableCellFormat arial11format = new WritableCellFormat(arial11font);
        WritableCellFormat dollarFormat = new WritableCellFormat(NumberFormats.ACCOUNTING_FLOAT);

        try {
            String json = request.getParameter("compobj");
            //System.out.println("JSON IS: " + json);
            System.out.println("Parsing Request Object");
            JSONObject compObj = getJSONData(json);

            jxl.write.Number num;
            Label label = new Label(0, 0, "Building Block", arial11format);
            sheet.addCell(label);
            label = new Label(1, 0, "Cost Component", arial11format);
            sheet.addCell(label);
            label = new Label(2, 0, "Quantity", arial11format);
            sheet.addCell(label);
            label = new Label(3, 0, "Average Input Unit Cost", arial11format);
            sheet.addCell(label);
            label = new Label(4, 0, "Average Simulated Unit Cost", arial11format);
            sheet.addCell(label);
            label = new Label(5, 0, "Average Simulated Total Cost", arial11format);
            sheet.addCell(label);

            JSONObject page1Obj = compObj.getJSONObject("page1Obj");
            compObj.remove("page1Obj");
            JSONObject page2Obj = compObj.getJSONObject("page2Obj");
            compObj.remove("page2Obj");

            //Add compObj to Sheet 1
            //Parse request json to get type, quantity, and cost of components
            Iterator keys = compObj.sortedKeys();
            ArrayList<String> keyList = new ArrayList<String>();
            while (keys.hasNext()) {
                String compObjKey = (String) keys.next();
                keyList.add(compObjKey);
            }
            int rowCount = 1;
            double aveTotalCost = 0;
            double OandMCostBasis = 0;
            for (String compObjKey : keyList) {
                if (!compObjKey.equals("Systems Engineering") && !compObjKey.equals("Outreach") && !compObjKey.equals("O and M Cost")) {
                    JSONObject compSubObj = compObj.getJSONObject(compObjKey);
                    String quantity = compSubObj.getString("quantity");
                    double aveCost = Double.parseDouble((compSubObj.getString("inputCost")).replaceAll("[^\\d.]+", ""));
                    double costSum = compSubObj.getDouble("costSum");
                    String block = compSubObj.getString("block");

                    //If the cost was not input by the user, we will estimate using triangular distribution
                    //Add Block, Component Name, and Input Cost to Spreadsheet
                    if (block.equals("")) {
                        label = new Label(0, rowCount, "Software Development");
                    } else {
                        label = new Label(0, rowCount, block);
                    }
                    sheet.addCell(label);
                    String comp = compObjKey.split(",")[1];
                    label = new Label(1, rowCount, comp);
                    sheet.addCell(label);
                    int quantNum = Integer.parseInt(quantity);
                    num = new jxl.write.Number(2, rowCount, quantNum);
                    sheet.addCell(num);
                    double aveSubtotalCost = costSum / MonteCarloLoops;
                    double aveUnitCost;
                    if (quantNum != 0) {
                        aveUnitCost = aveSubtotalCost / quantNum;
                    } else {
                        aveUnitCost = 0;
                    }
                    num = new jxl.write.Number(3, rowCount, aveCost, dollarFormat);
                    sheet.addCell(num);
                    num = new jxl.write.Number(4, rowCount, aveUnitCost, dollarFormat);
                    sheet.addCell(num);
                    num = new jxl.write.Number(5, rowCount, aveSubtotalCost, dollarFormat);
                    sheet.addCell(num);
                    aveTotalCost += aveSubtotalCost;
                    if (!compObjKey.equals("App Support (per user)") && !compObjKey.equals("Mobile device cellular data plan for 12 months")) {
                        OandMCostBasis += aveSubtotalCost;
                    }
                    rowCount++;
                }
            }
            rowCount += 1; //Add Spacer Row

            label = new Label(0, rowCount, "Systems Engineering Costs");
            sheet.addCell(label);
            double systemCost = aveTotalCost * 0.14;
            num = new jxl.write.Number(5, rowCount, systemCost, dollarFormat);
            sheet.addCell(num);
            rowCount += 2; //Move down two more rows
            OandMCostBasis += systemCost; //Add the systems cost to O&M cost basis

            label = new Label(0, rowCount, "Outreach Costs");
            sheet.addCell(label);
            double advertCost = aveTotalCost * 0.06;
            num = new jxl.write.Number(5, rowCount, advertCost, dollarFormat);
            sheet.addCell(num);
            rowCount += 2; //Move down two more rows

            label = new Label(0, rowCount, "Operation and Maintenance Costs");
            sheet.addCell(label);
            double OandMCost = OandMCostBasis * 0.07;
            num = new jxl.write.Number(5, rowCount, OandMCost, dollarFormat);
            sheet.addCell(num);
            rowCount += 2; //Move down two more rows

            label = new Label(0, rowCount, "TOTAL COSTS");
            sheet.addCell(label);
            double totalTotalCost = aveTotalCost + systemCost + advertCost + OandMCost;
            num = new jxl.write.Number(5, rowCount, totalTotalCost, dollarFormat);
            sheet.addCell(num);
            rowCount += 2; //Move down two more rows
            //ADD DISCLAIMER
            WritableFont disclaimFont = new WritableFont(WritableFont.ARIAL, 11, WritableFont.BOLD, true, UnderlineStyle.NO_UNDERLINE, Colour.BLACK);
            WritableCellFormat disclaimFormat = new WritableCellFormat(disclaimFont);
            label = new Label(0, rowCount, "CO-PILOT is intended for high-level, preliminary planning purposes to support Connected Vehicle Pilot Deployment cost estimation ONLY, not detailed cost proposal preparation.", disclaimFormat);
            sheet.addCell(label);

            //Add Page1 Object to Sheet2
            JSONObject appObj = page1Obj.getJSONObject("appObj");
            if (appObj.has("Yes")) {
                appObj.remove("Yes");
            }
            if (appObj.has("No")) {
                appObj.remove("No");
            }

            keys = appObj.sortedKeys();
            int colCount = 0;
            while (keys.hasNext()) {
                String appObjKey = (String) keys.next();
                label = new Label(colCount, 0, appObjKey, arial11format);
                page1Sheet.addCell(label);
                colCount++;
            }
            JSONObject blockObj = page1Obj.getJSONObject("blockObj");
            keys = blockObj.sortedKeys();
            rowCount = 2;
            while (keys.hasNext()) {
                String blockObjKey = (String) keys.next();
                String blockObjVal = blockObj.getString(blockObjKey);
                label = new Label(0, rowCount, blockObjKey, arial11format);
                page1Sheet.addCell(label);
                label = new Label(1, rowCount, blockObjVal, arial11format);
                page1Sheet.addCell(label);
                rowCount++;
            }

            //Add Page2 Object to Sheet2
            //Add singles object
            JSONObject singlesObj = page2Obj.getJSONObject("singlesObj");
            keys = singlesObj.sortedKeys();
            rowCount = 0;
            while (keys.hasNext()) {
                String singleObjKey = (String) keys.next();
                String[] singleKeyArr = singleObjKey.split("_");
                String singleObjKeySpaces = StringUtils.join(singleKeyArr, " ");
                label = new Label(0, rowCount, singleObjKeySpaces, arial11format);
                page2Single.addCell(label);
                blockObj = singlesObj.getJSONObject(singleObjKey);
                Iterator blockKeys = blockObj.sortedKeys();
                colCount = 1;
                while (blockKeys.hasNext()) {
                    String blockObjKey = (String) blockKeys.next();
                    label = new Label(colCount, rowCount, blockObjKey, arial11format);
                    page2Single.addCell(label);
                    colCount++;
                }
                rowCount++;
            }
            //Add duplicates object
            JSONObject dupesObj = page2Obj.getJSONObject("dupesObj");
            keys = dupesObj.sortedKeys();
            rowCount = 0;
            while (keys.hasNext()) {
                String dupeObjKey = (String) keys.next();
                blockObj = dupesObj.getJSONObject(dupeObjKey);
                Iterator blockKeys = blockObj.sortedKeys();
                while (blockKeys.hasNext()) {
                    String blockObjKey = (String) blockKeys.next();
                    JSONObject subsetObj = blockObj.getJSONObject(blockObjKey);
                    String quant = subsetObj.getString("quant");
                    String[] dupeKeyArr = dupeObjKey.split("_");
                    String dupeObjKeySpaces = StringUtils.join(dupeKeyArr, " ");
                    subsetObj.remove("quant");
                    label = new Label(0, rowCount, dupeObjKeySpaces, arial11format);
                    page2Dupe.addCell(label);
                    label = new Label(1, rowCount, blockObjKey, arial11format);
                    page2Dupe.addCell(label);
                    label = new Label(2, rowCount, quant, arial11format);
                    page2Dupe.addCell(label);

                    Iterator subsetKeys = subsetObj.sortedKeys();
                    colCount = 3;
                    while (subsetKeys.hasNext()) {
                        String subsetObjKey = (String) subsetKeys.next();
                        label = new Label(colCount, rowCount, subsetObjKey, arial11format);
                        page2Dupe.addCell(label);
                        colCount++;
                    }
                    rowCount++;
                }
            }

            System.out.println("Sending Response");
            w.write();
            w.close();

        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } catch (WriteException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            if (out != null) {
                out.close();
            }
        }
        // </editor-fold>
    }

    private void doRunSimulation(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="doRunSimulation">

        DatabaseUtils du = new DatabaseUtils();
        Connection conn = du.getConnection(PATH_JSON_DB);
        try {
            //Create response object
            JSONObject objRS = new JSONObject();

            //Read request JSON object
            String json = request.getParameter("costobj");
            JSONObject costObj = getJSONData(json);

            JSONObject componentObj = new JSONObject();
            String quantity;
            String aveCost;
            boolean fixedCost;
            JSONObject detailsMap;
            //Parse request json to get type, quantity, and cost of components
            Iterator keys = costObj.sortedKeys();
            ArrayList<String> keyList = new ArrayList<String>();
            while (keys.hasNext()) {
                String key = (String) keys.next();
                keyList.add(key);
            }
            for (String key : keyList) {
                JSONObject costSubObj = costObj.getJSONObject(key);
                quantity = costSubObj.getString("quantity");
                aveCost = costSubObj.getString("cost").replace("$", "");
                fixedCost = costSubObj.getBoolean("fixedcost");

                List<String> blockAppComp = Arrays.asList(key.split(","));
                String block = blockAppComp.get(0);
                String comp = blockAppComp.get(2);
                String subset = blockAppComp.get(3);
                if (subset != null && !subset.isEmpty() && !subset.equalsIgnoreCase("undefined")) {
                    block = block + " " + subset;
                }

                detailsMap = new JSONObject();
                detailsMap.put("quantity", quantity);
                detailsMap.put("costSum", 0.0);
                if (block.equals("")) {
                    block = "Software Development";
                }
                detailsMap.put("block", block);
                detailsMap.put("inputCost", aveCost);

                //If the cost was not input by the user, we will estimate using triangular distribution
                if (!fixedCost) {
                    HashMap<String, String> whereClause = new HashMap<String, String>();
                    whereClause.put("comp_name", comp);

                    ArrayList<HashMap<String, String>> resultSet = du.selectRecords(conn, "cvp_cet", "components", whereClause);
                    if (!resultSet.isEmpty()) {
                        HashMap<String, String> row = resultSet.get(0);

                        String minCost = row.get("min_cost");
                        String maxCost = row.get("max_cost");
                        String modeCost = row.get("mode_cost");
                        detailsMap.put("fixed", false);
                        detailsMap.put("minCost", minCost);
                        detailsMap.put("maxCost", maxCost);
                        detailsMap.put("modeCost", modeCost);
                    } else {
                        detailsMap.put("fixed", true);
                    }
                } //Otherwise just use the cost they gave us
                else {
                    detailsMap.put("fixed", true);
                }
                String compObjKey = block + "," + comp;
                componentObj.put(compObjKey, detailsMap);
            }
            JSONObject sysEngDetails = new JSONObject();
            sysEngDetails.put("quantity", 1);
            sysEngDetails.put("block", "Systems Engineering");
            sysEngDetails.put("costSum", 0.0);

            JSONObject advertDetails = new JSONObject();
            advertDetails.put("quantity", 1);
            advertDetails.put("block", "Outreach");
            advertDetails.put("costSum", 0.0);

            JSONObject OandMDetails = new JSONObject();
            OandMDetails.put("quantity", 1);
            OandMDetails.put("block", "Operations and Maintenance");
            OandMDetails.put("costSum", 0.0);

            //Now that we have built our object, we want to iterate through it X times for our Monte-Carlo simulation
            int i = 0;
            double cost;
            double curCost;
            double totalCost;
            JSONArray resultArray = new JSONArray();
            while (i < MonteCarloLoops) {
                totalCost = 0;
                keys = componentObj.keys();
                while (keys.hasNext()) {
                    String compObjKey = (String) keys.next();
                    if (componentObj.get(compObjKey) instanceof JSONObject) {
                        JSONObject details = (JSONObject) componentObj.get(compObjKey);
                        int quant = Integer.parseInt((String) details.get("quantity"));
                        boolean fixed = ((java.lang.Boolean) details.get("fixed"));
                        if (fixed) {
                            cost = Double.parseDouble((String) details.get("inputCost")) * quant;
                        } else {
                            double minCost = Double.parseDouble((String) details.get("minCost"));
                            double maxCost = Double.parseDouble((String) details.get("maxCost"));
                            double modeCost = Double.parseDouble((String) details.get("modeCost"));
                            cost = getRandTriangular(minCost, maxCost, modeCost) * quant;
                        }
                        totalCost += cost;
                        curCost = new Double(details.get("costSum").toString()) + cost;
                        details.put("costSum", curCost);
                    }
                }
                //Calculate Systems Engineering Cost and Add to total Systems Engineering Costs
                double sysEngCost = totalCost * 0.14;
                curCost = new Double(sysEngDetails.get("costSum").toString()) + sysEngCost;
                sysEngDetails.put("costSum", curCost);
                //Calculate Outreach Cost and Add to total Outreach Costs
                double advertCost = totalCost * 0.06;
                curCost = new Double(advertDetails.get("costSum").toString()) + advertCost;
                advertDetails.put("costSum", curCost);
                //Calculate Operations and Maintenance Cost and Add to total Outreach Costs
                double OandMCost = (totalCost + sysEngCost) * 0.07;
                curCost = new Double(OandMDetails.get("costSum").toString()) + OandMCost;
                OandMDetails.put("costSum", curCost);
                //Add totalCost, Systems Engineering Cost and Outreach Cost together and add them to the results array
                resultArray.put(totalCost + sysEngCost + advertCost + OandMCost);
                i++;
            }
            componentObj.put("Systems Engineering", sysEngDetails);
            componentObj.put("Outreach", advertDetails);
            componentObj.put("O and M Cost", OandMDetails);
            objRS.put("costArray", resultArray);
            objRS.put("compObj", componentObj);
            response.setContentType("application/json");
            response.setHeader("Cache-Control", "nocache");
            response.setCharacterEncoding("utf-8");
            try {
                PrintWriter out = response.getWriter();
                out.print(objRS.toString(4));
                out.flush();
            } catch (IOException ex) {
                Logger.getLogger(DataViewController.class
                        .getName()).log(Level.SEVERE, null, ex);
            } catch (JSONException ex) {
                Logger.getLogger(DataViewController.class
                        .getName()).log(Level.SEVERE, null, ex);
            }

        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class
                    .getName()).log(Level.SEVERE, null, ex);
        } finally {
            du.closeConnection(conn);
        }
        // </editor-fold>
    }

    private void doExportSVG(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="doRunSimulation">
        exportSVGtoPNG(request.getParameter("svg"), request.getParameter("filename"), response, request);
        // </editor-fold>
    }

    private void doDefault(HttpServletRequest request, HttpServletResponse response) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="doDefault">
        System.out.println("Request did not have a vaild action:" + request.toString());
        response.setContentType("text/json;charset=UTF-8");
        response.getOutputStream().println("{ \"success\": true, \"message\": \"default\" }");
        // </editor-fold>
    }

    private JSONObject getJSONData(String json) {
        // <editor-fold defaultstate="collapsed" desc="getJSONData">
        JSONObject obj = new JSONObject();
        try {
            json = json.replaceAll("%(?![0-9a-fA-F]{2})", "|||");
            json = json.replaceAll(Pattern.quote("+"), " ");
            obj = new JSONObject(URLDecoder.decode(json, "UTF-8"));

        } catch (JSONException ex) {
            Logger.getLogger(DataViewController.class
                    .getName()).log(Level.SEVERE, null, ex);
        } catch (UnsupportedEncodingException ex) {
            Logger.getLogger(DataViewController.class
                    .getName()).log(Level.SEVERE, null, ex);
        }
        return obj;
        // </editor-fold>
    }

    private double getRandTriangular(double min, double max, double mode) {
        // <editor-fold defaultstate="collapsed" desc="getRandTriangular">
        double a = min;
        double b = max;
        double c = mode;
        Random rnd = new Random();
        double U = rnd.nextDouble();
        double F = (c - a) / (b - a);
        if (U <= F) {
            return a + Math.sqrt(U * (b - a) * (c - a));
        } else {
            return b - Math.sqrt((1 - U) * (b - a) * (b - c));
        }
        // </editor-fold>
    }

    private void exportSVGtoPNG(String svg, String filename, HttpServletResponse response, HttpServletRequest request) throws IOException {
        // <editor-fold defaultstate="collapsed" desc="exportSVGtoPNG">
        int BYTES_DOWNLOAD = 1024;
        String curDate = new SimpleDateFormat("yyyy-MM-dd").format(new java.util.Date());
        filename = filename + "_" + curDate;
        File outputFile = null;
        File svgFile = null;
        try {
            svg = URLDecoder.decode(svg, "UTF-8");

            InputStream in = IOUtils.toInputStream(svg);
            Document svgXmlDoc = createDocument(in);

            // Save this SVG into a file (required by SVG -> PNG transformation process)
            svgFile = File.createTempFile("graphic-", ".svg");
            svgFile.deleteOnExit();
            Transformer transformer = TransformerFactory.newInstance().newTransformer();
            DOMSource source = new DOMSource(svgXmlDoc);
            FileOutputStream fos = null;

            try {
                fos = new FileOutputStream(svgFile);
                transformer.transform(source, new StreamResult(fos));
            } catch (TransformerException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            } finally {
                if (fos != null) {
                    fos.close();
                }
            }

            /*
             * THIS IS FOR TESTING PURPOSES
             FileUtils.copyFileToDirectory(svgFile, new File("/data/crp/temp/"));
             */
            // Convert the SVG into PNG
            int rand = 1000 + (int) (Math.random() * 5000);
            outputFile = File.createTempFile(filename + String.valueOf(rand), ".png");
            SVGConverter converter = new SVGConverter();
            converter.setDestinationType(DestinationType.PNG);
            converter.setSources(new String[]{svgFile.toString()});
            converter.setDst(outputFile);
            try {
                converter.execute();
            } catch (SVGConverterException ex) {
                Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
            }

            // return the file
            response.setContentType("image/png");
            response.setHeader("Content-Disposition", "attachment;filename=" + filename + ".png");
            InputStream is = null;
            OutputStream os = null;
            try {
                is = FileUtils.openInputStream(outputFile);
                int read;
                byte[] bytes = new byte[BYTES_DOWNLOAD];

                os = response.getOutputStream();
                while ((read = is.read(bytes)) != -1) {
                    os.write(bytes, 0, read);
                }
                os.flush();
            } finally {
                if (os != null) {
                    os.close();
                }
                if (is != null) {
                    is.close();
                }
            }
        } catch (TransformerConfigurationException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        } finally {
            if (outputFile != null && outputFile.delete()) {
                //System.out.println("deleted the png file");
            } else if (outputFile != null) {
                System.err.println("set the png to be deleted on jvm exit");
                outputFile.deleteOnExit();
            }
            if (svgFile != null && svgFile.delete()) {
                //System.out.println("deleted the svg file");
            } else if (svgFile != null) {
                System.err.println("set the svg to be deleted on jvm exit");
                svgFile.deleteOnExit();
            }
        }
        // </editor-fold>
    }

    private Document createDocument(InputStream in) {
        // <editor-fold defaultstate="collapsed" desc="createDocument">
        Document doc = null;
        try {
            // Create a new svg document.
            String parser = XMLResourceDescriptor.getXMLParserClassName();
            SAXSVGDocumentFactory f = new SAXSVGDocumentFactory(parser);
            doc = f.createSVGDocument(null, in);

        } catch (IOException ex) {
            Logger.getLogger(DataViewController.class.getName()).log(Level.SEVERE, null, ex);
        }
        return doc;
        // </editor-fold>
    }

    private void doSubmitIP(HttpServletRequest request, HttpServletResponse response) {
        // <editor-fold defaultstate="collapsed" desc="doSubmitIP">
        DatabaseUtils du = new DatabaseUtils();
        //Attempt to get user IP address
        String ipAddress = request.getHeader("X-FORWARDED-FOR");
        if (ipAddress == null) {
            ipAddress = request.getRemoteAddr();
        }
        String timeStamp = du.getTimeStamp();
        Connection conn = du.getConnection(PATH_JSON_DB);
        HashMap<String, String> whereClause = new HashMap<String, String>();
        whereClause.put("unique_ip", ipAddress);
        try {
            HashMap<String, String> newIP = new HashMap<String, String>();
            newIP.put("unique_ip", ipAddress);
            newIP.put("date_time", timeStamp);
            du.insertData(conn, "cvp_cet", "unique_visits", newIP);
        } catch (Exception ex) {
            Logger.getLogger(DataViewController.class
                    .getName()).log(Level.SEVERE, null, ex);
        } finally {
            du.closeConnection(conn);
        }
        // </editor-fold>
    }

    private void doLoadSpreadsheet(HttpServletRequest request, HttpServletResponse response) throws IOException, JSONException {
        //Create response object
        JSONObject objRS = new JSONObject();

        //Read request JSON object
        String costFileName = request.getParameter("costfile");
        String rand = request.getParameter("rand");
        String costFile = "/data/copilot/" + rand + "_" + costFileName;
        File inputWorkbook = new File(costFile);
        Workbook w;
        try {
            w = Workbook.getWorkbook(inputWorkbook);
            // Get the first sheet
            Sheet sheet = w.getSheet(1);
            //Create object to populate Page 1
            JSONObject blockObj = new JSONObject();
            JSONArray appArr = new JSONArray();
            for (int i = 0; i < (sheet.getColumns()); i++) {
                Cell cell = sheet.getCell(i, 0);
                String app = cell.getContents();
                if (!app.equals("") && !app.isEmpty()) {
                    appArr.put(app);
                }
            }
            for (int i = 1; i < (sheet.getRows()); i++) {
                Cell blockCell = sheet.getCell(0, i);
                Cell quantCell = sheet.getCell(1, i);
                String block = blockCell.getContents();
                String quant = quantCell.getContents();
                if (!block.isEmpty() && !quant.isEmpty()) {
                    blockObj.put(block, quant);
                }
            }
            objRS.put("blockObj", blockObj);
            objRS.put("appArr", appArr);

            //Create object to populate Page 2 singles object
            sheet = w.getSheet(2);
            JSONObject singlesObj = new JSONObject();
            for (int i = 0; i < (sheet.getRows()); i++) {
                blockObj = new JSONObject();
                appArr = new JSONArray();
                Cell blockCell = sheet.getCell(0, i);
                String block = blockCell.getContents();
                for (int j = 1; j < (sheet.getColumns()); j++) {
                    Cell appCell = sheet.getCell(j, i);
                    String app = appCell.getContents();
                    if (!app.isEmpty()) {
                        appArr.put(app);
                    }
                }
                if (!block.isEmpty()) {
                    blockObj.put("apps", appArr);
                    singlesObj.put(block, blockObj);
                }
            }
            //Create object to populate Page 2 singles object
            sheet = w.getSheet(3);
            JSONObject dupesObj = new JSONObject();
            String lastBlock = "";
            String block = "";
            JSONObject subsetObj = new JSONObject();
            for (int i = 0; i < (sheet.getRows()); i++) {
                blockObj = new JSONObject();
                appArr = new JSONArray();
                Cell blockCell = sheet.getCell(0, i);
                Cell subsetCell = sheet.getCell(1, i);
                Cell quantCell = sheet.getCell(2, i);
                String subset = subsetCell.getContents();
                block = blockCell.getContents();
                String quant = quantCell.getContents();
                //System.out.println("Block is: " + block);
                for (int j = 3; j < (sheet.getColumns()); j++) {
                    Cell appCell = sheet.getCell(j, i);
                    String app = appCell.getContents();
                    if (!app.isEmpty()) {
                        appArr.put(app);
                    }
                }
                if (!block.isEmpty() && !quant.isEmpty()) {
                    blockObj.put("apps", appArr);
                    blockObj.put("quant", quant);
                    if (block.equals(lastBlock) || lastBlock.isEmpty()) {
                        subsetObj.put(subset, blockObj);
                        //System.out.println("Adding to SubsetObj: " + blockObj);
                    } else {
                        //System.out.println("Adding to dupesObj: " + subsetObj);
                        dupesObj.put(lastBlock, subsetObj);
                        subsetObj = new JSONObject();
                        subsetObj.put(subset, blockObj);
                    }
                    lastBlock = block;
                }
            }
            dupesObj.put(block, subsetObj);
            objRS.put("singlesObj", singlesObj);
            objRS.put("dupesObj", dupesObj);

            //Write the JSON to output
            response.setContentType("application/json");
            response.setHeader("Cache-Control", "nocache");
            response.setCharacterEncoding("utf-8");
            PrintWriter out = response.getWriter();
            out.print(objRS.toString(4));
            out.flush();

        } catch (Exception ex) {
            try {
                PrintWriter out = response.getWriter();
                out.print(objRS.toString(4));
                out.flush();
            } catch (IOException e) {
                Logger.getLogger(DataViewController.class
                        .getName()).log(Level.SEVERE, null, e);
            }
            Logger.getLogger(DataViewController.class
                    .getName()).log(Level.SEVERE, null, ex);
        } finally {
            Path path = Paths.get(costFile);
            Files.delete(path);
        }
    }
}
