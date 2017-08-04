<?php 

    include("config.php");
    
	ini_set("auto_detect_line_endings", true);
	chdir(dirname($_SERVER['SCRIPT_FILENAME']));

	date_default_timezone_set('America/Los_Angeles'); 
	
	// Configure logs
	ini_set('error_log', getcwd().'/logs/gps_error.log');
	
	$debug = false;
	
    function logf($msg) 
    {
        $fd = fopen("./logs/gps_log.txt", "a"); // write string 
        fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
        fclose($fd); 
        
        if($debug)
        	echo "<font color='green'>" . $msg . "</font><br>";
    }
	
	// Create recon SQL table if it doesn't exist
	$query = 	"CREATE TABLE IF NOT EXISTS `gps_traces` (
				`id` int(11) NOT NULL AUTO_INCREMENT,
				`Latitude` float NOT NULL,
				`Longitude` float NOT NULL,
				`Speed` varchar(64),
				`Direction` varchar(64),
				`Accuracy` varchar(128),
				`DateTime` datetime,
				PRIMARY KEY (`id`), INDEX(DateTime), INDEX(Latitide), INDEX(Longitude) )";
				
	run_query($query);
	
	logf("-------------- Fetching GPS Data --------------");

    // Navigate into the correct directory
    $target = "gps";  
    
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
		
		$file = fopen($dest,'r');
		if ($file) 
		{
        	while (($gpsEntry = fgets($file)) !== false) 
			{ 
				$gpsVars = explode(",", $gpsEntry);
				
				if(sizeof($gpsVars) == 6)
				{
					$Latitude 	= $gpsVars[0];
					$Longitude 	= $gpsVars[1];
					$Speed 		= $gpsVars[2];
					$Direction	= $gpsVars[3];
					$Accuracy 	= $gpsVars[4];
					$DateTime 	= $gpsVars[5];
					
								// Add data to SQL Table
					$query="INSERT INTO gps_traces 
							(Latitude, Longitude, Speed, Direction, Accuracy, DateTime)
							VALUES
							('$Latitude', '$Longitude', '$Speed', '$Direction', '$Accuracy', '$DateTime');";
							
					run_query($query);
				}
	        }
	        fclose($file);
		}
		unlink($dest);
		
        echo "uploaded";
	}
    
?>
