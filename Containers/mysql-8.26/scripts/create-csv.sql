use `panama-core`;

create table if not exists `Csv` (
    `_ID` bigint auto_increment,
    `ID` int not null,
    `First` varchar(50) null,
    `Last` varchar(50) null,
	`Age` int null,
    `Street` varchar(150) null,
	`City` varchar(100) null,
	`State` varchar(100) null,
	`Zip` varchar(15) null,
	`Dollar` varchar(10) null,
	`Pick` varchar(25) null,
    `Date` datetime null,
    PRIMARY KEY (_ID),
	UNIQUE KEY `_ID` (`_ID`)
) engine=innodb;