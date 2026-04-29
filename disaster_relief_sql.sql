-- ============================================================
-- DISASTER RELIEF MANAGEMENT SYSTEM
-- Stored Procedures, User-Defined Functions, and Triggers
-- ============================================================


-- ============================================================
--  STORED PROCEDURES (USP)
-- ============================================================

-- ------------------------------------------------------------
-- USP 1: usp_RegisterDonationWithItems
-- Registers a new donation record along with its donated items
-- and updates inventory accordingly.
-- ------------------------------------------------------------
CREATE PROCEDURE usp_RegisterDonationWithItems
    @Donor_ID       INT,
    @Event_ID       INT,
    @Date_Received  DATE,
    @ItemName       NVARCHAR(100),
    @Quantity       INT,
    @Unit           NVARCHAR(50),
    @Category       NVARCHAR(50),
    @ExpirationDate DATE = NULL,
    @StorageLocation NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Insert Donation record
        DECLARE @Donation_ID INT;

        INSERT INTO DONATION (Donor_ID, Event_ID, Date_Received)
        VALUES (@Donor_ID, @Event_ID, @Date_Received);

        SET @Donation_ID = SCOPE_IDENTITY();

        -- 2. Insert Donated Item
        DECLARE @DonatedItem_ID INT;

        INSERT INTO DONATED_ITEMS (Donation_ID, Item_Name, Quantity_Received)
        VALUES (@Donation_ID, @ItemName, @Quantity);

        SET @DonatedItem_ID = SCOPE_IDENTITY();

        -- 3. Check if matching Item exists; if not, create it
        DECLARE @Item_ID INT;

        SELECT @Item_ID = Item_ID
        FROM ITEM
        WHERE Item_Name = @ItemName AND Category = @Category;

        IF @Item_ID IS NULL
        BEGIN
            INSERT INTO ITEM (Item_Name, Unit, Category)
            VALUES (@ItemName, @Unit, @Category);
            SET @Item_ID = SCOPE_IDENTITY();
        END

        -- 4. Check if Inventory record exists for this item+donated item combo
        DECLARE @Inventory_ID INT;

        SELECT @Inventory_ID = Inventory_ID
        FROM INVENTORY
        WHERE Item_ID = @Item_ID AND DonatedItem_ID = @DonatedItem_ID;

        IF @Inventory_ID IS NULL
        BEGIN
            INSERT INTO INVENTORY (Item_ID, DonatedItem_ID, Quantity_Available, Expiration_Date, Storage_Location)
            VALUES (@Item_ID, @DonatedItem_ID, @Quantity, @ExpirationDate, @StorageLocation);
        END
        ELSE
        BEGIN
            UPDATE INVENTORY
            SET Quantity_Available = Quantity_Available + @Quantity
            WHERE Inventory_ID = @Inventory_ID;
        END

        COMMIT TRANSACTION;

        SELECT 'Donation registered successfully.' AS Message,
               @Donation_ID AS Donation_ID,
               @DonatedItem_ID AS DonatedItem_ID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- ------------------------------------------------------------
-- USP 2: usp_DistributeItemsToBeneficiary
-- Records a distribution of items to a beneficiary during a
-- disaster event, deducting quantities from inventory.
-- ------------------------------------------------------------
CREATE PROCEDURE usp_DistributeItemsToBeneficiary
    @Beneficiary_ID  INT,
    @Event_ID        INT,
    @Center_ID       INT,
    @Inventory_ID    INT,
    @QuantityToGive  INT,
    @DeliveryDate    DATE,
    @DeliveryStatus  NVARCHAR(50) = 'Pending'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Validate inventory availability
        DECLARE @Available INT;

        SELECT @Available = Quantity_Available
        FROM INVENTORY
        WHERE Inventory_ID = @Inventory_ID;

        IF @Available IS NULL
            THROW 50001, 'Inventory record not found.', 1;

        IF @Available < @QuantityToGive
            THROW 50002, 'Insufficient inventory quantity.', 1;

        -- 2. Create Distribution record
        DECLARE @Distribution_ID INT;

        INSERT INTO DISTRIBUTION (Beneficiary_ID, Event_ID, Center_ID, Date_Distributed)
        VALUES (@Beneficiary_ID, @Event_ID, @Center_ID, GETDATE());

        SET @Distribution_ID = SCOPE_IDENTITY();

        -- 3. Create Distribution Item record
        INSERT INTO DISTRIBUTION_ITEMS (Distribution_ID, Inventory_ID, Quantity)
        VALUES (@Distribution_ID, @Inventory_ID, @QuantityToGive);

        -- 4. Deduct from Inventory
        UPDATE INVENTORY
        SET Quantity_Available = Quantity_Available - @QuantityToGive
        WHERE Inventory_ID = @Inventory_ID;

        -- 5. Create Delivery Schedule
        INSERT INTO DELIVERY_SCHEDULE (Distribution_ID, Delivery_Date, Delivery_Status)
        VALUES (@Distribution_ID, @DeliveryDate, @DeliveryStatus);

        COMMIT TRANSACTION;

        SELECT 'Distribution recorded successfully.' AS Message,
               @Distribution_ID AS Distribution_ID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- ------------------------------------------------------------
