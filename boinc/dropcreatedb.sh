#test
# name of the MySQL database
dbname=boinctest
# name of the MySQL user with access to above database
dbuser=boincadm
# password for write access to your project database
dbpasswd=boinc

   # piping commands to mysql shell
EOMYSQL="DROP DATABASE IF EXISTS $dbname; \
CREATE USER '$dbuser'@'localhost' IDENTIFIED BY '$dbpasswd';\
GRANT ALL PRIVILEGES ON $dbname.* TO '$dbuser'@'localhost';"
echo $EOMYSQL 
