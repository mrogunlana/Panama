use `devdb`; 

create table if not exists `Forecast` (
    _ID int auto_increment,
    ID char(36) not null,
    `Date` datetime null,
    TemperatureC int null,
    Summary varchar(200) null,
    Created datetime not null,
    PRIMARY KEY (_ID)
) engine=innodb;