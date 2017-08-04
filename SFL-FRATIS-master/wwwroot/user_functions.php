<?php 
require 'access.class.php';

$user = new flexibleAccess();

// Configure logs
ini_set('error_log', getcwd().'/logs/user_error.log');
header('Access-Control-Allow-Origin: *');  // Allow remote requests for JSON

function logff($msg) 
{
	$fd = fopen("./logs/user_functions_log.txt", "a"); // write string 
	fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
	fclose($fd); 
}

$func = "";
if(isset($_REQUEST["func"]))
	$func = $_REQUEST["func"];

switch ($func) 
{
	case "registerForm":
	{
		echo'
		<form id="registerUserForm" method="post">
		<div id="userType" style="width:600px">
			<input type="radio" id="civilian" name="userType" checked="checked"><label for="civilian"><img src="images/user/civilian.png" /><br>Civilian</label>
			<input type="radio" id="business" name="userType"><label for="business"><img src="images/user/business.png" /><br>Business Owner</label>
			<input type="radio" id="recon" name="userType"><label for="recon"><img src="images/user/recon.png" /><br>Emergency Crew</label>
		</div>
		<br>
		 <table width="95%" border="0" cellspacing="0" cellpadding="5">
		   <tr>
		     <td width="150" align="right">First Name:</td>
		     <td><input name="first" type="text" size="30" /></td>
	       </tr>
		   <tr>
		     <td align="right">Last Name: </td>
		     <td><input name="last" type="text" size="30" /></td>
	       </tr>
		   <tr>
		     <td align="right">Username:</td>
		     <td><input type="text" name="login" size="30"/></td>
	       </tr>
		   <tr>
		     <td align="right">Password:</td>
		     <td><input type="password" name="password"  size="30"/></td>
	       </tr>
		   <tr>
		     <td align="right">Email:</td>
		     <td><input name="email" type="text" size="30" /></td>
	       </tr>
		   <tr>
		     <td align="right">Mobile Phone:</td>
		     <td><input name="phone" type="text" size="30" /></td>
	       </tr>
        </table>
		
		<button id="registerSubmit" type="submit">Next</button>
		</form>';
	}
    break;
	
	case "loginForm":
	{
		echo'
		<center></center>
		<table width="100%">
		<tr>
		<td>
			<img src="images/login_icon.png">
		</td>
		<td>
			<form id="loginForm" method="post">
			 <table width="95%" border="0" cellspacing="0" cellpadding="5">
			   <tr>
				 <td align="right">Username:</td>
				 <td><input type="text" name="login" size="30"/></td>
			   </tr>
			   <tr>
				 <td align="right">Password:</td>
				 <td><input type="password" name="password"  size="30"/></td>
			   </tr>
			</table>
			<div align="center">
			<button id="userLogin" type="submit">Login</button>
			</div>
			</form>
		</td>
		</tr>
		</table>';
		
	}
    break;
	
	case "businessForm":
	{
		echo'
		<form id="businessForm" method="post">
		What type of business do you own?<br>
		<div id="businessType">
			<input type="radio" id="freight" 				name="businessType"><label for="freight"	>Sea Port</label>
			<input type="radio" id="terminal" 				name="businessType"><label for="terminal"	>Freight Terminal</label>
			<input type="radio" id="depot" 					name="businessType"><label for="depot"		>Point of Distribution</label>
			<input type="radio" id="gas_station" 			name="businessType"><label for="gas_station"	>Gas Station</label>
			<input type="radio" id="grocery_or_supermarket" name="businessType"><label for="grocery_or_supermarket"	>Grocery Store/Supermarket</label>
			<input type="radio" id="hardware_store"			name="businessType"><label for="hardware_store"	>Hardware Store</label>
			<input type="radio" id="hospital" 				name="businessType"><label for="hospital"		>Hospital/Doctor</label>
			<input type="radio" id="pharmacy" 				name="businessType"><label for="pharmacy"		>Pharmacy</label>
			<input type="radio" id="store" 					name="businessType"><label for="store"			>Sporting Goods</label>
			<input type="radio" id="clothing_store" 		name="businessType"><label for="clothing_store"	>Clothing</label>
			<input type="radio" id="other" 					name="businessType"><label for="other"			>Other</label>
		</div>
		<hr>
		Find Business:
		 <table width="95%" border="0" cellspacing="0" cellpadding="5">
		   <tr>
		     <td width="150" align="right">Business Name:</td>
		     <td><input name="businessName" id="businessName" type="text" size="20" /></td>
			 <td><button id="businessFind" type="submit">Find</button></td>
	       </tr>
        </table>
		<hr>
		Or enter the business manually:
		<br />
		<div id="businessPlace">Place on Map</div>
		<div id="businessAddress">Enter Address</div>
		
		</form>';
		
	}
    break;
	
	case "logout":
		echo $user->logout('');
	break;
	
	case "getBusinessType":
		echo $user->get_property("business_type");
	break;
	
	case "getLoginType":
		echo $user->get_property("type");
	break;
	
	case "phone_login":
	{
		if ( isset($_REQUEST['login']) && isset($_REQUEST['password']))
		{
			$password = $_REQUEST['password'];
			
			if(strlen(preg_replace('/[^A-Za-z0-9\-]/', '', $password)) != 40)
				$password = sha1($password);
			
			if ( !$user->login($_REQUEST['login'],$password, true, true))
				echo 'invalid,null,null,null';
			else
				echo 'valid,' . $user->get_property("type") . "," . $user->get_property("business_id") . "," .  $user->get_property("business_name") . "," .  $user->get_property("business_type");
		} 
	}
	break;
	
	case "get_my_business_report":
	{
		if(isset($_REQUEST['business_id']) && $_REQUEST['business_id'] != "")
			$business_id = $_REQUEST['business_id'];
		else
			$business_id = $user->get_property("business_id");

		$query = "SELECT * FROM business_reports WHERE business_id='$business_id';";
		$businessReports = $user->query($query);
		while( $row = mysql_fetch_array( $businessReports ) ) 
		    $json[] = $row;

		echo json_encode( $json );	
	}
	break;
	
	case "login":
	{
		if (!empty($_REQUEST['login']) && !empty($_REQUEST['password']))
		{
			if($user->login($_REQUEST['login'], $_REQUEST['password']))
		  		echo "ok";
			else
				echo "error";
		}
	}
	break;
	
	case "add_business_info":
	{
		if ($user->is_loaded())
		{
			try
			{
				$business_id = mysql_escape_string($_REQUEST['business_id']);
				$business_type = mysql_escape_string($_REQUEST['business_type']);
				$business_name = mysql_escape_string($_REQUEST['business_name']);
				$business_location = mysql_escape_string($_REQUEST['business_location']);
				$business_icon = mysql_escape_string($_REQUEST['business_icon']);
				
				$user->add_business_info($business_id, $business_type, $business_name, $business_location, $business_icon);
				logff("Adding Business Info: " . $business_name);
			}
			catch(Exception $e)
			{
				logff("ERROR: " . $e->getMessage());
			}
			
		}
		else
		{
			echo "notloaded";
			logff("Error! Cannot add business info; User is not loaded!");
		}
	}
	break;
	
	case "attach_business":
	{
		if ($user->is_loaded())
		{
			if($user->attach_business($_REQUEST['business_id']))
			{
				logff("Attaching Business: " . $_REQUEST['business_id']);
		  		echo "ok";
			}
			else
				echo "error";
		}
		else
		{
			echo "notloaded";
			logff("Error! Cannot attach business; User is not loaded!");
		}
	}
	break;

	case "register":
	{
		if (!empty($_REQUEST['login']))
		{
		  //The logic is simple. We need to provide an associative array, where keys are the field names and values are the values :)
		  $data = array(
			'login' => mysql_escape_string($_REQUEST['login']),
			'password' => mysql_escape_string($_REQUEST['password']),
			'email' => mysql_escape_string($_REQUEST['email']),
			'active' => 1,
			'type' => $_REQUEST['type'],
			'first' => mysql_escape_string($_REQUEST['first']),
			'last' => mysql_escape_string($_REQUEST['last']),
			'phone' => mysql_escape_string($_REQUEST['phone']),
			'business_id' => ''
		  );
		  
		  $userID = $user->insertUser($data);//The method returns the userID of the new user or 0 if the user is not added
		  if ($userID==0)
		  {
			logff("Error: user exists");
			echo 'exists';//user is allready registered or something like that
		  }
		  else
		  {
			echo 'success';
			logff("Sucess: user created!");
			$user->login($_REQUEST['login'], $_REQUEST['password'], true, false);
		  }
		}
		else
			echo "Error: Access Denied";
	}
	break;
}
?>