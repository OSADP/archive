<?php
require 'access.class.php';
include_once("config.php");

$user = new flexibleAccess();

function logf($msg) 
{
	$fd = fopen("./logs/user_functions_log.txt", "a"); // write string 
	fwrite($fd, "----------" . date('d/m/Y - H:ia') . "----------\n" . $msg . "\n"); // close file 
	fclose($fd); 
}

function businessTypeLong($businessType)
{
	$businessTypes = array(
    	'freight'=> 'Sea Port', 
    	'terminal'=> 'Freight Terminal',
    	'depot'=> 'Point of Distribution',
    	'gas_station'=> 'Gas Station',
    	'grocery_or_supermarket'=> 'Grocery Store/Supermarket',
    	'hardware_store'=> 'Hardware Store',
    	'hospital'=> 'Hospital/Doctor',
    	'pharmacy'=> 'Pharmacy',
    	'store'=> 'Sporting Goods',
    	'clothing_store'=> 'Clothing',
    	'other'=> 'Other'
		);
	return $businessTypes[$businessType];
}


$func = "";
if(isset($_REQUEST["func"]))
	$func = $_REQUEST["func"];
	
if($func == "getRecon" || $func == "getBusiness" || $func == "getCitizen")
{
	header('Content-Type: application/javascript');
}

