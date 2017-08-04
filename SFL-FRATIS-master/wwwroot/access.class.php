<?php

// ==============================================================================
// MODIFIED BY: Dmitri Zyuzin - University of Washington TRAC team
// ==============================================================================
 
/**
 * PHP Class to user access (login, register, logout, etc)
 * 
 * <code><?php
 * include('access.class.php');
 * $user = new flexibleAccess();
 * ? ></code>
 * 
 * For support issues please refer to the webdigity forums :
 *				http://www.webdigity.com/index.php/board,91.0.html
 * or the official web site:
 *				http://phpUserClass.com/
 * also check and contribute to our gitHub repo:
 *				https://github.com/HumanWorks/phpUserClass
 * ==============================================================================
 * 
 * @version $Id: access.class.php,v 0.93 2008/05/02 10:54:32 $
 * @copyright Copyright (c) 2007 Nick Papanotas (http://www.webdigity.com)
 * @author Nick Papanotas <nikolas@webdigity.com>
 * @license http://opensource.org/licenses/gpl-license.php GNU General Public License (GPL)
 * 
 * ==============================================================================

 */
 
include_once("config.php");
 
class flexibleAccess{
    var $dbName = '';
    var $dbHost = '';
    var $dbPort = 3306;
    var $dbUser = '';
    var $dbPass = '';

    var $dbTable  = 'users';
	
    var $businessInfoTable  = 'business_info';

    var $sessionVariable = 'userSession';

    var $tbFields = array(
    	'id'=> 'id', 
    	'login' => 'login',
    	'password'  => 'password',
    	'email' => 'email',
    	'type'=> 'type',
    	'active'=> 'active',
    	'first'=> 'first',
    	'last'=> 'last',
    	'phone'=> 'phone',
		'business_id' => ''
    );

    /**
    * When user wants the system to remember him/her, how much time to keep the cookie? (seconds)
    * int
    */
    var $remTime = 2592000;//One month
    /**
    * The name of the cookie which we will use if user wants to be remembered by the system
    * string
    */
    var $remCookieName = 'ckSavePass';
    /**
    * The cookie domain
    * string
    */
    var $remCookieDomain = '';
    /**
    * The method used to encrypt the password. It can be sha1, md5 or nothing (no encryption)
    * string
    */
    var $passMethod = 'sha1';
    /**
    * Display errors? Set this to true if you are going to seek for help, or have troubles with the script
    * bool
    */
    var $displayErrors = true;
    /*Do not edit after this line*/
    var $userID = false;
    var $dbConn;
    var $userData=array();
    var $businessData=array();
    
    function logf($msg) 
    {
        $fd = fopen("./logs/user_log.txt", "a"); // write string 
        fwrite($fd, date('d/m/Y - H:ia') . "  :  " . $msg . "\n"); // close file 
        fclose($fd); 
    }
    
    /**
    * Class Constructure
    * 
    * @param string $dbConn
    * @param array $settings
    * @return void
    */
    function __construct()
    {
	    global $dbhost, $username, $password, $database;
		
		ini_set('error_log', getcwd().'/logs/user_error.log');
    
        $this->dbName = $database;
        $this->dbHost = $dbhost;
        $this->dbUser = $username;
        $this->dbPass = $password;
        
        $this->remCookieDomain = $this->remCookieDomain == '' ? $_SERVER['HTTP_HOST'] : $this->remCookieDomain;
        $this->dbConn = mysql_connect($this->dbHost.':'.$this->dbPort, $this->dbUser, $this->dbPass);
        if ( !$this->dbConn ) die(mysql_error($this->dbConn));
        mysql_select_db($this->dbName, $this->dbConn)or die(mysql_error($this->dbConn));
        if( !isset( $_SESSION ) ) { session_start(); $this->logf("Starting Session"); }
        if ( isset($_SESSION[$this->sessionVariable]) )
        {
    	    $this->loadUser( $_SESSION[$this->sessionVariable] );
        	$this->logf ("Session Exists: " . $_SESSION[$this->sessionVariable] . " Logging In");
        }
        //Maybe there is a cookie?
        if ( isset($_COOKIE[$this->remCookieName]) && !$this->is_loaded()){
			$u = unserialize(base64_decode($_COOKIE[$this->remCookieName]));
			$this->logf ("Cookie Exists: " . $u['login'] . " Logging In");
			$this->login($u['login'], $u['password']);
        }
    }
 
	
    function checkTable()
    {
        //include("config.php");
    	// Create user SQL table if it doesn't exist
    	$query = 	"CREATE TABLE IF NOT EXISTS `$this->dbTable` (
    				`id` int(11) NOT NULL AUTO_INCREMENT,
    				`login` varchar(64) NOT NULL,
    				`password` varchar(128) NOT NULL,
    				`email` varchar(128) NOT NULL,
    				`active` int(1),
    				`type` varchar(128),
    				`first` varchar(128),
    				`last` varchar(128),
    				`phone` varchar(128),
					`business_id` varchar(128),
    				PRIMARY KEY (`id`) )";
    				
