using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "msCOAs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    LOD = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    CBType = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: true),
                    VouCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    BsCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    IsCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    IsDetCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msCOAs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msCOATypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msCOATypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msCustomers",
                schema: "dbo",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CreditTerm = table.Column<short>(type: "smallint", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msCustomers", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "msCustomerTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msCustomerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msItemCategories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IsLowestLevel = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: true),
                    Seq = table.Column<int>(type: "int", nullable: true),
                    Lineage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<short>(type: "smallint", nullable: false),
                    CostOfGoodSold = table.Column<decimal>(type: "decimal(19,6)", nullable: true),
                    ValuationMethod = table.Column<short>(type: "smallint", nullable: true),
                    StockType = table.Column<short>(type: "smallint", nullable: true),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    UomSellId = table.Column<int>(type: "int", nullable: true),
                    SellPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UomBuyId = table.Column<int>(type: "int", nullable: true),
                    BuyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalesTaxId = table.Column<int>(type: "int", nullable: true),
                    PurchaseTaxId = table.Column<int>(type: "int", nullable: true),
                    Category1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CoaInventory = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCOGS = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSls = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaOffSet = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCost = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msSequenceNumbers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Format = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    LastRunNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSequenceNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msSuppliers",
                schema: "dbo",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSuppliers", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "msSupplierTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSupplierTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msSystemParameters",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Module = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msSystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msTaxes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<short>(type: "smallint", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Seq = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msTaxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msUoMConversions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitToConvert = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UnitEquivalent = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Conversion = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "bit", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msUoMConversions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msUoMs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    BaseUnit = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msUoMs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "msWarehouses",
                schema: "dbo",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msWarehouses", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "NewCodes",
                columns: table => new
                {
                    Value = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "trPurchaseOrderDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    QtyRcv = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaInventory = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCogs = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trPurchaseOrderDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trPurchaseOrderHeaders",
                schema: "dbo",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    RequestBy = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RcvStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trPurchaseOrderHeaders", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "trPurchaseReceiveDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PODetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trPurchaseReceiveDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trPurchaseReceiveHeaders",
                schema: "dbo",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    POCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ReceiveBy = table.Column<long>(type: "bigint", nullable: false),
                    ApproveBy = table.Column<long>(type: "bigint", nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trPurchaseReceiveHeaders", x => x.Code);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "msCOAs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msCOATypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msCustomers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msCustomerTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msItemCategories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msSequenceNumbers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msSuppliers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msSupplierTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msSystemParameters",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msTaxes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msUoMConversions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msUoMs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "msWarehouses",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NewCodes");

            migrationBuilder.DropTable(
                name: "trPurchaseOrderDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "trPurchaseOrderHeaders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "trPurchaseReceiveDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "trPurchaseReceiveHeaders",
                schema: "dbo");
        }
    }
}
