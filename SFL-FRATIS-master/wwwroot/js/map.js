
// Note: To change system's default location
// Please look below at the initialize function
window.showGps = true;
window.showRecon = true;

window.gpsMarkers = new Array();
window.gpsCluster = null;

window.gpsMarker = null;

window.reconCluster = null;
window.lastReconStart = true;
window.lastReconUpdate = "";

window.businessMarkers = new Array();
window.businessTooltips = new Array();
window.lastBusinessStart = true;
window.lastReconUpdate = "";
window.clearingBusiness = false;

window.citizenCluster = null;

window.lastCitizenStart = true;
window.lastCitizenUpdate = "";

window.styledMapOptions = { name: 'US Road Atlas'};
window.usRoadMapType;


var reconUpdater = function()
{
	$.get( "mapfunctions.php?func=lastReconUpdate", function( data ) 
	{
		if(window.lastReconUpdate != data)
		{
			window.lastReconUpdate = data;
			if(window.lastReconStart)
				window.lastReconStart = false
			else
			{
				//Push Update
				console.log( "Updating Recon Markers... last update: " + window.lastReconUpdate + "  new update: " + data);
				
				var newreconElement = document.createElement('script');
				newreconElement.src = 'mapfunctions.php?func=getRecon';
				newreconElement.type = 'text/javascript';
				
				window.Reconelement.innerHTML = '';
				window.Reconelement.appendChild(newreconElement);
			}
		}
	});
}

// Periodically checks for business report updates and reloads new markers
var businessUpdater = function()
{
	$.get( "mapfunctions.php?func=lastBusinessUpdate", function( data ) 
	{
		if(getCookie("business_reports") != "false")
		{
			if(window.lastBusinessUpdate != data)
			{
				window.lastBusinessUpdate = data;
				if(window.lastBusinessStart)
					window.lastBusinessStart = false
				else
				{
					//Push Update
					console.log( "Updating Business Markers... " + data);
					
					clearBusinessMarkers();
					
					var newbusinessElement = document.createElement('script');
					newbusinessElement.src = 'mapfunctions.php?func=getBusiness';
					newbusinessElement.type = 'text/javascript';
					
					window.Businesselement.innerHTML = '';
					window.Businesselement.appendChild(newbusinessElement);
				}
			}
		}
	});
}

// Periodically checks for gps marker updates and reloads new markers
var gpsUpdater = function()
{
	updateMarkers();
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
	
	if(window.loginType == "recon")
		setRecon();
	if(window.loginType == "business")
	{
		setBusiness();
		
		$.get("user_functions.php?func=getBusinessType", function( data ) 
		{
			window.businessType = data;
		});
	}
	
	setCitizen();
}

function setRecon()
{
	$( "#recon_report" ).attr('style', function(i,s) { return s + 'background: #FC6 !important;' });
	$( "#recon_report" ).val("RECON Report");
	$( "#recon_report" ).show();
	
	$('#recon_report').click(function()
	{
		if(typeof Android !== 'undefined')
			Android.reconReport();
		else
		{
			var data = "<iframe width='400' height='475' src='reportpage.php?report=recon.xml'></iframe>";
			$('#userDialog').html( data );
			$( '#userDialog' ).dialog("open");
			$( '#userDialog' ).dialog('option', 'title', 'RECON Report');
		}
	});
}

function setBusiness()
{
	$( "#recon_report" ).attr('style', function(i,s) { return s + 'background: #99C !important;' });	
	$( "#recon_report" ).val("Business Report");
	$( "#recon_report" ).show();
	
	$('#recon_report').click(function()
	{
		if(typeof Android !== 'undefined')
			Android.businessReport();
		else
		{
			var data = "<iframe width='400' height='475' src='reportpage.php?report=business.xml'></iframe>";
			
			// Special Business Reports for depot freight and terminal
			if(window.business_type == 'depot')
				var data = "<iframe width='400' height='475' src='reportpage.php?report=depot.xml'></iframe>";
				
			if(window.business_type == 'terminal')
				var data = "<iframe width='400' height='475' src='reportpage.php?report=terminal.xml'></iframe>";
	
			$('#userDialog').html( data );
			$( '#userDialog' ).dialog("open");
			$( '#userDialog' ).dialog('option', 'title', 'Business Report');
		}
	});
}

function setCitizen()
{
	$( "#citizen_report" ).attr('style', function(i,s) { return s + 'background: #6FF !important;' });	

	$('#citizen_report').click(function()
	{
		if(typeof Android !== 'undefined')
			Android.citizenReport();
		else
		{
			var data = "<iframe width='400' height='475' src='reportpage.php?report=citizen.xml'></iframe>";
			$('#userDialog').html( data );
			$( '#userDialog' ).dialog("open");
			$( '#userDialog' ).dialog('option', 'title', 'Citizen Report');
		}
	});
}

var isMobile = {
    Android: function() {
        return navigator.userAgent.match(/Android/i);
    },
    BlackBerry: function() {
        return navigator.userAgent.match(/BlackBerry/i);
    },
    iOS: function() {
        return navigator.userAgent.match(/iPhone|iPad|iPod/i);
    },
    Opera: function() {
        return navigator.userAgent.match(/Opera Mini/i);
    },
    Windows: function() {
        return navigator.userAgent.match(/IEMobile/i);
    },
    any: function() {
        return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
    }
};

$(document).ready(function() 
{
    if(isMobile.any()) 
        $("#logo_img").hide();
});

var drive_blink = false;

function drivingCallback()
{
	drive_blink = !drive_blink;
	
	if(drive_blink)
		$("#driving_overlay").css("border", "5px dashed red");
	else
		$("#driving_overlay").css("border", "5px dashed black");
	
}

function drivingStarted()
{
	$("#driving_overlay").css("visibility", "visible");
	setInterval(drivingCallback, 500);
}

function drivingEnded()
{
	$("#driving_overlay").css("border", "5px dashed red");
	$("#driving_overlay").css("visibility", "hidden");
}

function hideReconMarkers()
{
	window.Reconelement.parentNode.removeChild(window.Reconelement);
	window.Reconelement.innerHTML = "";
	window.reconCluster.clearMarkers();
}

function showReconMarkers()
{
	window.Reconelement = document.createElement('script');
	window.Reconelement.src = 'mapfunctions.php?func=getRecon';
	window.Reconelement.type = 'text/javascript';
	var scripts = document.getElementsByTagName('script')[0];
	scripts.parentNode.insertBefore(window.Reconelement, scripts);
}

