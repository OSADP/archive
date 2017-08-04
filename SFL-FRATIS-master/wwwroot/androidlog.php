<?php

	$logpath = "./logs/Android";
	
	date_default_timezone_set('America/Los_Angeles');

	if(isset($_REQUEST["device"]))
	{
		$device = "";
		if(isset($_REQUEST["device"]))
			$device = $_REQUEST["device"];
		
		$type = "";
		if(isset($_REQUEST["type"]))
			$type = $_REQUEST["type"];
			
		$tag = "";
		if(isset($_REQUEST["tag"]))
			$tag = $_REQUEST["tag"];
			
		$text = "";
		if(isset($_REQUEST["text"]))
			$text = $_REQUEST["text"];
			
		$current_date = date('d/m/Y H:i:s');
		
		
		mkdir( $logpath, 0777, true );	
			
		$logfile = preg_replace(array('/\s/', '/\.[\.]+/', '/[^\w_\.\-]/'), array('_', '.', ''), $device) . ".log";
			
		$fd = fopen($logpath . "/" . $logfile, "a"); // write string 
		fwrite($fd, $type . ":-:" . $tag . ":-:" . $text . ":-:" . $current_date . "\n"); // close file 
		fclose($fd); 
	}
	else
	{
		echo '<html><head></head><body onload="window.location=\'#end\'">';
		if(isset($_REQUEST["log"]))
		{
			$handle = fopen($logpath . "/" . $_REQUEST["log"], "r");
			if ($handle) {
			    while (($line = fgets($handle)) !== false) 
			    {
			        $l = explode(":-:", $line);
			        
			        $message = "<table width='1000' style='color:";
			        
			        if($l[0] == "i")
			        	$message .= "green";
			        if($l[0] == "e")
			        	$message .= "red";
			        if($l[0] == "d")
			        	$message .= "orange";
			        	
			        
			        if(count($l) == 4)
			        	$message .= "'<tr><td width='150'>" . $l[3] . "</td><td width='200'>" . $l[1] . "</td><td>" . $l[2] . "</td></tr></table></font>\n";
			        else	
			        	$message .= "'><tr><td width='200'>" . $l[1] . "</td><td>" . $l[2] . "</td></tr></table></font>\n";
			        
			        echo $message;
			   
			    }
			} else {
			    echo "Error reading file!";
			} 
			fclose($handle);
			
		}
		else
		{
			echo "<h1>Android Logs:</h1>";
			
			$logs = array_diff(scandir($logpath), array('..', '.'));
			
			foreach ($logs as $key => $value) 
			{
				if(!is_dir($logpath . "/" . $value))
					echo "<font size='4'><a href='androidlog.php?log=" . $value . "'>" . $value . "</a></font><br>";	
			} 
		}
		echo "<div id='end'></div>";
		echo "</body><html>";
	}
?>