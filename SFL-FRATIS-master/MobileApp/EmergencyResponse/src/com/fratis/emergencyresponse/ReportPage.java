
//---------------------------------------------------------------------------------------------------------------------
// DEPRECATED 3/25/14 - Moved to HTML based reports (WebReportPage.java)
//Class:	 	Report Page
//Author: 	Dmitri Zyuzin - University of Washington TRAC
//Purpose: 	- Transforms XML file into a dynamic Android Layout with page navigation
//			- Generates a DOM element for the page similar to input XML file to store user input data
//			- Stores DOM element into temporary XML file on Filesystem between pages appends data from current page
//			- If page is returned to, the data is loaded into the correct controls
//Issues:		-This is not the most efficient way to do this. There is several redundant statements and tasks
//TODO:		-The class originally used only XMLPullParser to read data, it did not need to store XML. Now that a
//			DOM Document is created, using DocumentBuilder, it would make more sense to use DOM for reading the input
//			XML file as well for consistency. 
//---------------------------------------------------------------------------------------------------------------------

package com.fratis.emergencyresponse;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;
import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import com.fratis.emergencyresponse.XMLControls.xmlCheckBox;
import com.fratis.emergencyresponse.XMLControls.xmlRadioGroup;

import android.content.Intent;
import android.graphics.Color;
import android.graphics.PorterDuff;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.support.v4.app.FragmentActivity;
import android.text.Editable;
import android.text.TextWatcher;
import android.text.format.Time;
import android.util.Xml;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnFocusChangeListener;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

public class ReportPage extends FragmentActivity 
{
	protected MyApplication myApplication;	// Application Object with global variables
	
	private final int pageIntent = 0110;
	
	private String	layoutFileName = "";	// Name of XML layout file
	private String	layoutPage = "";		// Which Page are we loading?
	private String	reportFileName = "";	// Output XML file name
	private String	reportName="";			// For Using In Other Data Elements
	private File	reportFile;				// Output XML File
	private File	layoutFile;				// Handle to XML layout file
	private String	uploadScript = "";		// Which php script is defined in the report
	
	public String	storagePath = Environment.getExternalStorageDirectory().toString();		// Safe path to storage
	
	private View	activeView = null;
	
	// XML Document Builder
	private DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
	private DocumentBuilder documentBuilder = null;
	private Document xmlDocument = null;
	private Element reportElement;
	
	// JSON holding business status
	JSONArray businessJSON = new JSONArray();
	
	public XMLControls xmlControls = new XMLControls();
	
	// Layout parameters for Report Controls
	private LinearLayout.LayoutParams full_width = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
	private LinearLayout.LayoutParams full_width_margin = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
	
	// View for storing dynamically loaded controls
	private LinearLayout 	pageLayout;
	private ScrollView		scrollContainer;
	
	private static final int photoIntent = 135;
	private Uri photoUri;
	
		
	public Element getElementById(String id)
	{
		NodeList nodes = xmlDocument.getElementsByTagName("*");
		
		for(int n = 0; n < nodes.getLength(); n++)
		{
			Element node = (Element)nodes.item(n);
			
			if(node.hasAttribute("xml:id"))
			{
				if(node.getAttribute("xml:id").equals(id))
				{
					return (Element)nodes.item(n);
				}
			}
		}
		return null;
	}
	
