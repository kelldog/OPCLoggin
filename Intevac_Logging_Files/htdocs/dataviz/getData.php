<?php

ini_set("display_errors","1");
ERROR_REPORTING(E_ALL);

$link = mysql_connect('localhost', 'root', 'aqt');
mysql_select_db('aqt', $link);

$q = (isset($_GET['q']) ? $_GET['q'] : '');
$id = (isset($_GET['id']) ? $_GET['id'] : '');
$fid = (isset($_GET['fid']) ? $_GET['fid'] : '');
$sd =  (isset($_GET['sd']) ? $_GET['sd'] : '');
$ed =  (isset($_GET['ed']) ? $_GET['ed'] : '');
$ww = (isset($_GET['ww']) ? $_GET['ww'] : '');

//oldest mem timestamp (the table switch)
/*
if(isset($_GET['sw'])) //try to post to save time
{
  $sw = $_GET['sw'];
}
else
{
  $result = mysql_query("select UNIX_TIMESTAMP(Time) as tstamp FROM opc_data_mem  where 1 ORDER BY Time ASC LIMIT 1", $link);
  $row = mysql_fetch_assoc($result);
	$sw = $row['tstamp'];
}*/



if ($q == 's'){
	$result = mysql_query('select distinct stationid from aqt_fields', $link);
	$res = array();
	while ($obj = mysql_fetch_object($result)) {
	    $res[] = $obj;  
	}
	echo json_encode($res);
}

if ($q == 'f'){
	$result = mysql_query("SELECT id, Name, Scale, StationTypeID, ChamberTypeID FROM aqt_fields where StationID = $id", $link);
	while ($row = mysql_fetch_assoc($result)) {
		if ($fid == $row['id']){
			echo "<option value='".$row['id']."' SELECTED>".$row['Name']."</option>";
		}
		else echo "<option value='".$row['id']."'>".$row['Name']."</option>";
	}
}

if ($q == 'z')
{
  $result = mysql_query("SELECT UNIX_TIMESTAMP(Time) as tstamp FROM opc_data_mem where 1 ORDER BY Time ASC LIMIT 1", $link);
  while ($row = mysql_fetch_assoc($result)) 
  {
	  echo $row['tstamp'];
  }
}

if ($q == 'd'){
	sort($fid);
	
	$i = 0;
	$where = '';
	foreach($fid as $f){
		$where = $where."id=$f";
		if ($i<sizeof($fid)-1){
			$where = $where." or ";
		}
		$i++;
	}
	
	$query = 'select id, Name , Units , StationID from aqt_fields where '.$where.' order by id';
	//echo $query;
	$result = mysql_query($query, $link);
	$res['status'] = 'ok';
	//$res['table']['cols'][0] = array('id' => '','label' => 'Time', 'type' => 'datetime');
	$i = 0;
	while ($row = mysql_fetch_assoc($result)) {
	    $res['table']['cols'][$row["id"]] = array('id' => $row["id"], 'label'=> 'ST:'.$row["StationID"].' '.strrchr( $row["Name"] ,'.').' '.$row["Units"], 'type'=>'number','units'=>$row["Units"]);
	    $i++; 
	}
	
	$i = 0;
	$query = "select unix_timestamp(time) as time, ";
	if( count($fid) == 1 ) # if the query only has one field this query is way faster
	{
		$query = $query."Value as '$fid[0]'";
	}
	else
	{
		foreach($fid as $f)
		{
			$query = $query."max(IF(id=$f,value,'')) as '$f'";
		
			if ($i<sizeof($fid)-1)
			{
				$query = $query.", ";
			}
			$i++;
		}
	}
	$query .= ' from opc_data_mem';
	if ($sd && $ed)
  	{
		$query .= " where Time > '$sd' and Time < '$ed'";
	}
  	else 
  	{
    		$query .= " where time > NOW() - INTERVAL $ww SECOND";
  	}
	if( count($fid) == 1 )
	{
		$query .= " AND ID='$fid[0]' ";
		$query .= 'order by time ASC';
	}
	else
	{
		$query .=' AND ID IN(';
		$r = 0;
		foreach($fid as $f)
		{
			$query.="$f";
			if ($r != sizeof($fid)-1)
			{
				$query.=',';
			}
			$r++;
		}
		$query .=')';
		$query .= ' group by time order by time ASC';
	}
	$query .= ' limit 466000';
  

	$res['query'] = $query;
	$startunixtime = 0;
	$res['table']['time'] = array();
	$res['table']['ys'] = array();
	foreach($fid as $f)
	{
		$res['table']['ys'][$f] = array();
	}
	$result = mysql_query($query, $link);
	$i = 0;
	while ($row = mysql_fetch_assoc($result)) 
	{		
		if( $i == 0 )
		{
			$startunixtime = intval($row['time']);
		}
		$res['table']['time'][] = intval($row['time'])  -  $startunixtime;//$res['table']['rows'][$i]['c'][0]['v']
		foreach($fid as $f)
		{
			$res['table']['ys'][$f][] = $row[$f];
		}
	$i++;
	}
	

	$res['startunixtime'] = $startunixtime;
	echo json_encode($res);
}

?>
