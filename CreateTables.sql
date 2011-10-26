CREATE TABLE `fields` (
  `ID` smallint(6) NOT NULL AUTO_INCREMENT,
  `Name` varchar(250) DEFAULT NULL,
  `Scale` double DEFAULT NULL,
  `StationID` int(11) DEFAULT NULL,
  `StationTypeID` int(11) DEFAULT NULL,
  `ChamberTypeID` int(11) DEFAULT NULL,
  `Units` varchar(150) DEFAULT NULL,
  `AQT_Name` varchar(50) DEFAULT NULL,
  `Type` varchar(10) DEFAULT NULL,
  `Comments` varchar(350) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 |



CREATE TABLE `aqt_fields` (
  `ID` smallint(6) NOT NULL,
  `Name` varchar(250) DEFAULT NULL,
  `Scale` double DEFAULT NULL,
  `StationID` int(11) DEFAULT NULL,
  `StationTypeID` int(11) DEFAULT NULL,
  `ChamberTypeID` int(11) DEFAULT NULL,
  `Units` varchar(150) DEFAULT NULL,
  `AQT_Name` varchar(50) DEFAULT NULL,
  `Type` varchar(10) DEFAULT NULL,
  `Comments` varchar(350) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1


CREATE TABLE `opc_data_mem` (
  `ID` smallint(6) DEFAULT NULL,
  `Time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Value` float DEFAULT NULL,
  KEY `T_id` (`Time`) USING BTREE
) ENGINE=MEMORY DEFAULT CHARSET=latin1

CREATE TABLE `opc_data` (
  `ID` smallint(6) DEFAULT NULL,
  `Time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Value` float DEFAULT NULL,
  KEY `T_id` (`Time`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1