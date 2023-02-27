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

DROP TABLE IF EXISTS `Outbox`;

CREATE TABLE IF NOT EXISTS `Outbox` (
  `_Id` bigint NOT NULL AUTO_INCREMENT,
  `Id` varchar(150) DEFAULT NULL,
  `CorrelationId` varchar(150) DEFAULT NULL,
  `Version` varchar(20) DEFAULT NULL,
  `Name` varchar(400) NOT NULL,
  `Group` varchar(200) DEFAULT NULL,
  `Content` longtext,
  `Retries` int(11) DEFAULT NULL,
  `Added` datetime NOT NULL,
  `ExpiresAt` datetime DEFAULT NULL,
  `StatusName` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_ExpiresAt`(`ExpiresAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;