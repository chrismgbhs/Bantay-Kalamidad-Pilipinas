CREATE OR ALTER PROCEDURE usp_AddPledge
    @donorID VARCHAR(10),
    @datePledge DATE
AS
BEGIN

    INSERT INTO Pledge (Pledge_ID, Date_Pledge, Pledge_Status)
    VALUES ('P' + RIGHT('000' + CAST((SELECT COUNT(*) FROM Pledge) + 1 AS VARCHAR), 4), @datePledge, 'Pending');
END;

CREATE OR ALTER PROCEDURE usp_AddPledgeItem
     @itemName VARCHAR(255),
     @quantity INT,
     @expectedDeliveryDate DATE
AS
BEGIN

    INSERT INTO [Pledge Item] (PledgeItem_ID, Pledge_ID, Item_Name, Quantity, ExpectedDelivery_Date)
    VALUES ('PT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Pledge Item]) + 1 AS VARCHAR), 4), (SELECT COUNT(*) FROM Pledge), @itemName, @quantity, @expectedDeliveryDate);
END;

CREATE OR ALTER PROCEDURE usp_AddDonation
     @donorID VARCHAR(10),
     @eventID VARCHAR(10),
     @dateReceived DATE
AS
BEGIN

    INSERT INTO [Donation] (Donation_ID, Donor_ID, Event_ID, Date_Received)
    VALUES ('DN' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donation]) + 1 AS VARCHAR), 4), @donorID, @eventID, @dateReceived);
END;

CREATE OR ALTER PROCEDURE usp_AddDonatedItem
     @itemName VARCHAR(255),
     @quantity INT
AS
BEGIN

    INSERT INTO [Donated Items] (DonatedItem_ID, Donation_ID, Item_Name, Quantity_Received)
    VALUES ('DT' + RIGHT('000' + CAST((SELECT COUNT(*) FROM [Donated Items]) + 1 AS VARCHAR), 4), (SELECT COUNT(*) FROM Donation), @itemName, @quantity);
END;