function hideCitizenMarkers()
{
	window.Citizenelement.parentNode.removeChild(window.Citizenelement);
	window.Citizenelement.innerHTML = "";
	window.citizenCluster.clearMarkers();
}

function showCitizenMarkers()
{
	window.Citizenelement = document.createElement('script');
	window.Citizenelement.src = 'mapfunctions.php?func=getCitizen';
	window.Citizenelement.type = 'text/javascript';
	var scripts = document.getElementsByTagName('script')[0];
	scripts.parentNode.insertBefore(window.Citizenelement, scripts);
}

function initialize() 
{
	//Different Locations
	var locationSeattle = new google.maps.LatLng(47.7, -122.2);
	var locationMiami = new google.maps.LatLng(25.787676, -80.224145);
	
	
	//This is the location of the current system
	//Change this variable to change where the map centers by default
	window.defaultLocation = locationMiami; 
	
	window.defaultZoom = 11;
	window.gpsZoom = 15;
	window.watchID = -1;

	document.getElementById('map-canvas').style.width = '100%';
	document.getElementById('map-canvas').style.height = '100%'; 
	
	google.maps.visualRefresh = true;	
			
	var mapOptions = {
		zoom: window.defaultZoom,
		maxZoom: 16,
		center: window.defaultLocation,
		panControl: false,
		zoomControl: true,
		zoomControlOptions: {
			style: google.maps.ZoomControlStyle.LARGE,
			position: google.maps.ControlPosition.LEFT_BOTTOM
		},
		mapTypeControl: false,
		scaleControl: true
		};
		
	map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);
	
	geocoder = new google.maps.Geocoder();
	
	window.gpsCluster = new MarkerClusterer(map, window.gpsMarkers, {gridSize: 30,minimumClusterSize: 1, styles: gpsStyles});
	window.gpsCluster.setCalculator(gpsCalculator);   

	window.reconCluster = new MarkerClusterer(map, new Array(), {gridSize: 100, minimumClusterSize: 1, recon_mode: 1, styles: reconStyles, zoomOnClick: false});
	window.reconCluster.setCalculator(reconCalculator);  
	
	window.citizenCluster = new MarkerClusterer(map, new Array(), {gridSize: 100, minimumClusterSize: 1, citizen_mode: 1, styles: citizenStyles, zoomOnClick: false});
	window.citizenCluster.setCalculator(reconCalculator);  
	
	window.mouse_down = false;
	
	google.maps.event.addListener(map, 'mousedown', function(e) 
	{
		$("#service_selector").hide();
		$("#services").hide();	
		$("#map_actions").hide();
		$("#map_filter").hide();
		
		window.mouse_down = true;
	});
	
	google.maps.event.addListener(map, 'mouseup', function(e) 
	{
		window.mouse_down = false;
	});
	
	google.maps.event.addListener(map, 'center_changed', function(e) 
	{
		if(window.mouse_down)
    		locationMapPanned();
    		
    	mapChanged();
	});

	var Customelement = document.createElement('script');
	Customelement.src = 'js/mapcustom.js';
	Customelement.type = 'text/javascript';
	var scripts = document.getElementsByTagName('script')[0];
	scripts.parentNode.insertBefore(Customelement, scripts);

	var eventLoadListener = function (e) 
	{ 
		// Load Recon
		window.Reconelement = document.createElement('script');
		window.Reconelement.src = 'mapfunctions.php?func=getRecon';
		window.Reconelement.type = 'text/javascript';
		var scripts = document.getElementsByTagName('script')[0];
		scripts.parentNode.insertBefore(window.Reconelement, scripts);
		
		// Load Business
		window.Businesselement = document.createElement('script');
		window.Businesselement.src = 'mapfunctions.php?func=getBusiness';
		window.Businesselement.type = 'text/javascript';
		//var scripts = document.getElementsByTagName('script')[0];
		scripts.parentNode.insertBefore(window.Businesselement, scripts);
		
		// Load Business
		window.Citizenelement = document.createElement('script');
		window.Citizenelement.src = 'mapfunctions.php?func=getCitizen';
		window.Citizenelement.type = 'text/javascript';
		//var scripts = document.getElementsByTagName('script')[0];
		scripts.parentNode.insertBefore(window.Citizenelement, scripts);
	
		// Enable update checking for business/recon reports
		setInterval(reconUpdater,5000);
		setInterval(businessUpdater,5000);
		setInterval(gpsUpdater,60000);
		
		getCookies();
	};
	
	if (Customelement.addEventListener) 
    	Customelement.addEventListener("load", eventLoadListener, false);
	else 
	    Customelement.attachEvent("onload", eventLoadListener);	
}

$(window).unload(function()
{
	//Store last map location
	var c = map.getCenter();
	setCookie("lastMap", c.lat() + ',' + c.lng() + ',' + map.getZoom());
});

function showInfo(warningText)
{
	$( "#infoDialog" ).html(warningText);
	$( "#infoDialog" ).show();	
}

function hideInfo()
{
	$( "#infoDialog" ).html("");
	$( "#infoDialog" ).hide();		
}

function showWarning(warningText)
{
	$( "#warningDialog" ).html(warningText);
	$( "#warningDialog" ).fadeIn( 1000, function() {
		$( "#warningDialog" ).fadeOut( 3000 );
	});
}

function showError(warningText)
{
	$( "#errorDialog" ).html(warningText);
	$( "#errorDialog" ).fadeIn( 1000, function() {
		$( "#errorDialog" ).fadeOut( 3000 );
	});
}
	
function furtherSetup()
{
	window.usRoadMapType = new google.maps.StyledMapType(
	  roadAtlasStyles, window.styledMapOptions);

	//Define OSM map type pointing at the OpenStreetMap tile server
	map.mapTypes.set("OSM", new google.maps.ImageMapType({
		getTileUrl: function(coord, zoom) {
			return "http://tile.openstreetmap.org/" + zoom + "/" + coord.x + "/" + coord.y + ".png";
		},
		tileSize: new google.maps.Size(256, 256),
		name: "OpenStreetMap",
		maxZoom: 18
	}));
}

