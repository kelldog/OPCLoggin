<?php

$link = mysql_connect('localhost', 'root', 'aqt');
mysql_select_db('aqt', $link);

$id = (isset($_GET['id']) ? $_GET['id'] : '');
$id = 11259;
$sd =  (isset($_GET['sd']) ? $_GET['sd'] : '');
$ed =  (isset($_GET['ed']) ? $_GET['ed'] : '');

$result = mysql_query("SELECT Name FROM aqt_fields where ID = $id ", $link);

$row = mysql_fetch_assoc($result);

$FN = $row['Name'];
$FN .= ".csv";


header("Content-type: text/csv");
header("Content-Disposition: attachment; filename=$FN");
header("Pragma: no-cache");
header("Expires: 0");


$result = mysql_query("SELECT Time,Value FROM opc_data where ID = $id AND time > '$sd' AND time < '$ed' ORDER BY Time ASC", $link);

while ($row = mysql_fetch_assoc($result)) 
{
	echo $row['Time'].",".$row['Value']."\r\n";
}

?>