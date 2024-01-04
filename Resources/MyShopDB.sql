-- Create database
use master
go
if DB_ID('MyShopDB') is not null
	drop database [MyShopDB]
go
create database [MyShopDB]
go
use [MyShopDB]
go

-- Create tables
create table ACCOUNT(
	id int identity(1,1),
	name nvarchar(100) null,
	phone varchar(20) null,
	address nvarchar(200) null,
	username nvarchar(100) null,
	password varchar(max) null,
	entropy varchar(max) null,

	primary key(id)
)
go

create table CUSTOMER(
	id int identity(1,1),
	name nvarchar(100) null,
	phone varchar(100) null,
	address nvarchar(200) null,

	primary key(id)
)
go

create table BILL(
	id int identity(1,1),
	customer_id int null,
	total_price int null,
	transaction_date date null,

	primary key (id)
)
go


create table DETAILED_BILL(
	bill_id int not null,
	book_id int not null,
	number int null,
	price int null,
	promotion_id int null,
	primary key (bill_id, book_id)
)
go

create table BOOK(
	id int identity(1,1),
	title nvarchar(100) null,
	author nvarchar(100) null,
	image nvarchar(300) null,
	genre_id int null,
	description nvarchar(max) null,
	published_date date null,
	price int null,
	quantity int null,

	primary key (id)
)
go

create table PROMOTION (
	id int identity(1,1),
	name nvarchar(200),
	discount int null, -- unit: %
	start_date date null,
	end_date date null,

	primary key (id)
)
go

create table BOOK_PROMOTION(
	promotion_id int not null,
	book_id int not null,

	primary key(promotion_id, book_id)
)
go

create table GENRE(
	id int identity(1,1),
	name nvarchar(100) null,

	primary key (id)
)
go

-- Add foreign keys
alter table BILL add foreign key (customer_id) references CUSTOMER
alter table DETAILED_BILL add foreign key (bill_id) references BILL
alter table DETAILED_BILL add foreign key (book_id) references BOOK
alter table DETAILED_BILL add foreign key (promotion_id) references PROMOTION
alter table BOOK add foreign key (genre_id) references GENRE
alter table BOOK_PROMOTION add foreign key (book_id) references BOOK
alter table	BOOK_PROMOTION add foreign key (promotion_id) references PROMOTION
go

-- ********* Start: Store procedures *********

--lay danh sach cac tuan dang co
create procedure GetListOfWeeks
as
begin
	declare @currentWeekOffSet INT = 0;
	declare @startDate DATE = (select min(bill.transaction_date) from BILL as bill)
	declare @maxWeek INT = (select weeks.*
	from (select CEILING(DATEDIFF(DAY, min(bill.transaction_date), GETDATE())/7.0) as 'tong_tuan'
	from BILL as bill) as weeks)

	declare @weekTable table(weekID INT, startDateOfWeek DATE)

	while @currentWeekOffSet <= @maxWeek
	begin
		insert into @weekTable(weekID, startDateOfWeek) values(@currentWeekOffSet, DATEADD(DAY, @currentWeekOffSet*7, @startDate))
		set @currentWeekOffSet = @currentWeekOffSet + 1;
	end

	select wt.weekID as 'week', wt.startDateOfWeek 'start_date'
	from @weekTable as wt
	return
end
go

--lay thong ke theo ngay
create procedure GetDailyRevenue @start_date date, @end_date date
as
begin

	declare @date date
	declare @result table(transaction_date DATE, total_price INT)
	declare @temp_value INT

	set @date = @start_date
	--insert into result table the list of tupples(@date-price) that @start_date <= @date <= @end_date
	while @date <= @end_date
	begin
		set @temp_value = (select SUM(bill.total_price) from BILL as bill where bill.transaction_date = @date)
		if(@temp_value is null)
		begin
			set @temp_value= 0
		end

		insert into @result(transaction_date, total_price) values(@date, @temp_value)
		--increase @date
		set @date = DATEADD(DAY, 1, @date)
	end

	select result.transaction_date as 'date', result.total_price as 'revenue'  from @result as result
	return
end
go

