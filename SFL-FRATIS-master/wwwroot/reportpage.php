<?php
	header('Access-Control-Allow-Origin: *'); 
	date_default_timezone_set('America/Los_Angeles'); 
		
	// Configure logs
	ini_set('error_log', getcwd().'/logs/report_error.log');
	
    function logf($msg) 
    {
        $fd = fopen("./logs/report_log.txt", "a"); // write string 
        fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
        fclose($fd); 
    }
		
	// GPS Location
	$phone = false;
	if(isset($_REQUEST['phone']))
	{
		$phone = true;
	}
	else
	{
		require 'access.class.php';
		$user = new flexibleAccess();
	}

	// GPS Location
	$location = "";
	
	if(isset($_REQUEST['location']))
	{
		$location = $_REQUEST['location'];
	}
		
	if(!isset($_REQUEST['post']))
	{
?>


<head>
<link rel="stylesheet" type="text/css" href="css/main.css">
<link href="css/fratis/jquery-ui-1.10.4.custom.css" rel="stylesheet">

<script src="js/jquery-1.10.2.min.js" type="text/javascript"></script>
<script src="js/jquery-ui-1.10.4.custom.min.js" type="text/javascript"></script>
<script src="js/reportpage.js" type="text/javascript"></script>
<script src="js/jquery.nicescroll.js" type="text/javascript"></script>
<meta name="viewport" content="initial-scale=1,minimum-scale=1,maximum-scale=1" />
</head>  

<body>
<?php
	
	$report = "business.xml";
	
	$depot = 1;
	$freight = 2;
	$special_report = 0;
	
	if(isset($_REQUEST['report']))
	{
		$report = $_REQUEST['report'];
		logf("report: " . $report);
	}
	
	$cbid = "";
	$cbname = "";
	if(isset($_REQUEST['cbid']))
	{
		$cbid = $_REQUEST['cbid'];
		$cbname = $user->cbidToBusinessName($_REQUEST['cbid']);
	}

	// Initialize DOM
	$xmlDocument = new DOMDocument();
	$xmlDocument->load("./reports/" . $report);
	$reportElement = $xmlDocument->getElementsByTagName("report")->item(0);

	echo "<div id='report'>\n";
	
	foreach($reportElement->childNodes as $reportPage)
	{
		// Found Page
		if($reportPage->nodeName == "page")
		{
			echo "<div class='reportPage' id='" . $reportPage->getAttribute("xml:id") . "' dbid='". $reportPage->getAttribute("dbid") . "'>\n";
			
			foreach($reportPage->childNodes as $pageControl)
			{
				if($pageControl->nodeName != "#text")
				{
					//container
					$cls = "";
					if(!$pageControl->hasAttribute("parent"))
						$cls = "ui-corner-all ui-widget-content parent-container";
					else
						$cls = "child-container";
						
					$style = "";
					if( ($pageControl->getAttribute('action') == 'photo' && $phone == false) || ($pageControl->getAttribute('action') == 'map' && $phone) )
						$style = "display:none";
						
					echo "<div class='" . $cls . "' style='". $style . "' id='". $pageControl->getAttribute("xml:id") ."container'>\n\n"; 
					
					$lcls = "label ui-widget-header ui-state-focus";
					if($pageControl->hasAttribute("parent") || ($pageControl->nodeName == "checkbox" && $report=="citizen.xml"))
						$lcls = "label";

					//echo $pageControl->nodeName . "<br>";
					switch($pageControl->nodeName)
					{
						case "textinput":
						case "textarea":
						case "dateinput":
						{
							$bid = "";
							if(!$phone)
							{
								if($user->is_loaded() && $pageControl->getAttribute("xml:id") == "businessName")
								{
									$bid = " value='" . $user->get_property("business_name") . "' ";
									$bid = $bid . " business_id='" . $user->get_property("business_id") . "' ";
								}
							}
							
							if($pageControl->getAttribute("xml:id") == "businessCName")
							{
								$bid = " value='" . $cbname . "' ";
								$bid = $bid . " business_id='" . $cbid . "' ";
							}
							
							//disabled
							$dis = "";
							if($pageControl->hasAttribute("noedit"))
								$dis = " disabled";
							
							echo "<div class='" . $lcls . "'>" . $pageControl->getAttribute("text") . "</div>\n";
							
							$datetext = "";
							if($pageControl->nodeName == "dateinput")
								$datetext = "subtype='dateinput' value='" . date("Y-m-d H:i:s") . "'";
								
							if($pageControl->nodeName == "textarea")
								echo "<textarea class='reportTextBox ui-widget ui-state-default ui-corner-all' id='" . $pageControl->getAttribute("xml:id") . "'" . $datetext . $bid . 'onkeyup="$(\'#' . $pageControl->getAttribute("xml:id") . '\').attr(\'value\', $(\'#' . $pageControl->getAttribute("xml:id") . '\').val());"'  . $dis . "></textarea>\n";
							else
								echo "<input class='reportTextBox ui-widget ui-state-default ui-corner-all' type='text' id='" . $pageControl->getAttribute("xml:id") . "'" . $datetext . $bid . 'onkeyup="$(\'#' . $pageControl->getAttribute("xml:id") . '\').attr(\'value\', $(\'#' . $pageControl->getAttribute("xml:id") . '\').val());"'  . $dis . "></input>\n";
						}
						break;
						
						case "checkbox": 
						{
							$icon = "";
							if($pageControl->hasAttribute("icon"))
							{
								
								$icon = "<img src='images/" . $pageControl->getAttribute("icon") . "' ><br>";
								echo "<div class='" . $lcls . "' style='text-align: center; vertical-align: middle;'>\n";
							}
							else
								echo "<div class='" . $lcls . "'>\n";

							echo "<table width='100%'><tr><td width='15px'><input class='big-checkbox' type='checkbox' id='" . $pageControl->getAttribute("xml:id") . "' " . 'onchange="$(\'#' . $pageControl->getAttribute("xml:id") . '\').attr(\'value\', $(\'#' . $pageControl->getAttribute("xml:id") . '\').is(\':checked\'));"' . "></input></td>\n";
							echo "<td align='center' valign='middle'><label for='" . $pageControl->getAttribute("xml:id") . "' style='display: inline-block; width:100%;'>" . $icon . $pageControl->getAttribute("text") . "</label>\n";
							echo "</td></tr></table></div>\n\n";
							
						}
						break;
						
						case "radio":
						{
							echo "<div class='" . $lcls . "'>" . $pageControl->getAttribute("text") . "</div>\n";
							echo "<div id='" . $pageControl->getAttribute("xml:id") . "' style='display:none;'></div>";
							foreach($pageControl->childNodes as $radioOption)
							{
								if($radioOption->nodeName != "#text")
								{
									echo '<input type="radio" id="' . $pageControl->getAttribute("xml:id") . $radioOption->getAttribute("value") . '" name="' . $pageControl->getAttribute("xml:id") . '" value="'. $radioOption->getAttribute("value") . '" onchange="$(\'#' . $pageControl->getAttribute("xml:id") . '\').attr(\'value\', \'' . $radioOption->getAttribute("value") . '\');"></input>' . "\n";
									echo "<label for='" . $pageControl->getAttribute("xml:id") . $radioOption->getAttribute("value") . "' style='display: inline-block; width:90%'>" . $radioOption->getAttribute("text") . "</label><br>\n";
								}
							}
						}
						break;
						
						case "button":
						{
							$icon = '';
							$id = '';
							
							if($pageControl->getAttribute('action') == 'photo')
							{
								$icon = "<img src='images/recon/takePhoto.png' ><br>";
								$id = " id='" . $pageControl->getAttribute('dbid') . "Photo'";
							}
							if($pageControl->getAttribute('action') == 'map')
							{
								$icon = "<img src='images/recon/placeMap.png' ><br>";
								$id = " id='" . $pageControl->getAttribute('dbid') . "Map'";
							}
							echo "<button class='" . $pageControl->getAttribute("action") . "Button' ". $id . ">" . $icon . $pageControl->getAttribute("text") . "</button>\n";
						}
						break;
						
						case "menu":
						{
							echo "<div class='" . $lcls . "'>" . $pageControl->getAttribute("text") . "</div>\n";
							foreach($pageControl->childNodes as $radioOption)
							{
								if($radioOption->nodeName != "#text")
								{
									$icon = "";
									if($radioOption->hasAttribute("icon"))
										$icon = "<img src='images/" . $radioOption->getAttribute("icon") . "' ><br>";
									echo "<button class='menuButton' location='" . $radioOption->getAttribute("location") . "'>" . $icon . $radioOption->getAttribute("text") . "</button>\n";
								}
							}
						}
						break;
						
						case "special":
						{
							$bd = new DOMDocument();
							$bd->load("reports/business.xml");
					
							$menus = $bd->getElementsByTagName('menu');
							$services = $menus->item(0)->getElementsByTagName('link');
							
							echo "<div id='special_services'>";
							foreach($services as $srvc)
							{
								$service = $srvc->getAttribute('text');
								$value = $srvc->getAttribute('location');

								echo "<div class='" . $lcls . "' id='service_" . $value . "' dbid='". $value . "'>" . $service . "</div>\n";
								
								echo '<input type="radio" id="' . $value . '_available" name="' . $value . '" value="' . $value . '_available" onchange="$(\'#service_' . $value . '\').attr(\'value\', \'available' . '\');"></input>' . "\n";
								echo "<label for='" . $value . "_available' style='display: inline-block; width:90%'>Available</label><br>\n";
								
								echo '<input type="radio" id="' . $value . '_limited" name="' . $value . '" value="' . $value . '_limited" onchange="$(\'#service_' . $value . '\').attr(\'value\', \'limited' . '\');"></input>' . "\n";
								echo "<label for='" . $value . "_limited' style='display: inline-block; width:90%'>Limited Availability</label><br>\n";

								echo '<input type="radio" id="' . $value . '_needed" name="' . $value . '" value="' . $value . '_needed" onchange="$(\'#service_' . $value . '\').attr(\'value\', \'needed' . '\');"></input>' . "\n";
								echo "<label for='" . $value . "_needed' style='display: inline-block; width:90%'>Needed</label><br>\n";

							}
							echo "</div>";
						}
						break;
					}
					echo "</div>\n";
					
					if($pageControl->hasAttribute("parent"))
						echo "<script type='text/javascript'> $('#" . $pageControl->getAttribute("parent")	 . "container'). append( $('#" . $pageControl->getAttribute("xml:id") . "container')); </script>\n\n";

                    $attrs = $pageControl->attributes;  
					echo "<script type='text/javascript'>\n";
					
					$ctrl = $pageControl->getAttribute('xml:id');
					echo "$('[subtype=dateinput]').attr('value', getDateSQL());\n";
					
                    foreach ($attrs as $i => $attr) 
					{
						if($attr->name != "business_id" && $attr->name != "id" && $attr->name != "gpsposition")
							echo "$('#" . $pageControl->getAttribute('xml:id') . "').attr('" . $attr->name . "', '" . urlencode($attr->value) . "');\n";
						if($attr->name == "gpsposition")	
							echo "$('#" . $pageControl->getAttribute('xml:id') . "').attr('gpsposition', '" . urlencode($location) . "');\n";
					}
					echo "$('#" . $pageControl->getAttribute('xml:id') . "').attr('xml:id', '" . $pageControl->getAttribute('xml:id') . "');\n";
					echo "</script>\n";
				}
			}
			echo "</div>";
		}
	}
?>
</div>

<script type="text/javascript">  
	<?php 
		if(isset($_REQUEST['user']))
			echo "window.loginUser = '" . $_REQUEST['user'] . "';";
		else
			echo "window.loginUser = '" . $user->userID . "';";
			?>
	//Error handler hook for saving clientside errors.
	var originalOnError = window.onerror; //Store original for calling later

	window.onerror = function(msg, url, line) 
	{
		$.ajax({
			cache: false,
			type: "post",
			data: {msg:msg, user:window.loginUser, url:url, page:'reportpage.php', line:line, browser:navigator.userAgent, post:1},
			url: "<?php echo 'http://' . $_SERVER['SERVER_NAME'] . '/'; ?>error_log.php",
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
			data: {msg:msg, user:window.loginUser, page:'reportpage.php', browser:navigator.userAgent, console:1},
			url: "<?php echo 'http://' . $_SERVER['SERVER_NAME'] . '/'; ?>error_log.php",
			async: true
		});
		oroginalLog.call(console,msg);
	}
	
	$(document).ready(function() {
	    $('html').niceScroll({horizrailenabled:false, autohidemode:false});
		getBusinessReport();
	});
			
	function getBusinessReport()
	{
		var reportType = '<?php echo $report; ?>';
		
		if(reportType == 'business.xml' || reportType == 'depot.xml' || reportType == 'terminal.xml')
		{
			<?php 
			if(!$phone)
			{
				if($user->is_loaded())
				{
					echo "window.businessId = '" .  $user->get_property("business_id") . "';";
				}
			}?>
			
			jQuery.ajax({
				url:	'<?php echo 'http://' . $_SERVER['SERVER_NAME'] . '/'; ?>user_functions.php?func=get_my_business_report&business_id=' + window.businessId,
				async: true,
				crossDomain: true,
				success: function(result) 
				{
					var businessReports = JSON.parse(result);
					
					if(businessReports.length > 0)
					{
						$("#businessStatus").attr("value", businessReports[0].businessStatus);
						$("#businessStatus" + businessReports[0].businessStatus).attr('checked', true);	
						
						$("#businessHours").attr("value", businessReports[0].businessHours);
						$("#businessHours" + businessReports[0].businessHours).attr('checked', true);
						
						$("#businessHoursComment").attr("value", businessReports[0].businessHoursComment);
						$("#businessHoursComment").val(businessReports[0].businessHoursComment);
						
						$("#businessComment").attr("value", businessReports[0].businessComment);
						$("#businessComment").val(businessReports[0].businessComment);
						
						businessReports.forEach(function(businessReport)
						{
							if(reportType == 'business.xml')
							{
								$("[location=" + businessReport.SupplyType + "]").attr('style', function(i,s) { return s + 'background: #BFFF95 !important;' });
															
								//console.log(businessReport.SupplyType + "_" + businessReport.SupplySubType);
								$("#" + businessReport.SupplyType + "_" + businessReport.SupplySubType.toLowerCase().replace(/[^\w\s]/gi, '')).attr("value", "true");
								$("#" + businessReport.SupplyType + "_" + businessReport.SupplySubType.toLowerCase().replace(/[^\w\s]/gi, '')).attr('checked', true);
								
								$("#" + businessReport.SupplyType + "_comment").attr("value", businessReport.SupplyComment);
								$("#" + businessReport.SupplyType + "_comment").val(businessReport.SupplyComment);
							}
							if(reportType == 'terminal.xml')
							{
								$("#" + businessReport.SupplyType).attr("value", businessReport.SupplySubType.toLowerCase());
								$("#" + businessReport.SupplyType + businessReport.SupplySubType.toLowerCase()).attr('checked', true);
							}
							if(reportType == 'depot.xml')
							{
								$("#supply_" + businessReport.SupplyType).attr("value", businessReport.SupplySubType.toLowerCase());
								$("#" + businessReport.SupplyType + "_" + businessReport.SupplySubType.toLowerCase()).attr('checked', true);
							}
						});
					}
				}
			}); 
		}
	}
		
	$(".submitButton").button();
	$(".submitButton").css("width", "100%");
	
	$(".submitButton").click(function() 
	{
		var reportType = '<?php echo $report; ?>';
		
		var jsonReport = '';
		
		if( reportType == 'business.xml' )
			jsonReport = processBusinessReport();
		
		else if (reportType == 'depot.xml')
			jsonReport = processSpecialReport(false);
		
		else if (reportType == 'terminal.xml')
			jsonReport = processSpecialReport(true);
		
		else if ( reportType == 'recon.xml' )
			jsonReport = processReconReport();
			
		else if ( reportType == 'citizen.xml' )
			jsonReport = processCitizenReport();
		
		else if ( reportType == 'business-citizen.xml' )
			jsonReport = processBusinessCitizenReport();
						  
		jsonReport = JSON.stringify(jsonReport)
		//alert(jsonReport);	
		<?php 
			if($phone == true) 
			{ 
				echo "Android.processReport(jsonReport);"; 
			} 
			else
			{
		?>
		
		$.ajax({
			type: 'POST',
			url: 'reportpage.php?post=1&report=' + reportType,
			dataType: 'json',
			data: {'jsonReport': jsonReport},
			success: function(msg) 
			{
				parent.userDialog.dialog("close");
				
				console.log("Ok");
				
				if(reportType == 'business-citizen.xml')
					parent.location.reload();

			},
			error: function (xhr, ajaxOptions, thrownError) {
				console.log("Error:" + thrownError);
				console.log(xhr.statusText + "\n" + xhr.responseText + "\n" + xhr.status + "\n" +  thrownError);
			}
		});
		
		<?php  }  ?>
	});
	
	$(".finishButton").button(); 
	$(".finishButton").css("width", "100%");
	$(".finishButton").click(function() {
		$(".reportPage").hide();
		$(".reportPage#home").show();
		$("#reportTitle").text('<?php echo $reportElement->getAttribute('title'); ?>');
		window.scrollTo(0, 0);
	});
	
	$(".cancelButton").button(); 
	$(".cancelButton").css("width", "100%");
	$(".cancelButton").attr('style', function(i,s) { return s + 'background: #CC3333 !important;' });
	$(".cancelButton").click(function() 
	{
		if(parent.phoneReport)
			$(parent.phoneReport).remove();
			
		parent.userDialog.dialog("close");
	});
	
	$(".menuButton").button();
	$(".menuButton").css("width", "100%");
	
	$(".mapButton").button();
	$(".mapButton").css("width", "100%");
	$(".mapButton").attr('style', function(i,s) { return s + 'background: #FC6 !important;' });
	$(".mapButton").click(function() 
	{
		parent.userDialog.dialog("close");
		parent.manualMarker("cluster", function(loc) 
		{ 
			parent.userDialog.dialog("open");
			var mLoc = loc.lat() + "," + loc.lng();
			console.log(mLoc);
			updateLocation(mLoc);
			parent.showOtherMarkers();
		});
	});
	
	$(".photoButton").button();
	$(".photoButton").css("width", "100%");
	$(".photoButton").attr('style', function(i,s) { return s + 'background: #FC6 !important;' });
	$(".photoButton").click(function() 
	{
		<?php if($phone){ ?>
			var photoFile = Android.capturePhoto($(this).attr('id'));
			$(this).attr('photoFileName', photoFile);
			console.log("Attached photofile to photoButton: " + $(this).attr('photoFileName')); 
		<?php } else { ?>
			alert("In the future will allow attaching of photo previously taken from computer.")
		<?php } ?> 
		
	});
	
	$(".menuButton").click(function() {
		var menuButton = this;
		$(this).attr('style', function(i,s) { return s + 'background: #BFFF95 !important;' });
		$(".reportPage").hide();
		$(".reportPage#" + $(this).attr("location")).show();
		$("#reportTitle").text($(this).attr("value"));
		window.scrollTo(0, 0);
	});
	
	$(".reportPage").hide();
	$(".reportPage#home").show();

	if(typeof Android !== 'undefined')
	{
		var rec = Android.getReconTeam();
		$('#reconTeam').val(rec); 
		$('#reconTeam').attr('value', rec);
		updateLocation(); 
	}
	
//});
</script>

</body>

<?php
	}
	else
	{
		$report = "business.xml";
		
		if(isset($_REQUEST['report']))
			$report = $_REQUEST['report'];
	
		if(isset($_REQUEST['jsonReport']))
		{
			if($report == "business.xml" || $report == "depot.xml" || $report == "terminal.xml")
			{
				// Create recon SQL table if it doesn't exist
				$query = 	"CREATE TABLE IF NOT EXISTS `business_reports` (
							`id` int(11) NOT NULL AUTO_INCREMENT,
							`loginId` varchar(64),
							`business_id` varchar(64) NOT NULL,
							`businessStatus` varchar(64) NOT NULL,
							`businessHours` varchar(64),
							`businessHoursComment` varchar(128),
							`businessComment` varchar(512) NULL DEFAULT '',
							`updateTime` datetime,
							`SupplyType` varchar(64),
							`SupplySubType` varchar(64),
							`SupplyComment` varchar(128),
							PRIMARY KEY (`id`), INDEX(updateTime))";
	
				$user->query($query);
				$jsonReport = json_decode($_REQUEST['jsonReport'], true);
				
				$query = "DELETE FROM `business_reports`  WHERE `business_id` = '" . $jsonReport[0]['business_id'] . "'"; 		
				$user->query($query);
			
				foreach($jsonReport as $jsonEntry)
				{
					$query = "INSERT INTO `business_reports` (`".implode('`, `', array_keys($jsonEntry))."`) VALUES ('".urldecode(implode("', '", $jsonEntry))."')";
					$user->query($query);
				}	
				
				$output = array("ok");
				echo json_encode($output);
			}
			elseif($report == "recon.xml")
			{
				// Create recon SQL table if it doesn't exist
				$query = 	"CREATE TABLE IF NOT EXISTS `recon_reports` (
							  `id` int(11) NOT NULL AUTO_INCREMENT,
							  `loginId` varchar(64) NOT NULL DEFAULT ' ',
							  `reconTeam` varchar(64) NOT NULL,
							  `reportId` varchar(128) NOT NULL,
							  `reportType` varchar(64) DEFAULT NULL,
							  `targetName` varchar(128) DEFAULT NULL,
							  `reportDateTime` datetime DEFAULT NULL,
							  `damageType` varchar(64) DEFAULT NULL,
							  `damageSubType` varchar(64) DEFAULT NULL,
							  `damageModifiers` varchar(128) DEFAULT NULL,
							  `damageComments` varchar(256) DEFAULT NULL,
							  `damagePhoto` varchar(256) DEFAULT NULL,
							  `routeName` varchar(128) DEFAULT NULL,
							  `gpsPosition` varchar(128) DEFAULT NULL,
							  PRIMARY KEY (`id`),
							  KEY `reportId` (`reportId`),
							  KEY `reportDateTime` (`reportDateTime`)
							) ENGINE=InnoDB  DEFAULT CHARSET=latin1;";
							
				$user->query($query);
				$jsonReport = json_decode($_REQUEST['jsonReport'], true);
				
				foreach($jsonReport as $jsonEntry)
				{
					$damagePhoto = $jsonEntry['damagePhoto'];
					$photoDest = './reports/recon/data/' . $jsonEntry['damagePhoto'] . ".jpg";
					
					if(!file_exists('./reports/recon/'))
						mkdir('./reports/recon/');
						
					if(!file_exists('./reports/recon/data/'))
						mkdir('./reports/recon/data/');
					
					if($damagePhoto != "")
						logf("Damage photo found: " . $damagePhoto);

					if(isset($_FILES[$damagePhoto]))
						logf("Damage photo file: " . $damagePhoto);

					if($damagePhoto != "" && isset($_FILES[$damagePhoto]) && !file_exists($photoDest))
					{
						logf("Uploading photo: " . $damagePhoto);
						move_uploaded_file($_FILES[$damagePhoto]['tmp_name'], $photoDest);
					}
					
					if(file_exists($photoDest))
						$jsonEntry['damagePhoto'] = $jsonEntry['damagePhoto'] . ".jpg";
					else
						$jsonEntry['damagePhoto'] = "";
					
					$query = "INSERT INTO `recon_reports` (`".implode('`, `', array_keys($jsonEntry))."`) VALUES ('".urldecode(implode("', '", $jsonEntry))."')";
					$user->query($query);
				}	
				
				$output = array("ok");
				echo json_encode($output);
			}
			elseif($report == "citizen.xml")
			{
				$query = 	"CREATE TABLE IF NOT EXISTS `citizen_reports` (
							`id` int(11) NOT NULL AUTO_INCREMENT,
							`loginId` varchar(64),
							`reportType` varchar(64) NOT NULL,
							`reportSubType` varchar(64) NOT NULL,
							`reportDateTime` datetime,
							`damagePhoto`	varchar(256),
							`reportComment` varchar(256),
							`gpsPosition` varchar(128),
							PRIMARY KEY (`id`),
							KEY `reportDateTime` (`reportDateTime`))";
							
				$user->query($query);
				$jsonReport = json_decode($_REQUEST['jsonReport'], true);
				
				foreach($jsonReport as $jsonEntry)
				{
					$damagePhoto = $jsonEntry['damagePhoto'];
					$photoDest = './reports/citizen/data/' . $jsonEntry['damagePhoto'] . ".jpg";
					
					if(!file_exists('./reports/citizen/'))
						mkdir('./reports/citizen/');
						
					if(!file_exists('./reports/citizen/'))
						mkdir('./reports/citizen/data/');

					if($damagePhoto != "")
						logf("Damage photo found: " . $damagePhoto);

					if(isset($_FILES[$damagePhoto]))
						logf("Damage photo file: " . $damagePhoto);

					if($damagePhoto != "" && isset($_FILES[$damagePhoto]) && !file_exists($photoDest))
					{
						logf("Uploading photo: " . $damagePhoto);
						move_uploaded_file($_FILES[$damagePhoto]['tmp_name'], $photoDest);
					}
					
					if(file_exists($photoDest))
						$jsonEntry['damagePhoto'] = $jsonEntry['damagePhoto'] . ".jpg";
					else
					{
						$jsonEntry['damagePhoto'] = "";
					}
					
					$query = "INSERT INTO `citizen_reports` (`".implode('`, `', array_keys($jsonEntry))."`) VALUES ('".urldecode(implode("', '", $jsonEntry))."')";
					$user->query($query);
				}	
				
				$output = array("ok");
				echo json_encode($output);
			}
			
			elseif($report == "business-citizen.xml")
			{
				$query = 	"CREATE TABLE IF NOT EXISTS `business_citizen_reports` (
							`id` int(11) NOT NULL AUTO_INCREMENT,
							`business_id` varchar(64),
							`loginId` varchar(64),
							`reportDateTime` datetime,
							`businessStatus` varchar(64),
							`businessComment`	varchar(256),
							PRIMARY KEY (`id`) )";
							
				$user->query($query);
				$jsonReport = json_decode($_REQUEST['jsonReport'], true);
				$jsonEntry = $jsonReport[0];
				
				$query = "INSERT INTO `business_citizen_reports` (`".implode('`, `', array_keys($jsonEntry))."`) VALUES ('".urldecode(implode("', '", $jsonEntry))."')";
				logf($query);
				$user->query($query);
				$output = array("ok");
				echo json_encode($output);
			}
		}
	}
?>