function setCookie(cname,cvalue)
{
	var exdays = 30;
	var d = new Date();
	d.setTime(d.getTime()+(exdays*24*60*60*1000));
	var expires = "expires="+d.toGMTString();
	document.cookie = cname + "=" + cvalue + "; " + expires;
}
function getCookie(cname)
{
	var name = cname + "=";
	var ca = document.cookie.split(';');
	for(var i=0; i<ca.length; i++) 
	{
	  var c = ca[i].trim();
	  if (c.indexOf(name)==0) return c.substring(name.length,c.length);
	}
	return "";
}

var gps_seq = 0;
var ticker_counter;
var gps_ticker = function ()
{
    $("#gps_image").attr("src","./images/gps/gps_" + gps_seq + ".png");
    if(gps_seq < 3)
        gps_seq++;
    else
        gps_seq = 0;   
}

//Android Friendly Position
function repositionMap(lat, lon)
{
	var currentLocation = new google.maps.LatLng(lat, lon);
	map.setCenter(currentLocation);
	if(window.gpsMarker)
	{
	    window.gpsMarker.setLocation(currentLocation);
	    window.gpsMarker.draw();
    }
}

function prepareMap()
{
	map.setZoom(window.gpsZoom);
	
	if(window.gpsMarker == null)
	{
    	window.gpsMarker = new GPSOverlay();
    	window.gpsMarker.setMap(map);
    }
}


function showLocation(position) {
  var latitude = position.coords.latitude;
  var longitude = position.coords.longitude;
  repositionMap(latitude, longitude);
}

function errorHandler(err) {
  if(err.code == 1) {
	showWarning("Error: Access is denied!");
  }else if( err.code == 2) {
	showWarning("Error: Position is unavailable!");
  }
}
function startLocationUpdate()
{
    ticker_counter = setInterval(gps_ticker,1000);
	if(typeof Android !== 'undefined')
	{
    	Android.startLocationUpdate();
    	prepareMap();
	}
	else
	{
        if(navigator.geolocation){
          var options = {timeout:1000,frequency: 1 };
          prepareMap();
          window.watchID = navigator.geolocation.watchPosition(showLocation, errorHandler, options);
          
        }else{
          showWarning("Sorry, browser does not support geolocation!");
          clearInterval(ticker_counter);
        }
    }
}

function locationMapPanned()
{
	if(typeof Android !== 'undefined')
    	Android.locationMapPanned();
	
	if(window.watchID > -1)
	{
	    navigator.geolocation.clearWatch(window.watchID);	  
	    window.watchID = -1;  
    }
    
    clearInterval(ticker_counter);
    
    $("#gps_image").attr("src","./images/gps/gps_off.png");
}

function stopLocationUpdate()
{
    clearInterval(ticker_counter);
    $("#gps_image").attr("src","./images/gps/gps_off.png");
    
	if(typeof Android !== 'undefined')
    	Android.stopLocationUpdate();

	window.gpsMarker.setMap(null);
	window.gpsMarker = null;
	
	if(window.watchID > -1)
	{
	    navigator.geolocation.clearWatch(window.watchID);
	    window.watchID = -1;
    }
}

function businessOverlay()
{
	clearBusinessMarkers();
	
	var services_search = "";
	
	$('#services').find('label.ui-state-active').each(function() 
	{
		var service = $("#" + $(this).attr("for"));
		services_search += service.attr("service") + ",";
	});
	
	services_search = services_search.substring(0, services_search.length - 1);
	
	var newbusinessElement = document.createElement('script');
	newbusinessElement.src = 'mapfunctions.php?func=getBusiness&services=' + services_search;
	newbusinessElement.type = 'text/javascript';
	
	window.Businesselement.innerHTML = '';
	window.Businesselement.appendChild(newbusinessElement); 
}
  
function toggleService(val)
{
	$( "#gps_service").attr('checked',val).button("refresh");
}

function limitServices()
{
	var max = $(document).height() - $('#services').offset().top - 20;
	$('#services').css('max-height',max);
	$("#services").niceScroll({horizrailenabled:false, autohidemode:false});
}

$(window).resize(function() 
{
	limitServices();
});
  
