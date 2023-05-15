use `devdb`;

create table if not exists `Setting` (
    `_ID` int auto_increment,
    `ID` char(36) not null,
    `Key` varchar(25) null,
    `Value` varchar(25) null,
    `Created` datetime not null,
    PRIMARY KEY (_ID)
) engine=innodb;