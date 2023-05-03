# create databases
CREATE DATABASE IF NOT EXISTS `devdb`;
CREATE DATABASE IF NOT EXISTS `logdb`;

# create root user and grant rights
CREATE USER 'adminuser'@'%' IDENTIFIED BY 'Tx@XD&Zvlt=SDVo';
GRANT ALL ON devdb.* TO 'adminuser'@'%';
GRANT ALL ON logdb.* TO 'adminuser'@'%';
GRANT SUPER ON *.* TO 'adminuser'@'%';
GRANT PROCESS ON *.* TO 'adminuser'@'%';
GRANT REPLICATION CLIENT ON *.* TO 'adminuser'@'%';
GRANT REPLICATION SLAVE ON *.* TO 'adminuser'@'%';

use devdb; 