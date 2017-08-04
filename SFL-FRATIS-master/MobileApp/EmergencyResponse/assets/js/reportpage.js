
function getAttr(tag)
{
	if(document.getElementById(tag).hasAttribute("value"))
		return document.getElementById(tag).getAttribute("value");
	else
		return "";
}

function phoneAuth(loginUser, loginPassword, loginType, businessId, businessName)
{
	window.businessId = businessId;
	window.businessName = businessName;
	window.loginUser = loginUser;
	window.loginPassword = loginPassword;
	window.loginType = loginType;
	
	console.log("phoneAuth ok: " + loginUser);
	
	$('#businessName').val(window.businessName);
	$('#businessName').attr('value', window.businessName);
	$('#businessName').attr('business_id', window.businessId);
	
	getBusinessReport();
}

function mysql_real_escape_string (str) {
	if(str)
	{
	    return str.replace(/[\0\x08\x09\x1a\n\r"'\\\%]/g, function (char) {
	        switch (char) {
	            case "\0":
	                return "\\0";
	            case "\x08":
	                return "\\b";
	            case "\x09":
	                return "\\t";
	            case "\x1a":
	                return "\\z";
	            case "\n":
	                return "\\n";
	            case "\r":
	                return "\\r";
	            case "\"":
	            case "'":
	            case "\\":
	            case "%":
	                return "\\"+char; // prepends a backslash to backslash, percent,
	                                  // and double/single quotes
	        }
	    });
	}
	else
	{
		return "";
	}
}

if(typeof Android !== 'undefined')
{
	Android.windowLoaded();
}
else
{
	window.loginUser = window.parent.loginUser;
}


function getDateSQL()
{
	var date;
	date = new Date();
	date = date.getFullYear() + '-' +
			('00' + (date.getMonth() + 1)).slice(-2) + '-' +
			('00' + date.getDate()).slice(-2) + ' ' +
			('00' + date.getHours()).slice(-2) + ':' +
			('00' + date.getMinutes()).slice(-2) + ':' +
			('00' + date.getSeconds()).slice(-2);   
	return date;  	
}

function updateLocation(loc) 
{
	var pages = document.getElementById('report').children;
				
	for (var i = 0; i < pages.length; i++)
	{
		var page = pages[i];
		  
		var page_elements = page.getElementsByTagName("*");
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
			if(page_element.hasAttribute('gpsposition'))
			{
				if(typeof loc == 'undefined')
					page_element.setAttribute('gpsposition', Android.getLastLocation());
				else
					page_element.setAttribute('gpsposition', loc);
			}
		}
	}
}


function processBusinessReport()
{
	var loginId					=	window.loginUser;
	var businessId 				=	document.getElementById('businessName').getAttribute('business_id');
	var businessStatus 			=	getAttr('businessStatus');
	var businessHours			=	getAttr('businessHours');
	var businessHoursComment	=	getAttr('businessHoursComment');
	var businessComment			=	getAttr('businessComment');
	var updateTime				=	getDateSQL();
	
	
	var jsonReport = [];
				
	var pages = document.getElementById('report').children;
				
	for (var i = 0; i < pages.length; i++)
	{
		var page = pages[i];
		
		var SupplyType = page.getAttribute('id');
		
		var page_elements = page.getElementsByTagName("input");
		
		var SupplyComment = "";
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
			
			if(page_element.hasAttribute('dbid') && page_element.getAttribute('dbid') == 'SupplyComment')
				SupplyComment = page_element.getAttribute('value');
		}

		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
				
			if(page_element.hasAttribute('dbid') && page_element.getAttribute('dbid') == 'SupplySubType' && page_element.getAttribute('value') == 'true')
			{
				var SupplySubType		= page_element.getAttribute('text');
				
				var data = {};
				
				data['loginId'] = mysql_real_escape_string(loginId);
				data['business_id'] = mysql_real_escape_string(businessId);
				data['businessStatus'] = mysql_real_escape_string(businessStatus);
				data['businessHours'] = mysql_real_escape_string(businessHours);
				data['businessHoursComment'] = mysql_real_escape_string(businessHoursComment);
				data['businessComment'] = mysql_real_escape_string(businessComment);
				data['updateTime'] = updateTime;
				data['SupplyType'] = mysql_real_escape_string(SupplyType);
				data['SupplySubType'] = mysql_real_escape_string(SupplySubType);
				data['SupplyComment'] = mysql_real_escape_string(SupplyComment);
				
				jsonReport.push(data);
			}
		}
		
	}
	// If no supplies, add update anyway
	if(jsonReport.length == 0)
	{
		var data = {};
		data['loginId'] = mysql_real_escape_string(loginId);
		data['business_id'] = mysql_real_escape_string(businessId);
		data['businessStatus'] = mysql_real_escape_string(businessStatus);
		data['businessHours'] = mysql_real_escape_string(businessHours);
		data['businessHoursComment'] = mysql_real_escape_string(businessHoursComment);
		data['businessComment'] = mysql_real_escape_string(businessComment);
		data['updateTime'] = updateTime;
		data['SupplyType'] = "";
		data['SupplySubType'] = "";
		data['SupplyComment'] = "";
		
		jsonReport.push(data);
	}
	
	console.log(JSON.stringify(jsonReport));
	return jsonReport;
}