// JQuery immediate stuff
$(function() 
{
	$( "#recon_report" ).hide();
	$( "#map_filter_holder_phone" ).hide();
	$( "#map_actions_holder_phone" ).hide();
	
	if(typeof Android !== 'undefined')
		Android.windowLoaded();
	
	setCitizen();
	
	$( "#my_location").click(function( event ) 
	{
		if(!this.checked)
		{
			setCookie("my_location","false");
			stopLocationUpdate();
			getCookies();
		}
		else
		{
        	var c = map.getCenter();
        	setCookie("lastMap", c.lat() + ',' + c.lng() + ',' + map.getZoom());
        	
			setCookie("my_location","true");
			startLocationUpdate();
		}
		$( "#my_location label").removeClass( "ui-state-hover" )
	});
	
	$( "#gps_service").click(function( event ) 
	{
		if(typeof Android !== 'undefined')
	    	Android.toggleService();
	
		$( "#gps_service label").removeClass( "ui-state-hover" )
	});
	
	$( "#errorDialog" ).hide();
	$( "#warningDialog" ).hide();
	$( "#infoDialog" ).hide();	
	$( "#map_type" ).buttonset();
	$( "#map_filter" ).buttonsetv();
	$( "#map_actions" ).buttonsetv();
	$( "#service_selector" ).buttonset();
	$( "#services" ).buttonsetv();
	$( "#services").hide();
	$( "#service_selector").hide();
	$( "#search_services" ).attr('style', function(i,s) { return s + 'background: #6F3 !important;' }); 
	$( "#close_services" ).attr('style', function(i,s) { return s + 'background: #39C !important;' });	
	
	$('#services').find('label').each(function() 
	{
		$(this).width("140px");
	}); 
	
	$('#services').find('*').each(function() 
	{
		$(this).click(function(e) 
		{
			e.stopPropagation();
			if($(this).prop('tagName') != "DIV")
			{
				setTimeout(function() 
				{
					  businessOverlay();
				}, 100);
			}
		});
	});
	
	$('#map_filter').find('label').each(function() 
	{
		$(this).width("150px");
	});
	
	$("#search_services").click(function(e) 
	{
		e.stopPropagation();
		businessOverlay();
	});
		  
	$("#check_services").click(function(e) 
	{
		e.stopPropagation();
		hideOtherMarkers();
		$("#actions_header").text("Check Services");
		$("#services").show();	
		limitServices();
		
		$("#services").niceScroll({horizrailenabled:false, autohidemode:false});
		$("#map_actions").hide();
	});
	
	$("#close_services").click(function(e) 
	{
		e.stopPropagation();
		$("#actions_header").text("Actions");
		$("#services").hide();
		$("#map_actions").show();
		
		showOtherMarkers();
		getCookies();
	});
	
	$("#filter_header").click(function() 
	{
	  if($("#map_filter").is(":visible"))
		$("#map_filter").hide();
	  else
	  {
		$("#map_actions").hide();
		$("#map_filter").show();
	  }
	}).blur(function() {
		$("#map_filter").hide();
	});
	
	$("#actions_header").click(function() 
	{
	  if($("#map_actions").is(":visible") || $("#services").is(":visible"))
	  {
		$("#map_actions").hide();
		$("#services").hide();
		$("#service_selector").hide();
		$("#actions_header").text("Actions");
	  }
	  else
	  {
		$("#map_filter").hide();
		$("#map_actions").show();
	  }
	}).blur(function() 
	{
		$("#map_actions").hide();
	});

	$( "#google_traffic").click(function( event ) {
		if(!this.checked)
		{
			setCookie("google_traffic","false");
			window.trafficLayer.setMap(null);
		}
		else
		{
			setCookie("google_traffic","true");
			window.trafficLayer.setMap(map);
		}
	  });
	  
	$( "#gps_traces").click(function( event ) {
		if(!this.checked)
		{
			setCookie("gps_traces","false");
			window.showGps = false;
			window.gpsCluster.clearMarkers();
		}
		else
		{
			setCookie("gps_traces","true");
			window.showGps = true;
			updateMarkers();
		}
	});
	
	
	$( "#citizen_reports").click(function( event ) {
		if(!this.checked)
		{
			setCookie("citizen_reports","false");
			hideCitizenMarkers();
		}
		else
		{
			setCookie("citizen_reports","true");
			showCitizenMarkers();
		}
	});
	
	$( "#business_reports").click(function( event ) {
		if(!this.checked)
		{
			setCookie("business_reports","false");

			clearBusinessMarkers();
		}
		else
		{
			setCookie("business_reports","true");
			
			var newbusinessElement = document.createElement('script');
			newbusinessElement.src = 'mapfunctions.php?func=getBusiness';
			newbusinessElement.type = 'text/javascript';
			
			window.Businesselement.innerHTML = '';
			window.Businesselement.appendChild(newbusinessElement);
		}
	});
	
	$( "#recon_reports").click(function( event ) {
		if(!this.checked)
		{
			setCookie("recon_reports","false");		
			hideReconMarkers();
		}
		else
		{
			setCookie("recon_reports","true");
			showReconMarkers();
		}
	});
	  
	
	$( "#map_osm" ).button({
	  label: 'Open Street Map',
	  icons: {primary: 'ui-icon-map-osm', secondary: null}
	}).click(function( event ) {
		map.setMapTypeId('OSM');
	  });
	
	$( "#map_satellite" ).button({
	  label: 'Satellite',
	  icons: {primary: 'ui-icon-map-satellite', secondary: null}
	}).click(function( event ) {
		map.setMapTypeId(google.maps.MapTypeId.HYBRID);
	  });
	  
	$( "#map_road" ).button({
	  label: 'Road',
	  icons: {primary: 'ui-icon-map-road', secondary: null}
	}).click(function( event ) {
		map.setMapTypeId('usroadatlas');
	  });
});
  
  
function getCookies()
{
	//My Location Cookie
	if(getCookie("my_location") == "true")
	{
		$( "#my_location").checked = true;
		$( "#my_location").trigger( "click" );
	}
	if(getCookie("my_location") == "false")
		$( "#my_location").checked = false;
		
	//Google Traffic Cookie
	if(getCookie("google_traffic") == "true")
	{
		$( "#google_traffic").checked = true;
		$( "#google_traffic").trigger( "click" );
	}
	if(getCookie("google_traffic") == "false")
	{
		$( "#google_traffic").checked = false;
		$( "#google_traffic").trigger( "click" );
	}	
	
	//GPS Traces Cookie
	if(getCookie("gps_traces") == "true")
	{
		$( "#gps_traces").checked = true;
		$( "#gps_traces").trigger( "click" );
	}
	if(getCookie("gps_traces") == "false")
	{
		$( "#gps_traces").checked = false;
		$( "#gps_traces").trigger( "click" );
	}	
	
	//Business Cookie
	if(getCookie("business_reports") == "true")
	{
		$( "#business_reports").checked = true;
		$( "#business_reports").trigger( "click" );
	}
	if(getCookie("business_reports") == "false")
	{
		$( "#business_reports").checked = false;
		$( "#business_reports").trigger( "click" );
	}
	
	//Citizen Cookie
	if(getCookie("citizen_reports") == "true")
	{
		$( "#citizen_reports").checked = true;
		$( "#citizen_reports").trigger( "click" );
	}
	if(getCookie("citizen_reports") == "false")
	{
		$( "#citizen_reports").checked = false;
		$( "#citizen_reports").trigger( "click" );
	}
	
	//Recon Cookie
	if(getCookie("recon_reports") == "true")
	{
		$( "#recon_reports").checked = true;
		$( "#recon_reports").trigger( "click" );
	}
	if(getCookie("recon_reports") == "false")
	{
		$( "#recon_reports").checked = false;
		$( "#recon_reports").trigger( "click" );
	}
	
	var lastMap = getCookie("lastMap");
	if(lastMap != "")
	{
		var lmParms = lastMap.split(',');
		var loc = new google.maps.LatLng(parseFloat(lmParms[0]), parseFloat(lmParms[1]));
		var zoom = parseFloat(lmParms[2]);
		
		map.setCenter(loc);
		map.setZoom(zoom);
	}	
	

	$("#service_selector").hide();
	$("#services").hide();	
	$("#map_actions").hide();
	$("#map_filter").hide();
	$( "#map_filter_holder_phone" ).fadeIn( 2000 );
	$( "#map_actions_holder_phone" ).fadeIn( 2000 );
}
  
