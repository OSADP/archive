<?php 

    include("config.php");
	
	date_default_timezone_set('America/Los_Angeles'); 
	
	// Configure logs
	ini_set('error_log', getcwd().'/logs/business_error.log');
	
    function logf($msg) 
    {
        $fd = fopen("./logs/business_log.txt", "a"); // write string 
        fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
        fclose($fd); 
    }
	
	function getAttr($doc, $tag)
	{
		if($doc->getElementById($tag)->hasAttribute('value'))
			return $doc->getElementById($tag)->getAttribute('value');
		else
			return "";
	}
	
	// Create recon SQL table if it doesn't exist
	$query = 	"CREATE TABLE IF NOT EXISTS `business_reports` (
				`id` int(11) NOT NULL AUTO_INCREMENT,
				`business_id` varchar(64) NOT NULL,
				`businessStatus` varchar(64) NOT NULL,
				`businessHours` varchar(64),
				`businessHoursComment` varchar(128),
				`updateTime` datetime,
				`SupplyType` varchar(64),
				`SupplySubType` varchar(64),
				`SupplyComment` varchar(128),
				PRIMARY KEY (`id`), INDEX(updateTime))";
				
	run_query($query);
	
	logf("-------------- New Upload --------------");
	    
    // Navigate into the correct directory
    $target = "reports/business";  
    
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
		
		// ---------------------------------------------------------------
		// -------------------------Parse XML RECON Report
		// ---------------------------------------------------------------

		// Initialize DOM
		$doc = new DOMDocument();
		
		//$doc->validateOnParse = true;
	
		$doc->load($dest);
		
		// Grab report variables
		$business_id				= $doc->getElementById('businessName')->getAttribute('business_id');
		$businessStatus			= getAttr($doc, 'businessStatus');
		$businessHours 			= getAttr($doc, 'businessHours');
		$businessHoursComment 	= getAttr($doc, 'businessHoursComment');
		$updateTime				= date("Y-m-d H:i:s");

		$query = 	"DELETE FROM `business_reports`  WHERE `business_id` = '$business_id'"; 
					
		run_query($query);
		
		// Navigate through pages
		$pages = $doc->getElementsByTagName('page');
		
		foreach($pages as $page)
		{
			$SupplyType = $page->getAttribute('xml:id');
			$SupplyComment = "";
			
			logf("Parsing Page: " . $SupplyType);
			
			$page_elements = $page->getElementsByTagName('*');
			
			foreach($page_elements as $page_element)
			{
				if($page_element->hasAttribute('dbid') && $page_element->getAttribute('dbid') == 'SupplyComment')
					$SupplyComment = $page_element->getAttribute('value');
			}
			
			foreach($page_elements as $page_element)
			{
				if($page_element->hasAttribute('dbid') && $page_element->getAttribute('dbid') == 'SupplySubType' && $page_element->getAttribute('value') == 'true')
				{
					$SupplySubType		= $page_element->getAttribute('text');
					
					echo $business_id . " " . $SupplyType . " " . $SupplySubType . "<br>";
					
					// Add data to SQL Table
					$query="INSERT INTO business_reports 
							(business_id, businessStatus, businessHours, businessHoursComment, updateTime, SupplyType, SupplySubType, SupplyComment)
							VALUES
							('$business_id', '$businessStatus', '$businessHours', '$businessHoursComment', '$updateTime', '$SupplyType', '$SupplySubType', '$SupplyComment');";
							
					run_query($query);
				}
			}
		}
		
		//If no service pages entered, add an entry anyway.
		if($pages->length == 0)
		{
			// Add data to SQL Table
			$query="INSERT INTO business_reports 
					(business_id, businessStatus, businessHours, businessHoursComment, updateTime, SupplyType, SupplySubType, SupplyComment)
					VALUES
					('$business_id', '$businessStatus', '$businessHours', '$businessHoursComment', '$updateTime', '', '', '');";
					
			run_query($query);
		}
	}
?>