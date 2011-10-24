<?php

ini_set("display_errors","1");
ERROR_REPORTING(E_ALL);

$link = mysql_connect('localhost', 'root', 'aqt');
mysql_select_db('aqt', $link);

$q = (isset($_GET['q']) ? $_GET['q'] : '');
$id = (isset($_GET['id']) ? $_GET['id'] : '');
$n = (isset($_GET['n']) ? $_GET['n'] : '');
$sid = (isset($_GET['sid']) ? $_GET['sid'] : '');
$fid = (isset($_GET['fid']) ? $_GET['fid'] : '');

if ($q == 's'){
	$result = mysql_query('select id, description from save_state order by description', $link);
	echo "<option value=''>&nbsp;</option>";
	while ($row = mysql_fetch_assoc($result)) {
		echo "<option value='".$row['id']."'>".$row['description']."</option>";
	}
}

if ($q == 'si'){
	$result = mysql_query("select station_id, field_id from save_state_fields where state_id=$id", $link);
	$res = array();
	while ($obj = mysql_fetch_object($result)) {
	    $res[] = $obj;  
	}
	echo json_encode($res);
}

if ($q == 'sv'){
	if ($n){
		$query = "insert into save_state (description) values ('".$n."')";
		$result = mysql_query($query, $link);
		
		$nid = mysql_insert_id();
		
		$query = "insert into save_state_fields (state_id, station_id, field_id) values ";
		foreach ($sid as $k => $s) {
			$query .= "($nid,$s,$fid[$k])";
			if ($k < count($sid)-1){
				$query .= ",";
			}
		}
		$result = mysql_query($query, $link);
		$res['r'] = 'State saved';

		echo json_encode($res);
	}
}

if ($q == 'ds'){
	$result = mysql_query("delete from save_state_fields where state_id=$sid", $link);
	$result = mysql_query("delete from save_state where id=$sid", $link);
	$res['r'] = 'State deleted';
	echo json_encode($res);
}

?>