function hslToRgb(h, s, l){
	var r, g, b;

	if(s == 0){
		r = g = b = l; // achromatic
	}else{
		function hue2rgb(p, q, t){
			if(t < 0) t += 1;
			if(t > 1) t -= 1;
			if(t < 1/6) return p + (q - p) * 6 * t;
			if(t < 1/2) return q;
			if(t < 2/3) return p + (q - p) * (2/3 - t) * 6;
			return p;
		}

		var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
		var p = 2 * l - q;
		r = hue2rgb(p, q, h + 1/3);
		g = hue2rgb(p, q, h);
		b = hue2rgb(p, q, h - 1/3);
	}
	return [Math.floor(r * 255), Math.floor(g * 255), Math.floor(b * 255)];
}
  
  
function numberToColorHsl(i) 
{
	var hue = i * 1.2 / 360;
	var rgb = hslToRgb(hue, 1, .5);
	return 'rgb(' + rgb[0] + ',' + rgb[1] + ',' + rgb[2] + ')'; 
}


var gpsCalculator = function(markers, numStyles){
	//create an index for icon styles
	var index = 0,
		count = markers.length;
	var totalSpeed = 0;
	
	for(var m = 0; m < count; m++)
		totalSpeed += parseInt(markers[m].getTitle());

	index = Math.min(Math.round(totalSpeed / count / 5), numStyles);

	return {
		title: 'Average speed: ' + Math.round(totalSpeed / count),
		text: '',
		index: index
	};
};

var reconCalculator = function(markers, numStyles)
{
	//create an index for icon styles
	var index = 0,
		count = markers.length;

	index = Math.min(Math.round(count/1.5), numStyles);

	return {
		title: 'Total Reports in Area: ' + count,
		text: count,
		index: index
	};
};

var start;
var last;
var profile = false;

function profileStart()
{
	start = new Date().getTime();
}

function profileProfile(text)
{
	if(profile)
	{
		last = new Date().getTime();
		console.log( text  + (last - start) + "ms");
		start = last;
	}
}


function updateMarkers() 
{
	if(window.showGps)
	{
		var bounds = map.getBounds();
		if(bounds)
		{
			var lat1 = bounds.getNorthEast().lat();
			var lat2 = bounds.getSouthWest().lat();
			var lon1 = bounds.getNorthEast().lng();
			var lon2 = bounds.getSouthWest().lng();
			var last;
			
			profileStart();
			
			jQuery.ajax({
				url:	'mapfunctions.php?func=getMarkers&lat1=' + lat1 + "&lat2=" + lat2 + "&lon1=" + lon1 + "&lon2=" + lon2,
				async: true,
				success: function(result) {
					profileProfile("Got result: ");
					
					jsonMarkers = JSON.parse(result);
					window.gpsCluster.clearMarkers();
					
					profileProfile("Cleared Markers: ");
					
					for(var m = 0; m < jsonMarkers.length; m++)
					{
						var gpsData = jsonMarkers[m];
						var markerPosition = new google.maps.LatLng(gpsData['Latitude'], gpsData['Longitude']);
						
						var marker = new google.maps.Marker({
							position: markerPosition,
							title: "" + gpsData['Speed']
						});
																		
						window.gpsCluster.addMarker(marker, true);
					}
					
					profileProfile("Added Markers: ");

					window.gpsCluster.repaint();
					profileProfile("Repainted: ");
				}
			}); 
		}
	}
}

var changedTimeout;

function mapChanged()
{
	clearTimeout(changedTimeout);
	changedTimeout = setTimeout(function() {
		updateMarkers();
	}, 1000);
}


// User Functions------------------------------------------------------------------------------------------------------
var businessListener;

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

$(document).ready(function()
{
	$( "#userDialog" ).dialog({
							  resizable: false,
							  autoOpen: false,
							  show: {effect: 'fade', duration: 250},
							  hide: {effect: 'fade', duration: 500},
							  width: 'auto'});
	window.userDialog = $( "#userDialog" );
	resizeComponents();
	$( window ).resize(resizeComponents);		 
});

function hideUI()
{
	$("#map_filter_holder").hide();
	$("#map_actions_holder").hide();
	$("#map_filter_holder_phone").hide();
	$("#map_actions_holder_phone").hide();
	$("#toolbar").hide();
	$("#toolbarbg").hide();
	$("#map_type").hide();
}

function showUI()
{
	$("#map_filter_holder").show();
	$("#map_actions_holder").show();
	$("#map_filter_holder_phone").show();
	$("#map_actions_holder_phone").show();
	$("#toolbar").show();
	$("#toolbarbg").show();
	$("#map_type").show();
}

function isFunction(object) 
{
	return (typeof(object) == "function");
}

function manualMarker(type, callback)
{
	if(isFunction(callback))
		window.manualCallback = callback;
	
	window.manualType = type;

	hideOtherMarkers();
	setTimeout(function ()
	{
		var image = {
		  url: 'images/business/' + type + '.png',
		  size: new google.maps.Size(30, 30),
		  origin: new google.maps.Point(0, 0),
		  anchor: new google.maps.Point(15, 15),
		  scaledSize: new google.maps.Size(30, 30)
		};
		var mapcenter = map.getCenter();
	
		window.floatingMarker = new google.maps.Marker({
		  icon: image,
		  title: "New Business",
		  position: mapcenter
		});
		
		hideUI();
				
		showInfo("Please click map where you would like to place icon. <br> <div id='marker_ok'>Done</div> <div id='marker_cancel'>Cancel</div>");
		
		$("#marker_cancel").button({
		  icons: {
			primary: "ui-icon-circle-close"
		  }});
		  
		$("#marker_ok").button({
		  icons: {
			primary: "ui-icon-circle-check"
		  }});
		  
		$("#marker_ok").click(function (){
			hideInfo();

			google.maps.event.removeListener(window.floatingListener);
			
			if(isFunction(window.manualCallback))
			{
				window.manualCallback(window.floatingMarker.getPosition());
				window.manualCallback = null;
			}
			else
			{
				//No callback so return to normal
				showUI();
				window.floatingMarker.setMap(null); 
			}
		});
		
		$("#marker_cancel").click(function (){ hideInfo();	showUI(); showOtherMarkers(); window.floatingMarker.setMap(null); });
				
		window.floatingListener = google.maps.event.addListener(map, 'click', function(event) 
		{
			window.floatingMarker.setMap(map);
			//console.log("yolo:" + window.floatingMarker);
			window.floatingMarker.setPosition(event.latLng);
		});
	
	}, 100) ;
}

