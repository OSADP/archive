<?php
	// Lists reports for App's Automatic Report Updating functions
	$directory = 'reports';
	chmod($directory, 0777);
	$scanned_directory = array_diff(scandir($directory), array('..', '.'));
	
	foreach($scanned_directory as $fl)
	{
	    if (strpos($fl, '.xml') !== FALSE)
		    echo $fl . "," . filesize($directory . '/' . $fl) . "\n";
	}
?>