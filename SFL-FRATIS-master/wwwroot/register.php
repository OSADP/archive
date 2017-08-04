<?php
  require 'access.class.php';
  $user = new flexibleAccess();

if (!empty($_POST['login'])){
  $data = array(
  	'login' => $_POST['login'],
  	'password' => $_POST['password'],
  	'email' => $_POST['email'],
  	'active' => 1,
  	'type' => $_POST['type'],
  	'first' => $_POST['first'],
  	'last' => $_POST['last'],
  	'phone' => $_POST['phone'],
  );
  $userID = $user->insertUser($data);//The method returns the userID of the new user or 0 if the user is not added
  if ($userID==0)
  	echo 'exists';//user is allready registered or something like that
  else
  	echo 'success';
}
else
{
	echo "Error: Access Denied";	
}
?>