function processBusinessCitizenReport()
{
	var jsonReport = [];
	
	var data = {};
	data['loginId'] = window.loginUser;
	data['business_id'] = document.getElementById('businessCName').getAttribute('business_id');
	data['businessStatus'] = getAttr('businessStatus');
	data['businessComment'] = mysql_real_escape_string(getAttr('businessComment'));
	data['reportDateTime'] = getDateSQL();
	jsonReport.push(data);
	
	return jsonReport;			
}

function processSpecialReport(terminal)
{
	var loginId					=	window.loginUser;
	var businessId 				=	document.getElementById('businessName').getAttribute('business_id');
	var businessStatus 			=	getAttr('businessStatus');
	var businessHours			=	getAttr('businessHours');
	var businessHoursComment	=	mysql_real_escape_string(getAttr('businessHoursComment'));
	var businessComment			=	getAttr('businessComment');
	var updateTime				=	getDateSQL();
	
	
	var jsonReport = [];
	
	var reportField = 'special_services';
	
	if(terminal == true)
	{
		console.log("Special report: terminal");
		reportField = 'home';
	}
				
	var services = document.getElementById(reportField).children;
	
	
	for (var i = 0; i < services.length; i++)
	{
		var service = services[i];
		
		var svid = service.getAttribute("id");
		
		if(!svid) svid = "";
		
		if(terminal == true && svid.indexOf("container") > -1)
			if(svid.indexOf("scontainer") > -1) // hack for container entry
			{
				if(document.getElementById(svid.replace("scontainer", "s")) != null)
					service = document.getElementById(svid.replace("scontainer", "s"));
			}
			else
			{
				if(document.getElementById(svid.replace("container", "")) != null)
					service = document.getElementById(svid.replace("container", ""));
			}
		
		if(service.hasAttribute("dbid"))
		{
			
			if(service.hasAttribute("value"))
			{
				var SupplyType = service.getAttribute("dbid");
				var SupplySubType = service.getAttribute("value");

				var data = {};
				
				data['loginId'] = loginId;
				data['business_id'] = businessId;
				data['businessStatus'] = businessStatus;
				data['businessHours'] = businessHours;
				data['businessHoursComment'] = businessHoursComment;
				data['businessComment'] = mysql_real_escape_string(businessComment);
				data['updateTime'] = updateTime;
				data['SupplyType'] = SupplyType;
				data['SupplySubType'] = SupplySubType;
				data['SupplyComment'] = "";
				
				jsonReport.push(data);
			}
		}
	}
	// If no supplies, add update anyway
	if(jsonReport.length == 0)
	{
		var data = {};
		data['loginId'] = loginId;
		data['business_id'] = businessId;
		data['businessStatus'] = businessStatus;
		data['businessHours'] = businessHours;
		data['businessHoursComment'] = businessHoursComment;
		data['businessComment'] = mysql_real_escape_string(businessComment);
		data['updateTime'] = updateTime;
		data['SupplyType'] = "";
		data['SupplySubType'] = "";
		data['SupplyComment'] = "";
		
		jsonReport.push(data);
	}
	console.log(JSON.stringify(jsonReport));
	return jsonReport;
}