switch ($func) 
{
	case "deleteReport":
	{
		$id = "";
		
		if(isset($_REQUEST["id"]))
			$id = $_REQUEST["id"];
			
		if($id != "")
		{
			$r = substr($id, 0, 1);
			if(	$r == "r" )
				$report = 'recon';
			else if( $r == "c" )
				$report = 'citizen';
			else if( $r == "z" )
				$report = 'business_citizen';
				
			$id = substr($id, 1, strlen($id) - 1);
			$table = $report . "_reports";
			$query = "DELETE FROM " . $table . " WHERE id = '$id';";
			
			echo run_query($query);
		}
	}
	break;
	
	case "reconIdToText":
	{
		$id = "";
		
		if(isset($_REQUEST["id"]))
			$id = $_REQUEST["id"];
			
		$doc = new DOMDocument();	
		$doc->load("reports/recon.xml");
		if($doc->getElementById($id) != null)
			echo $doc->getElementById($id)->getAttribute("text");
		else
			echo $id;
		unset($doc);
	}
	break;
	
	case "lastReconUpdate":
	{
		$query = 'SELECT id FROM recon_reports ORDER BY id DESC LIMIT 1';
		$rep = run_query($query);
		
		echo $rep[0]['id'];
	}
	break;
	
	case "lastBusinessUpdate":
	{
		$query = 'SELECT id FROM business_reports ORDER BY id DESC LIMIT 1';
		$rep1 = run_query($query);
		
		echo $rep1[0]['id'];
	}
	break;
	
	case "lastCitizenUpdate":
	{
		$query = 'SELECT id FROM citizen_reports ORDER BY id DESC LIMIT 1';
		$rep1 = run_query($query);
		
		echo $rep1[0]['id'];
	}
	break;
	
	case "lastGpsUpdate":
	{
		$query = 'SELECT id FROM gps_traces ORDER BY id DESC LIMIT 1';
		$rep1 = run_query($query);
		
		echo $rep1[0]['id'];
	}
	break;
	
	case "getBusinessStatus":
	{
		$business_id = "";
		if(isset($_REQUEST["business_id"]))
			$business_id = $_REQUEST["business_id"];
			
		$query = "SELECT * FROM business_reports WHERE business_id='$business_id';";
		$rep = run_query($query);
		
		echo json_encode($rep);
	}
	break;
	
	case "getMarkers":
	{
		// -------           Get GPS Markers
		$lt1 = ""; $ln1 = "";  $lt2 = ""; $ln2 = "";
		
		if(isset($_REQUEST["lat1"]))
			$lt1 = $_REQUEST["lat1"];
			
		if(isset($_REQUEST["lon1"]))
			$ln1 = $_REQUEST["lon1"];
			
		if(isset($_REQUEST["lat2"]))
			$lt2 = $_REQUEST["lat2"];
			
		if(isset($_REQUEST["lon2"]))
			$ln2 = $_REQUEST["lon2"];
		
		$lat1 = min($lt1, $lt2);
		$lat2 = max($lt1, $lt2);
		
		$lon1 = min($ln1, $ln2);
		$lon2 = max($ln1, $ln2);
		
		$query = "SELECT * FROM gps_traces WHERE (Speed > 10) AND (Latitude BETWEEN $lat1 AND $lat2) AND (Longitude BETWEEN $lon1 AND $lon2) ORDER BY DateTime DESC LIMIT 5000;";
		//echo $query1 . "<br><br><br>";
		
		//$query = "SELECT * FROM gps_traces WHERE (Speed > 0) ORDER BY DateTime DESC LIMIT 300;";
		$res = run_query($query);
		
		$results = array();
		
		foreach($res as $trace)
		{
			$trace['Speed']	= intval(floatval($trace['Speed']) * 2.2369); //Convert speed to MPH
			
			$results[] = $trace;
		}
		echo json_encode($results);
	}
    break;
	
	case "getRecon":
	{
		// ------- Get RECON MARKERS
		$query = 'SELECT DISTINCT reportId FROM recon_reports ORDER BY reportDateTime DESC';
		$rep = run_query($query);
		?>
        window.reconCluster.clearMarkers();
        <?php
		foreach($rep as $rept)
		{
			if( $rept[0] != "" )
			{
				//echo "<hr>" . $rept[0] . "<hr>";
				$query = "SELECT * FROM recon_reports WHERE reportId='" . $rept[0] . "';";
				$res = run_query($query);
				
				foreach($res as $report_entry)
				{
					$id = $report_entry['id'];
					$gpsPosition = $report_entry['gpsPosition'];
					$damageType = $report_entry['damageType'];
					$damageSubType = $report_entry['damageSubType'];
					$damageComments = $report_entry['damageComments'];
					$damagePhoto = $report_entry['damagePhoto'];
					$damageModifiers = $report_entry['damageModifiers'];
					$routeName = $report_entry['routeName'];
					
					$dateTime = new DateTime($report_entry['reportDateTime']);
					$damageDate = $dateTime->format('m/d/y');
					$damageTime = $dateTime->format('H:i:s');
					
		
					?>

					var markerLocation = new google.maps.LatLng(<?php echo $gpsPosition; ?>);
                    
					var reconMarker = new google.maps.Marker({ 
                            position: markerLocation, 
                            zIndex: 1000, 
                            title: "<?php echo $damageType; ?>", 
                            reportId: "r<?php echo $id; ?>",
                            damageType: "<?php echo $damageType; ?>",
                            damageSubType: "<?php echo $damageSubType; ?>",
                           	damageModifiers: "<?php echo $damageModifiers; ?>",
                           	damageComments: "<?php echo $damageComments; ?>",
                           	damagePhoto: "<?php echo $damagePhoto; ?>",
                            damageDate: "<?php echo $damageDate; ?>",
                            damageTime: "<?php echo $damageTime; ?>",
                            routeName: "<?php echo $routeName; ?>" } );
                            
                            if(reconMarker.damageType == "casualties")
                            {
	                            reconMarker.damageSubType = "";
	                            reconMarker.damageModifiers = "";
                            }
                            
					window.reconCluster.addMarker(reconMarker, true);
                    //window.reconMarkers.push(reconMarker);
					<?php
		
				}
			}
		}
		echo "window.reconCluster.repaint();";
	}
    break;
	
	case "getCitizen":
	{
		// ------- Get Citizen MARKERS
		$query = 'SELECT DISTINCT id FROM citizen_reports ORDER BY reportDateTime DESC';
		$rep = run_query($query);
		?>
        window.citizenCluster.clearMarkers();

        <?php
		foreach($rep as $rept)
		{
			if( $rept[0] != "" )
			{
				$query = "SELECT * FROM citizen_reports WHERE id='" . $rept[0] . "';";
				$res = run_query($query);
				
				foreach($res as $report_entry)
				{
					$id = $report_entry['id'];
					$gpsPosition = $report_entry['gpsPosition'];
					$damageType = $report_entry['reportType'];
					$damageSubType = $report_entry['reportSubType'];
					$damageComments = $report_entry['reportComment'];
					$damagePhoto = $report_entry['damagePhoto'];
					
					$dateTime = new DateTime($report_entry['reportDateTime']);
					$damageDate = $dateTime->format('m/d/y');
					$damageTime = $dateTime->format('H:i:s');
					?>

					var markerLocation = new google.maps.LatLng(<?php echo $gpsPosition; ?>);
                    
					var citizenMarker = new google.maps.Marker({ 
                            position: markerLocation, 
                            zIndex: 1000, 
                            title: "<?php echo $damageType; ?>", 
                            reportId: "c<?php echo $id; ?>",
                            damageType: "<?php echo $damageType; ?>",
                            damageSubType: "<?php echo $damageSubType; ?>",
                           	damageComments: "<?php echo $damageComments; ?>",
                           	damagePhoto: "<?php echo $damagePhoto; ?>",
                            damageDate: "<?php echo $damageDate; ?>",
                            damageTime: "<?php echo $damageTime; ?>"} );
                            
					window.citizenCluster.addMarker(citizenMarker, true);
				<?php
				}
			}
		}
		echo "window.citizenCluster.repaint();";
	}
    break;
	
	case "getBusiness":
	{
		$services = "";
		$srvc_srch = array();
		$check_services = false;
		if(isset($_REQUEST["services"]))
		{
			$check_services = true;
			$srvc_srch = " and ";
			$services = $_REQUEST["services"];
			$srvc_srch = explode(",", $services);
		}

		// ------- Get BUSINESS Reports
		$query = "SELECT DISTINCT business_id FROM business_reports";
		$idRes = run_query($query);
		$i = 0;
		?>

        for( var bm = 0; bm < window.businessTooltips.length; bm++ )
        	window.businessTooltips[bm].marker = null;
        
        window.businessTooltips = new Array();
        
        <?php
		foreach($idRes as $row)
		{
			$business_id = $row['business_id'];			
			$query = "SELECT * FROM business_reports WHERE business_id='$business_id';";
			$businessReports = run_query($query);
			
			if($business_id != "null")
			{
    			$query = "SELECT * FROM business_info WHERE business_id='$business_id';";
    			$businessInfo = run_query($query);
    			$skip_searching = ($check_services && $businessStatus=='closed');

    			if( count($businessInfo) > 0 && $businessInfo[0]['business_location'] != 'null' && $businessInfo[0]['business_name'] != 'undefined' )
    			{
    				$availableSupplies = array();
    				$found_service = false;
    				
    				foreach($businessReports as $report)
    				{
    					$business_id = $report['business_id'];
    					$businessStatus = $report['businessStatus'];
    					$businessHours = $report['businessHours'];
    					$businessHoursComment = $report['businessHoursComment'];
    					$businessComment = $report['businessComment'];
    					$updateTime = $report['updateTime'];
    					$SupplyType = $report['SupplyType'];
    					$SupplySubType = $report['SupplySubType'];
    					$SupplyComment = $report['SupplyComment'];
    					$availableSupplies[ucwords($SupplyType)][] = $SupplySubType;
    					
    					if(in_array($SupplyType, $srvc_srch) && !($SupplyType == "") && !($SupplySubType == "needed"))
    						$found_service = true;
    					
    					if(in_array($businessInfo[0]['business_type'], $srvc_srch))
    						$found_service = true;
    				}
    				if($found_service || !isset($_REQUEST["services"]))
    				{
    					if($businessInfo[0]['business_type'] == "depot" || $businessInfo[0]['business_type'] == "terminal")
    					{
    						$supplyString = "<center><b>Supply Availability</b></center>";
    						$supplyString = $supplyString . "<center><table width='75%' border='0'>";
    					}
    					else
    						$supplyString = "<center><b>Available Supplies</b></center>";
    					
    					foreach($availableSupplies as $key => $availSupply)
    					{
							if($availSupply[0] != "")
							{
								if($businessInfo[0]['business_type'] == "depot")
									$supplyString = $supplyString . "<tr><td>". $key . "</td><td width='15'><img src='images/business/supply_" . $availSupply[0] . ".png' alt='" . ucfirst($availSupply[0]) . "'></td></tr>";

								elseif($businessInfo[0]['business_type'] == "terminal")
									$supplyString = $supplyString . "<tr><td>". $key . "</td><td width='15'><img src='images/business/terminal_" . $availSupply[0] . ".png' alt='" . ucfirst($availSupply[0]) . "'></td></tr>";

								else // Depot shows up different
								{
									$supplyString = $supplyString . "<center>" . $key . "</center>";
									foreach($availSupply as $subType)
										$supplyString = $supplyString . urldecode($subType) . "<br>";
								}
							}
    					}
    					
    					if($businessInfo[0]['business_type'] == "depot")
    						$supplyString = $supplyString . "</table><hr><font size='-2'><b>Supply Key</b><br> Available: <img src='images/business/supply_available.png'> Limited: <img src='images/business/supply_limited.png'> Needed: <img src='images/business/supply_needed.png'> </font></center>";
    					
    					if($businessInfo[0]['business_type'] == "terminal")
    						$supplyString = $supplyString . "</table><hr><font size='-2'><b>Supply Key</b><br> Regular: <img src='images/business/terminal_regular.png'> Outgoing Only: <img src='images/business/terminal_outgoing.png'> Incoming Only: <img src='images/business/terminal_incoming.png'> Unavailable: <img src='images/business/terminal_unavailable.png'></font></center>";
    					
    					$sticon = "";
    					
    					if($businessStatus == "closed")
    						$sticon = "_closed";
    						
    					if($businessStatus == "open" && $businessHours == "limited")
    						$sticon = "_limited";
    						
    					$supplyString = $supplyString . "<hr><center><b>Citizen Reports</b><br><div class='citizenfeedback" . $business_id . "' id='citizenfeedback" . $business_id . "'>Add Update</div></center><hr>";

						$query = "SELECT * FROM business_citizen_reports WHERE business_id='$business_id' ORDER BY reportDateTime DESC;";
						$businessCitizenReports = run_query($query);
						
	    				foreach($businessCitizenReports as $bcr)
	    				{
		    				$login = $bcr['loginId'];
							$query = "SELECT * FROM users WHERE login='$login' LIMIT 1;";
							$unameq = run_query($query);
							
							if(count($unameq) == 0)
								$unameq[0] = array(
							    	'id'=> '', 
							    	'login' => '',
							    	'password'  => '',
							    	'email' => '',
							    	'type'=> '',
							    	'active'=> '',
							    	'first'=> 'User',
							    	'last'=> '',
							    	'phone'=> '',
									'business_id' => ''
							    );
		    				
							$cdate = strtotime( $bcr['reportDateTime'] );
							$mysqldate = "<font size='1'>" . date( 'm/d H:i', $cdate ) . "</font>";
							
							$statimg = "";
							if($bcr['businessStatus'] == "open")
								$statimg = "<img src='images/business/supply_available.png'>";
							else
								$statimg = "<img src='images/business/supply_needed.png'>";
							
							$usercomment = "<table width='100%' cellspacing='0' cellpadding='0' border='0'><tr><td align='center'>" . $statimg . "<br>" . $bcr['businessStatus'] . "</td><td valign='top'>";
		    				$usercomment = $usercomment . "<table width='100%' cellspacing='0' cellpadding='0' border='0'><tr><td align='left'><b>" . $unameq[0]['first'] . " " .  substr($unameq[0]['last'], 0, 1) . "." .  "</b></td><td align='right'><b>" . $mysqldate . "</b></td></tr></table>";
		    				$usercomment =  $usercomment . $bcr['businessComment'] . "</td></tr></table>";
		    				
		    				if($user->userID != null)
	    					{
			    				if($login == $user->userID || $user->get_property('type') == 'admin')
				    				$usercomment =  $usercomment . "<center><a href='javascript:deleteBCR(" . $bcr['id'] . ")'><img width='10' height='10' src='images/delete.png'>Delete</a></center>";
	    					}
		    				
		    				$supplyString = $supplyString . $usercomment . "<hr>";	
	    				}
						
    					  
    					?>
    					var image<?php echo $i; ?> = {
    					  url: 'images/business/<?php echo $businessInfo[0]['business_type'] . $sticon; ?>.png',
    					  size: new google.maps.Size(25, 25),
    					  origin: new google.maps.Point(0, 0),
    					  anchor: new google.maps.Point(15, 15),
    					  scaledSize: new google.maps.Size(25, 25),
    					};
    					
    					var loc = jQuery.parseJSON('<?php echo $businessInfo[0]['business_location']; ?>');
    					var markerLocation<?php echo $i; ?> = new google.maps.LatLng(loc.d, loc.e);
    					var businessMarker<?php echo $i; ?> = new google.maps.Marker({ position: markerLocation<?php echo $i; ?>, map: map, zIndex: 1000, icon: image<?php echo $i; ?>, title: "<?php echo businessTypeLong($businessInfo[0]['business_type']) . " - " . $businessInfo[0]['business_name'];?>"});
    					
    					var content<?php echo $i; ?> = 	"<b><?php echo $businessInfo[0]['business_name']; ?></b><br /> \
    													<i>(<?php echo $businessStatus; ?>)</i><br /> \
    													<b>Hours:</b> <?php echo $businessHours; ?> <br />\
    													<b>Hours Info:</b> <?php echo mysql_escape_string($businessHoursComment); ?> <br />";
    													
    					<?php 
    					if($businessComment != "") 
    						echo "content" . $i . " += '<b>Status Update:</b> <br>" . mysql_escape_string($businessComment) . "<br>';"; ?>								
    													
    					content<?php echo $i; ?> += "<b>Last Update: </b><?php echo $updateTime; ?> <hr /> <?php echo $supplyString; ?>";
    					
    					var tooltipOptions<?php echo $i; ?>={
    						marker:businessMarker<?php echo $i; ?>,
    						content:content<?php echo $i; ?>,
    						id: 'tooltip<?php echo $i; ?>',
    						cssClass:'tooltip', // name of a css class to apply to tooltip
    						citizenfeedback: '.citizenfeedback<?php echo $business_id; ?>'
    					};
    					
    					var tooltip<?php echo $i; ?> = new Tooltip(tooltipOptions<?php echo $i; ?>);
    		
    					window.businessMarkers.push(businessMarker<?php echo $i; ?>);
    					window.businessTooltips.push(tooltip<?php echo $i; ?>);
    					<?php
    					$i ++;
    				}
    			}
			}
		}
		
	}
    break;
}
?>