function codeAddress(address, callback) 
{
	geocoder.geocode( { 'address': address}, function(results, status) {
	  if (status == google.maps.GeocoderStatus.OK) {
		map.setCenter(results[0].geometry.location);
		
		var image = {
		  url: 'images/business/' + window.manualType + '.png',
		  size: new google.maps.Size(30, 30),
		  origin: new google.maps.Point(0, 0),
		  anchor: new google.maps.Point(15, 15),
		  scaledSize: new google.maps.Size(30, 30)
		};
		
		var marker = new google.maps.Marker({
			icon: image,
			map: map,
			position: results[0].geometry.location,
			title: "New Business"
		});
		callback(results[0].geometry.location);
	  } else {
		showError("Can't find location!!!");
	  }
	});
  }

function markerTest()
{
	manualMarker("freight");
}


function resizeComponents()
{
	$( "#menuContainer" ).css({'left': $( window ).width() - $( "#menuContainer" ).width()});
	$( "#menuContainer" ).css({'top': $( "#menuButton" ).height()});
}

// Function for finding owner's business
function findBusiness(type, bname)
{
	hideOtherMarkers();
	var btype = [];
	
	if(type != "other")
	{
		if(type == "hospital")
			btype = ['hospital','doctor'];
		else
			btype = [type];
	}
	
	if(type == "freight")
	{
		$.get( "ports.php", function( data ) 
		{
			var results = JSON.parse(data);
			createMarkers(results, type);
		});
	}
	else
	{
		var request = {
			bounds: map.getBounds(),
			types: btype,
			name: bname
		};
		
		service = new google.maps.places.PlacesService(map);
		service.nearbySearch(request, function callback(results, status) 
		{
			if (status == google.maps.places.PlacesServiceStatus.OK) 
			{	
				createMarkers(results, type);
			}
		});
	}
}

function clearBusinessMarkers()
{
	window.businessMarkers.forEach(function(marker) 
	{
		marker.setMap(null)
	}); 

	window.businessMarkers = [];
}

function createMarker(place, type)
{
		var image = {
		  url: 'images/business/' + type + '.png',
		  size: new google.maps.Size(30, 30),
		  origin: new google.maps.Point(0, 0),
		  anchor: new google.maps.Point(15, 15),
		  scaledSize: new google.maps.Size(30, 30)
		};
		
		var marker = new google.maps.Marker({
		  map: map,
		  icon: image,
		  title: place.name,
		  id: place.id,
		  reference: place.reference,
		  type: type,
		  position: place.geometry.location
		});
		
		google.maps.event.addListener(marker, 'click', function(elementId) 
		{
			//console.log("Clicked marker, adding business: id:" + marker.id + " type:" + marker.type + " title:" + marker.title);
			console.log("Clicked Marker: " + marker.title);
			showWarning("Set business to: " + marker.title);
			hideInfo();
			showUI();
			google.maps.event.removeListener(businessListener);
			attachBusiness(marker.id);
			addBusinessInfo(marker.id, marker.type, marker.title, JSON.stringify(marker.position), marker.icon.url);
			google.maps.event.clearListeners(map, 'bounds_changed');
			clearBusinessMarkers();
			showOtherMarkers();
		});
		window.businessMarkers.push(marker);
}


function createMarkers(places, type) {
	
	clearBusinessMarkers();
	for (var i = 0; i<places.length; i++)
	{
		var place = places[i];
		createMarker(place, type);
	}
	
}

function showOtherMarkers()
{
	if(getCookie("gps_traces") == "true")
	{
		window.showGps = true;
		updateMarkers();
	}
	
	if(getCookie("google_traffic") == "true")
		trafficLayer.setMap(map);
	
	if(getCookie("recon_reports") == "true")
		showReconMarkers();

	
	if(getCookie("citizen_reports") == "true")
		showCitizenMarkers();

	
	if(getCookie("business_reports") == "true")
	{
		clearBusinessMarkers();
		
		var newbusinessElement = document.createElement('script');
		newbusinessElement.src = 'mapfunctions.php?func=getBusiness';
		newbusinessElement.type = 'text/javascript';
		
		window.Businesselement.innerHTML = '';
		window.Businesselement.appendChild(newbusinessElement);
	}
}

function hideOtherMarkers()
{
	window.showGps = false;
	window.gpsCluster.clearMarkers();
	window.gpsCluster.repaint();

	trafficLayer.setMap(null);
	clearBusinessMarkers();
	hideReconMarkers();
	hideCitizenMarkers();
}

function logoutUser()
{
	$.get("user_functions.php?func=logout", function( data ) 
	{
		location.reload();
	});
}

function addBusinessInfo(business_id, business_type, business_name, business_location, business_icon)
{
	console.log("Adding business info: " + business_name);
	business_location = business_location.replace("k", "d");
	business_location = business_location.replace("A", "e");
	business_location = business_location.replace("B", "e");
	
	var encodeURL = "user_functions.php?func=add_business_info&business_id=" + encodeURIComponent(business_id) + "&business_type=" + business_type + "&business_name=" + encodeURIComponent(business_name) + "&business_location=" + encodeURIComponent(business_location) + "&business_icon=" + encodeURIComponent(business_icon);
	console.log("data: " + encodeURL);
	$.get(encodeURL, function( data ) {});
}

function attachBusiness(business_id)
{
	console.log("Attaching business: " + business_id);
	$.get("user_functions.php?func=attach_business&business_id=" + business_id, function( data ) 
	{
		console.log("Attached Business; Response: " + data);
		if(data == "ok")
		{
			// Build blank business report so icon stays on screen.
			var jsonReport = [];
			
			var data = {};
			data['loginId'] = window.loginUser;
			data['business_id'] = business_id;
			data['businessStatus'] = "open";
			data['businessHours'] = "regular";
			data['businessHoursComment'] = "";
			data['updateTime'] = getDateSQL();
			data['SupplyType'] = "";
			data['SupplySubType'] = "";
			data['SupplyComment'] = "";
			
			jsonReport.push(data);
			jsonReport = JSON.stringify(jsonReport)
			
			$.ajax({
				type: 'POST',
				url: 'reportpage.php?post=1&report=business.xml',
				dataType: 'json',
				async: true,
				data: {'jsonReport': jsonReport},
				success: function(msg) 
				{
					console.log("Added empty business report.");
					location.reload();
				},
				error: function (xhr, ajaxOptions, thrownError) {
					console.log("Error:" + thrownError);
				}
			});
			
		}
		
	});
}

