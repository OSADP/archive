<?php

$dbhost = "localhost";
$username = "";
$password = "";
$database = "";

function run_query($sql) 
{
    global $dbhost, $username, $password, $database;
    $con = mysql_connect($dbhost, $username, $password);
    mysql_select_db($database, $con);
    $result = mysql_query($sql);
    if (!$result) {
        return ('Invalid query: ' . mysql_error());
    }
	
	if( is_bool($result) )
	{
		$res = $result;
	}
	else
	{
		$res = array();
		$x = 0;
		while ($row = mysql_fetch_array($result)) {
			$res[$x] = $row;
			$x++;
		}
	}
	
    mysql_close($con);
	
    return $res;
}

function update_query($sql) 
{
    global $dbhost, $username, $password, $database;
    $con = mysql_connect($dbhost, $username, $password);
    mysql_select_db($database, $con);
    $result = mysql_query($sql);
    return mysql_affected_rows();
    mysql_close($con);
}

?>