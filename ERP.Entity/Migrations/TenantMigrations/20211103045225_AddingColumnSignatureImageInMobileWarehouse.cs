using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnSignatureImageInMobileWarehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileTransferStockHeader",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemHeader",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            // Reorder column MobileWarehouse.MobileDeliveryItemHeader
            var sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemHeader
	DROP CONSTRAINT FK_MobileDeliveryItemHeader_DeliveryPlanHeader_DlvPlanCode
GO
ALTER TABLE Sales.DeliveryPlanHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileWarehouse.Tmp_MobileDeliveryItemHeader
	(
	Code varchar(17) NOT NULL,
	DlvPlanCode varchar(17) NOT NULL,
	SignatureImage varchar(MAX) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	RejectedBy int NULL,
	RejectedDate datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.Tmp_MobileDeliveryItemHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM MobileWarehouse.MobileDeliveryItemHeader)
	 EXEC('INSERT INTO MobileWarehouse.Tmp_MobileDeliveryItemHeader (Code, DlvPlanCode, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate)
		SELECT Code, DlvPlanCode, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate FROM MobileWarehouse.MobileDeliveryItemHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail
	DROP CONSTRAINT FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code
GO
DROP TABLE MobileWarehouse.MobileDeliveryItemHeader
GO
EXECUTE sp_rename N'MobileWarehouse.Tmp_MobileDeliveryItemHeader', N'MobileDeliveryItemHeader', 'OBJECT' 
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemHeader ADD CONSTRAINT
	PK_MobileDeliveryItemHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileDeliveryItemHeader_DlvPlanCode ON MobileWarehouse.MobileDeliveryItemHeader
	(
	DlvPlanCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemHeader ADD CONSTRAINT
	FK_MobileDeliveryItemHeader_DeliveryPlanHeader_DlvPlanCode FOREIGN KEY
	(
	DlvPlanCode
	) REFERENCES Sales.DeliveryPlanHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail ADD CONSTRAINT
	FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES MobileWarehouse.MobileDeliveryItemHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileDeliveryItemDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column MobileWarehouse.vwMobileReceiveItemHeader
            sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader
	DROP CONSTRAINT FK_MobileReceiveItemHeader_Supplier_SupCode
GO
ALTER TABLE General.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader
	DROP CONSTRAINT FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode
GO
ALTER TABLE Purchasing.PurchaseReceiveHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader
	DROP CONSTRAINT FK_MobileReceiveItemHeader_Employee_ReceiveBy
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileWarehouse.Tmp_MobileReceiveItemHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	RcvCode varchar(17) NULL,
	TransCode varchar(17) NOT NULL,
	SrcTrans smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ReceiveBy bigint NOT NULL,
	SignatureImage varchar(MAX) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	RejectedBy int NULL,
	RejectedDate datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.Tmp_MobileReceiveItemHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM MobileWarehouse.MobileReceiveItemHeader)
	 EXEC('INSERT INTO MobileWarehouse.Tmp_MobileReceiveItemHeader (Code, Date, RcvCode, TransCode, SrcTrans, SupCode, ReceiveBy, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate)
		SELECT Code, Date, RcvCode, TransCode, SrcTrans, SupCode, ReceiveBy, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate FROM MobileWarehouse.MobileReceiveItemHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail
	DROP CONSTRAINT FK_MobileReceiveItemDetail_MobileReceiveItemHeader_Code
GO
DROP TABLE MobileWarehouse.MobileReceiveItemHeader
GO
EXECUTE sp_rename N'MobileWarehouse.Tmp_MobileReceiveItemHeader', N'MobileReceiveItemHeader', 'OBJECT' 
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader ADD CONSTRAINT
	PK_MobileReceiveItemHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileReceiveItemHeader_RcvCode ON MobileWarehouse.MobileReceiveItemHeader
	(
	RcvCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileReceiveItemHeader_ReceiveBy ON MobileWarehouse.MobileReceiveItemHeader
	(
	ReceiveBy
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileReceiveItemHeader_SupCode ON MobileWarehouse.MobileReceiveItemHeader
	(
	SupCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileReceiveItemHeader_TransCode ON MobileWarehouse.MobileReceiveItemHeader
	(
	TransCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader ADD CONSTRAINT
	FK_MobileReceiveItemHeader_Employee_ReceiveBy FOREIGN KEY
	(
	ReceiveBy
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader ADD CONSTRAINT
	FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode FOREIGN KEY
	(
	RcvCode
	) REFERENCES Purchasing.PurchaseReceiveHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemHeader ADD CONSTRAINT
	FK_MobileReceiveItemHeader_Supplier_SupCode FOREIGN KEY
	(
	SupCode
	) REFERENCES General.Supplier
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail ADD CONSTRAINT
	FK_MobileReceiveItemDetail_MobileReceiveItemHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES MobileWarehouse.MobileReceiveItemHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileReceiveItemDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Reorder column MobileWarehouse.vwMobileTransferStockHeader
            sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileTransferStockHeader
	DROP CONSTRAINT FK_MobileTransferStockHeader_TransferStockHeader_TransferCode
GO
ALTER TABLE Inventory.TransferStockHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileWarehouse.Tmp_MobileTransferStockHeader
	(
	Code varchar(17) NOT NULL,
	TransferCode varchar(17) NOT NULL,
	SignatureImage varchar(MAX) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL,
	RejectedBy int NULL,
	RejectedDate datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.Tmp_MobileTransferStockHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM MobileWarehouse.MobileTransferStockHeader)
	 EXEC('INSERT INTO MobileWarehouse.Tmp_MobileTransferStockHeader (Code, TransferCode, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate)
		SELECT Code, TransferCode, SignatureImage, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate, RejectedBy, RejectedDate FROM MobileWarehouse.MobileTransferStockHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail
	DROP CONSTRAINT FK_MobileTransferStockDetail_MobileTransferStockHeader_Code
GO
DROP TABLE MobileWarehouse.MobileTransferStockHeader
GO
EXECUTE sp_rename N'MobileWarehouse.Tmp_MobileTransferStockHeader', N'MobileTransferStockHeader', 'OBJECT' 
GO
ALTER TABLE MobileWarehouse.MobileTransferStockHeader ADD CONSTRAINT
	PK_MobileTransferStockHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileTransferStockHeader_TransferCode ON MobileWarehouse.MobileTransferStockHeader
	(
	TransferCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileWarehouse.MobileTransferStockHeader ADD CONSTRAINT
	FK_MobileTransferStockHeader_TransferStockHeader_TransferCode FOREIGN KEY
	(
	TransferCode
	) REFERENCES Inventory.TransferStockHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail ADD CONSTRAINT
	FK_MobileTransferStockDetail_MobileTransferStockHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES MobileWarehouse.MobileTransferStockHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileWarehouse.MobileTransferStockDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view MobileWarehouse.vwMobileDeliveryItemHeader
            sql = @"execute sp_refreshview 'MobileWarehouse.vwMobileDeliveryItemHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view MobileWarehouse.vwMobileReceiveItemHeader
            sql = @"execute sp_refreshview 'MobileWarehouse.vwMobileReceiveItemHeader'";
            migrationBuilder.Sql(sql);

            // Refresh view MobileWarehouse.vwMobileDeliveryItemHeader
            sql = @"execute sp_refreshview 'MobileWarehouse.vwMobileTransferStockHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileTransferStockHeader");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemHeader");
        }
    }
}