-- USP 3: usp_AssignVolunteerToOperation
-- Assigns a volunteer to a rescue operation and validates that
-- the operation and volunteer both exist and aren't already
-- paired.
-- ------------------------------------------------------------
CREATE PROCEDURE usp_AssignVolunteerToOperation
    @Operation_ID     INT,
    @Volunteer_ID     INT,
    @Role             NVARCHAR(100),
    @Operation_Status NVARCHAR(50) = 'Active'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Validate Operation exists
        IF NOT EXISTS (SELECT 1 FROM RESCUE_OPERATION WHERE Operation_ID = @Operation_ID)
            THROW 50003, 'Rescue Operation not found.', 1;

        -- 2. Validate Volunteer exists
        IF NOT EXISTS (SELECT 1 FROM VOLUNTEER WHERE Volunteer_ID = @Volunteer_ID)
            THROW 50004, 'Volunteer not found.', 1;

        -- 3. Check for duplicate assignment
        IF EXISTS (
            SELECT 1 FROM OPERATION_ASSIGNMENT
            WHERE Operation_ID = @Operation_ID AND Volunteer_ID = @Volunteer_ID
        )
            THROW 50005, 'Volunteer is already assigned to this operation.', 1;

        -- 4. Insert assignment
        INSERT INTO OPERATION_ASSIGNMENT (Operation_ID, Volunteer_ID, Role, Operation_Status)
        VALUES (@Operation_ID, @Volunteer_ID, @Role, @Operation_Status);

        COMMIT TRANSACTION;

        SELECT 'Volunteer assigned successfully.' AS Message,
               SCOPE_IDENTITY() AS Assignment_ID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


-- ============================================================
--  USER-DEFINED FUNCTIONS (UDF)
-- ============================================================

-- ------------------------------------------------------------
-- UDF 1: udf_GetTotalDonationsByDonor
-- Returns the total number of donations and total items
-- received from a specific donor.
-- ------------------------------------------------------------
CREATE FUNCTION udf_GetTotalDonationsByDonor (@Donor_ID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT
        d.Donor_ID,
        dn.Donor_Name,
        COUNT(DISTINCT d.Donation_ID)      AS Total_Donations,
        COALESCE(SUM(di.Quantity_Received), 0) AS Total_Items_Donated
    FROM DONATION d
    JOIN DONOR dn ON dn.Donor_ID = d.Donor_ID
    LEFT JOIN DONATED_ITEMS di ON di.Donation_ID = d.Donation_ID
    WHERE d.Donor_ID = @Donor_ID
    GROUP BY d.Donor_ID, dn.Donor_Name
);
GO

-- Usage Example:
-- SELECT * FROM udf_GetTotalDonationsByDonor(1);


-- ------------------------------------------------------------
-- UDF 2: udf_GetInventoryStatusByCenter
-- Returns the current inventory status (item name, quantity
-- available, expiration) for all items at a given evacuation
-- center via distribution item tracing.
-- ------------------------------------------------------------
CREATE FUNCTION udf_GetInventoryStatusByCenter (@Center_ID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT
        ec.Center_Name,
        i.Item_Name,
        i.Category,
        inv.Quantity_Available,
        inv.Expiration_Date,
        inv.Storage_Location,
        CASE
            WHEN inv.Quantity_Available = 0       THEN 'Out of Stock'
            WHEN inv.Quantity_Available < 10      THEN 'Low Stock'
            ELSE                                       'Adequate'
        END AS Stock_Status
    FROM INVENTORY inv
    JOIN ITEM i          ON i.Item_ID = inv.Item_ID
    JOIN DISTRIBUTION_ITEMS di ON di.Inventory_ID = inv.Inventory_ID
    JOIN DISTRIBUTION d  ON d.Distribution_ID = di.Distribution_ID
    JOIN EVACUATION_CENTER ec ON ec.Center_ID = d.Center_ID
    WHERE d.Center_ID = @Center_ID
);
GO

-- Usage Example:
-- SELECT * FROM udf_GetInventoryStatusByCenter(2);


-- ------------------------------------------------------------
-- UDF 3: udf_GetBeneficiaryDistributionSummary
-- Scalar function — returns the total number of items a
-- specific beneficiary has received across all distributions.
-- ------------------------------------------------------------
CREATE FUNCTION udf_GetBeneficiaryDistributionSummary (@Beneficiary_ID INT)
RETURNS INT
AS
BEGIN
    DECLARE @TotalReceived INT;

    SELECT @TotalReceived = COALESCE(SUM(di.Quantity), 0)
    FROM DISTRIBUTION d
    JOIN DISTRIBUTION_ITEMS di ON di.Distribution_ID = d.Distribution_ID
    WHERE d.Beneficiary_ID = @Beneficiary_ID;

    RETURN @TotalReceived;