function updateBusiness()
{
	$.get("user_functions.php?func=businessForm", function( data ) 
	{
		$('#userDialog').html( data );
		$( '#userDialog' ).dialog("open");
		$( '#userDialog' ).dialog('option', 'title', 'Business Owner Registration');
		$( "#businessType" ).buttonsetv();

		$( "#businessFind" ).button({
		  icons: {
			secondary: "ui-icon-search"
		  }
		});
		
		$( "#businessPlace" ).button({
		  icons: {
			secondary: "ui-icon-flag"
		  }
		});
		
		// Place business marker manually on map and add business info.
		$( "#businessPlace" ).click(function()
		{
			$( '#userDialog' ).dialog("close");
			
			manualMarker($("#businessType :radio:checked").attr('id'), function(loc) 
			{ 
				window.tmpBusinessLoc = loc;
				showInfo("Please enter your business name: <br> <input type='text' id='bname' size='50'><br><div id='bname_ok'>Done</div> <div id='bname_cancel'>Cancel</div>");

				$("#bname_cancel").button({icons: {primary: "ui-icon-circle-close"}});
				$("#bname_ok").button({icons: {primary: "ui-icon-circle-zoomin"}});
				
				$("#bname_ok").click(function()
				{
					var bid = "0001" + Math.round(Math.random() * (1000 - 100) + 9000);
					var btype = window.manualType;
					var bname = $("#bname").val();
					var bloc = JSON.stringify(window.tmpBusinessLoc);
					var bicon = "";
										
					console.log(bid + " " + btype + " " + bname + " " + bloc);
					
					window.manualType = null;
					window.tmpBusinessLoc = null;
					
					addBusinessInfo(bid, btype, bname, bloc, bicon);				
					attachBusiness(bid);
					
					hideInfo();
					
					showWarning("Set business to: " + bname);
					showUI();
					showOtherMarkers();
				});
				
				$("#bname_cancel").click(function()
				{
					window.tmpBusinessLoc = null;
					showOtherMarkers();
					window.floatingMarker.setMap(null);
					showUI();
				});
				
			});
		});
		
		// Search for business by address
		$( "#businessAddress" ).button({
		  icons: {
			secondary: "ui-icon-home"
		  }
		});
			
		$( "#businessAddress" ).click(function()
		{
			window.manualType = $("#businessType :radio:checked").attr('id');
			$( '#userDialog' ).dialog("close");
			hideUI();
					
			showInfo("Please enter the address for your business: <br> <input id='baddress' type='text' size='50'><div id='address_find'>Search</div><br><div id='address_ok'>Done</div> <div id='address_cancel'>Cancel</div>");

			$("#address_cancel").button({icons: {primary: "ui-icon-circle-close"}});
			$("#address_ok").button({icons: {primary: "ui-icon-circle-check"}});
			$("#address_find").button({icons: {primary: "ui-icon-circle-zoomin"}});
			$("#address_ok").hide();
			
			$("#address_find").click(function() 
			{
				codeAddress($("#baddress").val(), function(loc)
				{
					$("#address_ok").show();
					window.tmpBusinessLoc = loc;
				});
			});
			
			$("#address_cancel").click(function()
			{
				showOtherMarkers();
				window.floatingMarker.setMap(null);
				showUI();
			});
			
			$("#address_ok").click(function()
			{
				showInfo("Please enter your business name: <br> <input type='text' id='bnamea' size='50'><br><div id='bnamea_ok'>Done</div> <div id='bnamea_cancel'>Cancel</div>");

				$("#bname_cancel").button({icons: {primary: "ui-icon-circle-close"}});
				$("#bnamea_ok").button({icons: {primary: "ui-icon-circle-zoomin"}});
				
				$("#bnamea_ok").click(function()
				{

					var bid = "0001" + Math.round(Math.random() * (1000 - 100) + 9000);
					var btype = window.manualType;
					var bname = $("#bnamea").val();
					var bloc = JSON.stringify(window.tmpBusinessLoc);
					var bicon = "";
										
					console.log(bid + " " + btype + " " + bname + " " + bloc);
					
					window.manualType = null;
					window.tmpBusinessLoc = null;
					
					addBusinessInfo(bid, btype, bname, bloc, bicon);				
					attachBusiness(bid);
					
					hideInfo();
					
					showWarning("Set business to: " + bname);
					showUI();
					showOtherMarkers();
					
				});
				
				$("#bnamea_cancel").click(function()
				{
					window.tmpBusinessLoc = null;
					showOtherMarkers();
					window.floatingMarker.setMap(null);
					showUI();
				});
			});
			
		});
		
		$( '#userDialog' ).dialog('option', 'position', [($(window).width() / 2) - ($( '#userDialog' ).width() / 2), 150]);
		
		$('#businessForm').submit(function(e){
			e.preventDefault();
			
			if($("#businessType :radio:checked").attr('id') == "terminal" || $("#businessType :radio:checked").attr('id') == "depot")
			{
				showWarning("Freight Terminals and Point of Distribution have to be created via Add Business");
			}
			else
			{
				hideUI();
				
				showInfo("Please click the marker which corresponds to your business. <br> <div id='marker_cancel'>Cancel</div>");
				
				$("#marker_cancel").button({icons: {primary: "ui-icon-circle-close"}});
				  
				$("#marker_cancel").click(function (){
					
					google.maps.event.removeListener(businessListener);
					hideInfo();
					
					showUI();
		
					clearBusinessMarkers();
					showOtherMarkers();
				});
				
				businessListener = google.maps.event.addListener(map, 'idle', function() 
				{	
					findBusiness($("#businessType :radio:checked").attr('id'), $('#businessName').val());
				});
				findBusiness($("#businessType :radio:checked").attr('id'), $('#businessName').val());
			
				$('#userDialog').dialog('close');
			}
		});
		
	});
}

