
create database ExpenseTrackerDB;
go



use ExpenseTrackerDB;
go

create table janu(
    id int identity(1,1) primary key,
    Name varchar(100),
    Email varchar(100),
    Password varchar(100),
    
);

create table jan(
    Id int identity(1,1) primary key,
    Name nvarchar(100),
    Email nvarchar(100),
    Password nvarchar(100)
);

CREATE TABLE Income (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    Amount DECIMAL(18,2),
    Date DATETIME,
    FOREIGN KEY (UserId) REFERENCES jan(Id)
);

CREATE TABLE Expenses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    Amount DECIMAL(18,2),
    Date DATETIME,
    FOREIGN KEY (UserId) REFERENCES jan(Id)
);

Alter table Expenses
add Category Varchar(100);

use ExpenseTrackerDB;
select * from jan;

DELETE FROM Expenses;
DELETE FROM Income;
DELETE FROM jan;


truncate table jan;
