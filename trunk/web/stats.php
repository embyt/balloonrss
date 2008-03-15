<html>
<head>
<title>Stats of BalloonRSS</title>
</head>
<body>
<?php
// constants
$DB_HOST = "mysql4-b";
$DB_USER = "b206266ro";
$DB_PWD = "logreader";
$DB_NAME = "b206266_releaseinfo";
$DB_TABLE = "logs";
$START_DATE = "2008-01-01 00:00:00";
 
$MY_IP = crc32($_SERVER['REMOTE_ADDR']);

// connect to database
$db = mysql_connect($DB_HOST, $DB_USER, $DB_PWD) or die("database error: " . mysql_error());
mysql_select_db($DB_NAME, $db) or die(mysql_error());

// build sql string
echo "<h1>Weekly access results</h1>\n";
$oneWeek = (7 * 24 * 60 * 60);
$curWeek = time();
while ($curWeek > strtotime($START_DATE))
{
  $startDate = strftime ("%Y-%m-%d %H:%M:%S", $curWeek-$oneWeek);
  $endDate = strftime ("%Y-%m-%d %H:%M:%S", $curWeek);

  // access database
  $sqlstr = "select crcip, version, count(*) from logs where date > \"$startDate\" and date <= \"$endDate\" and crcip <> $MY_IP group by crcip;";
  $result = mysql_query($sqlstr, $db) or die(mysql_error());

  $myrow = mysql_fetch_array($result);
  if ($myrow)
  {
	echo "Results of week $startDate to $endDate:\n";

	echo "<table bordercolor=#000000 frame=box rules=all>\n";
	echo "<tr><td bgcolor=#00c4c4 align=\"center\"><b>CRC(IP)</b></td><td bgcolor=#00c4c4 align=\"center\"><b>Version</b></td><td bgcolor=#00c4c4 align=\"center\"<b>Count</b></td></tr>\n";
	do
	{
		// display uut infos
		printf("<tr><td align=\"center\">%s</td><td align=\"center\">%s</td><td align=\"center\">%s</td>\n", $myrow[0], $myrow[1], $myrow[2]);
	} while ($myrow = mysql_fetch_array($result));
	echo "</table><p>\n";
  }

  $curWeek -= $oneWeek;
}

echo "<h1>Total access</h1>\n";
$sqlstr = "select file, count(*) from logs where crcip <> $MY_IP group by file;";
$result = mysql_query($sqlstr, $db) or die(mysql_error());
$myrow = mysql_fetch_array($result);
if ($myrow)
{
	echo "<table bordercolor=#000000 frame=box rules=all>\n";
	echo "<tr><td bgcolor=#00c4c4 align=\"center\"><b>File</b></td><td bgcolor=#00c4c4 align=\"center\"<b>Count</b></td></tr>\n";
	do
	{
		// display infos
		printf("<tr><td align=\"center\">%s</td><td align=\"center\">%s</td>\n", $myrow[0], $myrow[1]);
	} while ($myrow = mysql_fetch_array($result));
	echo "</table><p>\n";
}

$sqlstr = "select count(distinct(crcip)) from logs where crcip <> $MY_IP;";
$result = mysql_query($sqlstr, $db) or die(mysql_error());
$myrow = mysql_fetch_array($result);
if ($myrow)
{
	// display infos
	printf("Access from %s distinct IP addresses.<p>\n", $myrow[0]);
}
?>

</body>
</html>
