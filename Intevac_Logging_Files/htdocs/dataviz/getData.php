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
  $result = mysql_query("select Time FROM opc_data_mem  where 1 ORDER BY Time ASC LIMIT 1", $link);
  while ($row = mysql_fetch_assoc($result)) 
  {
	  echo $row['Time'];
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
	$res['table']['cols'][0] = array('id' => '','label' => 'Time', 'type' => 'datetime');
	$i = 1;
	while ($row = mysql_fetch_assoc($result)) {
	    $res['table']['cols'][$i] = array('id' => $row["id"], 'label'=> 'ST:'.$row["StationID"].'  '.strrchr( $row["Name"] ,'.').'  '.$row["Units"], 'type'=>'number');
	    $i++; 
	}
	
	$i = 0;
	$query = "select unix_timestamp(time) as time, ";
	foreach($fid as $f){
		$query = $query."max(IF(id=$f,value,'')) as '$f'";
		if ($i<sizeof($fid)-1){
			$query = $query.", ";
		}
		$i++;
	}
	
	$query .= ' from opc_data';
	if ($sd && $ed){
		$query .= " where time > '$sd' and time < '$ed'";
	}
  else {
    $query .= " where time > NOW() - INTERVAL 4 MINUTE";
  }
	$query .= ' group by time order by time limit 300000';
	//echo $query;
	$result = mysql_query($query, $link);
	$i = 0;
	while ($row = mysql_fetch_row($result)) {
		$j = 0;
		while ($j<=sizeof($fid)){
			$res['table']['rows'][$i]['c'][$j]['v'] = $row[$j];
			$j++;
		}
		$i++;
	}
	
	echo json_encode($res);
}

?>
