drop table if exists dbo.Documents
create table dbo.Documents
(
	id int not null identity primary key,
	metadata json,
	content nvarchar(max)
)
go

select * from dbo.Documents
