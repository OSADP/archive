<?php 
    include("config.php");
	
	date_default_timezone_set('America/Los_Angeles'); 
	
	// Configure logs
	ini_set('error_log', getcwd().'/logs/recon_error.log');
	
    function logf($msg) 
    {
        $fd = fopen("./logs/recon_log.txt", "a"); // write string 
        fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
        fclose($fd); 
    }
	
	// Create recon SQL table if it doesn't exist
	$query = 	"CREATE TABLE IF NOT EXISTS `recon_reports` (
				`id` int(11) NOT NULL AUTO_INCREMENT,
				`reconTeam` varchar(64) NOT NULL,
				`reportId` varchar(128) NOT NULL,
				`reportType` varchar(64),
				`targetName` varchar(128),
				`reportDateTime` datetime,
				`damageType` varchar(64),
				`damageSubType` varchar(64),
				`damageModifiers` varchar(128),
				`damageComments` varchar(256),
				`damagePhoto`	varchar(256),
				`routeName` varchar(128),
				`gpsPosition` varchar(128),
				PRIMARY KEY (`id`) )";
				
	run_query($query);
	
	logf("-------------- New Upload --------------");
	    
    // Navigate into the correct directory
    $target = "reports/recon";  
    
    if(!file_exists($target))
    { 
        mkdir($target); 
    } 
    
	// Fetch XML report file
	if(isset($_FILES['Report']))
	{
        $report = $_FILES["Report"]["name"];
		logf("Fetching file: " . $report);
        $dest = $target . "/" . $report;
        move_uploaded_file($_FILES['Report']['tmp_name'], $dest);
		logf("File fetched: " . $report);
        echo "uploaded";
		
		// -------------------------Parse XML RECON Report		
		// Initialize DOM
		$doc = new DOMDocument();
		$doc->load($dest);
		
		// Grab report variables
		$reconTeam		= $doc->getElementById('reconTeam')->getAttribute('value');
		$reportId 		= $doc->getElementById('reportId')->getAttribute('value');
		$reportType 	= $doc->getElementById('reportType')->getAttribute('value');
		$targetName 	= $doc->getElementById('targetName')->getAttribute('value');
		$reportDateTime = $doc->getElementById('dateTime')->getAttribute('value');
			
		// Navigate through pages
		$pages = $doc->getElementsByTagName('page');
		
		foreach($pages as $page)
		{
			$damageType 	= $page->getAttribute('xml:id');
			$damagePhoto	= "";
			
			logf("Parsing Page: " . $damageType);
			
			$page_elements = $page->getElementsByTagName('*');
			
			foreach($page_elements as $page_element)
			{
				// Fetch Report Photos
				if($page_element->hasAttribute('photoFileName'))
				{
					$damagePhoto = $page_element->getAttribute('photoFileName');
					logf("Found Photo Element: " . $damagePhoto);
					if(isset($_FILES[$damagePhoto]))
					{
						$target = "reports/recon/data";  
						
						if(!file_exists($target))
							mkdir($target); 
						
						logf("Fetching photo: " . $damagePhoto);
						
						$dest = $target . "/" . $damagePhoto . ".jpg";
						move_uploaded_file($_FILES[$damagePhoto]['tmp_name'], $dest);
						$damagePhoto = $damagePhoto . ".jpg";
					}
					else
					{
						logf("Warning, no file sent: " . $damagePhoto);	
						logf("Sent Files: " + var_dump($_FILES));
						$damagePhoto = ""; //Reset if there is no photo file
					}
					
				}
				if($page_element->hasAttribute('dbid') && $page_element->getAttribute('dbid') == 'DamageSubType' && $page_element->getAttribute('value') == 'true')
				{
					$damageSubType		= $page_element->getAttribute('xml:id');
					$damageModifiers 	= "";
					$damageComments		= "";
					$routeName			= "";
					$gpsPosition		= "";
					
					if($page_element->hasAttribute('gpsposition'))
						$gpsPosition = $page_element->getAttribute('gpsposition');
					
					
					foreach($page_elements as $pelement)
					{
						if($pelement->getAttribute('parent') == $page_element->getAttribute('xml:id'))
						{
							if($pelement->getAttribute('dbid') == 'DamageModifier')
							{	
								// Handle Checkboxes
								if($pelement->getAttribute('value') == 'true')
									$damageModifiers = $damageModifiers . $pelement->getAttribute('text') . ':';
								elseif($pelement->getAttribute('value') != '' && $pelement->getAttribute('value') != 'false')
									$damageModifiers = $damageModifiers . $pelement->getAttribute('value') . ':';
							}
							elseif($pelement->getAttribute('dbid') == 'DamageComment')
							{	
								if($pelement->hasAttribute('value'))
									$damageComments = $pelement->getAttribute('value');
							}
							elseif($pelement->getAttribute('dbid') == 'DamageRoute')
							{	
								if($pelement->hasAttribute('value'))
									$routeName = $pelement->getAttribute('value');
							}
						}
					}
					
					// Add data to SQL Table
					$query="INSERT INTO recon_reports 
							(reconTeam, reportId, reportType, targetName, reportDateTime, damageType, damageSubType, damageModifiers, damageComments, damagePhoto, routeName, gpsPosition)
							VALUES
							('$reconTeam', '$reportId', '$reportType', '$targetName', '$reportDateTime', '$damageType', '$damageSubType', '$damageModifiers', '$damageComments', '$damagePhoto', '$routeName', '$gpsPosition');";
							
					run_query($query);
				}
			}
		}
	}
?>
