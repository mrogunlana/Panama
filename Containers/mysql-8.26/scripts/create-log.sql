use logdb; 

drop table if exists `Log`;

create table if not exists `Log` (
	Id int auto_increment,
	CorrelationId char(36) null,
    MachineName varchar(200) not null,
	Logged datetime not null,
    Level varchar(50) null,
	Message text not null,
	Logger varchar(250) null,
	Callsite varchar(250) null,
	Exception text null,
	PRIMARY KEY (Id)
) engine=innodb;