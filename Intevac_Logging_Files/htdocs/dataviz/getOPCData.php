<?php


	#echo "areare";
	ini_set("display_errors","1");
	ERROR_REPORTING(E_ALL);



	$link = mysql_connect('localhost', 'root', 'aqt');
	mysql_select_db('aqt', $link);

	$ids = (isset($_GET['ids']) ? $_GET['ids'] : '');
	$sd =  $_GET['sd'];
	$ed =  (isset($_GET['ed']) ? $_GET['ed'] : '');

	//$sd = intval($sd);
	$fid = array();

	$tok = strtok($ids, ",");

	while ($tok != false) 
	{
	    $fid[] = $tok;
	    $tok = strtok(",");
	}

	$data = array();

	/*
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
	*/


	$data['time'] = array();

	$query = "SELECT unix_timestamp(time) AS time, ";
	$i = 0;
	foreach($fid as $f)
	{
		$data[$f] = array();
		$query = $query."max(IF(id=$f,value,'')) as '$f'";
		if ($i<sizeof($fid)-1)
		{
			$query = $query.", ";
		}
		$i++;
	}

	$query .= ' FROM opc_data_mem';


	$stime = date("Y-m-d H:i:s", $sd);

	$query .= " WHERE  time > '$stime' ";#NOW() - INTERVAL $ww SECOND";


	//$query .= " WHERE  time> NOW() - INTERVAL 3600 SECOND";
	if ($ed)
	{	
		$etime = date("Y-m-d H:i:s", $ed);
		$query .= " AND  time <'$etime'";
	}

	$query .= ' group by time order by time LIMIT 30000';

	#echo $query;

	$result = mysql_query($query, $link);
	$last_time = 0;
	$z = 0;
	$time_offset = 0;

	while ($row=mysql_fetch_row($result)) 
	{
		if( $z == 0 )
		{
			$time_offset = intval( $row[0] );
		}

		$data['time'][] = $row[0] - $time_offset;	

		$j = 0;

		while ( $j < sizeof($fid) )
		{
			$data[$fid[$j]][] = $row[ $j + 1 ];
			$j++;
		}

		$last_time =  $row[0];
		$z++;
	}

	$data['start_time'] = $sd;
	if($last_time == 0 )
	{
		$data['end_time'] = $sd;
	}
	else
	{
		$data['end_time'] = $last_time;
	}

	$data['query'] = $query;
	

	echo json_encode($data);

?>