function userLogin(message)
{
	$.get("user_functions.php?func=loginForm", function( data ) {
		if(message)
			$( "#userDialog" ).html( '<font color="red">' + message + '</font><br>' + data );
		else
			$( "#userDialog" ).html( data );
			
		$('input:text, input:password, input[type=email]').addClass('ui-textfield ui-corner-all ui-widget ui-state-default');
		
		$( "#userDialog" ).dialog('option', 'title', 'Login');
		$( "#userDialog" ).dialog("open");
		$( "#userLogin" ).button();
		
		$('#loginForm').submit(function(e){
			e.preventDefault();
			
			var userPass = "func=login&" + $("#loginForm").serialize();
			$('#userDialog').html("<center><br><br><img src='images/loading.gif'/><br><br></center>");
			$.post( 
			 "user_functions.php",
			 userPass,
				function(data) 
				{
					if(data == "ok")
					{
						$( "#userDialog" ).dialog("close");
						location.reload();
					}
					else
						userLogin('Error logging in');
				}
			);
			
		
			
		});
	});
			
}

function deleteReport(reportId)
{
	 var result = null;
	 var scriptUrl = "mapfunctions.php?func=deleteReport&id=" + reportId;
	 $.ajax({
		url: scriptUrl,
		type: 'get',
		dataType: 'html',
		async: true,
		success: function(data) {
			if(data == "1")
				showWarning("Report Deleted!");
		} 
	 });	
}

function deleteBCR(reportId)
{
	deleteReport('z' + reportId);
	location.reload();
}


function registerUser(message)
{
	//findBusiness("freight", "harbor");
	
	$.get("user_functions.php?func=registerForm", function( data ) {
		if(message)
			$( "#userDialog" ).html( '<font color="red">' + message + '</font><br>' + data );
		else
			$( "#userDialog" ).html( data );
			
		$('input:text, input:password, input[type=email]').addClass('ui-textfield ui-corner-all ui-widget ui-state-default');
				
		$( "#userType" ).buttonset();
		$( "#userType label" ).css({"width" : "180px"});
		
		$( "#registerSubmit" ).button({
		  icons: {
			secondary: "ui-icon-carat-1-e"
		  }
		});
				
		$( "#userDialog" ).dialog('option', 'title', 'User Registration');
		$( "#userDialog" ).dialog("open");
		$( "#userDialog" ).css('width', $( "#userDialog" ).width());
		
		$('#registerUserForm').submit(function(e){
			e.preventDefault();
			
			var userType = $("#userType :radio:checked").attr('id');
			var sdata = "func=register&" + $("#registerUserForm").serialize() + "&type=" + userType;
			
			$('#userDialog').html("<center><br><br><img src='images/loading.gif'/><br><br></center>");
			console.log("user_functions.php?" + sdata);

			$.post( 
			 "user_functions.php",
			 sdata,
				function(data) 
				{
					console.log("response: " + data);
					if(data == "exists")
					{
						$('#userDialog').dialog("close");
						console.log("Error: User already exists!");
						registerUser("Error: User already exists!");
					}
					else
					{
						if(userType == "business")
						{
							$('#userDialog').dialog("close");
							updateBusiness();
						}
						else
						{
							location.reload();
						}
					}
				}
			);
		});
	});

}

(function( $ )
{
	$.fn.buttonsetv = function() {
	  $(':radio, :checkbox', this).wrap('<div style="margin: 1px"/>');
	  $(this).buttonset();
	  $('label:first', this).removeClass('ui-corner-left').addClass('ui-corner-top');
	  $('label:last', this).removeClass('ui-corner-right').addClass('ui-corner-bottom');
	  mw = 0; // max witdh
	  $('label', this).each(function(index){
		 w = $(this).width();
		 if (w > mw) mw = w; 
	  })
	  $('label', this).each(function(index){
		$(this).width(mw);
	  })
	};
})( jQuery );


///////////////////////////CONSTANTS
var roadAtlasStyles=[
  {
	"featureType": "road.highway",
	"elementType": "geometry",
	"stylers": [
	  { "visibility": "simplified" },
	  { "color": "#dd662b" }
	]
  },{
	"featureType": "landscape.man_made",
	"stylers": [
	  { "color": "#ffffff" }
	]
  },{
	"featureType": "landscape.natural",
	"stylers": [
	  { "color": "#ffffff" }
	]
  },{
	"featureType": "road.local",
	"elementType": "geometry",
	"stylers": [
	  { "visibility": "simplified" },
	  { "color": "#e8e9e8" }
	]
  },{
	"featureType": "poi",
	"stylers": [
	  { "visibility": "off" }
	]
  },{
	"featureType": "transit.line",
	"elementType": "geometry",
	"stylers": [
	  { "visibility": "on" },
	  { "invert_lightness": true }
	]
  }
];

var roadAtlasStyles1= [
	{
	  featureType: 'road.highway',
	  elementType: 'labels',
	  stylers: [
		{ visibility: 'simplified' },
		{ saturation: 98 }
	  ]
	},{
	  featureType: 'administrative.locality',
	  elementType: 'labels',
	  stylers: [
		{ hue: '#0022ff' },
		{ saturation: 50 },
		{ lightness: -10 },
		{ gamma: 0.90 }
	  ]
	},{
	  featureType: 'transit.line',
	  elementType: 'geometry',
	  stylers: [
		{ hue: '#ff0000' },
		{ visibility: 'on' },
		{ lightness: -70 }
	  ]
	},
	  {
		"featureType": "landscape.natural",
		"stylers": [
		  { "color": "#ffffff" }
		]
	  },{
		"featureType": "landscape.man_made",
		"stylers": [
		  { "color": "#fffdf9" }
		]
	  },{
		"featureType": "water",
		"stylers": [
		  { "color": "#d7e9ff" },
		  { "visibility": "simplified" }
		]
	  }
	];

var gpsStyles = [{
url: 'images/gpsMarkers/0.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/5.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/10.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/15.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/20.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/25.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/30.png',
width: 15,
height: 15
},{
url: 'images/gpsMarkers/35.png',
width: 15,
height: 15
}
];

var reconStyles = [{
url: 'images/recon/cluster0.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster1.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster2.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster3.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster4.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster5.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster6.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster7.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster8.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster9.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster10.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster11.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster12.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster13.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster14.png',
width: 40,
height: 40
},{
url: 'images/recon/cluster15.png',
width: 40,
height: 40
}
];

var citizenStyles = [{
url: 'images/citizen/cluster0.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster1.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster2.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster3.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster4.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster5.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster6.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster7.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster8.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster9.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster10.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster11.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster12.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster13.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster14.png',
width: 40,
height: 40
},{
url: 'images/citizen/cluster15.png',
width: 40,
height: 40
}
];