--lay thong ke theo thang
create procedure GetMonthlyRevenue @start_date_start_month DATE, @start_date_end_month DATE
as
begin
	declare @total_revenue_month INT
	declare @exec_date DATE
	declare @flag_date DATE
	declare @result table(start_date_of_month DATE, total_revenue INT)

	--initialize
	set @exec_date = @start_date_start_month
	set @flag_date = @exec_date

	while @exec_date <= @start_date_end_month
	begin
		set @flag_date = @exec_date
		set @exec_date = DATEADD(MONTH, 1, @flag_date)
		set @total_revenue_month = (select SUM(bill.total_price) from BILL as bill where bill.transaction_date >= @flag_date and bill.transaction_date <= @exec_date)
		if(@total_revenue_month is null)
		begin
			set @total_revenue_month = 0
		end
		insert into @result(start_date_of_month, total_revenue) values(@flag_date, @total_revenue_month)
	end

	select result.start_date_of_month as 'date', result.total_revenue as 'revenue' from @result as result
	return
end
go

--thuc hien lay thong ke theo tuan
create procedure GetWeeklyRevenue @start_date_start_week DATE, @start_date_end_week DATE
as
begin
	declare @total_revenue_of_a_week INT
	--declare @temp_value INT
	declare @exec_date DATE
	declare @flag_date DATE
	declare @result table(start_date_of_week DATE, total_revenue INT) 
	
	set @total_revenue_of_a_week = 0
	set @exec_date = @start_date_start_week
	set @flag_date = @exec_date
	
	while @exec_date <= @start_date_end_week
	begin
		set @flag_date = @exec_date
		set @exec_date = DATEADD(DAY, 7, @flag_date)
		set @total_revenue_of_a_week = (select SUM(bill.total_price) from BILL as bill where bill.transaction_date >= @flag_date and bill.transaction_date <= @exec_date)
		if(@total_revenue_of_a_week is null)
		begin
			set @total_revenue_of_a_week = 0
		end
		insert into @result(start_date_of_week, total_revenue) values(@flag_date, @total_revenue_of_a_week)
	end

	select result.start_date_of_week as 'date', result.total_revenue as 'revenue' from @result as result
	return 

end
go

--thuc hien lay thong ke theo thang
create procedure GetYearlyRevenue @start_date_start_year DATE, @start_date_end_year DATE
as
begin
	declare @exec_date DATE
	declare @flag_date DATE
	declare @total_revenue_a_year INT
	declare @result table(start_date_of_year DATE, total_revenue INT)

	set @exec_date = @start_date_start_year
	set @flag_date = @exec_date
	set @total_revenue_a_year = 0

	while @exec_date <= @start_date_end_year
	begin
		set @flag_date = @exec_date
		set @exec_date = DATEADD(YEAR, 1, @exec_date)
		set @total_revenue_a_year = (select SUM(bill.total_price) from BILL as bill where bill.transaction_date >= @flag_date and bill.transaction_date <= @exec_date)
		if(@total_revenue_a_year is null)
		begin
			set @total_revenue_a_year = 0
		end
		insert into @result(start_date_of_year, total_revenue) values(@flag_date, @total_revenue_a_year)
	end

	select result.start_date_of_year as 'date', result.total_revenue as 'revenue' from @result as result
	return
end
go
-- ********* End: Store procedures *********

-- Insert data

USE [MyShopDB]
GO
SET IDENTITY_INSERT [dbo].[CUSTOMER] ON 

INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (1, N'Nguyễn Võ Minh Trí', N'+085746332', N'102, phường 21, quận Bình Thạnh, Hồ Chí Minh')
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (2, N'Huỳnh Minh Chiến', N'+0123446723', N'số 11, phường 4, quận Tân Phú, Hồ Chí Minh')
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (3, N'Nguyễn Tuyên Thế', N'+0896662863', N'số 23, đường Phan Đăng Lưu, quận Bình Thạnh, Hồ Chí Minh')
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (4, N'Lê Hữu An', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (5, N'Dương Thanh Thanh', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (6, N'Trương Tấn Kiệt', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (7, N'Lương Liêm Khiết', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (8, N'Lê Minh Quyên', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (9, N'Bành Dạ Quang', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (10, N'Hào Thi Khải', N'0336155478', N'')
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (11, N'Nguyễn Văn A', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (12, N'Phạm Thị B', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (13, N'Trần Văn C', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (14, N'Lê Thị D', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (15, N'Hồ Văn E', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (16, N'Nguyễn Thị F', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (17, N'Phan Văn G', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (18, N'Nguyễn Thị H', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (19, N'Trần Văn I', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (20, N'Lê Thị K', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (21, N'Phạm Văn L', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (22, N'Nguyễn Thị M', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (23, N'Trần Văn N', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (24, N'Lê Thị O', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (25, N'Hồ Văn P', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (26, N'Nguyễn Thị Q', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (27, N'Phan Văn R', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (28, N'Nguyễn Thị S', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (29, N'Trần Văn T', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (30, N'Lê Thị U', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (31, N'Phạm Văn V', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (32, N'Nguyễn Thị X', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (33, N'Trần Văn Y', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (34, N'Lê Thị Z', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (35, N'Hồ Văn A1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (36, N'Nguyễn Thị B1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (37, N'Phan Văn C1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (38, N'Nguyễn Thị D1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (39, N'Trần Văn E1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (40, N'Lê Thị F1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (41, N'Phạm Văn G1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (42, N'Nguyễn Thị H1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (43, N'Trần Văn I1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (44, N'Lê Thị K1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (45, N'Hồ Văn L1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (46, N'Nguyễn Thị M1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (47, N'Phan Văn N1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (48, N'Nguyễn Thị O1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (49, N'Trần Văn P1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (50, N'Lê Thị Q1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (51, N'Phạm Văn R1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (52, N'Nguyễn Thị S1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (53, N'Trần Văn T1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (54, N'Lê Thị U1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (55, N'Hồ Văn V1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (56, N'Nguyễn Thị X1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (57, N'Phan Văn Y1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (58, N'Nguyễn Thị Z1', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (59, N'Trần Văn A2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (60, N'Lê Thị B2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (61, N'Phạm Văn C2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (62, N'Nguyễn Thị D2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (63, N'Trần Văn E2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (64, N'Lê Thị F2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (65, N'Hồ Văn G2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (66, N'Nguyễn Thị H2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (67, N'Phan Văn I2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (68, N'Nguyễn Thị K2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (69, N'Trần Văn L2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (70, N'Trần Văn M2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (71, N'Phạm Văn N2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (72, N'Nguyễn Thị O2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (73, N'Trần Văn P2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (74, N'Lê Thị Q2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (75, N'Hồ Văn R2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (76, N'Nguyễn Thị S2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (77, N'Phan Văn T2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (78, N'Nguyễn Thị U2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (79, N'Trần Văn V2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (80, N'Lê Thị X2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (81, N'Phạm Văn Y2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (82, N'Nguyễn Thị Z2', NULL, NULL)
INSERT [dbo].[CUSTOMER] ([id], [name], [phone], [address]) VALUES (83, N'Trần Văn A3', NULL, NULL)
SET IDENTITY_INSERT [dbo].[CUSTOMER] OFF
GO
SET IDENTITY_INSERT [dbo].[ACCOUNT] ON 

INSERT [dbo].[ACCOUNT] ([id], [name], [phone], [address], [username], [password], [entropy]) VALUES (1, N'Nguyễn Tuấn Đạt', N'0336187445', N'43 Amajhao, thị trấn quảng phú, huyện CuwMgar, tỉnh Đăk lăk', N'dat2k3',N'AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAANMU2HfXNlEqZea7Qt08s+AAAAAACAAAAAAAQZgAAAAEAACAAAAA3lvbTveKo6AbSi7QeYeEGIvQCfVNB0LHIVAPtSR8JwwAAAAAOgAAAAAIAACAAAACQx/QPbOUMsebJe71FjDl1vfgwrQ8B2OLoA8KPTkhSxxAAAABjZFks+CRxJbl/50ezKxaBQAAAABct6k1j9mKTOo0kYpMCPfsWL72zOl/V/ZNLIYGvrlWulE1+5QyiEcc+nYhxCYE+cozkoR0mkU/kAzdjz2owtLI=', N'rDnTNlV9mgRHR4qcO+nqffBD+c0=')
INSERT [dbo].[ACCOUNT] ([id], [name], [phone], [address], [username], [password], [entropy]) VALUES (2, N'Nguyễn Đình Ánh', NULL, NULL, N'dinhanh', N'AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAANMU2HfXNlEqZea7Qt08s+AAAAAACAAAAAAAQZgAAAAEAACAAAAA3lvbTveKo6AbSi7QeYeEGIvQCfVNB0LHIVAPtSR8JwwAAAAAOgAAAAAIAACAAAACQx/QPbOUMsebJe71FjDl1vfgwrQ8B2OLoA8KPTkhSxxAAAABjZFks+CRxJbl/50ezKxaBQAAAABct6k1j9mKTOo0kYpMCPfsWL72zOl/V/ZNLIYGvrlWulE1+5QyiEcc+nYhxCYE+cozkoR0mkU/kAzdjz2owtLI=', N'rDnTNlV9mgRHR4qcO+nqffBD+c0=')
INSERT [dbo].[ACCOUNT] ([id], [name], [phone], [address], [username], [password], [entropy]) VALUES (3, N'Triệu Hoàng Thiên Ân', NULL, NULL, N'thienan', N'AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAANMU2HfXNlEqZea7Qt08s+AAAAAACAAAAAAAQZgAAAAEAACAAAAA3lvbTveKo6AbSi7QeYeEGIvQCfVNB0LHIVAPtSR8JwwAAAAAOgAAAAAIAACAAAACQx/QPbOUMsebJe71FjDl1vfgwrQ8B2OLoA8KPTkhSxxAAAABjZFks+CRxJbl/50ezKxaBQAAAABct6k1j9mKTOo0kYpMCPfsWL72zOl/V/ZNLIYGvrlWulE1+5QyiEcc+nYhxCYE+cozkoR0mkU/kAzdjz2owtLI=', N'rDnTNlV9mgRHR4qcO+nqffBD+c0=')
INSERT [dbo].[ACCOUNT] ([id], [name], [phone], [address], [username], [password], [entropy]) VALUES (4, N'Lê Minh Huy', NULL, NULL, N'minhhuy', N'AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAANMU2HfXNlEqZea7Qt08s+AAAAAACAAAAAAAQZgAAAAEAACAAAAA3lvbTveKo6AbSi7QeYeEGIvQCfVNB0LHIVAPtSR8JwwAAAAAOgAAAAAIAACAAAACQx/QPbOUMsebJe71FjDl1vfgwrQ8B2OLoA8KPTkhSxxAAAABjZFks+CRxJbl/50ezKxaBQAAAABct6k1j9mKTOo0kYpMCPfsWL72zOl/V/ZNLIYGvrlWulE1+5QyiEcc+nYhxCYE+cozkoR0mkU/kAzdjz2owtLI=', N'rDnTNlV9mgRHR4qcO+nqffBD+c0=')

SET IDENTITY_INSERT [dbo].[ACCOUNT] OFF
GO
SET IDENTITY_INSERT [dbo].[BILL] ON 
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (1, 1, 613984, CAST(N'2022-01-23' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (2, 2, 727542, CAST(N'2022-01-26' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (3, 3, 1023383, CAST(N'2022-02-18' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (4, 4, 1182682, CAST(N'2022-03-22' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (5, 5, 545713, CAST(N'2022-04-03' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (6, 6, 1546339, CAST(N'2022-04-23' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (7, 7, 295614, CAST(N'2022-05-19' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (8, 8, 886841, CAST(N'2022-05-20' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (9, 9, 1887693, CAST(N'2022-05-25' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (10, 10, 4343857, CAST(N'2022-06-05' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (11, 11, 432156, CAST(N'2022-07-22' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (12, 12, 409171, CAST(N'2022-08-01' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (13, 13, 5752287, CAST(N'2022-08-05' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (14, 14, 250100, CAST(N'2022-10-10' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (15, 15, 591227, CAST(N'2022-11-05' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (16, 16, 181829, CAST(N'2022-11-09' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (17, 17, 1000626, CAST(N'2022-12-16' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (18, 18, 6252486, CAST(N'2023-01-17' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (19, 19, 636741, CAST(N'2023-01-18' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (20, 20, 1364510, CAST(N'2023-01-23' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (21, 21, 2273880, CAST(N'2023-02-12' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (22, 22, 432156, CAST(N'2023-02-16' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (23, 23, 4774419, CAST(N'2023-02-18' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (24, 24, 250100, CAST(N'2023-02-20' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (25, 25, 227343, CAST(N'2023-02-28' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (26, 26, 2046993, CAST(N'2023-03-07' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (27, 27, 6137563, CAST(N'2023-03-15' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (28, 28, 636741, CAST(N'2023-03-17' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (29, 29, 1296467, CAST(N'2022-03-20' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (30, 30, 4546849, CAST(N'2023-03-14' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (31, 31, 818000, CAST(N'2023-04-14' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (32, 32, 773000, CAST(N'2023-04-15' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (33, 33, 6360000, CAST(N'2023-04-20' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (34, 34, 1570000, CAST(N'2023-04-21' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (35, 35, 1092000, CAST(N'2023-04-22' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (36, 36, 4880000, CAST(N'2023-04-23' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (37, 37, 546000, CAST(N'2023-04-24' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (38, 38, 2024000, CAST(N'2023-04-24' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (39, 39, 318000, CAST(N'2023-04-26' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (40, 40, 977000, CAST(N'2023-04-27' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (41, 41, 2642000, CAST(N'2023-04-28' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (42, 42, 6000000, CAST(N'2023-04-29' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (43, 43, 205000, CAST(N'2023-05-01' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (44, 44, 841000, CAST(N'2023-05-02' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (45, 45, 2526000, CAST(N'2023-05-14' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (46, 46, 818000, CAST(N'2023-05-16' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (47, 47, 3092000, CAST(N'2023-05-20' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (48, 48, 546000, CAST(N'2023-06-03' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (49, 49, 1182000, CAST(N'2023-06-12' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (50, 50, 546000, CAST(N'2023-06-30' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (51, 51, 1636000, CAST(N'2023-07-02' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (52, 52, 227000, CAST(N'2023-07-04' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (53, 53, 205000, CAST(N'2023-07-10' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (54, 54, 2730000, CAST(N'2023-08-06' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (55, 55, 660000, CAST(N'2023-09-12' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (56, 56, 5221000, CAST(N'2023-09-16' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (57, 57, 500000, CAST(N'2023-09-17' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (58, 58, 205000, CAST(N'2023-09-25' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (59, 59, 888000, CAST(N'2023-10-03' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (60, 60, 1728000, CAST(N'2023-10-22' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (61, 61, 1681000, CAST(N'2023-10-23' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (62, 62, 205000, CAST(N'2023-10-24' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (63, 63, 5221000, CAST(N'2023-10-25' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (64, 64, 727000, CAST(N'2023-10-25' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (65, 65, 546000, CAST(N'2023-11-01' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (66, 66, 296000, CAST(N'2023-11-07' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (67, 67, 4770000, CAST(N'2023-11-10' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (68, 68, 636000, CAST(N'2023-11-13' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (69, 69, 273000, CAST(N'2023-11-17' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (70, 70, 1435000, CAST(N'2023-11-19' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (71, 71, 296000, CAST(N'2023-11-22' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (72, 72, 1935000, CAST(N'2023-11-24' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (73, 73, 591000, CAST(N'2023-11-29' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (74, 74, 1706000, CAST(N'2023-12-01' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (75, 75, 273000, CAST(N'2023-12-03' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (76, 76, 8514000, CAST(N'2023-12-07' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (77, 77, 592000, CAST(N'2023-12-10' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (78, 78, 296000, CAST(N'2023-12-15' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (79, 79, 500000, CAST(N'2023-12-24' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (80, 80, 773000, CAST(N'2023-12-28' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (81, 81, 592000, CAST(N'2024-01-01' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (82, 82, 205000, CAST(N'2024-01-02' AS Date))
INSERT [dbo].[BILL] ([id], [customer_id], [total_price], [transaction_date]) VALUES (83, 83, 6000000, CAST(N'2024-01-03' AS Date))
SET IDENTITY_INSERT [dbo].[BILL] OFF
GO
SET IDENTITY_INSERT [dbo].[GENRE] ON 

INSERT [dbo].[GENRE] ([id], [name]) VALUES (1, N'Fantasy')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (2, N'Science Fiction')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (3, N'Dystopian')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (4, N'Adventure')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (5, N'Romance')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (6, N'Detective & Mystery')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (7, N'Horror')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (8, N'Thriller')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (9, N'LGBTQ+')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (10, N'Historical Fiction')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (11, N'Young Adult (YA)')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (12, N'Children’s Fiction')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (13, N'Memoir & Autobiography')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (14, N'Biography')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (15, N'Cooking')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (16, N'Art & Photography')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (17, N'Self-Help/Personal Development')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (18, N'Motivational/Inspirational')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (19, N'Health & Fitness')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (20, N'History')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (21, N'Crafts, Hobbies & Home')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (22, N'Families & Relationships')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (23, N'Humor & Entertainment')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (24, N'Business & Money')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (25, N'Law & Criminology')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (26, N'Politics & Social Sciences')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (27, N'Religion & Spirituality')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (28, N'Education & Teaching')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (29, N'Travel')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (30, N'True Crime')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (31, N'Bildungsroman (coming-of-age novel)')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (32, N'Gothic Fiction')
INSERT [dbo].[GENRE] ([id], [name]) VALUES (33, N'Literary Fiction')
SET IDENTITY_INSERT [dbo].[GENRE] OFF
GO
SET IDENTITY_INSERT [dbo].[BOOK] ON 

INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (1, N'The Great Gatsby', N'F. Scott Fitzgerald', N'Assets\The_Great_Gatsby.jpg', 10, NULL, CAST(N'1925-04-10' AS Date), 364000, 50)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (2, N'To Kill a Mockingbird', N'Harper Lee', N'Assets\To_Kill_a_Mockingbird.jpg', 10, NULL, CAST(N'1960-07-11' AS Date), 296000, 75)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (3, N'1984', N'George Orwell', N'Assets\1984.jpg', 3, NULL, CAST(N'1949-06-08' AS Date), 250000, 100)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (4, N'Pride and Prejudice', N'Jane Austen', N'Assets\Pride_and_Prejudice.jpg', 10, NULL, CAST(N'1813-01-28' AS Date), 227000, 120)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (5, N'The Catcher in the Rye', N'J.D. Salinger', N'Assets\The_Catcher_in_the_Rye.jpg', 31, NULL, CAST(N'1951-07-16' AS Date), 273000, 90)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (6, N'To the Lighthouse', N'Virginia Woolf', N'Assets\To_the_Lighthouse.jpg', 10, NULL, CAST(N'1927-05-05' AS Date), 318000, 60)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (7, N'The Hobbit', N'J.R.R. Tolkien', N'Assets\The_Hobbit.jpg', 1, NULL, CAST(N'1937-09-21' AS Date), 341000, 80)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (8, N'The Picture of Dorian Gray', N'Oscar Wilde', N'Assets\The_Picture_of_Dorian_Gray.jpg', 32, NULL, CAST(N'1890-07-01' AS Date), 387000, 40)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (9, N'Jane Eyre', N'Charlotte Bronte', N'Assets\Jane_Eyre.jpg', 10, NULL, CAST(N'1847-10-16' AS Date), 250000, 95)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (10, N'Wuthering Heights', N'Emily Bronte', N'Assets\Wuthering_Heights.jpg', 32, NULL, CAST(N'1847-12-19' AS Date), 296000, 70)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (11, N'The Lord of the Rings', N'J.R.R. Tolkien', N'Assets\The_Lord_of_the_Rings.jpg', 1, NULL, CAST(N'1954-07-29' AS Date), 432000, 85)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (12, N'One Hundred Years of Solitude', N'Gabriel Garcia Marquez', N'Assets\One_Hundred_Years_of_Solitude.jpg', 10, NULL, CAST(N'1967-06-05' AS Date), 273000, 65)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (13, N'The Sun Also Rises', N'Ernest Hemingway', N'Assets\The_Sun_Also_Rises.jpg', 10, NULL, CAST(N'1926-10-22' AS Date), 318000, 55)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (14, N'The Sound and the Fury', N'William Faulkner', N'Assets\The_Sound_and_the_Fury.jpg', 10, NULL, CAST(N'1929-10-07' AS Date), 341000, 45)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (15, N'Brave New World', N'Aldous Huxley', N'Assets\Brave_New_World.jpg', 3, NULL, CAST(N'1932-06-17' AS Date), 227000, 110)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (16, N'The Grapes of Wrath', N'John Steinbeck', N'Assets\The_Grapes_of_Wrath.jpg', 10, NULL, CAST(N'1939-04-14' AS Date), 296000, 75)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (17, N'Crime and Punishment', N'Fyodor Dostoevsky', N'Assets\Crime_and_Punishment.jpg', 6, NULL, CAST(N'1866-12-01' AS Date), 387000, 43)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (18, N'Moby-Dick', N'Herman Melville', N'Assets\Moby_Dick.jpg', 4, NULL, CAST(N'1851-10-18' AS Date), 409000, 35)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (19, N'The Brothers Karamazov', N'Fyodor Dostoevsky', N'Assets\The_Brothers_Karamazov.jpg', 10, NULL, CAST(N'1880-11-26' AS Date), 364000, 50)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (20, N'Anna Karenina', N'Leo Tolstoy', N'Assets\Anna_Karenina.jpg', 10, NULL, CAST(N'1877-01-01' AS Date), 273000, 90)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (21, N'The Odyssey', N'Homer', N'Assets\The_Odyssey.jpg', 10, NULL, NULL, 205000, 150)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (22, N'The Iliad', N'Homer', N'Assets\The_Iliad.jpg', 10, NULL, NULL, 205000, 140)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (23, N'The Divine Comedy', N'Dante Alighieri', N'Assets\The_Divine_Comedy.jpg', 27, NULL, CAST(N'1320-01-01' AS Date), 387000, 45)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (24, N'The Canterbury Tales', N'Geoffrey Chaucer', N'Assets\The_Canterbury_Tales.jpg', 10, NULL, CAST(N'1400-01-01' AS Date), 318000, 60)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (25, N'The Adventures of Huckleberry Finn', N'Mark Twain', N'Assets\The_Adventures_of_Huckleberry_Finn.jpg', 4, NULL, CAST(N'1884-12-10' AS Date), 227000, 100)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (26, N'The Adventures of Tom Sawyer', N'Mark Twain', N'Assets\The_Adventures_of_Tom_Sawyer.jpg', 4, NULL, CAST(N'1876-12-01' AS Date), 205000, 120)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (27, N'The War of the Worlds', N'H.G. Wells', N'Assets\The_War_of_the_Worlds.jpg', 2, NULL, CAST(N'1898-04-01' AS Date), 250000, 95)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (28, N'Frankenstein', N'Mary Shelley', N'Assets\Frankenstein.jpg', 7, NULL, CAST(N'1818-01-01' AS Date), 273000, 80)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (29, N'Dracula', N'Bram Stoker', N'Assets\Dracula.jpg', 7, NULL, CAST(N'1897-05-26' AS Date), 296000, 70)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (30, N'The Time Machine', N'H.G. Wells', N'Assets\The_Time_Machine.jpg', 2, NULL, CAST(N'1895-01-01' AS Date), 227000, 105)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (31, N'Harry Potter and the Goblet of Fire', N'J.K. Rowling', N'Assets\harry_potter_goblet_of_fire.jpg', 1, NULL, CAST(N'2000-07-08' AS Date), 250000, 50)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (32, N'The Lovely Bones', N'Alice Sebold', N'Assets\the_lovely_bones.jpg', 8, NULL, CAST(N'2002-07-03' AS Date), 296000, 30)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (33, N'The Corrections', N'Jonathan Franzen', N'Assets\the_corrections.jpg', 22, NULL, CAST(N'2001-09-01' AS Date), 227000, 40)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (34, N'The Secret Life of Bees', N'Sue Monk Kidd', N'Assets\secret_life_of_bees.jpg', 10, NULL, CAST(N'2002-11-08' AS Date), 205000, 60)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (35, N'The Da Vinci Code', N'Dan Brown', N'Assets\da_vinci_code.jpg', 8, NULL, CAST(N'2003-03-18' AS Date), 273000, 20)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (36, N'The Hours', N'Michael Cunningham', N'Assets\the_hours.jpg', 33, NULL, CAST(N'2000-11-11' AS Date), 182000, 25)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (37, N'White Teeth', N'Zadie Smith', N'Assets\white_teeth.jpg', 33, NULL, CAST(N'2000-01-27' AS Date), 250000, 35)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (38, N'Life of Pi', N'Yann Martel', N'Assets\life_of_pi.jpg', 4, NULL, CAST(N'2001-09-11' AS Date), 318000, 45)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (39, N'The Road', N'Cormac McCarthy', N'Assets\the_road.jpg', 3, NULL, CAST(N'2006-09-26' AS Date), 205000, 30)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (40, N'The Kite Runner', N'Khaled Hosseini', N'Assets\the_kite_runner.jpg', 10, NULL, CAST(N'2003-05-29' AS Date), 227000, 40)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (41, N'Middlesex', N'Jeffrey Eugenides', N'Assets\middlesex.jpg', 9, NULL, CAST(N'2002-09-04' AS Date), 273000, 50)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (42, N'Atonement', N'Ian McEwan', N'Assets\atonement.jpg', 10, NULL, CAST(N'2001-09-10' AS Date), 296000, 20)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (43, N'The Curious Incident of the Dog in the Night-Time', N'Mark Haddon', N'Assets\curious_incident.jpg', 11, NULL, CAST(N'2003-05-18' AS Date), 227000, 30)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (44, N'The Catcher in the Rye', N'J.D. Salinger', N'Assets\catcher_in_the_rye.jpg', 31, NULL, CAST(N'2001-08-15' AS Date), 182000, 35)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (45, N'The Poisonwood Bible', N'Barbara Kingsolver', N'Assets\poisonwood_bible.jpg', 10, NULL, CAST(N'2004-03-01' AS Date), 250000, 40)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (46, N'Bel Canto', N'Ann Patchett', N'Assets\bel_canto.jpg', 33, NULL, CAST(N'2001-05-22' AS Date), 205000, 25)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (47, N'The Girl with the Dragon Tattoo', N'Stieg Larsson', N'Assets\girl_with_dragon_tattoo.jpg', 30, NULL, CAST(N'2005-08-01' AS Date), 341000, 30)
INSERT [dbo].[BOOK] ([id], [title], [author], [image], [genre_id], [description], [published_date], [price], [quantity]) VALUES (48, N'The Memory Keeper''s Daughter', N'Kim Edwards', N'Assets\memory_keepers_daughter.jpg', 33, NULL, CAST(N'2005-06-14' AS Date), 296000, 20)
SET IDENTITY_INSERT [dbo].[BOOK] OFF
GO
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (1, 1, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (1, 9, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (2, 20, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (2, 33, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (3, 9, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (3, 10, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (3, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (4, 7, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (4, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (4, 18, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (5, 10, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (6, 9, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (6, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (6, 29, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (6, 38, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (7, 48, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (8, 42, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (9, 11, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (9, 18, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (9, 39, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (10, 7, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (10, 11, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (10, 13, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (10, 15, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (10, 17, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (11, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (12, 21, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (13, 27, 23, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (14, 31, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (15, 29, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (15, 35, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (16, 36, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (17, 16, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (17, 18, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (18, 3, 25, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (19, 17, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (19, 31, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (20, 7, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (21, 7, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (21, 27, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (22, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (23, 33, 15, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (23, 35, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (24, 45, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (25, 40, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (26, 18, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (27, 21, 15, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (27, 22, 15, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (28, 6, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (29, 11, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (30, 15, 20, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (31, 18, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (31, 7, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (32, 11, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (32, 38, 20, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (33, 1, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (34, 3, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (35, 48, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (36, 20, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (36, 27, 16, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (37, 29, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (37, 12, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (38, 10, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (38, 11, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (39, 13, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (40, 13, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (41, 14, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (42, 2, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (43, 22, 10, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (44, 27, 24, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (45, 26, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (46, 13, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (47, 21, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (48, 8, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (49, 17, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (50, 20, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (51, 18, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (52, 7, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (53, 11, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (54, 5, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (55, 6, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (56, 11, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (57, 12, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (58, 18, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (59, 25, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (60, 26, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (61, 35, 10, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (62, 34, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (63, 37, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (64, 40, 23, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (65, 28, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (66, 30, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (67, 46, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (68, 48, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (69, 1, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (70, 3, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (71, 9, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (72, 13, 4, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (73, 18, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (74, 22, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (75, 15, 23, NULL)
GO

INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (76, 6, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (76, 18, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (77, 5, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (77, 2, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (77, 38, 15, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (78, 38, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (78, 41, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (78, 46, 7, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (78, 29, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (79, 23, 5, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (79, 1, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (79, 30, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (80, 11, 3, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (80, 39, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (80, 12, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (80, 17, 22, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (81, 10, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (81, 16, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (81, 31, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (82, 29, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (82, 30, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (82, 31, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (83, 32, 2, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (83, 22, 1, NULL)
INSERT [dbo].[DETAILED_BILL] ([bill_id], [book_id], [number], [price]) VALUES (83, 3, 24, NULL)
GO
