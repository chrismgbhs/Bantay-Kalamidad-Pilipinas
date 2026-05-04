CREATE OR ALTER PROCEDURE usp_AddPledge
    @donorID VARCHAR(10),
    @datePledge DATE
AS
BEGIN

    INSERT INTO Pledge (Pledge_ID, Date_Pledge, Pledge_Status)
    VALUES ('P' + RIGHT('000' + CAST((SELECT COUNT(*) FROM Pledge) + 1 AS VARCHAR), 4), @datePledge, 'Pending');
END;

==========================================================

CREATE OR ALTER PROCEDURE usp_AddPledgeItem
     @itemName VARCHAR(255),
     @quantity INT,
     @expectedDeliveryDate DATE
AS
BEGIN

    INSERT INTO [Pledge Item] (PledgeItem_ID, Pledge_ID, Item_Name, Quantity, ExpectedDelivery_Date)
    VALUES ('PT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Pledge Item]) + 1 AS VARCHAR), 4), ('P' + RIGHT('000' + CAST((SELECT COUNT(*) FROM Pledge) AS VARCHAR), 4), @itemName, @quantity, @expectedDeliveryDate);
END;

=================

CREATE OR ALTER PROCEDURE usp_AddDonation
     @donorID VARCHAR(10),
     @eventID VARCHAR(10),
     @dateReceived DATE
AS
BEGIN

    INSERT INTO [Donation] (Donation_ID, Donor_ID, Event_ID, Date_Received)
    VALUES ('DN' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donation]) + 1 AS VARCHAR), 4), @donorID, @eventID, @dateReceived);
END;

===================

CREATE OR ALTER PROCEDURE usp_AddDonatedItem
     @itemName VARCHAR(255),
     @quantity INT,
     @unit VARCHAR(255),
     @category VARCHAR(255),
     @expDate DATE,
     @location VARCHAR(255)
AS
BEGIN

    INSERT INTO [Donated Items] (DonatedItem_ID, Donation_ID, Item_Name, Quantity_Received)
    VALUES ('DT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donated Items]) + 1 AS VARCHAR), 4), 'DN' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donation]) AS VARCHAR), 4), @itemName, @quantity);

    INSERT INTO [Item] (Item_ID, Item_Name, Unit, Category)
    VALUES ('IT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Item]) + 1 AS VARCHAR), 4), @itemName, @unit, @category);

    INSERT INTO [Inventory] (Inventory_ID, Item_ID, DonatedItem_ID, Quantity_Available, Expiration_Date, Storage_Location)
    VALUES ('IN' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Inventory]) + 1 AS VARCHAR), 4), 'IT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Item]) AS VARCHAR), 4), 'DT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donated Items]) AS VARCHAR), 4), @quantity, @expDate, @location);
END;

========

CREATE OR ALTER PROCEDURE usp_AddBenificiary
     @name VARCHAR(255),
     @category VARCHAR(255),
     @centerID VARCHAR(10)
AS
BEGIN
    INSERT INTO [Benificiary] (Benificiary_ID, Name, Category, Center_ID)
    VALUES ('B' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Benificiary]) + 1 AS VARCHAR), 4), @name, @category, @centerID);
END;

=====

CREATE OR ALTER PROCEDURE usp_AddCenter
    @centerName VARCHAR(255),
    @locationID VARCHAR(10),
    @capacity INT
AS
BEGIN
    INSERT INTO [Evacuation Center] (Center_ID, Center_Name, Location_ID, Capacity)
    VALUES ('EC' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Evacuation Center]) + 1 AS VARCHAR), 4), @centerName, @locationID, @capacity);
END;

=======

CREATE OR ALTER PROCEDURE usp_AddLocation
    @barangay VARCHAR(255),
    @city VARCHAR(255),
    @province VARCHAR(255)
AS
BEGIN
    INSERT INTO [Location] (Location_ID, Barangay, City, Province)
    VALUES ('L' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Location]) + 1 AS VARCHAR), 4), @barangay, @city, @province);
END;

==========================

CREATE OR ALTER PROCEDURE usp_AddDistribution
    @beneficiaryID VARCHAR(10),
    @eventID VARCHAR(10),
    @centerID VARCHAR(10),
    @dateDistributed DATE
AS 
BEGIN
    INSERT INTO [Distribution] (Distribution_ID, Beneficiary_ID, Event_ID, Center_ID, Date_Distributed)
    VALUES ('DS' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Distribution]) + 1 AS VARCHAR), 4),@beneficiaryID, @eventID, @centerID, @dateDistributed);
END;


==========

CREATE OR ALTER PROCEDURE usp_AddDeliverySchedule
    @distributionID VARCHAR(10),
    @deliveryDate DATE,
    @deliveryStatus VARCHAR(255)
AS 
BEGIN
    INSERT INTO [Delivery Schedule] (Delivery_ID, Distribution_ID, Delivery_Date, Delivery_Status)
    VALUES ('DL' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Delivery Schedule]) + 1 AS VARCHAR), 4), @distributionID, @deliveryDate, @deliveryStatus);
END;

==========

CREATE OR ALTER PROCEDURE usp_AddRescueOperation
    @eventID VARCHAR(10),
    @locationID VARCHAR(10),
    @dateStarted DATE,
    @rescueStatus VARCHAR(255)
AS
BEGIN
    INSERT INTO [Rescue Operation] (Operation_ID, Event_ID, Location_ID, Date_Started, Rescue_Status)
    VALUES ('RO' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Rescue Operation]) + 1 AS VARCHAR), 4), @eventID, @locationID, @dateStarted, @rescueStatus);
END;

===========

CREATE OR ALTER PROCEDURE uspAddVolunteer
    @volunteerName VARCHAR(255),
    @organization VARCHAR(255),
    @contactNumber VARCHAR(50)
AS 
BEGIN
    INSERT INTO [Volunteer] (Volunteer_ID, Volunteer_Name, Organization, Contact_Number)
    VALUES ('V' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Rescue Operation]) + 1 AS VARCHAR), 4), @volunteerName, @organization, @contactNumber);
END;