<?php
	date_default_timezone_set('America/Los_Angeles'); 
	
	$zip = new ZipArchive();
	$filename = "./EmergencyResponse.apk";
	
	if($zip->open($filename))
	{
		for ($i=0; $i<$zip->numFiles;$i++) 
		{
			$fl = $zip->statIndex($i);
			if($fl['name'] == "classes.dex")
			   echo date ("d/m/Y H:i:s", $fl['mtime']);
		}
	}
?>