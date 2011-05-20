CREATE TABLE fields(
ID INT NOT NULL AUTO_INCREMENT, 
PRIMARY KEY(ID),
Name VARCHAR(250),
Scale REAL,
StationID INT,
StationTypeID INT,
ChamberTypeID INT,
Units VARCHAR(150),
AQT_Name VARCHAR(50),
TypeID INT,
Comments VARCHAR(350)
);