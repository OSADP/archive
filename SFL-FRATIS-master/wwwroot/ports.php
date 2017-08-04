<?php
	include("config.php");
	
	class locationObject
	{
		public $lat;
		public $lng;	
	}
	
	class geometryObject
	{
		public $location;	
	}
	
	class portObject
	{
		public $geometry;
		public $id;
		public $name;
		public $icon = "images/business/freight.png";
	}
	
	$pstring = file_get_contents("ports.dat");
	
	$ports = json_decode($pstring);
	
	$outPorts = array();
	echo '[';
	$first = true;
	
	foreach($ports as $port)
	{
		if(!$first)
			echo ",";
		
		$first = false;
		$portOut = new portObject();
		
		$portOut->id = $port->id;
		$portOut->name = $port->name;
		$portOut->geometry = new geometryObject();
		$portOut->geometry->location = new locationObject();
		$portOut->geometry->location->lat = floatval($port->lat);
		$portOut->geometry->location->lng = floatval($port->lng);
		print (json_encode($portOut,JSON_FORCE_OBJECT));
	}
	echo ']';
?>
