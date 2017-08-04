<?php
	require 'access.class.php';
	
	// Load User Functions
	$user = new flexibleAccess();
	
	$phone = false;
	if(isset($_REQUEST['phone']))
		$phone = true;
?>

<!DOCTYPE html>
<html lang="en"> 

<head>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	
	<?php if($phone){ ?>
	<meta name="viewport" content="initial-scale=1,minimum-scale=1,maximum-scale=1" />
	<?php }	else { ?>
	<meta name="viewport" content="width=device-width, initial-scale=.75,minimum-scale=.75,maximum-scale=1" />
	<?php } ?>
	
	<title>South Florida Emergency Response System</title>
	
	<link rel="stylesheet" type="text/css" href="css/main.css">
	<link href="css/fratis/jquery-ui-1.10.4.custom.css" rel="stylesheet">
	
	<script src="js/jquery-1.10.2.min.js" type="text/javascript"></script>
	<script src="js/jquery-ui-1.10.4.custom.min.js" type="text/javascript"></script>
	
	<script src="js/Point.js" type="text/javascript"></script>
	<script src="js/map.js" type="text/javascript"></script>
	<script src="js/touchscroll.js" type="text/javascript"></script>
	<script src="js/jquery.nicescroll.js" type="text/javascript"></script>
	<script src="js/markerclusterer.js" type="text/javascript"></script>
	<script src="js/transify.js" type="text/javascript"></script> 
	
	
	<script type="text/javascript">
	
		<?php if($phone){ echo 'window.isPhone = true'; } ?>
		//Error handler hook for saving clientside errors.
		var originalOnError = window.onerror; //Store original for calling later
	
		window.onerror = function(msg, url, line) 
		{
			$.ajax({
				cache: false,
				type: "post",
				data: {msg:msg, user:window.loginUser, url:url, page:'index.php', line:line, browser:navigator.userAgent, post:1},
				url: "error_log.php",
				async: true
			});
			return false;	
		}
		
		var oroginalLog = console.log;
		
		console.log = function(msg)
		{
			$.ajax({
				cache: false,
				type: "post",
				data: {msg:msg, user:window.loginUser, page:'index.php', browser:navigator.userAgent, console:1},
				url: "error_log.php",
				async: true
			});
			oroginalLog.call(console,msg);
		}
		
		//});
		
	
	</script>
	
	<script type="text/javascript">
		
		// Google map constants
		var map;
		var geocoder;
		var icon;
		var markers;
		var oms;
		var boms;
		var trafficLayer;
		
		var AppCacheTimeout = -1;
		function AppCacheReady() 
		{
			if(window.applicationCache)
			{
				clearTimeout(AppCacheTimeout);
				window.applicationCache.removeEventListener('noupdate', arguments.callee, false);
				window.applicationCache.removeEventListener('cached', arguments.callee, false);
			}
			
			var element = document.createElement('script');
			element.src = 'http://maps.google.com/maps/api/js?sensor=true&libraries=places&callback=initialize';
			element.type = 'text/javascript';
			var scripts = document.getElementsByTagName('script')[0];
			scripts.parentNode.insertBefore(element, scripts); 
		
		}
		if (window.applicationCache && 
		    window.applicationCache.status != window.applicationCache.UNCACHED) {
		
		  // Wait at most 2 seconds for an update event
		  AppCacheTimeout = setTimeout(AppCacheReady, 2000);
		
		  //New version of the page, reload to use
		  window.applicationCache.addEventListener('updateready', function () {
		    window.applicationCache.swapCache();
		    location.reload();
		  }, false);
		
		  //Cache manifest file url changed. Reload the page.
		  window.applicationCache.addEventListener('obsolete',function () {
		    window.location.reload(true);
		  }, false);
		  
		  //Everything has loaded - initialize the page
		  window.applicationCache.addEventListener('noupdate', AppCacheReady, false);
		  window.applicationCache.addEventListener('cached', AppCacheReady, false);
		  
		  //Something unexpected happened ...
		  window.applicationCache.addEventListener('error', function() {
		    // provide user feedback - your page is probably broken.
		  }, false);
		} else {
		  AppCacheReady();
		}
		
		function todoList()
		{
			$("#todoList").dialog({
		      height: 500,
		      width: 500
		    },"open");
		}
		
		$(document).ready(function()
		{		  
			$("#todoList").dialog().dialog("close");
			$( "#menu" ).menu().hide().mouseleave(function() {
				$('#menu').hide();
			});;
			$("#menuButton").button({label: "Menu",
			  icons: {
				primary: "ui-icon-carat-1-s"
			  }}).mouseenter(function() { 
				  $( "#menu" ).menu().show();
			  });
			  
		      
		             
			<?php if(!$user->is_loaded()){?>
			$("#registerButton").button({label: "Register",
			  icons: {
				primary: " ui-icon-document-b"
			  }}).click(function() 
			  {
				registerUser();
			  });
			  
			$("#loginButton").button({label: "Login",
			  icons: {
				primary: "ui-icon-person"
			  }}).click(function() 
			  {
				userLogin();
			  });
			<?php }else{ ?>
				window.business_type = '<?php echo $user->get_property("business_type"); ?>';
				
				$("#loginButton").html("Logged in as: <?php echo $user->userID; ?>");
				
				$("#registerButton").button({label: "Logout",
				  icons: {
					primary: " ui-icon-person"
				  }}).click(function() 
				  {
					logoutUser();
				  });
				  
				  window.loginUser = '<?php echo $user->userID; ?>';
				  <?php if($user->get_property("type") == "business") echo "setBusiness();"; ?>
				  <?php if($user->get_property("type") == "recon" || $user->get_property("type") == "admin") echo "setRecon();"; ?>
				  <?php if($user->get_property("type") == "admin") echo "window.loginType = 'admin';"; ?>
			<?php }?>	 
		});
	</script>