END;
GO

-- Usage Example:
-- SELECT dbo.udf_GetBeneficiaryDistributionSummary(5) AS TotalItemsReceived;


-- ============================================================
--  TRIGGERS
-- ============================================================

-- ------------------------------------------------------------
-- TRIGGER 1: trg_LogWasteOnExpiredInventory
-- Fires AFTER UPDATE on INVENTORY.
-- When Quantity_Available is reduced to 0 AND the item is
-- expired, automatically inserts a WASTE_TRACKING record.
-- ------------------------------------------------------------
CREATE TRIGGER trg_LogWasteOnExpiredInventory
ON INVENTORY
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO WASTE_TRACKING (Inventory_ID, Quantity_Wasted, Waste_Reason, Date_Recorded)
    SELECT
        i.Inventory_ID,
        -- Wasted = whatever was available before the update
        d.Quantity_Available,
        'Expired item — quantity zeroed out',
        GETDATE()
    FROM inserted i
    JOIN deleted  d ON d.Inventory_ID = i.Inventory_ID
    WHERE i.Quantity_Available = 0
      AND i.Expiration_Date IS NOT NULL
      AND i.Expiration_Date < CAST(GETDATE() AS DATE)
      AND d.Quantity_Available > 0;  -- only when it just hit zero
END;
GO


-- ------------------------------------------------------------
-- TRIGGER 2: trg_UpdatePledgeStatusOnAllItemsDelivered
-- Fires AFTER UPDATE on PLEDGE_ITEM.
-- Checks if all pledge items for a pledge have been delivered;
-- if so, automatically updates PLEDGE.Pledge_Status to
-- 'Fulfilled'.
-- ------------------------------------------------------------
CREATE TRIGGER trg_UpdatePledgeStatusOnAllItemsDelivered
ON PLEDGE_ITEM
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Get distinct Pledge_IDs affected by the update
    DECLARE @Pledge_ID INT;

    DECLARE pledge_cursor CURSOR FOR
        SELECT DISTINCT Pledge_ID FROM inserted;

    OPEN pledge_cursor;
    FETCH NEXT FROM pledge_cursor INTO @Pledge_ID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Check if all items under this pledge have been delivered
        -- (ExpectedDelivery_Date <= today and no item is still pending)
        IF NOT EXISTS (
            SELECT 1
            FROM PLEDGE_ITEM
            WHERE Pledge_ID = @Pledge_ID
              AND (ExpectedDelivery_Date > CAST(GETDATE() AS DATE)
                   OR ExpectedDelivery_Date IS NULL)
        )
        BEGIN
            UPDATE PLEDGE
            SET Pledge_Status = 'Fulfilled'
            WHERE Pledge_ID = @Pledge_ID
              AND Pledge_Status <> 'Fulfilled';
        END

        FETCH NEXT FROM pledge_cursor INTO @Pledge_ID;
    END

    CLOSE pledge_cursor;
    DEALLOCATE pledge_cursor;
END;
GO


-- ------------------------------------------------------------
-- TRIGGER 3: trg_PreventOverbooking_EvacuationCenter
-- Fires INSTEAD OF INSERT on BENEFICIARY.
-- Checks the total beneficiaries already assigned to a center
-- against its Capacity before allowing a new beneficiary to
-- be registered there.
-- ------------------------------------------------------------
CREATE TRIGGER trg_PreventOverbooking_EvacuationCenter
ON BENEFICIARY
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Center_ID   INT;
    DECLARE @Capacity    INT;
    DECLARE @CurrentCount INT;

    SELECT @Center_ID = Center_ID FROM inserted;

    -- Get center capacity
    SELECT @Capacity = Capacity
    FROM EVACUATION_CENTER
    WHERE Center_ID = @Center_ID;

    -- Count current beneficiaries in center
    SELECT @CurrentCount = COUNT(*)
    FROM BENEFICIARY
    WHERE Center_ID = @Center_ID;

    IF @CurrentCount >= @Capacity
    BEGIN
        RAISERROR (
            'Cannot register beneficiary: Evacuation Center (ID=%d) is at full capacity (%d/%d).',
            16, 1,
            @Center_ID, @CurrentCount, @Capacity
        );
        RETURN;
    END

    -- Safe to insert
    INSERT INTO BENEFICIARY (Beneficiary_ID, Name, Category, Center_ID)
    SELECT Beneficiary_ID, Name, Category, Center_ID
    FROM inserted;
END;
GO
