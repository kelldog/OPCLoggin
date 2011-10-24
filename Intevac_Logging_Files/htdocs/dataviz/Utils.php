<?php

ini_set("display_errors","1");
ERROR_REPORTING(E_ALL);

$link = mysql_connect('localhost', 'root', 'aqt');
mysql_select_db('aqt', $link);

$result = mysql_query("SELECT Time FROM opc_data_mem  where 1 ORDER BY Time ASC LIMIT 1", $link);

while ($row = mysql_fetch_assoc($result)) 
{
	echo $row['Time'];
}

?>
