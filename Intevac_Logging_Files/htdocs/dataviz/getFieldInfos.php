<?php


#echo "areare";
ini_set("display_errors","1");
ERROR_REPORTING(E_ALL);


$link = mysql_connect('localhost', 'root', 'aqt');
mysql_select_db('aqt', $link);

$ids = (isset($_GET['ids']) ? $_GET['ids'] : '');


$fid = array();

$tok = strtok($ids, ",");

while ($tok != false) 
{
    $fid[] = $tok;
    $tok = strtok(",");
}


$data = array();


$fieldinfo = array();
foreach($fid as $id )
{
	$result = mysql_query("SELECT id, Name, Scale, StationID, AQT_Name,StationTypeID, ChamberTypeID,Type FROM aqt_fields WHERE id = $id", $link);
	$info = array();
	while ($row = mysql_fetch_assoc($result)) 
	{
		$info['id' ]=$row['id'];
		$info['Name' ]=$row['Name'];
		$info['StationID' ] = $row['StationID'];
		$info['AQT_Name'] = $row['AQT_Name'];	
		$info['Type'] = $row['Type'];
	}
	$fieldinfo[] = $info;
}


$data['fieldinfo'] = $fieldinfo;

echo json_encode($data);


?>