</head>

<body>
	<div id="map-canvas"></div>
	
	<div id="gps_image_div">
	    <img id="gps_image" src="images/gps/gps_off.png" width="42" height="48">    
	</div>
	
	<div id="driving_overlay" style="visibility:hidden"></div>
	
	<?php 
		if(!$phone)
		{
			?>	
	    <div id="toolbarbg"></div>
	    <div id="toolbar">
	    	<table width="100%" border="0" cellpadding="0" cellspacing="0">
	        <tr>
	        <td width="10">
	    		<img src="images/logo.png" id="logo_img" width="500" height="70">
	        </td>
	        <td align="left">
	            <div align="center" style="width:100px">
	            	<a href="App/EmergencyResponse.apk" alt="Get the Android App">
	                <img src="images/android_icon_.png" width="50" height="50" border="0">
	                <font size="-2"><?php echo date ("d/m/Y H:i:s", filemtime('./App/EmergencyResponse.apk')); ?></font></a>
	            </div>
			</td>
	        <td valign="top" align="right">
	        	<div id="loginButton"></div>
	        	<div id="registerButton"></div>
	        	<div id="menuButton">Menu</div>
	        </td>
	        </tr>
	        </table>
	    </div>
	        
	    <div id="map_type">
	        <input type="radio" id="map_road" name="map_type" checked="checked"><label for="map_road">Road</label>
	        <input type="radio" id="map_satellite" name="map_type"><label for="map_satellite">Satellite</label>
	        <input type="radio" id="map_osm" name="map_type"><label for="map_osm">Open Street Map</label>
	    </div>
	    
	<?php
		}
		
		if($phone){ ?>
	    <div id="map_filter_holder_phone" class="ui-corner-right ui-widget-content" align="center">
	<?php } else { ?>
	    <div id="map_filter_holder" class="ui-corner-right ui-widget-content" align="center">
	<?php } ?>
	    
	    	<div id="filter_header" class="ui-widget-header ui-corner-tr ui-state-focus phdr" align="center">Map Features</div>
	        <div id="map_filter" align="center">
	        	<input type="checkbox" id="my_location" name="map_filter"><label style="width:100%;" for="my_location"><div class="menu_icon"><img src="images/interface/my_location.png" width="24" height="24"></div><div align="right">My Location</div></label>
	        	<input type="checkbox" checked id="gps_traces" name="map_filter"><label style="width:100%" for="gps_traces"><div class="menu_icon"><img src="images/interface/gps_traces.png" width="24" height="24"></div><div align="right">GPS Traces</div></label>
	            <input type="checkbox" checked id="google_traffic" name="map_filter"><label style="width:100%" for="google_traffic"><div class="menu_icon"><img src="images/interface/google_traffic.png" width="24" height="24"></div><div align="right">Google Traffic</div></label>
	            <input type="checkbox" checked id="business_reports" name="map_filter"><label style="width:100%" for="business_reports"><div class="menu_icon"><img src="images/interface/business_reports.png" width="24" height="24"></div><div align="right">Business Info</div></label>
	            <input type="checkbox" checked id="recon_reports" name="map_filter"><label style="width:100%" for="recon_reports"><div class="menu_icon"><img src="images/interface/damage_reports.png" width="24" height="24"></div><div align="right">RECON Reports</div></label>
	            <input type="checkbox" checked id="citizen_reports" name="map_filter"><label style="width:100%" for="citizen_reports"><div class="menu_icon"><img src="images/interface/citizen_reports.png" width="24" height="24"></div><div align="right">Citizen Reports</div></label>
				<?php if($phone){ ?>
					<input type="checkbox" id="gps_service" name="map_filter"><label style="width:100%" for="gps_service"><div class="menu_icon"><img src="images/interface/gps_service.png" width="24" height="24"></div><div align="right">Record GPS</div></label>	
				<?php } ?>
	        </div>
	    </div>
	    
		<?php if($phone){ ?>
	    <div id="map_actions_holder_phone" class="ui-corner-left ui-widget-content" align="center">
	<?php } else { ?>
	    <div id="map_actions_holder" class="ui-corner-left ui-widget-content" align="center">
	<?php } ?>
	    	<div id="actions_header" class="ui-widget-header ui-corner-tl ui-state-focus phdr" align="center">Actions</div>
	        <div id="map_actions" align="center">
	        	<input type="button" style="width:140px!important;" id="recon_report" name="map_actions" value="RECON Report" />
	        	<input type="button" style="width:140px!important;" id="citizen_report" name="map_actions" value="Citizen Report" />
	        	<input type="button" style="width:140px!important;" id="check_services" name="map_actions" value="Check Services" />
	
	        </div>
	        
	        <div id="service_selector" style="display:none">
	            <input type="radio" id="service_all" name="service_selector" checked="checked"><label for="service_all">All</label>
	            <input type="radio" id="service_closest" name="service_selector"><label for="service_closest">Closest</label>
	        </div>
	        
	        <div id="services" align="center">
	        
		        <input type="checkbox" id="service_freight" class="service_button" name="check_services" service="freight,terminal,depot"><label style="width:100%" for="service_freight"><div style="position:absolute; top:2px; left:0px;"></div><div align="right">Freight Status</div></label>
		        <?php //Read Business Report for Services List
				
					$doc = new DOMDocument();
					$doc->load("reports/business.xml");
			
					$menus = $doc->getElementsByTagName('menu');
					$services = $menus->item(0)->getElementsByTagName('link');
					foreach($services as $srvc)
					{
						$service = $srvc->getAttribute('text');
						$value = $srvc->getAttribute('location');
		        		echo '<input type="checkbox" id="service_' . $service . '" class="service_button" name="check_services" service="' . $value . '"><label style="width:100%" for="service_' . $service . '"><div style="position:absolute; top:2px; left:0px;"></div><div align="right">' . $service . '</div></label>';
					}
		        ?>
	        	<input type="button" style="width:140px!important;" id="search_services" value="Search" />
	        	<input type="button" style="width:140px!important;" id="close_services" value="< Go Back" />
	        </div>
	       </div>
	    </div>
	    
	    <div id="infoDialog" class="ui-state-highlight ui-corner-all" align="center"> dfgdgf</div>
	    
	<?php 
		if(!$phone)
		{
			?>	
	
	    <div id="menuContainer">
	        <ul id="menu">
	          <li><a href="javascript:todoList()">Todo List</a></li>
	          
	          <?php if($user->is_loaded() && $user->get_property("type") == "business"){?>
	          <li><a href="javascript:updateBusiness();">Update Business</a></li>
	          <?php } ?>
	          <li><a href="javascript:markerTest();">Marker Test</a></li>
	  		</ul>
	    </div>
	<?php
		}
		?>
	    
	    <div id="userDialog" align="center"></div>
	    <div id="warningDialog" class="ui-state-highlight ui-corner-all" align="center"> dfgdgf</div>
	    <div id="errorDialog" class="ui-state-error ui-corner-all" align="center"> dfgdgf</div>
	    <div id="todoList" class="todo">
	    	<iframe width="99%" height="98%" src="https://docs.google.com/document/d/1Ax6SWSThheSGyGMVm90fVDM5x2sW7eUsI4MsO-GOrIQ/pub?embedded=true"></iframe>
	    </div>
   	

</body>
</html>