function processCitizenReport()
{
	var loginId				=	window.loginUser;
	var reportDateTime		=	getDateSQL();
	
	var jsonReport = [];
	
	var pages = document.getElementById('report').children;
	for (var i = 0; i < pages.length; i++)
	{
		var page = pages[i];
		
		var reportType = page.getAttribute('id');
		var damagePhoto = "";
		
		var page_elements = page.getElementsByTagName("*"); 
		
		console.log("Total page elements: " + page_elements.length);
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
					
			if(page_element.hasAttribute('photoFileName'))
			{
				damagePhoto = page_element.getAttribute('photoFileName');
				console.log("Found photoFileName");
			}
		}
		
		page_elements = page.getElementsByTagName("input");
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
			
			if(page_element.hasAttribute('dbid') && page_element.getAttribute('dbid') == 'ReportSubType' && page_element.getAttribute('value') == 'true')
			{
				var reportSubType		= page_element.getAttribute('text');
				var reportComment		= "";
				var gpsPosition			= "";
				
				if(page_element.hasAttribute('gpsposition'))
					gpsPosition = page_element.getAttribute('gpsposition');
					
				for (var pd = 0; pd < page_elements.length; pd++)
				{
					var delement = page_elements[pd];
					
					//if(delement.getAttribute('parent') == page_element.getAttribute('xml:id'))
					//{
						if(delement.getAttribute('dbid') == 'ReportComment')
						{	
							if(delement.hasAttribute('value'))
								reportComment = delement.getAttribute('value');
						}

					//}
				}
				
				var data = {};

				data['loginId'] = loginId;
				data['reportType'] = reportType;
				data['reportSubType'] = reportSubType;
				data['reportDateTime'] = reportDateTime;
				data['damagePhoto'] = damagePhoto;
				data['reportComment'] = mysql_real_escape_string(reportComment);
				data['gpsPosition'] = gpsPosition;
								
				jsonReport.push(data);
				
			}
		}
	}
	return jsonReport;
}

function processReconReport()
{
	var loginId				=	window.loginUser;
	var reconTeam 			=	mysql_real_escape_string(getAttr('reconTeam'));
	var reportId 			=	mysql_real_escape_string(getAttr('reportId'));
	var reportType			=	getAttr('reportType');
	var targetName			=	mysql_real_escape_string(getAttr('targetName'));
	var reportDateTime		=	getAttr('dateTime');
	
	var jsonReport = [];
	
	// Save recon report
	if(typeof Android !== "undefined")
		Android.setReconTeam($('#reconTeam').attr('value'));
				
	var pages = document.getElementById('report').children;
				
	for (var i = 0; i < pages.length; i++)
	{
		var page = pages[i];
		
		
		var damageType = page.getAttribute('id');
		var damagePhoto = "";
		
		var page_elements = page.getElementsByTagName("*"); 
		
		//console.log("Total page elements: " + page_elements.length);
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
					
			if(page_element.hasAttribute('photoFileName'))
			{
				damagePhoto = page_element.getAttribute('photoFileName');
				console.log("Found photoFileName");
			}
		}
		
		page_elements = page.getElementsByTagName("input");
		
		for (var pe = 0; pe < page_elements.length; pe++)
		{
			var page_element = page_elements[pe];
			
									
			if(page_element.hasAttribute('dbid') && page_element.getAttribute('dbid') == 'DamageSubType' && page_element.getAttribute('value') == 'true')
			{
				
				var damageSubType		= page_element.getAttribute('text');
				var damageModifiers 	= "";
				var damageComments		= "";
				var routeName			= "";
				var gpsPosition			= "";
				
				if(page_element.hasAttribute('gpsposition'))
					gpsPosition = page_element.getAttribute('gpsposition');
					
				for (var pd = 0; pd < page_elements.length; pd++)
				{
					var delement = page_elements[pd];
					
					if(delement.getAttribute('parent') == page_element.getAttribute('xml:id'))
					{
						if(delement.getAttribute('dbid') == 'DamageModifier')
						{	
							// Handle Checkboxes
							if(delement.getAttribute('value') == 'true')
								damageModifiers +=  delement.getAttribute('text') + ':';
							else if(delement.getAttribute('value') != '' && delement.getAttribute('value') != 'false')
								damageModifiers += delement.getAttribute('value') + ':';
						}
						
						else if(delement.getAttribute('dbid') == 'DamageComment')
						{	
							if(delement.hasAttribute('value'))
								damageComments = delement.getAttribute('value');
						}

						else if(delement.getAttribute('dbid') == 'DamageRoute')
						{	
							if(delement.hasAttribute('value'))
								routeName = delement.getAttribute('value');
						}
					}
				}
					
				
				var data = {};

				data['loginId'] = loginId;
				data['reconTeam'] = reconTeam;
				data['reportId'] = reportId;
				data['reportType'] = reportType;
				data['targetName'] = targetName;
				data['reportDateTime'] = reportDateTime;
				
				data['damageType'] = damageType;
				data['damagePhoto'] = damagePhoto;
				data['damageSubType'] = damageSubType;
				data['damageModifiers'] = damageModifiers;
				data['damageComments'] = damageComments;
				data['routeName'] = routeName;
				data['gpsPosition'] = gpsPosition;
								
				jsonReport.push(data);
			}
		}
		
	}
	return jsonReport;
}

