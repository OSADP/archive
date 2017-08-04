<?php
	header('Access-Control-Allow-Origin: *'); 
	
	if(isset($_REQUEST["post"]))
	{
		$msg = "";
		if(isset($_REQUEST["msg"]))
			$msg = $_REQUEST["msg"];
			
		$url = "";
		if(isset($_REQUEST["url"]))
			$url = $_REQUEST["url"];
			
		$user = "";
		if(isset($_REQUEST["user"]))
			$user = $_REQUEST["user"];
			
		$line = "";
		if(isset($_REQUEST["line"]))
			$line = $_REQUEST["line"];
			
		$page = "";
		if(isset($_REQUEST["page"]))
			$page = $_REQUEST["page"];
			
		$styletext = " style='line-height:auto;";
		if($page == "reportpage.php")
			$styletext = $styletext . "background-color:yellow;";
			
		$styletext = $styletext . "'";
			
		$browser = "";
		if(isset($_REQUEST["browser"]))
			$browser = $_REQUEST["browser"];
			
		$cli = $_SERVER['REMOTE_ADDR'];
			
		if (strpos($cli, '66.235.54') !== FALSE)
			$cli = "Dmitri - HOME";
			

		$fd = fopen("./logs/JavascriptErrors.log", "a"); // write string 
		fwrite($fd, "<tr" . $styletext . "><td height='10'>" . $user . "</td><td height='10'>" . $cli . "</td><td height='10'>" . date('d/m/Y - H:ia') . "</td> <td height='10'>" . $url . "</td> <td height='10'>" . $line . "</td> <td height='10'>" . $msg . "</td> <td height='10'>" . $browser . "</td></tr>\n"); // close file 
		fclose($fd); 
		
		echo "ok";
		
	}
	elseif(isset($_REQUEST["console"]))
	{
		$msg = "";
		if(isset($_REQUEST["msg"]))
			$msg = $_REQUEST["msg"];
			
		$user = "";
		if(isset($_REQUEST["user"]))
			$user = $_REQUEST["user"];
			
		$page = "";
		if(isset($_REQUEST["page"]))
			$page = $_REQUEST["page"];
			
		$styletext = " style='line-height:auto;";
		if($page == "reportpage.php")
			$styletext = $styletext . "background-color:yellow;";
			
		$styletext = $styletext . "'";
			
		$browser = "";
		if(isset($_REQUEST["browser"]))
			$browser = $_REQUEST["browser"];
			
		$cli = $_SERVER['REMOTE_ADDR'];
		
			
		if (strpos($cli, '66.235.54') !== FALSE)
			$cli = "Dmitri";
			
		$fd = fopen("./logs/JavascriptErrors.log", "a"); // write string 
		fwrite($fd, "<tr" . $styletext . "><td height='10'>" . $user . "</td><td height='10'>" . $cli . "</td><td height='10'>" . date('d/m/Y - H:ia') . "</td> <td height='10'>Console - " . $page . "</td> <td height='10'>Log</td> <td>" . $msg . "</td> <td height='10'>" . $browser . "</td></tr>\n"); // close file 
		fclose($fd);
	}
	else
	{
		if(isset($_REQUEST["delete"]))
		{
			if(file_exists("./logs/JavascriptErrors.log")) 
			{
				unlink("./logs/JavascriptErrors.log");
			}
		}
?>
        <!DOCTYPE html>
        <html lang="en">
        <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <title>Fratis Client Javascript Error Log</title>
        <script src="js/jquery-1.10.2.min.js" type="text/javascript"></script>
        <script type="text/javascript">
		<?php 
			if(isset($_REQUEST["delete"]))
			{
				echo "location.href = 'error_log.php';";	
			}
			
			?>
		
		</script>
        </head>
        <body>
        <a href="error_log.php?delete=1"><font size=6>Clear Log</font></a>
        <table width="100%" border="1" cellspacing="0" cellpadding="0">
        <tr><td>User</td><td>Client</td> <td>Date/Time</td> <td> URL </td> <td> Line </td> <td> Error Message </td> <td> Browser </td></tr>
        <?php if(file_exists("./logs/JavascriptErrors.log")) { echo file_get_contents("./logs/JavascriptErrors.log"); }?>
        <tr style='height:100%'><td>User</td><td>Client</td> <td>Date/Time</td> <td> URL </td> <td> Line </td> <td> Error Message </td> <td> Browser </td></tr>
        </table>
        </body>
        
        </html>
<?php } ?>