	// Grab XML String from Document
	public void loadXMLFile()
	{
		//myApplication.Log.i("loadXMLFile", "Loading XML File: " + reportFile.getPath());
		if(reportFile.exists())
		{
			myApplication.Log.i("loadXMLFile", "Loading XML File: " + reportFile.getPath());
			try 
			{
		        xmlDocument = documentBuilder.parse(reportFile);
		        
		        //e.toString();
			} catch (SAXException e) {
				e.printStackTrace();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}
	
	// Grab XML String from Document
	public void saveXMLFile()
	{
		Document doc = xmlDocument;
        TransformerFactory factory = TransformerFactory.newInstance();
        Transformer transformer;
		try {	
			myApplication.Log.i("saveXMLFile", "Saving XML File: " + reportFile);
			reportFile.mkdirs();
	    	
	    	if(reportFile.exists())
	    		reportFile.delete();			
			
			transformer = factory.newTransformer();
	        Properties outFormat = new Properties();
	        outFormat.setProperty(OutputKeys.INDENT, "yes");
	        outFormat.setProperty("{http://xml.apache.org/xslt}indent-amount", "2");
	        outFormat.setProperty(OutputKeys.METHOD, "xml");
	        outFormat.setProperty(OutputKeys.OMIT_XML_DECLARATION, "no");
	        outFormat.setProperty(OutputKeys.VERSION, "1.0");
	        outFormat.setProperty(OutputKeys.ENCODING, "UTF-8");
	        transformer.setOutputProperties(outFormat);
	        DOMSource domSource = new DOMSource(doc);

            reportFile.createNewFile();

	        StreamResult result = new StreamResult(new FileOutputStream(reportFile));
	        
	        transformer.transform(domSource, result);
	        
	       	MediaScannerConnection.scanFile(this,
	       	          new String[] { reportFile.toString() }, null,
	       	          new MediaScannerConnection.OnScanCompletedListener() {public void onScanCompleted(String path, Uri uri) {} });
	       	
    		myApplication.Log.i("saveXMLFile", "Transformed to XML File: " + reportFile);
	        
		} catch (TransformerConfigurationException e) {
			e.printStackTrace();
		} catch (TransformerException e) {
			e.printStackTrace();
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} 
	}
	
	// Gets json variable from previously updated business status
	public String getJSONvar(String var)
	{
		if( businessJSON.length() > 0 )
		{
			JSONObject it;
			try {
				it = (businessJSON.getJSONObject(0));
				myApplication.Log.i("getJSONvar", it.getString(var));
				return it.getString(var);
			} catch (JSONException e) 
			{
				myApplication.Log.e("getJSONvar", "JSON Error!");
			}
		}
		
		return "";
	}
	
	public boolean getJSONsupply(String var)
	{		
		for(int jo = 0; jo < businessJSON.length(); jo++)
		{
			
			JSONObject it;
			try {
				it = (businessJSON.getJSONObject(jo));
				
				if(it.getString("SupplySubType").equals(var) && it.getString("SupplyType").equals(layoutPage))
				{
					myApplication.Log.i("getJSONvar", var + " = true");
					return true;
				}
			} catch (JSONException e) 
			{
				myApplication.Log.e("getJSONvar", "JSON Error!");
			}
		}
		
		return false;
	}
	
	
	//[XML Parsing Functions]	----------------------------------------------
	//	Initialize XML Parser
	public void parse(InputStream in, String page) throws XmlPullParserException, IOException {
        try {
            XmlPullParser parser = Xml.newPullParser();
            parser.setFeature(XmlPullParser.FEATURE_PROCESS_NAMESPACES, false);
            parser.setInput(in, null);
            parser.nextTag();
            readPage(parser, page);
            
        } finally {
            in.close();
        }
    }
	
	@Override
	protected void onActivityResult(int request, int result, Intent intent) 
	{
		if(request == pageIntent && result == RESULT_OK)
		{
			reportFileName = intent.getStringExtra("reportFileName");
			loadXMLFile();
			reportElement = (Element) xmlDocument.getElementsByTagName("report").item(0);

		}
		else if(request == photoIntent && result == RESULT_OK)
		{
			
		}
		super.onActivityResult(request, result, intent);
	}

	//	readPage
	//	Walk xml by tags
	//	TODO: change to DOM Document for parsing for consistensy
    private void readPage(XmlPullParser parser, String page) throws XmlPullParserException, IOException 
    {
    	parser.require(XmlPullParser.START_TAG, null, "report");
    	
    	for(int a = 0; a<parser.getAttributeCount(); a++) // Go through report attributes
    	{
    		if(parser.getAttributeName(a).equalsIgnoreCase("uploadscript"))
    		{
    			uploadScript = parser.getAttributeValue(a);
    			myApplication.Log.i("Parser", "Upload Script: " + uploadScript);
    		}
    	}
    	
    	
        while (parser.next() != XmlPullParser.END_DOCUMENT) {
            if (parser.getEventType() != XmlPullParser.START_TAG) {
                continue;
            }
            String name = parser.getName();
            String pagedbId = "";
            String pageId = "";

            if (name.equals("page")) // Found Page
            {

            	for(int a = 0; a<parser.getAttributeCount(); a++) // Go through page's attributes and store them
            	{
            		if(parser.getAttributeName(a).equalsIgnoreCase("xml:id")){
            			pageId = parser.getAttributeValue(a);
            		}
            		else if(parser.getAttributeName(a).equalsIgnoreCase("dbid")){
            			pagedbId = parser.getAttributeValue(a);
            		}
            	}
            	
            	// See if page title is for the page we're looking for
            	if(pageId.equals(page))
    			{
        			myApplication.Log.i("Parser", "Found Page: " + page);
        			
        			if(!pageId.equals("home"))
        			{
        				NodeList pages = xmlDocument.getElementsByTagName("page");
        				boolean pageFound = false;
        				for(int p = 0; p< pages.getLength(); p++)
        				{
        					if(pages.item(p).getAttributes().getNamedItem("xml:id").getTextContent().equals(page))
        					{
        						pageFound = true;
        						myApplication.Log.i("xmlBuilder", "Found page in xml: " + page);
        						reportElement = (Element) pages.item(p);
        					}
        				}
        				
    					if(!pageFound)
    					{
    						myApplication.Log.i("xmlBuilder", "Adding page to xml: " + page);
            				Element xmlp = xmlDocument.createElement("page");
            				xmlp.setAttribute("xml:id", pageId);
            				xmlp.setAttribute("dbid", pagedbId);
            				reportElement.appendChild(xmlp);
            				reportElement = xmlp;
    					}
        			}
        			
    				// Walk page's child tags
        	        while (parser.next() != XmlPullParser.END_DOCUMENT) 
        	        {
        	        	//Keep going until finding the closing "page" tag
        	        	if(parser.getEventType() == XmlPullParser.END_TAG && parser.getName().equals("page"))
        	        		return;
        	        	
        	        	//Skip all events other than START_TAG
        	            if (parser.getEventType() != XmlPullParser.START_TAG) 
        	                continue;

        	            myApplication.Log.i("Parser", parser.getName());
        	            
        	            //Process Page Controls
        	            if(parser.getName().equals("textinput") || parser.getName().equals("dateinput") 
        	            		|| parser.getName().equals("radio") || parser.getName().equals("menu") 
        	            		|| parser.getName().equals("button") || parser.getName().equals("checkbox")
        	            		|| parser.getName().equals("label"))
        	            {
        	            	// Find id and text attributes for tag
        	            	String id="";
        	            	String text="";
        	            	String action="";
        	            	Boolean noedit = false;
        	            	View parent=null;
        	            	
        	            	Element item = xmlDocument.createElement(parser.getName());
        	            	
        	            	for(int a = 0; a < parser.getAttributeCount();a++)
        	            	{
        	            		//myApplication.Log.i("Parser", "--" + parser.getAttributeName(a));
        	            		item.setAttribute(parser.getAttributeName(a), parser.getAttributeValue(a));
        	            		
        	            		if(parser.getAttributeName(a).equals("xml:id"))
        	            			id = parser.getAttributeValue(a);

        	            		else if(parser.getAttributeName(a).equals("text"))
        	            			text = parser.getAttributeValue(a);

        	            		else if(parser.getAttributeName(a).equals("action"))
        	            			action = parser.getAttributeValue(a);
        	            		
        	            		else if(parser.getAttributeName(a).equals("noedit"))
        	            			noedit = true;
        	            		
        	            		// If parent defined, try to find parent by id
        	            		else if(parser.getAttributeName(a).equals("parent"))
        	            		{
                	            	for(int c = 0; c < pageLayout.getChildCount(); c++)
                	            		if(pageLayout.getChildAt(c).getContentDescription().toString().equals(parser.getAttributeValue(a)))
                	            		{
                	            			parent = pageLayout.getChildAt(c);
                	            		}
        	            		}
        	            	}
        	            	
        	            	
        	            	// Create container for control
        	            	LinearLayout controlContainer = new LinearLayout(this);
        	            	controlContainer.setContentDescription(id);
        	            	
        	            	LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        	            	controlContainer.setOrientation(LinearLayout.VERTICAL);
        	            	// Do not color child containers or labels
        	            	// Horizontal Layout for child elements
        	            	if(parent == null && !parser.getName().equals("label"))
        	            	{
        	            		controlContainer.setBackgroundColor(Color.argb(20, 0, 50, 75));
            	            	lp.setMargins(0, 0, 0, 15);
        	            		
        	            	}
        	            	
        	            	controlContainer.setLayoutParams(lp);
        	            	
        	            	// Add a label to layout
        	            	if(!parser.getName().equals("button") && !parser.getName().equals("checkbox") && text != "")
        	            	{
           	            		TextView labelControl = new TextView(this);
        	            		labelControl.setText(text);
        	            		labelControl.setLayoutParams(full_width); 
        	            		labelControl.setTextColor(Color.argb(255, 35, 135, 176));
        	            		
        	            		if(parent==null)
	        	            		labelControl.setTextSize(22f);
        	            		else
        	            			labelControl.setTextSize(18f);
        	            		
        	            		if(parser.getName().equals("label"))
        	            			labelControl.setTextSize(24f);
        	            		
        	            		controlContainer.addView(labelControl);
        	            	}
    	            		//pageLayout.addView(labelControl);
    	            		
    	            		// textview / dateinput tags
    	            		if(parser.getName().equals("textinput") || parser.getName().equals("dateinput"))
    	            		{
    	            			
    	            			XMLControls.xmlEditText textControl = xmlControls.new xmlEditText(this);
        	            		textControl.setContentDescription(id);
        	            		
        	            		if(noedit)
        	            			textControl.setKeyListener(null);
        	            		
        	            		Element e = getElementById(id);
        	            		
            	            	if( (id != "") && (e != null) )
            	            	{
            	            		textControl.xmlElement = getElementById(id);
            	            		if(textControl.xmlElement.getAttribute("value") != "")
            	            			textControl.setText(textControl.xmlElement.getAttribute("value"));
            	            	}
            	            	else
            	            	{
            	            		textControl.xmlElement = item;
                	            	reportElement.appendChild(item);
            	            	}
            	            	
           	            		if(id.equals("businessName"))
        	            		{
        	            			myApplication.Log.i("Parser", "Found businessName");
        	            			textControl.setText(myApplication.businessName);
        	            			textControl.xmlElement.setAttribute("value", myApplication.businessName);
        	            			textControl.xmlElement.setAttribute("business_id", myApplication.businessId);
        	            		}
           	            		
           	            		if(id.equals("businessHoursComment"))
           	            		{
           	            			String val = this.getJSONvar("businessHoursComment");
        	            			textControl.setText(val);
        	            			textControl.xmlElement.setAttribute("value", val);
           	            		}
           	            		
           	            		if(id.equals("reconTeam"))
        	            		{
        	            			myApplication.Log.i("Parser", "Found reconTeam");
        	            			String val = myApplication.settings.getString("reconTeam","");
        	            			textControl.setText(val);
        	            			textControl.xmlElement.setAttribute("value", val);
        	            		}
            	            	
            	            	
        	            		textControl.setLayoutParams(full_width_margin);
        	            		if(parent != null)
        	            			textControl.setTextColor(Color.argb(255, 35, 135, 176));
        	            		
        	            		textControl.setTextSize(18f);
        	            		
        	            		textControl.setOnFocusChangeListener(new OnFocusChangeListener()
        	            		{
									@Override
									public void onFocusChange(View v,boolean hasFocus) {
										if(hasFocus)
											activeView = v;
									}
        	            		});
        	            		
        	            		textControl.addTextChangedListener(new TextWatcher(){

									@Override
									public void afterTextChanged(
											Editable arg0) {}

									@Override
									public void beforeTextChanged(
											CharSequence arg0, int arg1,
											int arg2, int arg3) {}

									@Override
									public void onTextChanged(
											CharSequence arg0, int arg1,
											int arg2, int arg3) 
									{
										if(activeView != null)
										{
											XMLControls.xmlEditText thisText = (XMLControls.xmlEditText)activeView;
											thisText.xmlElement.setAttribute("value", arg0.toString());
											
											if(thisText.xmlElement.hasAttribute("gpsposition"))
												thisText.xmlElement.setAttribute("gpsposition", String.valueOf(myApplication.getLastLocation().getLatitude()) + "," + String.valueOf(myApplication.getLastLocation().getLongitude()));
											
											if(thisText.xmlElement.getAttribute("xml:id").equals("reconTeam"))
												myApplication.settings.edit().putString("reconTeam", arg0.toString()).apply();
											
											myApplication.Log.i("Parser", "ChangedText: id:" + thisText.xmlElement.getAttribute("xml:id") + " text:" + thisText.xmlElement.getAttribute("value"));
										}

									}});
        	            		
        	            		if(parser.getName().equals("dateinput")) // Add date 
        	            		{
        	            			Time now = new Time();
        	            			now.setToNow();
        	            			String datestring = android.text.format.DateFormat.format("yyyy/MM/dd hh:mm:ss", new java.util.Date()).toString();
        	            			
        	            			textControl.setText(datestring);
        	            			textControl.xmlElement.setAttribute("value", datestring);
        	            		}

        	            		controlContainer.addView(textControl);
    	            		}
    	            		// radio tag
    	            		else if(parser.getName().equals("radio"))
    	            		{
    	            			XMLControls.xmlRadioGroup radioContainer = xmlControls.new xmlRadioGroup(this);
    	            			radioContainer.setContentDescription(id);
    	            			radioContainer.setLayoutParams(full_width_margin);
    	            			
    	            			int option = 0;
    	            			// Walk through child tags to find all "option" tags
    	            			while( !(parser.getEventType() == XmlPullParser.END_TAG && parser.getName().equals("radio")))
    	            			{
    	            				parser.next();
    	            	        	//Skip all events other than START_TAG
    	            	            if (parser.getEventType() != XmlPullParser.START_TAG) 
    	            	                continue;
    	            	            
            	            		if(parser.getName().equals("option"))
            	            		{
            	            			
                    	            	String rText="";
                    	            	String rValue="";
                    	            	for(int a = 0; a < parser.getAttributeCount();a++)
                    	            	{
                    	            		if(parser.getAttributeName(a).equals("text"))
                    	            			rText = parser.getAttributeValue(a);
                    	            		else if(parser.getAttributeName(a).equals("value"))
                    	            			rValue = parser.getAttributeValue(a);
                    	            	}
                    	            	                    	            	
            	            			RadioButton radio = new RadioButton(this);
            	            			radio.setLayoutParams(full_width);
            	            			radio.setText(rText);
            	            			radio.setContentDescription(rValue);
    	        	            		if(parent != null)
    	        	            			radio.setTextColor(Color.argb(255, 35, 135, 176));
            	            			radio.setTextSize(18f);
            	            			radio.setId(option);
            	            			radioContainer.addView(radio);
            	            			option++;
            	            		}
    	            				
    	            			}

    	            			controlContainer.addView(radioContainer);
    	            			
    	            			// Load previous value if set
            	            	if( (id != "") && (getElementById(id) != null) )
            	            	{
            	            		radioContainer.xmlElement = getElementById(id);
            	            	}
            	            	else
            	            	{
            	            		radioContainer.xmlElement = item;
                	            	reportElement.appendChild(item);
                	            	
               	            		if(id.equals("businessStatus") || id.equals("businessHours"))
               	            		{
               	            			String val = this.getJSONvar(id);
               	            			radioContainer.xmlElement.setAttribute("value", val);
               	            		}
            	            	}
    	            			
           	            		
        	            		if(radioContainer.xmlElement.getAttribute("value") != "")
    	            			{
        	            			// Check radio button if it's checked
        	            			for(int c = 0; c < radioContainer.getChildCount(); c++)
        	            			{
        	            				RadioButton child = (RadioButton)radioContainer.getChildAt(c);
        	            				if(child.getContentDescription().equals(radioContainer.xmlElement.getAttribute("value")))
        	            					child.setChecked(true);
        	            				else
        	            					child.setChecked(false);
        	            			}
    	            			}
        	            		
    	            			radioContainer.setOnCheckedChangeListener(new OnCheckedChangeListener(){
									@Override
									public void onCheckedChanged(
									RadioGroup group, int checkedId) 
									{
										XMLControls.xmlRadioGroup rContainer = (xmlRadioGroup) group;
										
										myApplication.Log.i("Parser", "Checked Item:" + group.getContentDescription() + ":" + group.getChildAt(checkedId).getContentDescription());
										rContainer.xmlElement.setAttribute("value", (String) group.getChildAt(checkedId).getContentDescription());	
										
										if(rContainer.xmlElement.hasAttribute("gpsposition"))
											rContainer.xmlElement.setAttribute("gpsposition", String.valueOf(myApplication.getLastLocation().getLatitude()) + "," + String.valueOf(myApplication.getLastLocation().getLongitude()));
										
									}});
    	            		}
    	            		
    	            		// button tag
    	            		else if(parser.getName().equals("checkbox"))
    	            		{
    	            			XMLControls.xmlCheckBox checkboxControl = xmlControls.new xmlCheckBox(this);
        	            		checkboxControl.setLayoutParams(full_width_margin);
        	            		checkboxControl.setTextSize(18f);
        	            		checkboxControl.setContentDescription(id);
        	            		checkboxControl.setText(text);
        	            		        	            		
            	            	if( (id != "") && (getElementById(id) != null) )
            	            	{
            	            		checkboxControl.xmlElement = getElementById(id);
            	            	}
            	            	else
            	            	{
            	            		checkboxControl.xmlElement = item;
                	            	reportElement.appendChild(item);
                	            	
            	            		if(getJSONsupply(text))
            	            			checkboxControl.xmlElement.setAttribute("value", "true");
            	            	}
        	            		
        	            		if(checkboxControl.xmlElement.getAttribute("value").equals("true"))
        	            			checkboxControl.setChecked(true);
        	            		else
        	            			checkboxControl.setChecked(false);
        	            		
            	            	checkboxControl.setOnCheckedChangeListener(new android.widget.CompoundButton.OnCheckedChangeListener(){
									@Override
									public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) 
									{
										XMLControls.xmlCheckBox cControl  = (xmlCheckBox) buttonView;
										if(isChecked)
											cControl.xmlElement.setAttribute("value", "true");
										else
											cControl.xmlElement.setAttribute("value", "false");
										
										if(cControl.xmlElement.hasAttribute("gpsposition"))
											cControl.xmlElement.setAttribute("gpsposition", String.valueOf(myApplication.getLastLocation().getLatitude()) + "," + String.valueOf(myApplication.getLastLocation().getLongitude()));
										
									}
            	            	}
            	            	);
        	            		
        	            		if(text == null)
        	            			checkboxControl.setTextColor(Color.argb(255, 35, 135, 176));
        	            		
        	            		controlContainer.addView(checkboxControl);
    	            		}
    	            		
    	            		
    	            		// menu tag
    	            		else if(parser.getName().equals("menu"))
    	            		{
    	            			//Don't apply background color to container
    	            			controlContainer.setBackgroundColor(Color.argb(0, 0, 0, 0));
    	            			
    	            			// Walk through child tags to find all "link" tags
    	            			while( !(parser.getEventType() == XmlPullParser.END_TAG && parser.getName().equals("menu")))
    	            			{
    	            				parser.next();
    	            	        	//Skip all events other than START_TAG
    	            	            if (parser.getEventType() != XmlPullParser.START_TAG) 
    	            	                continue;
    	            	            
            	            		if(parser.getName().equals("link"))
            	            		{
                    	            	String rText="";
                    	            	String rLocation="";
                    	            	for(int a = 0; a < parser.getAttributeCount();a++)
                    	            	{
                    	            		if(parser.getAttributeName(a).equals("text"))
                    	            			rText = parser.getAttributeValue(a);
                    	            		else if(parser.getAttributeName(a).equals("location"))
                    	            			rLocation = parser.getAttributeValue(a);
                    	            	}
                    	            	
            	            			Button menuButton = new Button(this);
            	            			menuButton.setLayoutParams(full_width);
            	            			menuButton.setText(rText);
            	            			menuButton.setContentDescription(rLocation);
            	            			menuButton.setTextSize(18f);
            	            			menuButton.setOnClickListener(new OnClickListener()
            	            			{
											@Override
											public void onClick(View v) 
											{
			        	            			v.getBackground().setColorFilter(Color.argb(255, 35, 200, 35), PorterDuff.Mode.MULTIPLY);
												// Start new activity and load new page
										    	Intent intent = new Intent(v.getContext(), ReportPage.class);
										    	intent.putExtra("file", layoutFileName);
										    	intent.putExtra("page", v.getContentDescription());
										    	
										    	if(xmlDocument != null)
										    	{
										    		saveXMLFile();
										    		intent.putExtra("reportFileName", reportFileName);
										    	}
										    	else
										    	{
										    		myApplication.Log.e("xmlDocument", "xmlDocument Null");
										    	}
										    	
										    	myApplication.Log.i("Report Page","Following Link: " + layoutFileName + " - " + v.getContentDescription());
										    	
										    	startActivityForResult(intent, pageIntent);
											}});
            	            			//pageLayout.addView(menuButton);
            	            			controlContainer.addView(menuButton);
            	            		}
    	            				
    	            			}
    	            		}
    	            		
    	            		// button tag
    	            		else if(parser.getName().equals("button"))
    	            		{
    	            			//Don't apply background color to container
    	            			controlContainer.setBackgroundColor(Color.argb(0, 0, 0, 0));
    	            			
    	            			XMLControls.xmlButton buttonControl = xmlControls.new xmlButton(this);
        	            		buttonControl.setLayoutParams(full_width_margin);
        	            		buttonControl.setTextSize(18f);
        	            		buttonControl.setContentDescription(action);
        	            		buttonControl.setText(text);
        	            		
              	            	if( (id != "") && (getElementById(id) != null) )
            	            	{
              	            		buttonControl.xmlElement = getElementById(id);
            	            	}
            	            	else
            	            	{
            	            		buttonControl.xmlElement = item;
                	            	reportElement.appendChild(item);
            	            	}
        	            		

        	            		if(action.equals("finish") || action.equals("submit"))
        	            		{
        	            			buttonControl.getBackground().setColorFilter(Color.argb(255, 35, 135, 176), PorterDuff.Mode.MULTIPLY);
        	            		}
        	            		if(action.equals("photo"))
        	            		{
        	            			buttonControl.getBackground().setColorFilter(Color.argb(255, 185, 236, 35), PorterDuff.Mode.MULTIPLY);
        	            			buttonControl.setCompoundDrawablesWithIntrinsicBounds(getResources().getDrawable(android.R.drawable.ic_menu_camera), null, getResources().getDrawable(android.R.drawable.ic_menu_camera), null);
        	            		}
        	            		
        	            		buttonControl.setOnClickListener(new OnClickListener()
        	            		{
									@Override
									public void onClick(View v) 
									{
										XMLControls.xmlButton thisButton = (XMLControls.xmlButton)v;
										if(v.getContentDescription().toString().equals("finish"))
										{
											Intent ret = new Intent();
											ret.putExtra("reportFileName", reportFileName);
											setResult(RESULT_OK, ret);
											saveXMLFile();
											finish();
										}
										if(v.getContentDescription().toString().equals("submit"))
										{
											saveXMLFile();
											
											myApplication.Log.i("Report Page", uploadScript + " " + reportFile);
											// Initialize new Data Uploader and point it to the right php script
											DataUploader reportUploader = new DataUploader(v.getContext(), uploadScript, false);
											// Upload the reportFile
											reportUploader.execute(reportFile);
											finish();
										}
		        	            		if(v.getContentDescription().toString().equals("photo"))
		        	            		{
		        	            		    Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
		        	            		    
		        	            		    File photoFile= new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp");
		        	            		    
		        	            		    photoFile.mkdirs();
		        	            		   
		        	            		    String photoFileName = reportName + "-" + thisButton.xmlElement.getAttribute("dbid");
		        	            		    
		        	            		    thisButton.xmlElement.setAttribute("photoFileName", photoFileName);
		        	            		    
		        	            		    photoFile = new File (photoFile, photoFileName + ".jpg"); 
		        	            		    
		        	            		    if (photoFile.exists ()) photoFile.delete (); 	
		        	            		    
		        	            		    try {
												photoFile.createNewFile();
											} catch (IOException e) {
												e.printStackTrace();
											}
		        	            		    
		        	            		    photoUri = Uri.fromFile(photoFile);
		        	            		    
		        	            		    intent.putExtra(MediaStore.EXTRA_OUTPUT, photoUri); 

		        	            		    startActivityForResult(intent, photoIntent);
		        	            		}
										
									}});
        	            		
        	            		controlContainer.addView(buttonControl);
    	            		}
    	            		

    	            		
    	            		if(parent != null)
    	            			((LinearLayout) parent).addView(controlContainer);
    	            		else
    	            			pageLayout.addView(controlContainer);

        	            }
        	        }
    			
        	}
            }
        }  
    }
	//[End]	----------------------------------------------
    
    @Override
    public void onBackPressed() 
    {
    	if(!layoutPage.equals("home"))
    	{
			Intent ret = new Intent();
			ret.putExtra("reportFileName", reportFileName);
			setResult(RESULT_OK, ret);
			saveXMLFile();

			finish();
    	}
    	else
    	{
    		// TODO: Gotta implement "Are you sure you want to exit without saving report?
    		Toast.makeText(this, "Report not uploaded. \n Need to implement warning message" , Toast.LENGTH_LONG).show();
    		finish();
    	}
    }
	
	@Override
	protected void onCreate(Bundle savedInstanceState) 
	{
		myApplication = (MyApplication)getApplication();
		
		super.onCreate(savedInstanceState);
		
		layoutFileName = getIntent().getStringExtra("file");
		
		layoutPage	= getIntent().getStringExtra("page");
		

		try {
			documentBuilderFactory.setNamespaceAware(true);
			documentBuilder = documentBuilderFactory.newDocumentBuilder();
			
			// Grab previous business update from server as JSON
			if(layoutFileName.equalsIgnoreCase("business.xml"))
			{
				CheckBusinessStatus bs = new CheckBusinessStatus(this);
				bs.execute("business.xml");
				
				try 
				{
					businessJSON = bs.get(10, TimeUnit.SECONDS);
				} catch (InterruptedException e) 
				{
					myApplication.Log.e("CheckBusinessStatus", "Interrupted Exception");
				} catch (ExecutionException e) {
					myApplication.Log.e("CheckBusinessStatus", "Execution Exception");
				} catch (TimeoutException e) {
					myApplication.Log.e("CheckBusinessStatus", "Timed Out");
				}
			}
			
			// If we're opening home page, initialize a document builder and make a new document!
			if(layoutPage.equals("home"))
			{
				reportName = layoutFileName.replace(".xml",  "") + android.text.format.DateFormat.format("yyyy-MM-dd-hh-mm-ss", new java.util.Date());
				
				reportFileName = reportName + ".xml";

				reportFile = new File(storagePath + "/EmergencyResponse/Reports/" + reportFileName);
				
				xmlDocument = documentBuilder.newDocument();
				reportElement = xmlDocument.createElement("report");
				xmlDocument.appendChild(reportElement);
			}
			else
			{

				// Grab XML Document from home page
				if(getIntent().hasExtra("reportFileName"))
				{
					myApplication.Log.i("reportFileName", "Loading Report: " + reportFileName );
					reportFileName = getIntent().getStringExtra("reportFileName");
					reportName = reportFileName.replace(".xml",  "");
					reportFile = new File(storagePath + "/EmergencyResponse/Reports/" + reportFileName);
					loadXMLFile();

					myApplication.Log.i("reportFileName", "Loaded Report: " + reportFileName );
					if(xmlDocument == null)
						myApplication.Log.e("reportFileName", "Report Null!!!!  ");
					
					reportElement = (Element) xmlDocument.getElementsByTagName("report").item(0);
					myApplication.Log.i("reportFileName", "reportElement: " + reportElement.getNodeName());
				}
			}
			
		} catch (ParserConfigurationException e1) 
		{
			e1.printStackTrace();
		}

		layoutFile = new File(storagePath + "/EmergencyResponse/" + layoutFileName);
		
		//Don't show keyboard by default
		getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_HIDDEN);
    	
		if(layoutFile.exists())
		{
			myApplication.Log.i("Report Page", "Found file: " + layoutFileName);
			
			myApplication.Log.i("Report Page", "Looking for page: " + layoutPage);
			
	    	//Generate Linear Layout for XML file
	    	pageLayout = new LinearLayout(this);
	    	pageLayout.setOrientation(LinearLayout.VERTICAL);
	    	
	    	scrollContainer = new ScrollView(this);
	    	scrollContainer.setLayoutParams(full_width);
	    	
			
			LinearLayout.LayoutParams llp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.MATCH_PARENT);
			
			pageLayout.setLayoutParams(llp);
			
			full_width_margin.bottomMargin = 20;
			
			try {
				FileInputStream xmlStream = new FileInputStream(layoutFile);
				
				parse(xmlStream, layoutPage);
				scrollContainer.addView(pageLayout);
				setContentView(scrollContainer);
				
			} catch (FileNotFoundException e) {
				TextView err = new TextView(this);
				err.setLayoutParams(full_width);
				err.setText("Critical Error: Cannot find XML layout file!");
				err.setTextColor(Color.RED);
				scrollContainer.addView(err);
				setContentView(scrollContainer);

				e.printStackTrace();
			} catch (XmlPullParserException e) {
				TextView err = new TextView(this);
				err.setLayoutParams(full_width);
				err.setText("Critical Error: Failed to parse XML file!\nPlease Check XML Syntax.");
				err.setTextColor(Color.RED);
				scrollContainer.addView(err);
				setContentView(scrollContainer);
				
				e.printStackTrace();
			} catch (IOException e) {
				TextView err = new TextView(this);
				err.setLayoutParams(full_width);
				err.setText("Critical Error: I/O Exception!");
				err.setTextColor(Color.RED);
				scrollContainer.addView(err);
				setContentView(scrollContainer);
				e.printStackTrace();
			}
		}
		else
		{
			Toast.makeText(this, "Can't Read XML File!!!", Toast.LENGTH_LONG).show();
			myApplication.Log.e("Report Page", "Cannot read XML File: " + layoutFile.getPath());
			finish();
		}
	}
	
	@Override
	protected void onResume() 
	{
		super.onResume();

	}

	@Override
	public void onPause() 
	{
		super.onPause();

	}
	
	@Override
	public void onSaveInstanceState(Bundle savedInstanceState) 
	{
	  super.onSaveInstanceState(savedInstanceState);

	}
	
	@Override
	public void onRestoreInstanceState(Bundle savedInstanceState) 
	{
	  super.onRestoreInstanceState(savedInstanceState);
	}
}
