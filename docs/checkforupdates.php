<?php
// this parses a version information file and outputs it as a number

// constants
$VERSION_FILE = "lastversion.txt";
$VERSION_SEPARATOR = '.';

// read version file
$fh = fopen($VERSION_FILE, 'r');
$version_string = fgets($fh);

// get version information
$ver_major = 0;
$ver_minor = 0;
if (is_int(strpos($version_string, $VERSION_SEPARATOR)))
{
        $versions = explode($VERSION_SEPARATOR, $version_string);
        $ver_major = trim($versions[0]);
        $ver_minor = trim($versions[1]);
}

// output version string
echo ($ver_major*100 + $ver_minor);

?>
<?php
// this logs the access to the database

// constants
$DB_HOST = "mysql4-b";
$DB_USER = "b206266rw";
$DB_PWD = "logwriter";
$DB_NAME = "b206266_releaseinfo";
$DB_TABLE = "logs";

// connect to database
$db = mysql_connect($DB_HOST, $DB_USER, $DB_PWD) or die("database error: " . mysql_error());
mysql_select_db($DB_NAME, $db) or die(mysql_error());

// build sql string
$file = $_SERVER['PHP_SELF'];
$date = strftime ("%Y-%m-%d %H:%M:%S" ,time());	// format: 2003-09-26 14:42:40
// do not log the IP directly because of privacy reasons
// only log some derived hash code
$crcip = crc32($_SERVER['REMOTE_ADDR']); 
if (isset($_GET['curVersion']))
   $version = $_GET['curVersion'];
else
   $version = 0;
if (!is_numeric($version))
   $version = 0;

$sqlstr = "INSERT INTO $DB_TABLE VALUES ('$file', '$date', $crcip, $version)";

// access database
$result = mysql_query($sqlstr, $db) or die(mysql_error());
?>
