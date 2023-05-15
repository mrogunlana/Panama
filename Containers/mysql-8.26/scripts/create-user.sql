use `panama-core`;

create table if not exists `User` (
    _ID int auto_increment,
    ID char(36) not null,
    FirstName varchar(25) null,
    LastName varchar(25) null,
    Email varchar(100) null,
    `Password` varchar(100) null,
    Created datetime not null,
    PRIMARY KEY (_ID)
) engine=innodb;