    	$this->query($query);  
    }
	
    function checkBusinessInfoTable()
    {
        //include("config.php");
    	// Create user SQL table if it doesn't exist
    	$query = 	"CREATE TABLE IF NOT EXISTS `$this->businessInfoTable` (
    				`business_id` varchar(128) NOT NULL,
    				`business_type` varchar(64) NOT NULL,
    				`business_name` varchar(128) NOT NULL,
    				`business_location` varchar(256) NOT NULL,
    				`business_icon` varchar(128) NOT NULL,
    				PRIMARY KEY (`business_id`) )";
    				
    	$this->query($query);  
    }
	
  
    /**
    	* Login function
    	* @param string $uname
    	* @param string $password
    	* @param bool $loadUser
    	* @return bool
    */
    function login($uname, $password, $remember = true, $hashed = false)
    {
		 $this->logf('Pss: ' . $password);
        $this->logf('logging in: ' . $uname);
    	$uname    = $this->escape(str_replace("'", "", $uname));
    	$password = $originalPassword = $this->escape(str_replace("'", "", $password));
		

		
		
		if($hashed == false)
		{
			switch(strtolower($this->passMethod)){
			  case 'sha1':
				$password = "SHA1('$password')"; break;
			  case 'md5' :
				$password = "MD5('$password')";break;
			  case 'nothing':
				$password = "'$password'";
			}
		}
		else
		{
		    //$this->logf('Pss: ' . $password);
			$password = "'$password'";
		}

		$query = "SELECT * FROM `{$this->dbTable}` 
    	WHERE `{$this->tbFields['login']}` = '$uname' AND `{$this->tbFields['password']}` = $password LIMIT 1";
		
    	$res = $this->query($query,__LINE__);

    	if ( @mysql_num_rows($res) == 0)
		{
    		return false;
		}
			
		if($this->is_loaded())
			$this->logout("");
			
		if ( $remember ){
			$cookie = base64_encode(serialize(array('login'=>$uname,'password'=>$originalPassword)));
			$a = setcookie($this->remCookieName, 
							$cookie,time()+$this->remTime, '/', $this->remCookieDomain);
							
			$this->loadUser($uname);
		}


    	return true;
    }
    
    function cbidToBusinessName($cbid)
    {
		$query = "SELECT * FROM `business_info` 
    	WHERE `business_id` = '$cbid' LIMIT 1";
		
    	$res = $this->query($query);

    	if ( @mysql_num_rows($res) == 0)
		{
    		return false;
		}
		else
	    {
    	    $bn = mysql_fetch_array($res);
    	    return $bn['business_name'];
	    }
    }
    
    /**
    	* Logout function
    	* param string $redirectTo
    	* @return bool
    */
    function logout($redirectTo = '')
    {
		$this->logf("goodbye " . $this->userID);
		
		$this->logf("session_destroy(): " . session_destroy());
        $this->logf("setcookie(): " . setcookie($this->remCookieName, '', time()-3600,'/', $this->remCookieDomain));
		unset($_COOKIE[$this->remCookieName]);
        //$_GET['logout'] = 0;
        unset($this->userData);
		unset($this->userID);
		
		
		return "ok";
		
        //if ( $redirectTo != '' && !headers_sent()){
        //   header('Location: '.$redirectTo );
        //   exit;//To ensure security
        //}
    }
    /**
    	* Function to determine if a property is true or false
    	* param string $prop
    	* @return bool
    */
    function is($prop){
    	return $this->get_property($prop)==1?true:false;
    }
    
    /**
    	* Get a property of a user. You should give here the name of the field that you seek from the user table
    	* @param string $property
    	* @return string
    */
    function get_property($property)
    {
        if (empty($this->userID)) $this->error('No user is loaded', __LINE__);
		//Check User Data
        if (isset($this->userData[$property]) && !empty( $this->userData[$property] ))
        	return $this->userData[$property];
			
		//Check Business Data
		elseif(isset($this->businessData[$property]) && !empty( $this->businessData[$property] ) )
			return $this->businessData[$property];
		else
			return "null";
    }
    /**
    	* Is the user an active user?
    	* @return bool
    */
    function is_active()
    {
		$this->logf("is_active(): " . $this->userData[$this->tbFields['active']]);
        return $this->userData[$this->tbFields['active']];
    }
    
    /**
    * Is the user loaded?
    * @ return bool
    */
    function is_loaded()
    {
        return ($this->userID == false) ? false : true;
    }
    /**
    	* Activates the user account
    	* @return bool
    */
    function activate()
    {
        if (empty($this->userID)) $this->error('No user is loaded', __LINE__);
        if ( $this->is_active()) $this->error('Allready active account', __LINE__);
        $res = $this->query("UPDATE `{$this->dbTable}` SET {$this->tbFields['active']} = 1 
        WHERE `{$this->tbFields['login']}` = '".$this->escape($this->userID)."' LIMIT 1");
        if (@mysql_affected_rows() == 1)
        {
        	$this->userData[$this->tbFields['active']] = true;
        	return true;
        }
        return false;
    }
    /*
    * Creates a user account. The array should have the form 'database field' => 'value'
    * @param array $data
    * return int
    */  
    function insertUser($data){
        $this->checkTable();
        if (!is_array($data)) $this->error('Data is not an array', __LINE__);
        switch(strtolower($this->passMethod)){
          case 'sha1':
          	$password = "SHA1('".$data[$this->tbFields['password']]."')"; break;
          case 'md5' :
          	$password = "MD5('".$data[$this->tbFields['password']]."')";break;
          case 'nothing':
          	$password = $data[$this->tbFields['password']];
        }
        foreach ($data as $k => $v ) $data[$k] = "'".$this->escape($v)."'";
        $data[$this->tbFields['password']] = $password;
				
		if(!$this->userExists($data[$this->tbFields['login']]))
		{
			$this->logf("User does not exist, creating");
			$this->query("INSERT INTO `{$this->dbTable}` (`".implode('`, `', array_keys($data))."`) VALUES (".implode(", ", $data).")");
			
			$this->logf("Psss: ");
			
			$this->login($data[$this->tbFields['login']], $data[$this->tbFields['password']], true, false);
			return (int)1;
		}
		else
		{
			$this->logf("Error: user exists: " . $data[$this->tbFields['login']]);
			return 0;
		}
    }
    /*
    * Creates a random password. You can use it to create a password or a hash for user activation
    * param int $length
    * param string $chrs
    * return string
    */
    function randomPass($length=10, $chrs = '1234567890qwertyuiopasdfghjklzxcvbnm'){
        for($i = 0; $i < $length; $i++) {
            $pwd .= $chrs{mt_rand(0, strlen($chrs)-1)};
        }
        return $pwd;
    }
    ////////////////////////////////////////////
    // PRIVATE FUNCTIONS
    ////////////////////////////////////////////
    
    /**
    	* SQL query function
    	* @access private
    	* @param string $sql
    	* @return string
    */
    function query($sql, $line = 'Uknown')
    {
        //if (defined('DEVELOPMENT_MODE') ) echo '<b>Query to execute: </b>'.$sql.'<br /><b>Line: </b>'.$line.'<br />';
        $res = mysql_query($sql, $this->dbConn);
        if ( !$res )
        	$this->error(mysql_error($this->dbConn), $line);
        return $res;
    }
    
    /**
    	* A function that is used to load one user's data
    	* @access private
    	* @param string $userID
    	* @return bool
    */
    function loadUser($userID)
    {
		$this->logf("Loading User:" . $userID);
        $res = $this->query("SELECT * FROM `{$this->dbTable}` WHERE `{$this->tbFields['login']}` = '".$this->escape($userID)."' LIMIT 1");
        if ( mysql_num_rows($res) == 0 )
        	return false;
        $this->userData = mysql_fetch_array($res);
        $this->userID = $userID;
        $_SESSION[$this->sessionVariable] = $this->userID;
		$this->logf("User Loaded:" . $this->userID);
		
		
		if($this->userData["business_id"] != "" && $this->userData["type"] == "business")
		{
			$res = $this->query("SELECT * FROM `{$this->businessInfoTable}` WHERE `business_id` = '".$this->userData["business_id"]."' LIMIT 1");
			if ( mysql_num_rows($res) == 1 )
			{
				$this->businessData = mysql_fetch_array($res);
				$this->logf("Business Data Loaded: " . $this->businessData["business_name"]);
			}
		}
		
        return true;
    }
	
    function userExists($userID)
    {
		$userID = str_replace("'", "", $userID); 
        $res = $this->query("SELECT * FROM `{$this->dbTable}` WHERE `{$this->tbFields['login']}` = '".$this->escape($userID)."' LIMIT 1");
		
		if ( mysql_num_rows($res) == 0 )
        	return false;
		else
        	return true;
    }
    
    /**
    	* Produces the result of addslashes() with more safety
    	* @access private
    	* @param string $str
    	* @return string
    */  
    function escape($str) {
        $str = get_magic_quotes_gpc()?stripslashes($str):$str;
        $str = mysql_real_escape_string($str, $this->dbConn);
        return $str;
    }
    
    /**
    	* Error holder for the class
    	* @access private
    	* @param string $error
    	* @param int $line
    	* @param bool $die
    	* @return bool
    */  
    function error($error, $line = '', $die = false) {
        if ( $this->displayErrors )
        	echo '<b>Error: </b>'.$error.'<br /><b>Line: </b>'.($line==''?'Unknown':$line).'<br />';
        if ($die) exit;
        return false;
    }
	
	function attach_business($business_id)
	{
		if($this->is_loaded())
		{
			$bid = $this->get_property("business_id");
			
			if($bid != "null") //Delete old business reports
			{
				$res = $this->query("DELETE from business_reports WHERE business_id= '".$this->escape($bid)."'");
			}
			
			$res = $this->query("UPDATE `{$this->dbTable}` SET business_id='{$business_id}' WHERE `{$this->tbFields['login']}` = '".$this->escape($this->userID)."'");
	
			$this->loadUser($this->userID);
			return true;
		}
		else
		{
			$this->logf("Attempted to attach business; but not loaded!!!");
			return false;
		}
	}
	
	function add_business_info($business_id, $business_type, $business_name, $business_location, $business_icon)
	{
		if($this->is_loaded())
		{
			$this->checkBusinessInfoTable();
			
			$bFields = array(
				'business_id'=> $business_id, 
				'business_type' => $business_type,
				'business_name'  => $business_name,
				'business_location' => $business_location,
				'business_icon' => $business_icon,
			);
			
			$query = 	"DELETE FROM `business_info`  WHERE `business_id` = '$business_id'"; 
			$this->query($query);
			
			$query = "INSERT INTO `{$this->businessInfoTable}` (`".implode('`, `', array_keys($bFields))."`) VALUES ('".implode("', '", $bFields)."')";
			
			$this->query($query);
			return true;
		}
		else
		{
			$this->logf("Attempted to add business info; but not loaded!!!");
			return false;
		}
	}
	
	
}
?>