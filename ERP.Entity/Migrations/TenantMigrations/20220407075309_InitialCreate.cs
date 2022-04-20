using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SystemManagement");

            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.EnsureSchema(
                name: "Sales");

            migrationBuilder.EnsureSchema(
                name: "AssetManagement");

            migrationBuilder.EnsureSchema(
                name: "HumanResource");

            migrationBuilder.EnsureSchema(
                name: "Accounting");

            migrationBuilder.EnsureSchema(
                name: "Finance");

            migrationBuilder.EnsureSchema(
                name: "General");

            migrationBuilder.EnsureSchema(
                name: "Purchasing");

            migrationBuilder.EnsureSchema(
                name: "Expedition");

            migrationBuilder.EnsureSchema(
                name: "MobileSales");

            migrationBuilder.EnsureSchema(
                name: "MobileWarehouse");

            migrationBuilder.EnsureSchema(
                name: "MobileCustomer");

            migrationBuilder.CreateTable(
                name: "Action",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Action", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Area",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: true),
                    Lineage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetType",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CoaDeprecExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaAccumDeprec = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaAsset = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CoaExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashBankType",
                schema: "Finance",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SysParCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Seq = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBankType", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ClosingMonth",
                schema: "Accounting",
                columns: table => new
                {
                    Period = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IsClose = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingMonth", x => x.Period);
                });

            migrationBuilder.CreateTable(
                name: "COAType",
                schema: "Accounting",
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
                    table.PrimaryKey("PK_COAType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogTenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Zip = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    NPWP = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PKPDate = table.Column<DateTime>(type: "date", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                schema: "General",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRate",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerType",
                schema: "General",
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
                    table.PrimaryKey("PK_CustomerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicReportTemplate",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DataTypeParameter1 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter1 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter2 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter2 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter3 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter3 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter3 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter4 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter4 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter4 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DataTypeParameter5 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    SourceParameter5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DropdownValueParameter5 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DropdownTextParameter5 = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Query = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicReportTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralJournalDetail",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralJournalDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeStatementFormat",
                schema: "Accounting",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    Category = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    PercentOf = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    PercentFrom = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    Position = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    SubtotalSort = table.Column<int>(type: "int", nullable: false),
                    Detail = table.Column<bool>(type: "bit", nullable: false),
                    Hidden = table.Column<bool>(type: "bit", nullable: false),
                    Bold = table.Column<bool>(type: "bit", nullable: false),
                    ByAccount = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatementFormat", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ItemGroup",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    Link = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Icon = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Regex = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileActivityLog",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobilePaymentMethod",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Seq = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilePaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileReason",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerm",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Due = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostingLog",
                schema: "Accounting",
                columns: table => new
                {
                    Period = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    PostedBy = table.Column<int>(type: "int", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingLog", x => x.Period);
                });

            migrationBuilder.CreateTable(
                name: "PostingState",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "date", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Notes = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    ApplyTo = table.Column<short>(type: "smallint", nullable: false),
                    CoaCost = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    QtyRcv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    FinalDiscHeader = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaInventory = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCOGS = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaPurcReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    RequestBy = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReceiveDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    FinalDiscHeader = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReceiveDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReceiveHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ReceiveBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReceiveHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    RcvDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyRcv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCodeIn = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetailExchDiffItem",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ReturnDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyRcv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetailExchDiffItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ShippedBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NoTax = table.Column<bool>(type: "bit", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "SystemManagement",
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
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    SODetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    FinalDiscHeader = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetailFreeGood",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvOrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryDetailFreeGood", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ShippedBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    FromDirectInvoice = table.Column<bool>(type: "bit", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SalesmanGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SupervisorId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesmanSchedule",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Recurrence = table.Column<byte>(type: "tinyint", nullable: false),
                    VisitDay = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanSchedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesmanScheduleCustomer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesmanScheduleId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanScheduleCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    QtyDlv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    FinalDiscHeader = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaInventory = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCOGS = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSls = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetailDiscount",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetailDiscount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetailFreeGood",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyClosed = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetailFreeGood", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyDlv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetailExchDiffItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ReturnDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyDlv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetailExchDiffItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    SalesBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NoTax = table.Column<bool>(type: "bit", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SalesTargetHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SequenceNumber",
                schema: "SystemManagement",
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
                    table.PrimaryKey("PK_SequenceNumber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierType",
                schema: "General",
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
                    table.PrimaryKey("PK_SupplierType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameterModule",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameterModule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tax",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<short>(type: "smallint", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_Tax", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UoM",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_UoM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleType",
                schema: "General",
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
                    table.PrimaryKey("PK_VehicleType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitOrder",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    VisitPlanCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitOrder", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "VisitOrderCustomer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ReplacingForSalesmanId = table.Column<long>(type: "bigint", nullable: true),
                    Visited = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitOrderCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitOrderInvoice",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Collecting = table.Column<bool>(type: "bit", nullable: false),
                    FailCollect = table.Column<bool>(type: "bit", nullable: false),
                    NotesFailCollect = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitOrderInvoice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanDetailCustomer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanDetailCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "COA",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    CBType = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: true),
                    VouCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    BsCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    IsCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    IsDetCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    ShowInMobile = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_COA_COAType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Accounting",
                        principalTable: "COAType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_COA_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "GeneralCashBankHeader",
                schema: "Finance",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    VouCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChequeNo = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsInterCashBank = table.Column<bool>(type: "bit", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralCashBankHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_GeneralCashBankHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "GeneralJournalHeader",
                schema: "Accounting",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralJournalHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_GeneralJournalHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Journal",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    TypeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    RefCode1 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RefCode2 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RefCode3 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RefCode4 = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Group = table.Column<short>(type: "smallint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Period = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CustomRate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Type = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    SrcTrans = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journal_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "IncomeStatementFormatSubtotal",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    SubCode = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatementFormatSubtotal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeStatementFormatSubtotal_IncomeStatementFormat_Code",
                        column: x => x.Code,
                        principalSchema: "Accounting",
                        principalTable: "IncomeStatementFormat",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_IncomeStatementFormatSubtotal_IncomeStatementFormat_SubCode",
                        column: x => x.SubCode,
                        principalSchema: "Accounting",
                        principalTable: "IncomeStatementFormat",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ItemCategory",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ItemCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCategory_ItemGroup_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemGroupSubGroup",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    ShowInMobile = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemGroupSubGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemGroupSubGroup_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MenuAction",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "SystemManagement",
                        principalTable: "Action",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MenuAction_Menu_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "SystemManagement",
                        principalTable: "Menu",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PromoDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ApplyTo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    PromoType = table.Column<short>(type: "smallint", nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    ValuePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ValueAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPromoWithBudget = table.Column<bool>(type: "bit", nullable: false),
                    BudgetMaximumValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OverBudgetAction = table.Column<short>(type: "smallint", nullable: false),
                    SubGroup1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoDetail_PromoHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "RoleMenu",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenu_Menu_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "SystemManagement",
                        principalTable: "Menu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleMenu_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "SystemManagement",
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuAction",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "SystemManagement",
                        principalTable: "Action",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleMenuAction_Menu_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "SystemManagement",
                        principalTable: "Menu",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoleMenuAction_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "SystemManagement",
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                schema: "General",
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
                    table.PrimaryKey("PK_Supplier", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Supplier_SupplierType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "General",
                        principalTable: "SupplierType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SystemParameter",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemParameter_SystemParameterModule_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "SystemManagement",
                        principalTable: "SystemParameterModule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UoMConversion",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitToConvert = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UnitEquivalent = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Conversion = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "bit", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UoMConversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UoMConversion_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GeneralCashBankDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Type = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TypeAmount = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    TransAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Src = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralCashBankDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralCashBankDetail_CashBankType_Type",
                        column: x => x.Type,
                        principalSchema: "Finance",
                        principalTable: "CashBankType",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_GeneralCashBankDetail_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_GeneralCashBankDetail_GeneralCashBankHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Finance",
                        principalTable: "GeneralCashBankHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SalesTargetDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    ItemSubGroupId = table.Column<int>(type: "int", nullable: false),
                    SubGroup = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_ItemGroupSubGroup_ItemSubGroupId",
                        column: x => x.ItemSubGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroupSubGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetDetail_SalesTargetHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "SalesTargetHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "PromoDetailTier",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: false),
                    FromQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ToQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaleUnit = table.Column<int>(type: "int", nullable: true),
                    ApplyToAllUnit = table.Column<bool>(type: "bit", nullable: false),
                    FreeGoodItemId = table.Column<int>(type: "int", nullable: true),
                    UnitFreeGood = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsMultiple = table.Column<bool>(type: "bit", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetailTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoDetailTier_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceAP",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceAP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAP_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAP_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceDebitMemo",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceDebitMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDebitMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDebitMemo_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "DebitMemo",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitMemo", x => x.Code);
                    table.ForeignKey(
                        name: "FK_DebitMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_DebitMemo_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionInvoiceHeader",
                schema: "Expedition",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionInvoiceHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ExpeditionInvoiceHeader_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "FixedAsset",
                schema: "AssetManagement",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartDepreciateOn = table.Column<DateTime>(type: "date", nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AcquiredValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalvageValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciationMonth = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    PurchaseOrderNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    PaymentVoucherNo = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    YearWarranty = table.Column<int>(type: "int", nullable: false),
                    CodeWarranty = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    EstimatedLife = table.Column<short>(type: "smallint", nullable: false),
                    DepreciationMethod = table.Column<int>(type: "int", nullable: false),
                    InitDepreciationExpense = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaExpense = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAsset", x => x.Code);
                    table.ForeignKey(
                        name: "FK_FixedAsset_AssetType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "AssetManagement",
                        principalTable: "AssetType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FixedAsset_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Item",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CostOfGoodSold = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: true),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    UomSellId = table.Column<int>(type: "int", nullable: true),
                    SellPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UomBuyId = table.Column<int>(type: "int", nullable: true),
                    BuyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalesTaxId = table.Column<int>(type: "int", nullable: true),
                    PurchaseTaxId = table.Column<int>(type: "int", nullable: true),
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
                    Length = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_ItemCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "Inventory",
                        principalTable: "ItemCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_Tax_PurchaseTaxId",
                        column: x => x.PurchaseTaxId,
                        principalSchema: "General",
                        principalTable: "Tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_Tax_SalesTaxId",
                        column: x => x.SalesTaxId,
                        principalSchema: "General",
                        principalTable: "Tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_UoMConversion_UomBuyId",
                        column: x => x.UomBuyId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Item_UoMConversion_UomSellId",
                        column: x => x.UomSellId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionInvoiceDetail",
                schema: "Expedition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpeditionInvoiceDetail_ExpeditionInvoiceHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Expedition",
                        principalTable: "ExpeditionInvoiceHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetDepartment",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetDepartment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssetDepartment_FixedAsset_Code",
                        column: x => x.Code,
                        principalSchema: "AssetManagement",
                        principalTable: "FixedAsset",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetHistory",
                schema: "AssetManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    JournalCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    FiscalYear = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    NumberOfMonth = table.Column<short>(type: "smallint", nullable: false),
                    Period = table.Column<short>(type: "smallint", nullable: false),
                    DepreciateDate = table.Column<DateTime>(type: "date", nullable: false),
                    DepreciateValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssetHistory_FixedAsset_Code",
                        column: x => x.Code,
                        principalSchema: "AssetManagement",
                        principalTable: "FixedAsset",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "PromoDetailMultipleItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetailMultipleItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoDetailMultipleItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PromoDetailMultipleItem_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AdjustmentDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyOnTransfer = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyAdjust = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    BaseQtyOnHand = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseQtyOnTransfer = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    COGS = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdjustmentDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdjustmentDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdjustmentDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AdjustmentDetailDiffUnit",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentDetailId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    QtyAdjust = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentDetailDiffUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdjustmentDetailDiffUnit_AdjustmentDetail_AdjustmentDetailId",
                        column: x => x.AdjustmentDetailId,
                        principalSchema: "Inventory",
                        principalTable: "AdjustmentDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdjustmentDetailDiffUnit_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AdjustmentHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                schema: "HumanResource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CheckInLat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckInLng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckInImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CheckInNotes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CheckOut = table.Column<DateTime>(type: "datetime", nullable: true),
                    CheckOutLat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckOutLng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    CheckOutImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CheckOutNotes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    TotalHours = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceAR",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceAR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceAR_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceCreditMemo",
                schema: "Accounting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceCreditMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceCreditMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BeginningBalanceDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BeginningBalanceHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeginningBalanceHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "CreditMemo",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Used = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemo", x => x.Code);
                    table.ForeignKey(
                        name: "FK_CreditMemo_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "General",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditUsed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    BillingAddressId = table.Column<int>(type: "int", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: true),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    IsConsignee = table.Column<bool>(type: "bit", nullable: false),
                    MobileSignIn = table.Column<bool>(type: "bit", nullable: false),
                    CatalogUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MobileUsername = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsMobileLoggedIn = table.Column<bool>(type: "bit", nullable: false),
                    MobileLastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    MobileSessionId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MobileTokenId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    MobileIPAddress = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FirebaseTokenId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Customer_Area_AreaId1",
                        column: x => x.AreaId1,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_Area_AreaId2",
                        column: x => x.AreaId2,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_Area_AreaId3",
                        column: x => x.AreaId3,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_Area_AreaId4",
                        column: x => x.AreaId4,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_Area_AreaId5",
                        column: x => x.AreaId5,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_CustomerType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "General",
                        principalTable: "CustomerType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customer_PaymentTerm_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "General",
                        principalTable: "PaymentTerm",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerAddress",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddress_Customer_Code",
                        column: x => x.Code,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileCustomer",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    InitialAddress = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Area_AreaId1",
                        column: x => x.AreaId1,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Area_AreaId2",
                        column: x => x.AreaId2,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Area_AreaId3",
                        column: x => x.AreaId3,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Area_AreaId4",
                        column: x => x.AreaId4,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Area_AreaId5",
                        column: x => x.AreaId5,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileCustomer_CustomerType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "General",
                        principalTable: "CustomerType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PromoSubject",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CustTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoSubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoSubject_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PromoSubject_CustomerType_CustTypeId",
                        column: x => x.CustTypeId,
                        principalSchema: "General",
                        principalTable: "CustomerType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PromoSubject_PromoHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Warehouse_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    SalesGroupId = table.Column<int>(type: "int", nullable: true),
                    FirstName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Sex = table.Column<bool>(type: "bit", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    BirthPlace = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MaritalStatus = table.Column<byte>(type: "tinyint", nullable: true),
                    IdentityCardNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Religion = table.Column<byte>(type: "tinyint", nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_SalesmanGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesmanGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employee_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "StockMutation",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    BaseUnit = table.Column<int>(type: "int", nullable: false),
                    BaseQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    BaseNettPrice = table.Column<decimal>(type: "decimal(22,9)", precision: 22, scale: 9, nullable: false),
                    RefCode1 = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    RefDetailId1 = table.Column<long>(type: "bigint", nullable: false),
                    RefCode2 = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Src = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMutation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMutation_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMutation_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMutation_UoMConversion_BaseUnit",
                        column: x => x.BaseUnit,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMutation_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMutation_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TransferStockHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    OriginTransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    WarehouseCodeFrom = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    WarehouseCodeTo = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsConsignee = table.Column<bool>(type: "bit", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferStockHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_TransferStockHeader_Warehouse_WarehouseCodeFrom",
                        column: x => x.WarehouseCodeFrom,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_TransferStockHeader_Warehouse_WarehouseCodeTo",
                        column: x => x.WarehouseCodeTo,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseQuantity",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyOnOrder = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyOnIndent = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyReorderPoint = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    QtyOnTransfer = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseQuantity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseQuantity_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseQuantity_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileCostHeader",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CashBankCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileCostHeader_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileCostHeader_GeneralCashBankHeader_CashBankCode",
                        column: x => x.CashBankCode,
                        principalSchema: "Finance",
                        principalTable: "GeneralCashBankHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileReceiveItemHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ReceiveBy = table.Column<long>(type: "bigint", nullable: false),
                    SignatureImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileReceiveItemHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_Employee_ReceiveBy",
                        column: x => x.ReceiveBy,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode",
                        column: x => x.RcvCode,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseReceiveHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileVisitLog",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    VisitOrderCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Scheduled = table.Column<bool>(type: "bit", nullable: false),
                    Visited = table.Column<bool>(type: "bit", nullable: true),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UnscheduledVisitReasonId = table.Column<int>(type: "int", nullable: true),
                    NoVisitReasonId = table.Column<int>(type: "int", nullable: true),
                    NoOrderReasonId = table.Column<int>(type: "int", nullable: true),
                    Image = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileVisitLog", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileVisitLog_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileVisitLog_MobileReason_NoOrderReasonId",
                        column: x => x.NoOrderReasonId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileReason",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileVisitLog_MobileReason_NoVisitReasonId",
                        column: x => x.NoVisitReasonId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileReason",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileVisitLog_MobileReason_UnscheduledVisitReasonId",
                        column: x => x.UnscheduledVisitReasonId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileReason",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileVisitLog_VisitOrder_VisitOrderCode",
                        column: x => x.VisitOrderCode,
                        principalSchema: "Sales",
                        principalTable: "VisitOrder",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceHeader",
                schema: "Purchasing",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    POCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    RefNo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    IssuedBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceHeader_Employee_IssuedBy",
                        column: x => x.IssuedBy,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode",
                        column: x => x.POCode,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceHeader_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SalesmanMapTrackingHistory",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    Lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    TrackedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanMapTrackingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesmanMapTrackingHistory_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    SalesBy = table.Column<long>(type: "bigint", nullable: false),
                    BillingAddressId = table.Column<int>(type: "int", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    FromDirectInvoice = table.Column<bool>(type: "bit", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_Employee_SalesBy",
                        column: x => x.SalesBy,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_PaymentTerm_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "General",
                        principalTable: "PaymentTerm",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesTargetSubject",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargetSubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargetSubject_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesTargetSubject_SalesTargetHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "SalesTargetHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    MobileSignIn = table.Column<bool>(type: "bit", nullable: false),
                    IsLoggedIn = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    SessionId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TokenId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    IsMobileLoggedIn = table.Column<bool>(type: "bit", nullable: false),
                    MobileLastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    MobileSessionId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MobileTokenId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    MobileIPAddress = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FirebaseTokenId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "SystemManagement",
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    MaxLoadVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxLoadWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    HelperId1 = table.Column<long>(type: "bigint", nullable: true),
                    HelperId2 = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicle_Employee_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleType_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "General",
                        principalTable: "VehicleType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileItemRequestHeader",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    TransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileItemRequestHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId1",
                        column: x => x.AreaId1,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId2",
                        column: x => x.AreaId2,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId3",
                        column: x => x.AreaId3,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId4",
                        column: x => x.AreaId4,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Area_AreaId5",
                        column: x => x.AreaId5,
                        principalSchema: "Sales",
                        principalTable: "Area",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestHeader_TransferStockHeader_TransferCode",
                        column: x => x.TransferCode,
                        principalSchema: "Inventory",
                        principalTable: "TransferStockHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileTransferStockHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    TransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SignatureImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileTransferStockHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileTransferStockHeader_TransferStockHeader_TransferCode",
                        column: x => x.TransferCode,
                        principalSchema: "Inventory",
                        principalTable: "TransferStockHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "TransferStockDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferStockDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferStockDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferStockDetail_TransferStockHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Inventory",
                        principalTable: "TransferStockHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_TransferStockDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferStockDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileCostDetail",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileCostDetail_MobileCostHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileCostHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileCostImage",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    Image = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCostImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileCostImage_MobileCostHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileCostHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileReceiveItemDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileReceiveItemDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_MobileReceiveItemHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileWarehouse",
                        principalTable: "MobileReceiveItemHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobilePaymentInvoice",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    VisitLogCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CoaCode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NotesFailCollect = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SrcTrans = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobilePaymentInvoice", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobilePaymentInvoice_Employee_SalesmanId",
                        column: x => x.SalesmanId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobilePaymentInvoice_MobileVisitLog_VisitLogCode",
                        column: x => x.VisitLogCode,
                        principalSchema: "MobileSales",
                        principalTable: "MobileVisitLog",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileVisitReason",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitLogCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    VisitReasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileVisitReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileVisitReason_MobileReason_VisitReasonId",
                        column: x => x.VisitReasonId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileReason",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileVisitReason_MobileVisitLog_VisitLogCode",
                        column: x => x.VisitLogCode,
                        principalSchema: "MobileSales",
                        principalTable: "MobileVisitLog",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDebitMemo",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DebitMemoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DebitMemoAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDebitMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDebitMemo_DebitMemo_DebitMemoCode",
                        column: x => x.DebitMemoCode,
                        principalSchema: "Purchasing",
                        principalTable: "DebitMemo",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDebitMemo_PurchaseInvoiceHeader_InvCode",
                        column: x => x.InvCode,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseInvoiceHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetail",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseInvoiceHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode",
                        column: x => x.RcvCode,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseReceiveHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderHeader",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SalesOrderCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode",
                        column: x => x.SalesOrderCode,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderHeader",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    VisitLogCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SalesOrderCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    SalesBy = table.Column<long>(type: "bigint", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileOrderHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_Employee_SalesBy",
                        column: x => x.SalesBy,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_MobileVisitLog_VisitLogCode",
                        column: x => x.VisitLogCode,
                        principalSchema: "MobileSales",
                        principalTable: "MobileVisitLog",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_PaymentTerm_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "General",
                        principalTable: "PaymentTerm",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderHeader_SalesOrderHeader_SalesOrderCode",
                        column: x => x.SalesOrderCode,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SOCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    FromDirectInvoice = table.Column<bool>(type: "bit", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHeader_Currency_CurrCode",
                        column: x => x.CurrCode,
                        principalSchema: "General",
                        principalTable: "Currency",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHeader_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceHeader_SalesOrderHeader_SOCode",
                        column: x => x.SOCode,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    TotalVolume = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TotalVehicleVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalVehicleWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewedBy = table.Column<int>(type: "int", nullable: true),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_DeliveryPlanHeader_Employee_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanHeader_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "General",
                        principalTable: "Vehicle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanHeader_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileItemRequestDetail",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileItemRequestDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_MobileItemRequestHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileItemRequestHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileItemRequestDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileTransferStockDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OriginalQty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    RealizeQty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileTransferStockDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_MobileTransferStockHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileWarehouse",
                        principalTable: "MobileTransferStockHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileTransferStockDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetail",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Tax_TaxId",
                        column: x => x.TaxId,
                        principalSchema: "General",
                        principalTable: "Tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetail",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_Tax_TaxId",
                        column: x => x.TaxId,
                        principalSchema: "General",
                        principalTable: "Tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceCreditMemo",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CreditMemoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditMemoAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceCreditMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceCreditMemo_CreditMemo_CreditMemoCode",
                        column: x => x.CreditMemoCode,
                        principalSchema: "Sales",
                        principalTable: "CreditMemo",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceCreditMemo_SalesInvoiceHeader_InvCode",
                        column: x => x.InvCode,
                        principalSchema: "Sales",
                        principalTable: "SalesInvoiceHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    DOCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode",
                        column: x => x.DOCode,
                        principalSchema: "Sales",
                        principalTable: "SalesDeliveryHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetail_SalesInvoiceHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "SalesInvoiceHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    IsFailShipment = table.Column<bool>(type: "bit", nullable: false),
                    FailedSendAll = table.Column<bool>(type: "bit", nullable: false),
                    NotesFailShipment = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetail_DeliveryPlanHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileDeliveryItemHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SignatureImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDeliveryItemHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemHeader_DeliveryPlanHeader_DlvPlanCode",
                        column: x => x.DlvPlanCode,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetailDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileCustomer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileCustomer_MobileOrderDetailFreeGood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileCustomer",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileOrderDetailDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailDiscount_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    PromoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileOrderDetailFreeGood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalSchema: "MobileSales",
                        principalTable: "MobileOrderDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_MobileOrderHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileSales",
                        principalTable: "MobileOrderHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_PromoHeader_PromoCode",
                        column: x => x.PromoCode,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileOrderDetailFreeGood_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanDetailItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanDetailItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_DeliveryPlanDetail_DlvPlanDetailId",
                        column: x => x.DlvPlanDetailId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_DeliveryPlanHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanUndeliveredItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanUndeliveredItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanDetail_DlvPlanDetailId",
                        column: x => x.DlvPlanDetailId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanUndeliveredItem_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileDeliveryItemDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OriginalQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RealizeQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDeliveryItemDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileWarehouse",
                        principalTable: "MobileDeliveryItemHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Action_Name",
                schema: "SystemManagement",
                table: "Action",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetail_Code",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetail_ItemId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetail_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetail_UomId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetailDiffUnit_AdjustmentDetailId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit",
                column: "AdjustmentDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetailDiffUnit_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentHeader_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EmployeeId",
                schema: "HumanResource",
                table: "Attendance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_Code",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAP_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_Code",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceAR_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_Code",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceCreditMemo_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_Code",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_CurrCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDebitMemo_SupCode",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_Code",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_ItemId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_UnitId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceDetail_UomId",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_BeginningBalanceHeader_WarehouseCode",
                schema: "Inventory",
                table: "BeginningBalanceHeader",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_CashBankType_SysParCode",
                schema: "Finance",
                table: "CashBankType",
                column: "SysParCode");

            migrationBuilder.CreateIndex(
                name: "IX_COA_Code",
                schema: "Accounting",
                table: "COA",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_COA_CurrCode",
                schema: "Accounting",
                table: "COA",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_COA_TypeId",
                schema: "Accounting",
                table: "COA",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemo_CurrCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemo_CustCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemo_TransCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_Date_CurrCode",
                schema: "Accounting",
                table: "CurrencyRate",
                columns: new[] { "Date", "CurrCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AreaId1",
                schema: "General",
                table: "Customer",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AreaId2",
                schema: "General",
                table: "Customer",
                column: "AreaId2");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AreaId3",
                schema: "General",
                table: "Customer",
                column: "AreaId3");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AreaId4",
                schema: "General",
                table: "Customer",
                column: "AreaId4");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AreaId5",
                schema: "General",
                table: "Customer",
                column: "AreaId5");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_BillingAddressId",
                schema: "General",
                table: "Customer",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PaymentTermId",
                schema: "General",
                table: "Customer",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_ShippingAddressId",
                schema: "General",
                table: "Customer",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_TypeId",
                schema: "General",
                table: "Customer",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddress_Code",
                schema: "General",
                table: "CustomerAddress",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DebitMemo_CurrCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_DebitMemo_SupCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_DebitMemo_TransCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetail_Code",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetail_TransCode",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_Code",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "DlvPlanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_ItemId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_TransDetailId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "TransDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_UnitId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_UomId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanHeader_DriverId",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanHeader_VehicleId",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanHeader_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_Code",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "DlvPlanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_ItemId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_UnitId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_UomId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanUndeliveredItem_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_SalesGroupId",
                schema: "General",
                table: "Employee",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_WarehouseCode",
                schema: "General",
                table: "Employee",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionInvoiceDetail_Code",
                schema: "Expedition",
                table: "ExpeditionInvoiceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionInvoiceHeader_SupCode",
                schema: "Expedition",
                table: "ExpeditionInvoiceHeader",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAsset_SupCode",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAsset_TypeId",
                schema: "AssetManagement",
                table: "FixedAsset",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetDepartment_Code",
                schema: "AssetManagement",
                table: "FixedAssetDepartment",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetHistory_Code",
                schema: "AssetManagement",
                table: "FixedAssetHistory",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_CoaCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_Code",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_CurrCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_TransCode",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankDetail_Type",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankHeader_CoaCode",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralCashBankHeader_CurrCode",
                schema: "Finance",
                table: "GeneralCashBankHeader",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralJournalDetail_CoaCode",
                schema: "Accounting",
                table: "GeneralJournalDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralJournalHeader_CurrCode",
                schema: "Accounting",
                table: "GeneralJournalHeader",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatementFormatSubtotal_Code",
                schema: "Accounting",
                table: "IncomeStatementFormatSubtotal",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatementFormatSubtotal_SubCode",
                schema: "Accounting",
                table: "IncomeStatementFormatSubtotal",
                column: "SubCode");

            migrationBuilder.CreateIndex(
                name: "IX_Item_CategoryId",
                schema: "Inventory",
                table: "Item",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_PurchaseTaxId",
                schema: "Inventory",
                table: "Item",
                column: "PurchaseTaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_SalesTaxId",
                schema: "Inventory",
                table: "Item",
                column: "SalesTaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UomBuyId",
                schema: "Inventory",
                table: "Item",
                column: "UomBuyId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UomId",
                schema: "Inventory",
                table: "Item",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UomSellId",
                schema: "Inventory",
                table: "Item",
                column: "UomSellId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategory_GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemGroupSubGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemGroupSubGroup",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_CoaCode",
                schema: "Accounting",
                table: "Journal",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Code",
                schema: "Accounting",
                table: "Journal",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_CurrCode",
                schema: "Accounting",
                table: "Journal",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Period",
                schema: "Accounting",
                table: "Journal",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode1",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode1");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode2",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode2");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode3",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode3");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefCode4",
                schema: "Accounting",
                table: "Journal",
                column: "RefCode4");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_SrcTrans",
                schema: "Accounting",
                table: "Journal",
                column: "SrcTrans");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_TypeCode",
                schema: "Accounting",
                table: "Journal",
                column: "TypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAction_ActionId",
                schema: "SystemManagement",
                table: "MenuAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAction_MenuId_ActionId",
                schema: "SystemManagement",
                table: "MenuAction",
                columns: new[] { "MenuId", "ActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobileActivityLog_Date_Username_TypeCode",
                schema: "MobileSales",
                table: "MobileActivityLog",
                columns: new[] { "Date", "Username", "TypeCode" });

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostDetail_CoaCode",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostDetail_Code",
                schema: "MobileSales",
                table: "MobileCostDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostHeader_CashBankCode",
                schema: "MobileSales",
                table: "MobileCostHeader",
                column: "CashBankCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostHeader_SalesmanId",
                schema: "MobileSales",
                table: "MobileCostHeader",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCostImage_Code",
                schema: "MobileSales",
                table: "MobileCostImage",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_AreaId1",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_AreaId2",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "AreaId2");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_AreaId3",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "AreaId3");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_AreaId4",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "AreaId4");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_AreaId5",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "AreaId5");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_CustCode",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_TypeId",
                schema: "MobileSales",
                table: "MobileCustomer",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemHeader_DlvPlanCode",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemHeader",
                column: "DlvPlanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_Code",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_ItemId",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestDetail_UnitId",
                schema: "MobileSales",
                table: "MobileItemRequestDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId1",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId2",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId2");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId3",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId3");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId4",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId4");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_AreaId5",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "AreaId5");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_SalesmanId",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileItemRequestHeader_TransferCode",
                schema: "MobileSales",
                table: "MobileItemRequestHeader",
                column: "TransferCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_ItemId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_TaxId ",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_UnitId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetail_UomId",
                schema: "MobileCustomer",
                table: "MobileOrderDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetail_Code",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetail_ItemId",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetail_TaxId",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetail_UnitId",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetail_UomId",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_OrderDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_PromoCode",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailDiscount_PromoDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailDiscount",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailDiscount_Code",
                schema: "MobileSales",
                table: "MobileOrderDetailDiscount",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailDiscount_OrderDetailId",
                schema: "MobileSales",
                table: "MobileOrderDetailDiscount",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailDiscount_PromoCode",
                schema: "MobileSales",
                table: "MobileOrderDetailDiscount",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailDiscount_PromoDetailId",
                schema: "MobileSales",
                table: "MobileOrderDetailDiscount",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_Code",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_ItemId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_OrderDetailId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_PromoCode",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_UnitId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderDetailFreeGood_UomId",
                schema: "MobileCustomer",
                table: "MobileOrderDetailFreeGood",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_Code",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_ItemId",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_OrderDetailId",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_PromoCode",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "PromoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_UnitId",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderDetailFreeGood_UomId",
                schema: "MobileSales",
                table: "MobileOrderDetailFreeGood",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderHeader_CustCode",
                schema: "MobileCustomer",
                table: "MobileOrderHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileCustomer_MobileOrderHeader_SalesOrderCode",
                schema: "MobileCustomer",
                table: "MobileOrderHeader",
                column: "SalesOrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderHeader_CustCode",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderHeader_PaymentTermId",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderHeader_SalesBy",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                column: "SalesBy");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderHeader_SalesOrderCode",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                column: "SalesOrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileOrderHeader_VisitLogCode",
                schema: "MobileSales",
                table: "MobileOrderHeader",
                column: "VisitLogCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobilePaymentInvoice_CoaCode",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobilePaymentInvoice_CustCode",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobilePaymentInvoice_SalesmanId",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobilePaymentInvoice_TransCode",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobilePaymentInvoice_VisitLogCode",
                schema: "MobileSales",
                table: "MobilePaymentInvoice",
                column: "VisitLogCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReason_Type",
                schema: "MobileSales",
                table: "MobileReason",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_WarehouseCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_RcvCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "RcvCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_ReceiveBy",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "ReceiveBy");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_SupCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_TransCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileTransferStockDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileTransferStockHeader_TransferCode",
                schema: "MobileWarehouse",
                table: "MobileTransferStockHeader",
                column: "TransferCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_CustCode",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_NoOrderReasonId",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "NoOrderReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_NoVisitReasonId",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "NoVisitReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_SalesmanId",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_UnscheduledVisitReasonId",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "UnscheduledVisitReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitLog_VisitOrderCode",
                schema: "MobileSales",
                table: "MobileVisitLog",
                column: "VisitOrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitReason_VisitLogCode",
                schema: "MobileSales",
                table: "MobileVisitReason",
                column: "VisitLogCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileVisitReason_VisitReasonId",
                schema: "MobileSales",
                table: "MobileVisitReason",
                column: "VisitReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetail_Code",
                schema: "Sales",
                table: "PromoDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetail_ItemId",
                schema: "Sales",
                table: "PromoDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailMultipleItem_ItemId",
                schema: "Sales",
                table: "PromoDetailMultipleItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailMultipleItem_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailMultipleItem",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailTier_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailTier",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_Code",
                schema: "Sales",
                table: "PromoSubject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_CustCode",
                schema: "Sales",
                table: "PromoSubject",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_CustTypeId",
                schema: "Sales",
                table: "PromoSubject",
                column: "CustTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDebitMemo_DebitMemoCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDebitMemo",
                column: "DebitMemoCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDebitMemo_InvCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDebitMemo",
                column: "InvCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetail_Code",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetail_RcvCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail",
                column: "RcvCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceHeader_CurrCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceHeader_IssuedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "IssuedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceHeader_POCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "POCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceHeader_SupCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceiveHeader_TransCode",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_RoleId_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu",
                columns: new[] { "RoleId", "MenuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuAction_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuAction_MenuId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuAction_RoleId_MenuId_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                columns: new[] { "RoleId", "MenuId", "ActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryHeader_TransCode",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceCreditMemo_CreditMemoCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                column: "CreditMemoCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceCreditMemo_InvCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                column: "InvCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetail_Code",
                schema: "Sales",
                table: "SalesInvoiceDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetail_DOCode",
                schema: "Sales",
                table: "SalesInvoiceDetail",
                column: "DOCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHeader_CurrCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "CurrCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHeader_CustCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHeader_SOCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "SOCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesmanMapTrackingHistory_SalesmanId",
                schema: "Sales",
                table: "SalesmanMapTrackingHistory",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_CustCode",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_Code",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_ItemGroupId",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_ItemSubGroupId",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "ItemSubGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetDetail_SubGroup",
                schema: "Sales",
                table: "SalesTargetDetail",
                column: "SubGroup");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetSubject_Code",
                schema: "Sales",
                table: "SalesTargetSubject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargetSubject_SalesmanId",
                schema: "Sales",
                table: "SalesTargetSubject",
                column: "SalesmanId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceNumber_Code",
                schema: "SystemManagement",
                table: "SequenceNumber",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceNumber_Format",
                schema: "SystemManagement",
                table: "SequenceNumber",
                column: "Format");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_BaseUnit",
                schema: "Inventory",
                table: "StockMutation",
                column: "BaseUnit");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_ItemId",
                schema: "Inventory",
                table: "StockMutation",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefCode1",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefCode1");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefCode2",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefCode2");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefDetailId1",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefDetailId1");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_Src",
                schema: "Inventory",
                table: "StockMutation",
                column: "Src");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_Type",
                schema: "Inventory",
                table: "StockMutation",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_UnitId",
                schema: "Inventory",
                table: "StockMutation",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_UomId",
                schema: "Inventory",
                table: "StockMutation",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_WarehouseCode",
                schema: "Inventory",
                table: "StockMutation",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_TypeId",
                schema: "General",
                table: "Supplier",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameter_Code",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameter_ModuleId",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_CoaCode",
                schema: "General",
                table: "Tax",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockDetail_Code",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockDetail_ItemId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockDetail_UnitId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockDetail_UomId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockHeader_WarehouseCodeFrom",
                schema: "Inventory",
                table: "TransferStockHeader",
                column: "WarehouseCodeFrom");

            migrationBuilder.CreateIndex(
                name: "IX_TransferStockHeader_WarehouseCodeTo",
                schema: "Inventory",
                table: "TransferStockHeader",
                column: "WarehouseCodeTo");

            migrationBuilder.CreateIndex(
                name: "IX_UoMConversion_UomId",
                schema: "Inventory",
                table: "UoMConversion",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_User_EmployeeId",
                schema: "SystemManagement",
                table: "User",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                schema: "SystemManagement",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_DriverId",
                schema: "General",
                table: "Vehicle",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_TypeId",
                schema: "General",
                table: "Vehicle",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_CustCode",
                schema: "Inventory",
                table: "Warehouse",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseQuantity_ItemId",
                schema: "Inventory",
                table: "WarehouseQuantity",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseQuantity_WarehouseCode",
                schema: "Inventory",
                table: "WarehouseQuantity",
                column: "WarehouseCode");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetail_AdjustmentHeader_Code",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "Code",
                principalSchema: "Inventory",
                principalTable: "AdjustmentHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentHeader_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Employee_EmployeeId",
                schema: "HumanResource",
                table: "Attendance",
                column: "EmployeeId",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceAR_Customer_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceCreditMemo_Customer_CustCode",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceDetail_BeginningBalanceHeader_Code",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                column: "Code",
                principalSchema: "Inventory",
                principalTable: "BeginningBalanceHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_BeginningBalanceHeader_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "BeginningBalanceHeader",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditMemo_Customer_CustCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustomerAddress_BillingAddressId",
                schema: "General",
                table: "Customer",
                column: "BillingAddressId",
                principalSchema: "General",
                principalTable: "CustomerAddress",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustomerAddress_ShippingAddressId",
                schema: "General",
                table: "Customer",
                column: "ShippingAddressId",
                principalSchema: "General",
                principalTable: "CustomerAddress",
                principalColumn: "Id");

            // Create function dbo.udf_string_split
            var sql = @"CREATE FUNCTION [dbo].[udf_string_split]
(
	@string nvarchar(max),
	@separator nvarchar(4000)
)
RETURNS @output TABLE ([value] varchar(max))
AS
BEGIN

	DECLARE @idx int = 1
	DECLARE @slice varchar(max)

	IF ( LEN(@string) < 1 OR @string IS NULL ) RETURN

    SET @string = @string + @separator

    WHILE @idx !=  0
    BEGIN
		SET @idx = CHARINDEX(@separator, @string)
        SET @slice = SUBSTRING(@string, 0, @idx)

        INSERT INTO @output ([value])
        VALUES (@slice)

        SET @string = SUBSTRING(@string, @idx + 1, LEN(@string))
		IF LEN(@string) = 0 BREAK
    END

    RETURN

END";
            migrationBuilder.Sql(sql);

            // Create function dbo.udf_current_local_time
            sql = @"CREATE FUNCTION dbo.udf_current_local_time()
RETURNS datetime
AS
BEGIN

	RETURN CONVERT(datetime, CONVERT(datetimeoffset, GETUTCDATE()) AT TIME ZONE 'SE Asia Standard Time');

END";
            migrationBuilder.Sql(sql);

            // Create function dbo.udf_num_to_words_id
            sql = @"CREATE FUNCTION [dbo].[udf_num_to_words_id] (

	@value Numeric (38, 2), -- Input number with as many as 18 digits
	@centToWord bit = 0

) RETURNS VARCHAR(8000) 
/*
* Converts a integer number as large as 34 digits into the 
* equivalent words.  The first letter is capitalized.
*
* Attribution: Based on NumberToWords by Srinivas Sampath
*        as revised by Nick Barclay
*
* Example:
select dbo.udf_num_to_words_id (1234567890) + CHAR(10)
      +  dbo.udf_num_to_words_id (0) + CHAR(10)
      +  dbo.udf_num_to_words_id (123) + CHAR(10)
select dbo.udf_num_to_words_id(76543210987654321098765432109876543210)
 
DECLARE @i numeric (38,0)
SET @i = 0
WHILE @I <= 1000 BEGIN 
    PRINT convert (char(5), @i)  
            + convert(varchar(255), dbo.udf_num_to_words_id(@i)) 
    SET @I  = @i + 1 
END
*
* Published as the T-SQL UDF of the Week Vol 2 #9 2/17/03
****************************************************************/
AS BEGIN

Declare @Number Numeric(38,0)
DECLARE @Cents as int
DECLARE @inputNumber VARCHAR(38)
DECLARE @NumbersTable TABLE (number CHAR(2), word VARCHAR(15))
DECLARE @outputString VARCHAR(8000)
DECLARE @length INT
DECLARE @counter INT
DECLARE @loops INT
DECLARE @position INT
DECLARE @chunk CHAR(3) -- for chunks of 3 numbers
DECLARE @tensones CHAR(2)
DECLARE @hundreds CHAR(1)
DECLARE @tens CHAR(1)
DECLARE @ones CHAR(1)

SET @Number = FLOOR(@value)
SET @Cents = 100 * (@value - CONVERT(decimal, @Number))

IF @Number = 0 Return '-'

-- initialize the variables
SELECT @inputNumber = CONVERT(varchar(38), @Number)
     , @outputString = ''
     , @counter = 1
SELECT @length   = LEN(@inputNumber)
     , @position = LEN(@inputNumber) - 2
     , @loops    = LEN(@inputNumber)/3

-- make sure there is an extra loop added for the remaining numbers
IF LEN(@inputNumber) % 3 <> 0 SET @loops = @loops + 1

-- insert data for the numbers and words
INSERT INTO @NumbersTable   SELECT '00', ''
    UNION ALL SELECT '01', 'satu'      UNION ALL SELECT '02', 'dua'
    UNION ALL SELECT '03', 'tiga'    UNION ALL SELECT '04', 'empat'
    UNION ALL SELECT '05', 'lima'     UNION ALL SELECT '06', 'enam'
    UNION ALL SELECT '07', 'tujuh'    UNION ALL SELECT '08', 'delapan'
    UNION ALL SELECT '09', 'sembilan'     UNION ALL SELECT '10', 'sepuluh'
    UNION ALL SELECT '11', 'sebelas'   UNION ALL SELECT '12', 'dua belas'
    UNION ALL SELECT '13', 'tiga belas' UNION ALL SELECT '14', 'empat belas'
    UNION ALL SELECT '15', 'lima belas'  UNION ALL SELECT '16', 'enam belas'
    UNION ALL SELECT '17', 'tujuh belas' UNION ALL SELECT '18', 'delapan belas'
    UNION ALL SELECT '19', 'sembilan belas' UNION ALL SELECT '20', 'dua puluh'
    UNION ALL SELECT '30', 'tiga puluh'   UNION ALL SELECT '40', 'empat puluh'
    UNION ALL SELECT '50', 'lima puluh'    UNION ALL SELECT '60', 'enam puluh'
    UNION ALL SELECT '70', 'tujuh puluh'  UNION ALL SELECT '80', 'delapan puluh'
    UNION ALL SELECT '90', 'sembilan puluh'

WHILE @counter <= @loops BEGIN

	-- get chunks of 3 numbers at a time, padded with leading zeros
	SET @chunk = RIGHT('000' + SUBSTRING(@inputNumber, @position, 3), 3)

	IF @chunk <> '000' BEGIN
		SELECT @tensones = SUBSTRING(@chunk, 2, 2)
		     , @hundreds = SUBSTRING(@chunk, 1, 1)
		     , @tens = SUBSTRING(@chunk, 2, 1)
		     , @ones = SUBSTRING(@chunk, 3, 1)

		-- If twenty or less, use the word directly from @NumbersTable
		IF CONVERT(INT, @tensones) <= 20 OR @Ones='0' BEGIN
			SET @outputString = CASE WHEN  @counter = 2 AND @tensones = '01' AND LEN(@outputString) > 0 THEN ''
								ELSE (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE @tensones = number)
								END
                   + CASE @counter WHEN 1 THEN '' -- No name
					   WHEN 2 THEN CASE WHEN @tensones = '01' AND LEN(@outputString) > 0 THEN 'seribu ' ELSE ' ribu ' END
					   WHEN 3 THEN ' juta '
                       WHEN 4 THEN ' milyar '  WHEN 5 THEN ' triliun '
                       WHEN 6 THEN ' kuadriliun ' WHEN 7 THEN ' kuantiliun '
                       WHEN 8 THEN ' sekstiliun '  WHEN 9 THEN ' septiliun '
                       WHEN 10 THEN ' oktiliun '  WHEN 11 THEN ' noniliun '
                       WHEN 12 THEN ' desiliun '   WHEN 13 THEN ' undesiliun '
                       ELSE '' END
                               + @outputString
		    END
		 ELSE BEGIN -- break down the ones and the tens separately

             SET @outputString = ' ' 
                            + (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE @tens + '0' = number)
					         + ' '
                             + (SELECT word 
                                    FROM @NumbersTable 
                                    WHERE '0'+ @ones = number)
                   + CASE @counter WHEN 1 THEN '' -- No name
                       WHEN 2 THEN ' ribu ' WHEN 3 THEN ' juta '
                       WHEN 4 THEN ' milyar '  WHEN 5 THEN ' triliun '
                       WHEN 6 THEN ' kuadriliun ' WHEN 7 THEN ' kuantiliun '
                       WHEN 8 THEN ' sekstiliun '  WHEN 9 THEN ' septiliun '
                       WHEN 10 THEN ' oktiliun '  WHEN 11 THEN ' noniliun '
                       WHEN 12 THEN ' desiliun '   WHEN 13 THEN ' undesiliun '
                       ELSE '' END
                            + @outputString
		END

		-- now get the hundreds
		IF @hundreds <> '0' BEGIN
			SET @outputString  = CASE WHEN @hundreds = '1' THEN ' seratus ' 
								 ELSE (SELECT word 
                                      FROM @NumbersTable 
                                      WHERE '0' + @hundreds = number)
					            + ' ratus ' END
                                + @outputString
		END
	END

	SELECT @counter = @counter + 1
	     , @position = @position - 3

END

-- Remove any double spaces
SET @outputString = LTRIM(RTRIM(REPLACE(@outputString, '  ', ' ')))
SET @outputstring = UPPER(LEFT(@outputstring, 1)) + SUBSTRING(@outputstring, 2, 8000)

IF (@outputString = 'Satu ribu') SET @outputString = 'Seribu'

IF @Cents > 0 
	IF @centToWord = 0 OR @centToWord IS NULL
		SET @outputstring = @outputstring + ' dan ' + convert(varchar, @Cents) + '/100'
	ELSE
		SET @outputstring = @outputstring + ' dan ' + LOWER(dbo.udf_num_to_words_id(@Cents, default)) + ' sen'

RETURN @outputString -- return the result
END";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceAP
            sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceAP]
AS
	SELECT ap.*,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceAP ap
	LEFT JOIN General.Supplier s
		ON ap.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON ap.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON ap.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceAR
            sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceAR]
AS
	SELECT ar.*,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceAR ar
	LEFT JOIN General.Customer c
		ON ar.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON ar.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON ar.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceCreditMemo
            sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceCreditMemo]
AS
	SELECT cm.*,
		cm.Amount - cm.Used AS Remaining,
		CASE cm.[Type]
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS TypeName,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceCreditMemo cm
	LEFT JOIN General.Customer c
		ON cm.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwBeginningBalanceDebitMemo
            sql = @"CREATE VIEW [Accounting].[vwBeginningBalanceDebitMemo]
AS
	SELECT dm.*,
		dm.Amount - dm.Used AS Remaining,
		CASE dm.[Type]
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS TypeName,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceDebitMemo dm
	LEFT JOIN General.Supplier s
		ON dm.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON dm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON dm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwClosingMonth
            sql = @"CREATE VIEW [Accounting].[vwClosingMonth]
AS
	SELECT cm.*,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial,
		(Select DateName( month , DateAdd( month , CAST((select RIGHT(cm.Period,2)) as INT) , -1 ) )+ '-' + (select LEFT(cm.Period,4))) AS PeriodName
	FROM Accounting.ClosingMonth cm
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwCOA
            sql = @"CREATE VIEW [Accounting].[vwCOA]
AS
	SELECT c.*,
		t.[Name] as TypeName,
		cr.Initial as CreatedInitial,
		up.Initial as UpdatedInitial,
		CASE 
			WHEN EXISTS (
				SELECT cd.ParentId 
				FROM Accounting.COA cd 
				WHERE cd.ParentId = c.Id
			) 
			THEN 1 
			ELSE 0 
		END AS IsParent
	FROM Accounting.COA c
	LEFT JOIN Accounting.COAType t
		ON c.TypeId = t.Id
	LEFT JOIN SystemManagement.[User] cr
		ON c.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON c.CreatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwGeneralJournalHeader
            sql = @"CREATE VIEW [Accounting].[vwGeneralJournalHeader]
AS
	SELECT g.*,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial,
		ap.Initial AS ApprovedInitial
	FROM Accounting.GeneralJournalHeader g
	LEFT JOIN SystemManagement.[User] cr
		ON g.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON g.UpdatedBy = up.Id
	LEFT JOIN SystemManagement.[User] ap
		ON g.ApprovedBy = ap.Id";
            migrationBuilder.Sql(sql);

            // Create view Accounting.vwIncomeStatementFormatSubtotal
            sql = @"CREATE VIEW [Accounting].[vwIncomeStatementFormatSubtotal]
AS
	SELECT f_st.*
	,f.[Name] AS SubName
	,f.Position AS SubPosition
	FROM Accounting.IncomeStatementFormatSubtotal f_st
	LEFT JOIN Accounting.IncomeStatementFormat f
		ON f.Code = f_st.SubCode";
            migrationBuilder.Sql(sql);

            // Create view AssetManagement.vwAssetType
            sql = @"CREATE VIEW [AssetManagement].[vwAssetType]
AS
	SELECT [at].*,
		u.Initial AS UpdatedInitial
	FROM AssetManagement.AssetType [at]
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = [at].UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view AssetManagement.vwFixedAsset
            sql = @"CREATE VIEW [AssetManagement].[vwFixedAsset] as
SELECT FA.*, A.[Name] as AssetType, S.[Name] SupName, 
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
    u_a.Initial AS ApprovedInitial,
    CASE FA.Mark
        WHEN 'A' THEN 'Active'
        WHEN 'V' THEN 'Void'
        WHEN 'CMP' THEN 'Completed' END AS [Status]
FROM [AssetManagement].[FixedAsset] FA
JOIN [AssetManagement].[AssetType] A ON FA.TypeId = A.Id
JOIN [General].[Supplier] S ON FA.SupCode = S.Code
LEFT JOIN SystemManagement.[User] u_c
    ON u_c.Id = FA.CreatedBy
LEFT JOIN SystemManagement.[User] u_u
    ON u_u.Id = FA.UpdatedBy
LEFT JOIN SystemManagement.[User] u_a
    ON u_a.Id = FA.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Expedition.vwExpeditionInvoiceHeader
            sql = @"CREATE VIEW [Expedition].[vwExpeditionInvoiceHeader]
AS
	SELECT e.*,
		Amount - PaidAmount AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		c.Initial AS CreatedInitial,
		u.Initial AS UpdatedInitial,
		a.Initial AS ApprovedInitial,
        CASE e.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Partial Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
	FROM Expedition.ExpeditionInvoiceHeader e
	LEFT JOIN General.Supplier s
		ON e.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] c
		ON c.Id = e.CreatedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy
	LEFT JOIN SystemManagement.[User] a
		ON a.Id = e.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwDebitMemo
            sql = @"CREATE VIEW [Purchasing].[vwDebitMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		s.Initial AS SupInitial,
		s.[Name] AS SupName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Purchasing.DebitMemo m
	LEFT JOIN General.Supplier s
		ON s.Code = m.SupCode";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseInvoiceHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
    SELECT pi_h.*,
		pi_h.Total - pi_h.PaidAmount as Remaining,
        s.[Name] AS SupName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE pi_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Partial Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Purchasing.PurchaseInvoiceHeader pi_h
    LEFT JOIN General.Supplier s
        ON s.Code = pi_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = pi_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = pi_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = pi_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = pi_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseOrderHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseOrderHeader]
AS
    SELECT po_h.*,
        s.[Name] AS SupName,
        e.Initial AS RequestInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE po_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'PR' THEN 'Partial Received'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Purchasing.PurchaseOrderHeader po_h
    LEFT JOIN General.Supplier s
        ON s.Code = po_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = po_h.RequestBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = po_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = po_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = po_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseOrderDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseOrderDetail]
AS
	SELECT po_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseOrderDetail po_d
	LEFT JOIN Inventory.Item i
		ON i.Id = po_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = po_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = po_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReceiveHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		pr_h.Total - pr_h.PaidAmount AS Remaining,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReceiveDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT rcv_d.*,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0)
			 ELSE 0 END AS OrderQty,
		CASE WHEN rcv_h.SrcTrans = 1 THEN ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyRcv, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0) - ISNULL(rtnx_d.QtyRcv, 0)
			 ELSE 0 END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReceiveDetail rcv_d
	LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
		ON rcv_h.Code = rcv_d.Code
	LEFT JOIN Purchasing.PurchaseReturnHeader rtn_h
		ON rtn_h.Code = rcv_h.TransCode
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 1
	LEFT JOIN Purchasing.PurchaseReturnDetail rtn_d
		ON rtn_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Purchasing.PurchaseReturnDetailExchDiffItem rtnx_d
		ON rtnx_d.Id = rcv_d.TransDetailId
		AND rcv_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = rcv_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = rcv_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = rcv_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReturnHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ShippedInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		CASE pr_h.[Type] 
			When '1' THEN 'Tukar Memo'
			When '2' THEN 'Tukar Barang Sama'
			When '3' THEN 'Tukar Barang Beda' End As TypeName,
		CASE pr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PR' THEN 'Partial Received'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Purchasing.PurchaseReturnHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pr_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReturnDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnDetail]
AS
	SELECT pr_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReturnDetail pr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Purchasing.vwPurchaseReturnDetailExchDiffItem
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReturnDetailExchDiffItem]
AS
	SELECT pr_d.*,
		i.[Name] AS ItemName,
		i.Initial AS ItemInitial,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReturnDetailExchDiffItem pr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwArea
            sql = @"CREATE VIEW [Sales].[vwArea]
AS
    SELECT a.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Sales.Area a
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = a.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = a.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwCreditMemo
            sql = @"CREATE VIEW [Sales].[vwCreditMemo]
AS
	SELECT m.*,
		m.Amount - m.Used AS Remaining,
		c.Initial AS CustInitial,
		c.[Name] AS CustName,
		CASE m.SrcTrans
			WHEN 1 THEN 'Deposit'
			WHEN 2 THEN 'Retur' END AS SrcTransName,
		CASE m.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PP' THEN 'Pending Payment'
			WHEN 'PU' THEN 'Partial Used'
			WHEN 'FU' THEN 'Full Used' END AS [Status]
	FROM Sales.CreditMemo m
	LEFT JOIN General.Customer c
		ON c.Code = m.CustCode";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwPromoHeader
            sql = @"CREATE VIEW [Sales].[vwPromoHeader]
AS
    SELECT p_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE p_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.PromoHeader p_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = p_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = p_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = p_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwDeliveryPlanHeader
            sql = @"CREATE VIEW [Sales].[vwDeliveryPlanHeader]
AS
	SELECT dp_h.*,
		v.VehicleNo,
		e.Initial AS DriverInitial,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE dp_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Sales.DeliveryPlanHeader dp_h
	LEFT JOIN General.Vehicle v
		ON v.Id = dp_h.VehicleId
	LEFT JOIN General.Employee e
		ON e.Id = dp_h.DriverId
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = dp_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = dp_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = dp_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = dp_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesDeliveryHeader
            sql = @"CREATE VIEW [Sales].[vwSalesDeliveryHeader]
AS
	SELECT do_h.*,
		c.[Name] AS CustName,
		ca.Address1 AS CustAddress,
		sa.[Name] AS CustArea,
		e.Initial AS ShippedInitial,
		u.Initial AS UpdatedInitial,
		CASE do_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'INV' THEN 'Invoiced' END AS [Status]
	FROM Sales.SalesDeliveryHeader do_h
	LEFT JOIN General.Customer c
		ON c.Code = do_h.CustCode
	LEFT JOIN General.CustomerAddress ca
		ON ca.Code = c.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area sa
		ON sa.Id = c.AreaId1
	LEFT JOIN General.Employee e
		ON e.Id = do_h.ShippedBy
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = do_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesDeliveryDetail
            sql = @"CREATE VIEW [Sales].[vwSalesDeliveryDetail]
AS
	SELECT do_d.*,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0)
			 ELSE 0 END AS OrderQty,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0) - ISNULL(so_d.QtyDlv, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyDlv, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0) - ISNULL(rtnx_d.QtyDlv, 0)
			 ELSE 0 END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Sales.SalesDeliveryHeader do_h
		ON do_h.Code = do_d.Code
	LEFT JOIN Sales.SalesReturnHeader rtn_h
		ON rtn_h.Code = do_h.TransCode
	LEFT JOIN Sales.SalesOrderDetail so_d
		ON so_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 1
	LEFT JOIN  Sales.SalesReturnDetail rtn_d
		ON rtn_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 2
	LEFT JOIN  Sales.SalesReturnDetailExchDiffItem rtnx_d
		ON rtnx_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = do_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = do_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = do_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesInvoiceHeader
            sql = @"CREATE VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
		si_h.Total - si_h.PaidAmount AS Remaining,
        c.[Name] AS CustName,
		ca.Address1 AS CustAddress,
		sa.[Name] AS CustArea,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'PP' THEN 'Pending Payment'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
	LEFT JOIN General.CustomerAddress ca
		ON ca.Code = c.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area sa
		ON sa.Id = c.AreaId1
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanGroup
            sql = @"CREATE VIEW [Sales].[vwSalesmanGroup]
AS
    SELECT sg.*,
        e.Initial AS SupervisorInitial,
		u.Initial AS UpdatedInitial
    FROM Sales.SalesmanGroup sg
    LEFT JOIN General.Employee e
        ON e.Id = sg.SupervisorId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = sg.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanSchedule
            sql = @"CREATE VIEW [Sales].[vwSalesmanSchedule]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        e.*,
        ss.AreaId1,
        a_1.[Name] AS AreaName1,
        ss.AreaId2,
        a_2.[Name] AS AreaName2,
        ss.AreaId3,
        a_3.[Name] AS AreaName3,
        ss.AreaId4,
        a_4.[Name] AS AreaName4,
        ss.AreaId5,
        a_5.[Name] AS AreaName5,
        ss.StartDate,
        ss.EndDate,
        ss.Recurrence,
        ss.VisitDay,
        LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName
    FROM Sales.SalesmanSchedule ss
    LEFT JOIN General.Employee e
        ON ss.SalesmanId = e.Id
    LEFT JOIN Sales.Area a_1
        ON ss.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON ss.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON ss.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON ss.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON ss.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanScheduleCustomer
            sql = @"CREATE VIEW [Sales].[vwSalesmanScheduleCustomer]
AS
    SELECT ss.Id AS SalesmanScheduleId,
        c.*,
        a_1.[Name] AS AreaName1,
        a_2.[Name] AS AreaName2,
        a_3.[Name] AS AreaName3,
        a_4.[Name] AS AreaName4,
        a_5.[Name] AS AreaName5,
		ca.Address1 AS Address1
    FROM Sales.SalesmanScheduleCustomer ssc
    LEFT JOIN Sales.SalesmanSchedule ss
        ON ssc.SalesmanScheduleId = ss.Id
    LEFT JOIN General.Customer c
        ON ssc.CustCode = c.Code
    LEFT JOIN Sales.Area a_1
        ON c.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON c.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON c.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON c.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON c.AreaId5 = a_5.Id
	LEFT JOIN General.CustomerAddress ca
		ON c.BillingAddressId = ca.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesOrderHeader
            sql = @"CREATE VIEW [Sales].[vwSalesOrderHeader]
AS
    SELECT so_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
		e.FirstName AS SalesName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE so_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'PS' THEN 'Partial Shipped'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Sales.SalesOrderHeader so_h
    LEFT JOIN General.Customer c
        ON c.Code = so_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = so_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = so_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = so_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = so_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesOrderDetail
            sql = @"CREATE VIEW [Sales].[vwSalesOrderDetail]
AS
	SELECT so_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesOrderDetail so_d
	LEFT JOIN Inventory.Item i
		ON i.Id = so_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = so_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = so_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesReturnHeader
            sql = @"CREATE VIEW [Sales].[vwSalesReturnHeader]
AS
	SELECT sr_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE sr_h.[Type]
			When '1' THEN 'Tukar Memo'
			When '2' THEN 'Tukar Barang Sama'
			When '3' THEN 'Tukar Barang Beda' End As TypeName,
		CASE sr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PS' THEN 'Partial Shipped'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Sales.SalesReturnHeader sr_h
	LEFT JOIN General.Customer c
		ON c.Code = sr_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = sr_h.SalesBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = sr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = sr_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = sr_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesReturnDetail
            sql = @"CREATE VIEW [Sales].[vwSalesReturnDetail]
AS
	SELECT sr_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesReturnDetail sr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = sr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = sr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = sr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesReturnDetailExchDiffItem
            sql = @"CREATE VIEW [Sales].[vwSalesReturnDetailExchDiffItem]
AS
	SELECT sr_d.*,
		i.initial as ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesReturnDetailExchDiffItem sr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = sr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = sr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = sr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesTargetHeader
            sql = @"CREATE VIEW [Sales].[vwSalesTargetHeader]
AS
    SELECT t_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        CASE t_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesTargetHeader t_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = t_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = t_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitOrder
            sql = @"CREATE VIEW [Sales].[vwVisitOrder]
AS
    SELECT vo.*,
        e.Initial AS SalesmanInitial,
        e.FirstName + ' ' + e.LastName AS SalesmanName,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupName,
        CASE WHEN vo.VisitPlanCode IS NULL
            THEN 'Manual'
            ELSE 'Jadwal Kunjungan' END AS SourceTransaction,
        CASE vo.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial
    FROM Sales.VisitOrder vo
    LEFT JOIN General.Employee e
        ON vo.SalesmanId = e.Id
    LEFT JOIN Sales.SalesmanGroup sg
        ON e.SalesGroupId = sg.Id
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vo.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vo.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vo.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitOrderCustomer
            sql = @"CREATE VIEW [Sales].[vwVisitOrderCustomer]
AS
	SELECT voc.*,
		c.Initial AS CustomerInitial,
		c.[Name] AS CustomerName,
		ca.Address1 AS [Address],
		a1.[Name] AS AreaName1,
		a2.[Name] AS AreaName2,
		e.Initial AS ReplacemanInitial,
		e.FirstName + ' ' + e.LastName AS ReplacemanName
	FROM Sales.VisitOrderCustomer voc
	LEFT JOIN General.Customer c
		ON voc.CustCode = c.Code
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code AND ca.IsDefault = 1
	LEFT JOIN General.Employee e
		ON voc.ReplacingForSalesmanId = e.Id
	LEFT JOIN Sales.Area a1
		ON c.AreaId1 = a1.Id
	LEFT JOIN Sales.Area a2
		ON c.AreaId2 = a2.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitOrderInvoice
            sql = @"CREATE VIEW [Sales].[vwVisitOrderInvoice]
AS
	SELECT voi.*,
		c.[Name] AS CustomerName,
		sih.[Date] AS TransactionDate,
		sih.DueDate AS InvoiceDueDate,
		e.FirstName + ' ' + e.LastName AS SalesName,
		sih.Total
	FROM Sales.VisitOrderInvoice voi
	LEFT JOIN Sales.SalesInvoiceHeader sih
		ON voi.InvCode = sih.Code
	LEFT JOIN Sales.SalesOrderHeader soh
		ON sih.SOCode = soh.Code
	LEFT JOIN General.Employee e
		ON soh.SalesBy = e.Id
	LEFT JOIN General.Customer c
		ON sih.CustCode = c.Code";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanHeader
            sql = @"CREATE VIEW [Sales].[vwVisitPlanHeader]
AS
    SELECT vph.*,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupName,
        e.FirstName + ' ' + e.LastName AS SupervisorName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE vph.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.VisitPlanHeader vph
    LEFT JOIN Sales.SalesmanGroup sg
        ON vph.GroupId = sg.Id
    LEFT JOIN General.Employee e
        ON sg.SupervisorId = e.Id
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vph.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vph.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vph.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetail
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetail]
AS
    SELECT vpd.Id AS VisitPlanDetailId,
        vpd.Code, e.*,
        ss.AreaId1,
        ss.AreaId2,
        ss.AreaId3,
        ss.AreaId4,
        ss.AreaId5,
        ss.StartDate,
        ss.EndDate,
        ss.Recurrence,
        ss.VisitDay,
        LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName,
        sg.Initial AS GroupInitial,
        sg.[Name] AS GroupNameInitial,
        CASE WHEN e.Sex = 1
            THEN 'Pria'
            ELSE 'Wanita'
            END AS GenderInitial
    FROM Sales.VisitPlanDetail vpd
    INNER JOIN General.Employee e
        ON vpd.SalesmanId = e.Id
    LEFT JOIN Sales.SalesmanSchedule ss
        ON vpd.SalesmanId = ss.SalesmanId
    LEFT JOIN Sales.SalesmanGroup sg
        ON e.SalesGroupId = sg.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetailCustomer
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetailCustomer]
AS
    SELECT vpdc.VisitPlanDetailId,
           c.Code,
           c.Initial,
           c.[Name],
           a_1.[Name] AS AreaName1,
           a_2.[Name] AS AreaName2,
           a_3.[Name] AS AreaName3,
           a_4.[Name] AS AreaName4,
           a_5.[Name] AS AreaName5,
           c.Email,
           c.Website,
           c.TypeId
    FROM Sales.VisitPlanDetailCustomer vpdc
    INNER JOIN General.Customer c
        ON vpdc.CustCode = c.Code
    LEFT JOIN Sales.Area a_1
        ON c.AreaId1 = a_1.Id
    LEFT JOIN Sales.Area a_2
        ON c.AreaId2 = a_2.Id
    LEFT JOIN Sales.Area a_3
        ON c.AreaId3 = a_3.Id
    LEFT JOIN Sales.Area a_4
        ON c.AreaId4 = a_4.Id
    LEFT JOIN Sales.Area a_5
        ON c.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwAP
            sql = @"CREATE VIEW [Finance].[vwAP]
AS
	SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date], CurrCode, Rate,
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceAP B
	JOIN General.Supplier S
		ON B.SupCode = S.Code
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT P.Code,P.SupCode, S.[Name] AS SupName, P.[Date], P.CurrCode, 1 AS Rate,
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, P.Notes, 'PI' AS Src
	FROM Purchasing.PurchaseInvoiceHeader P
	JOIN General.Supplier S
		ON P.SupCode = S.Code
	WHERE P.PaidAmount < P.Total AND P.Mark != 'V'";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwAR
            sql = @"CREATE VIEW [Finance].[vwAR]
AS
	SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date], CurrCode, Rate AS Rate,
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceAR B
	JOIN General.Customer C
		ON B.CustCode = C.Code
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT S.Code,S.CustCode, C.[Name] AS CustName, S.[Date], S.CurrCode, 1 AS Rate,
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, S.Notes, 'SI' AS Src
	FROM Sales.SalesInvoiceHeader S
	JOIN General.Customer C
		ON S.CustCode = C.Code
	WHERE S.PaidAmount < S.Total AND S.Mark != 'V'";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwCashBankType
            sql = @"CREATE VIEW [Finance].[vwCashBankType]
AS
	SELECT t.*,
		sp.[Value] AS CoaCode,
		c.[Name] AS CoaName,
		CASE t.Code
			WHEN 'AR' THEN 31
			WHEN 'AP' THEN 32
			WHEN 'EPAP' THEN 33
			WHEN 'TU' THEN 34
			WHEN 'DPC' THEN 35
			WHEN 'RDPC' THEN 36
			WHEN 'DPS' THEN 37
			WHEN 'RDPS' THEN 38
			WHEN 'SR' THEN 39
			WHEN 'PR' THEN 40
			ELSE 0 END AS ActionId
	FROM Finance.CashBankType t
	LEFT JOIN SystemManagement.SystemParameter sp
		ON sp.Code = t.SysParCode
	LEFT JOIN Accounting.COA c
		ON c.Code = sp.[Value]";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwDebitCreditPayment
            sql = @"CREATE VIEW [Finance].[vwDebitCreditPayment]
AS
	SELECT h.Code, TransCode, d.Amount, TypeAmount
	FROM Finance.GeneralCashBankHeader h
	JOIN Finance.GeneralCashBankDetail d
		ON h.Code = d.Code
	WHERE h.Mark = 'A'
	UNION
	SELECT DebitMemoCode AS Code, InvCode AS TransCode, DebitMemoAmount AS Amount, 'D' AS TypeAmount
	FROM Purchasing.PurchaseInvoiceDebitMemo 
	UNION
	SELECT CreditMemoCode AS Code, InvCode AS TransCode, CreditMemoAmount AS Amount, 'C' AS TypeAmount
	FROM Sales.SalesInvoiceCreditMemo";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwGeneralCashBankDetail
            sql = @"CREATE VIEW [Finance].[vwGeneralCashBankDetail]
AS
	SELECT d.*,
		c.[Name] AS CoaName
	FROM Finance.GeneralCashBankDetail d
	LEFT JOIN Accounting.COA c
		ON c.Code = d.CoaCode";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwGeneralCashBankHeader
            sql = @"CREATE VIEW [Finance].[vwGeneralCashBankHeader]
AS
	SELECT h.*, 
		c.[Name] AS CoaName, 
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE h.[Type]
			WHEN 'C' THEN 'Kas Bank Keluar'
			WHEN 'D' THEN 'Kas Bank Masuk' END AS [TypeName],
		CASE h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM (
		SELECT *
		FROM Finance.GeneralCashBankHeader
		WHERE IsInterCashBank = 0
	) h
	LEFT JOIN Accounting.COA c 
		ON c.Code = h.CoaCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwInterCashBankHeader
            sql = @"CREATE VIEW [Finance].[vwInterCashBankHeader]
AS
    SELECT gcbh.*,
        gcbd.TransCode,
        gcbh_t.CoaCode AS CoaCodeTo,
        gcbd.CurrCode AS CurrDetail,
        gcbd.Rate AS RateDetail,
        gcbd.Amount AS AmountDetail,
        gcbd.[Type] AS TypeDetail,
        gcbd.TypeAmount,
        gcbd.TransAmount,
        gcbd.Notes AS NotesDetail,
        c_f.[Name] AS CoaNameFrom,
        c_t.[Name] AS CoaNameTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE gcbh.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Finance.GeneralCashBankHeader gcbh
    LEFT JOIN Finance.GeneralCashBankDetail gcbd
        ON gcbh.Code = gcbd.Code and gcbd.[Type] = 'ICBO'
    LEFT JOIN Finance.GeneralCashBankHeader gcbh_t
        ON gcbh_t.Code = gcbd.TransCode
    LEFT JOIN Accounting.COA c_f
        ON c_f.Code = gcbh.CoaCode
    LEFT JOIN Accounting.COA c_t
        ON c_t.Code = gcbh_t.CoaCode
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = gcbh.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = gcbh.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = gcbh.ApprovedBy
    WHERE gcbd.TransCode IS NOT NULL";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwOutstandingCreditMemo
            sql = @"CREATE VIEW [Finance].[vwOutstandingCreditMemo]
AS
	SELECT bb.Code, [Date], bb.CustCode, c.[Name] AS CustName, bb.[Type], bb.CurrCode, bb.Rate,
		bb.Amount, bb.Used, bb.Amount - bb.Used AS Remaining, bb.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceCreditMemo bb
	JOIN General.Customer c
		ON bb.CustCode = c.Code
	WHERE bb.Used < bb.Amount AND bb.IsActive = 1
	UNION ALL
	SELECT Code, [Date], CustCode, CustName, SrcTrans, CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, Notes, 'CM' AS Src
	FROM Sales.vwCreditMemo
	WHERE Used < Amount AND Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwOutstandingDebitMemo
            sql = @"CREATE VIEW [Finance].[vwOutstandingDebitMemo]
AS
	SELECT bb.Code, [Date], bb.SupCode, c.[Name] AS SupName, bb.[Type], bb.CurrCode, bb.Rate,
		bb.Amount, bb.Used, bb.Amount - bb.Used AS Remaining, bb.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceDebitMemo bb
	JOIN General.Supplier c
		ON bb.SupCode = c.Code
	WHERE bb.Used < bb.Amount AND bb.IsActive = 1
	UNION ALL
	SELECT Code, [Date], SupCode, SupName, SrcTrans, CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, Notes, 'DM' AS Src
	FROM Purchasing.vwDebitMemo
	WHERE Used < Amount AND Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);

            // Create view General.vwApproval
            sql = @"CREATE VIEW [General].[vwApproval]
AS
	SELECT ts.Code, ts.[Date],
		w_f.[Name] + ' (' + w_f.Initial + ') - ' + w_t.[Name] + ' (' + w_t.Initial + ')' AS Descr,
		'Transfer Persediaan' AS SourceTrans, ts.UpdatedDate, 1 AS Seq, 9 AS ActionId
	FROM (
		SELECT Code, [Date], WarehouseCodeFrom, WarehouseCodeTo, UpdatedDate
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy IS NULL
		AND IsConsignee = 0
		AND Mark <> 'V'
	) ts
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts.WarehouseCodeTo

	UNION ALL
	SELECT ts.Code, ts.[Date],
		w_f.[Name] + ' (' + w_f.Initial + ') - ' + w_t.[Name] + ' (' + w_t.Initial + ')',
		'Konsinyasi', ts.UpdatedDate, 2, 13
	FROM (
		SELECT Code, [Date], WarehouseCodeFrom, WarehouseCodeTo, UpdatedDate
		FROM Inventory.TransferStockHeader
		WHERE ApprovedBy IS NULL
		AND IsConsignee = 1
		AND Mark <> 'V'
	) ts
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts.WarehouseCodeTo

	UNION ALL
	SELECT adj.Code, adj.[Date],
		w.Initial + ' - ' + w.[Name],
		'Penyesuaian', adj.UpdatedDate, 3, 14
	FROM (
		SELECT Code, [Date], WarehouseCode, UpdatedDate
		FROM Inventory.AdjustmentHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) adj
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj.WarehouseCode

	UNION ALL
	SELECT po.Code, po.[Date],
		po.SupCode + ' - ' + s.[Name],
		'Order Pembelian', po.UpdatedDate, 4, 15
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseOrderHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) po
	LEFT JOIN General.Supplier s
		ON s.Code = po.SupCode

	UNION ALL
	SELECT rcv.Code, rcv.[Date],
		rcv.SupCode + ' - ' + s.[Name],
		'Penerimaan', rcv.UpdatedDate, 5, 16
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseReceiveHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) rcv
	LEFT JOIN General.Supplier s
		ON s.Code = rcv.SupCode

	UNION ALL
	SELECT pi.Code, pi.[Date],
		pi.SupCode + ' - ' + s.[Name],
		'Faktur Pembelian', pi.UpdatedDate, 6, 17
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) pi
	LEFT JOIN General.Supplier s
		ON s.Code = pi.SupCode

	UNION ALL
	SELECT pr.Code, pr.[Date],
		pr.SupCode + ' - ' + s.[Name],
		'Retur Pembelian', pr.UpdatedDate, 7, 18
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Purchasing.PurchaseReturnHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) pr
	LEFT JOIN General.Supplier s
		ON s.Code = pr.SupCode

	UNION ALL
	SELECT Code, StartDate, [Name], 'Promo', UpdatedDate, 8, 19
	FROM Sales.PromoHeader
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'

	UNION ALL
	SELECT so.Code, so.[Date],
		so.CustCode + ' - ' + c.[Name],
		'Order Penjualan', so.UpdatedDate, 9, 20
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesOrderHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) so
	LEFT JOIN General.Customer c
		ON c.Code = so.CustCode

	UNION ALL
	SELECT dlv.Code, dlv.[Date],
		dlv.CustCode + ' - ' + c.[Name],
		'Surat Jalan', dlv.UpdatedDate, 10, 21
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesDeliveryHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) dlv
	LEFT JOIN General.Customer c
		ON c.Code = dlv.CustCode

	UNION ALL
	SELECT si.Code, si.[Date],
		si.CustCode + ' - ' + c.[Name],
		'Faktur Penjualan', si.UpdatedDate, 11, 22
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) si
	LEFT JOIN General.Customer c
		ON c.Code = si.CustCode

	UNION ALL
	SELECT sr.Code, sr.[Date],
		sr.CustCode + ' - ' + c.[Name],
		'Retur Penjualan', sr.UpdatedDate, 12, 23
	FROM (
		SELECT Code, [Date], CustCode, UpdatedDate
		FROM Sales.SalesReturnHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) sr
	LEFT JOIN General.Customer c
		ON c.Code = sr.CustCode

	UNION ALL
	SELECT dlv_p.Code, dlv_p.[Date],
		w.Initial + ' - ' + w.[Name],
		'Rencana Pengiriman', dlv_p.UpdatedDate, 13, 24
	FROM (
		SELECT Code, [Date], WarehouseCode, UpdatedDate
		FROM Sales.DeliveryPlanHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) dlv_p
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = dlv_p.WarehouseCode

	--UNION ALL
	--SELECT vst_p.Code, vst_p.[Date],
	--	 sg.Initial + ' - ' + sg.[Name],
	--	'Rencana Kunjungan', vst_p.UpdatedDate, 14
	--FROM (
	--	SELECT Code, [Date], GroupId, UpdatedDate
	--	FROM Sales.VisitPlanHeader
	--	WHERE ApprovedBy IS NULL
	--	AND Mark <> 'V'
	--) vst_p
	--LEFT JOIN Sales.SalesmanGroup sg
	--	ON sg.Id = vst_p.GroupId

	UNION ALL
	SELECT vst_o.Code, vst_o.[Date],
		e.Initial + ' - ' + e.[FirstName],
		'Perintah Kunjungan', vst_o.UpdatedDate, 14, 25
	FROM (
		SELECT Code, [Date], SalesmanId, UpdatedDate
		FROM Sales.VisitOrder
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) vst_o
	LEFT JOIN General.Employee e
		ON e.Id = vst_o.SalesmanId

	UNION ALL
	SELECT ei.Code, ei.[Date],
		ei.SupCode + ' - ' + s.[Name],
		'Faktur Ekspedisi', ei.UpdatedDate, 15, 26
	FROM (
		SELECT Code, [Date], SupCode, UpdatedDate
		FROM Expedition.ExpeditionInvoiceHeader
		WHERE ApprovedBy IS NULL
		AND Mark <> 'V'
	) ei
	LEFT JOIN General.Supplier s
		ON s.Code = ei.SupCode

	UNION ALL
	SELECT cb.Code, cb.[Date],
		cb.CoaCode + ' - ' + c.[Name],
		'Kas Bank Umum', cb.UpdatedDate, 16, 27
	FROM (
		SELECT Code, [Date], CoaCode, UpdatedDate
		FROM Finance.GeneralCashBankHeader
		WHERE ApprovedBy IS NULL
		AND IsInterCashBank = 0
		AND Mark <> 'V'
	) cb
	LEFT JOIN Accounting.COA c
		ON c.Code = cb.CoaCode

	UNION ALL
	SELECT cb_h.Code, cb_h.[Date],
		c_h.[Name] + ' (' + cb_h.CoaCode + ') - ' + c_d.[Name] + ' (' + cb_d.CoaCode + ')',
		'Pemindahan Dana', cb_h.UpdatedDate, 17, 28
	FROM (
		SELECT Code, [Date], CoaCode, UpdatedDate
		FROM Finance.GeneralCashBankHeader
		WHERE ApprovedBy IS NULL
		AND IsInterCashBank = 1
		AND Mark <> 'V'
	) cb_h
    LEFT JOIN Finance.GeneralCashBankDetail cb_d
        ON cb_d.Code = cb_h.Code
	LEFT JOIN Accounting.COA c_h
		ON c_h.Code = cb_h.CoaCode
	LEFT JOIN Accounting.COA c_d
		ON c_d.Code = cb_d.CoaCode
	WHERE cb_d.[Type] = 'ICBO'

	UNION ALL
	SELECT Code, [Date], Notes, 'Jurnal Umum', UpdatedDate, 18, 29
	FROM Accounting.GeneralJournalHeader
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'

	UNION ALL
	SELECT Code, PurchaseDate, [Name], 'Aktiva Tetap', UpdatedDate, 19, 30
	FROM AssetManagement.FixedAsset
	WHERE ApprovedBy IS NULL
	AND Mark <> 'V'";
            migrationBuilder.Sql(sql);

            // Create view General.vwCustomer
            sql = @"CREATE VIEW [General].[vwCustomer]
AS
	SELECT c.*, 
		t.[Name] AS TypeName,
		ca.Initial AS InitialAddress, ca.Address1, ca.Address2, 
		ca.ContactPerson, ca.Phone, ca.Fax, ca.Lat, ca.Lng,
		ISNULL(ca.IsDefault, 0) AS IsDefault,
		a1.[Name] as AreaName1,
		a2.[Name] as AreaName2,
		a3.[Name] as AreaName3,
		a4.[Name] as AreaName4,
		a5.[Name] as AreaName5,
		u.Initial as UpdatedInitial
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area A1
		ON c.AreaId1 = a1.Id
	LEFT JOIN Sales.Area A2
		ON c.AreaId2 = a2.Id
	LEFT JOIN Sales.Area A3
		ON c.AreaId3 = a3.Id
	LEFT JOIN Sales.Area A4
		ON c.AreaId4 = a4.Id
	LEFT JOIN Sales.Area A5
		ON c.AreaId5 = a5.Id
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwCustomerType
            sql = @"CREATE VIEW [General].[vwCustomerType]
AS
	SELECT ct.*,
		u.Initial AS UpdatedInitial
	FROM General.CustomerType ct
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = ct.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwEmployee
            sql = @"CREATE VIEW [General].[vwEmployee]
AS
	SELECT e.*,
		sg.Initial AS GroupInitial,
		sg.[Name] AS GroupName,
		e.FirstName + ' ' + e.LastName AS FullName,
		CASE WHEN e.Sex = 1 THEN 'Pria' ELSE 'Wanita' END AS Gender,
		u.Initial AS UpdatedInitial
	FROM General.Employee e
	LEFT JOIN Sales.SalesmanGroup sg
		ON e.SalesGroupId = sg.Id
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = e.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwPaymentTerm
            sql = @"CREATE VIEW [General].[vwPaymentTerm]
AS
	SELECT pt.*,
		u.Initial AS UpdatedInitial
	FROM General.PaymentTerm pt
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = pt.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwSupplier
            sql = @"CREATE VIEW [General].[vwSupplier]
AS
	SELECT c.*,
		t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
	FROM General.Supplier c
	LEFT JOIN General.SupplierType t
		ON t.Id = c.TypeId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwSupplierType
            sql = @"CREATE VIEW [General].[vwSupplierType]
AS
	SELECT st.*,
		u.Initial AS UpdatedInitial
	FROM General.SupplierType st
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = st.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwTax
            sql = @"CREATE VIEW [General].[vwTax]
AS
    SELECT c.*,
        x.[Name] AS CoaName,
		u.Initial AS UpdatedInitial
    FROM General.Tax c
    LEFT JOIN Accounting.COA x
        ON x.Code = c.CoaCode
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwVehicle
            sql = @"CREATE VIEW [General].[vwVehicle]
AS
    SELECT v.*,
        t.[Name] AS TypeName,
        e.Initial AS DriverInitial,
		u.Initial AS UpdatedInitial
    FROM General.Vehicle v
    LEFT JOIN General.VehicleType t
        ON t.Id = v.TypeId
    LEFT JOIN General.Employee e
        ON e.Id = v.DriverId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = v.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view General.vwVehicleType
            sql = @"CREATE VIEW [General].[vwVehicleType]
AS
    SELECT vt.*,
		u.Initial AS UpdatedInitial
    FROM General.VehicleType vt
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = vt.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view HumanResource.vwAttendanceReport
            sql = @"CREATE VIEW [HumanResource].[vwAttendanceReport] 
AS
	SELECT a.*, 
		CONCAT(a.CheckInLat, ' : ', a.CheckInLng) AS CoordinatIn,
		CONCAT(a.CheckOutLat, ' : ', a.CheckOutLng) AS CoordinatOut,
		e.Initial, CONCAT(e.FirstName, ' ', e.LastName) AS [Name], 
		e.[Type], 
		CASE 
			WHEN e.[Type] = 1 THEN 'Karyawan'
			WHEN e.[Type] = 2 THEN 'Penjual'
			WHEN e.[Type] = 3 THEN 'Supir'
			WHEN e.[Type] = 4 THEN 'Gudang'
		END AS TypeName,
		e.SalesGroupId
	FROM HumanResource.Attendance a
	JOIN General.Employee e
		ON e.Id = a.EmployeeId";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwAdjustmentDetail
            sql = @"CREATE VIEW [Inventory].[vwAdjustmentDetail]
AS
	SELECT adj_d.*,
		adj_d.QtyOnHand + adj_d.QtyAdjust AS QtyOpname,
		adj_d.QtyAdjust AS Different,
		i.[Name] AS ItemName
	FROM Inventory.AdjustmentDetail adj_d
	LEFT JOIN Inventory.Item i
		ON i.Id = adj_d.ItemId";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwAdjustmentHeader
            sql = @"CREATE VIEW [Inventory].[vwAdjustmentHeader]
AS
	SELECT adj_h.*,
		Case adj_h.[Type]
			WHEN 1 THEN 'Penyesuaian'
			WHEN 2 THEN 'Perhitungan Persediaan' END AS [Types],
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE adj_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Inventory.AdjustmentHeader adj_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = adj_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = adj_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = adj_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwAdjustmentItem
            sql = @"CREATE VIEW [Inventory].[vwAdjustmentItem]
AS 
SELECT i.*, 
	ISNULL(wq.QtyOnHand,0) AS QtyOnHand, w.code AS WarehouseCode,
	c.[Name] AS CategoryName,
	uom.Initial AS UomInitial,
	uom_c_s.UnitEquivalent AS UomSellName,
	uom_c_b.UnitEquivalent AS UomBuyName,
	u.Initial AS UpdatedInitial
FROM Inventory.Warehouse w
LEFT JOIN Inventory.Item i
    ON 1 = 1
LEFT JOIN Inventory.ItemCategory c
    ON c.Id = i.CategoryId
LEFT JOIN Inventory.WarehouseQuantity wq
    ON wq.WarehouseCode = w.Code
    AND wq.ItemId = i.Id 
LEFT JOIN Inventory.UoM uom
    ON uom.Id = i.UomId
LEFT JOIN Inventory.UoMConversion uom_c_s
    ON uom_c_s.Id = i.UomSellId
LEFT JOIN Inventory.UoMConversion uom_c_b
    ON uom_c_b.Id = i.UomBuyId
LEFT JOIN SystemManagement.[User] u
    ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwBeginningBalanceStockDetail
            sql = @"CREATE VIEW [Inventory].[vwBeginningBalanceStockDetail]
AS
	SELECT bb_d.*,
		i.[Name] AS ItemName,
		i.Initial AS ItemInitial,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice
	FROM Inventory.BeginningBalanceDetail bb_d
	LEFT JOIN Inventory.Item i
		ON i.Id = bb_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwBeginningBalanceStockHeader
            sql = @"CREATE VIEW [Inventory].[vwBeginningBalanceStockHeader]
AS
	SELECT bb_h.*,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial		
	FROM Inventory.BeginningBalanceHeader bb_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = bb_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = bb_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = bb_h.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwItem
            sql = @"CREATE VIEW [Inventory].[vwItem]
AS
	SELECT i.*,
		c.[Name] AS CategoryName,
		uom.Initial AS UomInitial,
		uom_c_s.UnitEquivalent AS UomSellName,
		uom_c_b.UnitEquivalent AS UomBuyName,
		uom_c_s.Seq AS SellSeq,
		uom_c_b.Seq AS BuySeq,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial
	FROM Inventory.Item i
	LEFT JOIN Inventory.ItemCategory c
		ON c.Id = i.CategoryId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = i.UomId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = c.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwItemCategory
            sql = @"CREATE VIEW [Inventory].[vwItemCategory]
AS
    SELECT ic.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Inventory.ItemCategory ic
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ic.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ic.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwItemGroup
            sql = @"CREATE VIEW [Inventory].[vwItemGroup]
AS
    SELECT ig.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Inventory.ItemGroup ig
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ig.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ig.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwTransferStockDetail
            sql = @"CREATE VIEW [Inventory].[vwTransferStockDetail]
AS
    SELECT ts_d.*,
        i.[Name] AS ItemName,
        uom_c.UnitEquivalent AS UnitName
    FROM Inventory.TransferStockDetail ts_d
    LEFT JOIN Inventory.Item i
        ON i.Id = ts_d.ItemId
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.Id = ts_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwTransferStockHeader
            sql = @"CREATE VIEW [Inventory].[vwTransferStockHeader]
AS
    SELECT ts_h.*,
        w_f.Initial AS WarehouseInitialFrom,
        w_t.Initial AS WarehouseInitialTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE ts_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'CMP' THEN 'Completed' END AS [Status],
        CASE ts_h.[Type]
            WHEN 'OUT' THEN 'Barang Keluar'
            WHEN 'IN' THEN 'Barang Masuk'
            WHEN 'DT' THEN 'Transfer Langsung'
			WHEN 'C' THEN 'Titip Barang'
			WHEN 'RC' THEN 'Retur Titipan'END AS TypeInitial
    FROM Inventory.TransferStockHeader ts_h
    LEFT JOIN Inventory.Warehouse w_f
        ON w_f.Code = ts_h.WarehouseCodeFrom
    LEFT JOIN Inventory.Warehouse w_t
        ON w_t.Code = ts_h.WarehouseCodeTo
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ts_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ts_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = ts_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwUoM
            sql = @"CREATE VIEW [Inventory].[vwUoM]
AS
SELECT uom.*,
    uc.initial AS CreatedInitial,
    up.initial AS UpdatedInitial
FROM Inventory.UoM uom
LEFT JOIN SystemManagement.[User] uc
	ON uc.Id = uom.CreatedBy
LEFT JOIN SystemManagement.[User] up
	ON up.Id = uom.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwWarehouse
            sql = @"CREATE VIEW [Inventory].[vwWarehouse]
AS
    SELECT w.*,
		u.Initial AS UpdatedInitial
    FROM Inventory.Warehouse w
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = w.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwWarehouseQuantity
            sql = @"CREATE VIEW [Inventory].[vwWarehouseQuantity]
AS
    SELECT wq.*,
        w.Initial + ' - ' + w.[Name] AS WarehouseInitial
    FROM Inventory.WarehouseQuantity wq
    LEFT JOIN Inventory.Warehouse w
        ON wq.WarehouseCode = w.Code";
            migrationBuilder.Sql(sql);

            // Create view MobileCustomer.vwMobileOrderDetail
            sql = @"CREATE VIEW [MobileCustomer].[vwMobileOrderDetail]
AS
	SELECT mo_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileCustomer.MobileOrderDetail mo_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mo_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mo_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mo_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view MobileCustomer.vwMobileOrderHeader
            sql = @"CREATE VIEW [MobileCustomer].[vwMobileOrderHeader]
AS
    SELECT mo_h.*,
        c.[Name] AS CustName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mo_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileCustomer.MobileOrderHeader mo_h
    LEFT JOIN General.Customer c
        ON c.Code = mo_h.CustCode
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mo_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mo_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mo_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mo_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileCostDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileCostDetail]
AS
	SELECT d.*,
		c.[Name] AS CoaName
	FROM MobileSales.MobileCostDetail d
	LEFT JOIN Accounting.COA c
		ON c.Code = d.CoaCode";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileCostHeader
            sql = @"CREATE VIEW [MobileSales].[vwMobileCostHeader]
AS
	SELECT h.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		CASE h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobileCostHeader h
	LEFT JOIN General.Employee emp
			ON emp.Id = h.SalesmanId
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileCustomer
            sql = @"CREATE VIEW [MobileSales].[vwMobileCustomer]
AS
    SELECT mc.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mc.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'APR' THEN 'Approved'
            WHEN 'REJ' THEN 'Rejected' END AS [Status]
    FROM MobileSales.MobileCustomer mc
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mc.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mc.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mc.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = mc.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileItemRequestHeader
            sql = @"CREATE VIEW [MobileSales].[vwMobileItemRequestHeader]
AS
    SELECT mir.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
        CASE mir.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'APR' THEN 'Approved'
            WHEN 'REJ' THEN 'Rejected' END AS [Status]
    FROM MobileSales.MobileItemRequestHeader mir
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mir.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mir.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mir.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mir.RejectedBy
	LEFT JOIN General.Employee emp
		ON emp.Id = mir.SalesmanId";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileItemRequestDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileItemRequestDetail]
AS
    SELECT mir.*,
        i.[Name] AS ItemName,
        uom_c.UnitEquivalent AS UnitName,
		uom_c.UomId AS UomId
    FROM MobileSales.MobileItemRequestDetail mir
    LEFT JOIN Inventory.Item i
        ON i.Id = mir.ItemId
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.Id = mir.UnitId";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileOrderHeader
            sql = @"CREATE VIEW [MobileSales].[vwMobileOrderHeader]
AS
    SELECT mo_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mo_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileSales.MobileOrderHeader mo_h
    LEFT JOIN General.Customer c
        ON c.Code = mo_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = mo_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mo_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mo_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mo_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mo_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileOrderDetail
            sql = @"CREATE VIEW [MobileSales].[vwMobileOrderDetail]
AS
	SELECT mo_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileSales.MobileOrderDetail mo_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mo_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mo_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mo_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobilePaymentInvoice
            sql = @"CREATE VIEW [MobileSales].[vwMobilePaymentInvoice]
AS
	SELECT pin.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		cus.Initial AS CustomerInitial,
		cus.[Name] AS CustomerName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		coa.[Name] AS CoaName,
		CASE pin.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobilePaymentInvoice pin
	LEFT JOIN General.Employee emp
		ON emp.Id = pin.SalesmanId
	LEFT JOIN General.Customer cus
		ON cus.Code = pin.CustCode
	LEFT JOIN Accounting.COA coa
		ON coa.Code = pin.CoaCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = pin.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = pin.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = pin.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = pin.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobilePaymentMethod
            sql = @"CREATE VIEW [MobileSales].[vwMobilePaymentMethod]
AS
	SELECT mp.*,
		u.Initial AS UpdatedInitial,
		c.[Name] AS CoaName
	FROM MobileSales.MobilePaymentMethod mp
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = mp.UpdatedBy
	LEFT JOIN Accounting.COA c
		ON c.Code = mp.CoaCode";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileReason
            sql = @"CREATE VIEW [MobileSales].[vwMobileReason]
AS
	SELECT mr.*,
		CASE 
		WHEN mr.Type = 'V' THEN 'Alasan Kunjungan'
		WHEN mr.Type = 'NV' THEN 'Alasan Tidak Berkunjung'
		WHEN mr.Type = 'USV' THEN 'Alasan Kunjungan Diluar Rute'
		WHEN mr.Type = 'NO' THEN 'Alasan Tidak Ada Pemesanan'
		END AS TypeName,
		u.Initial AS UpdatedInitial
	FROM MobileSales.MobileReason mr
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = mr.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileSales.vwMobileVisitLog
            sql = @"CREATE VIEW [MobileSales].[vwMobileVisitLog]
AS
	SELECT vl.*,
		emp.Initial AS SalesmanInitial,
		emp.FirstName AS SalesmanName,
		cus.Initial AS CustomerInitial,
		cus.[Name] AS CustomerName,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		u_r.Initial AS RejectedInitial,
		CASE vl.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'APR' THEN 'Approved'
			WHEN 'REJ' THEN 'Rejected' END AS [Status]
	FROM MobileSales.MobileVisitLog vl
	LEFT JOIN General.Employee emp
		ON emp.Id = vl.SalesmanId
	LEFT JOIN General.Customer cus
		ON cus.Code = vl.CustCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = vl.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = vl.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = vl.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
		ON u_r.Id = vl.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileDeliveryItemHeader
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileDeliveryItemHeader]
AS
    SELECT md_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE md_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status],
		dp_h.[Date],
		dp_h.WarehouseCode,
		dp_h.WarehouseInitial
    FROM MobileWarehouse.MobileDeliveryItemHeader md_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = md_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = md_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = md_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = md_h.RejectedBy
	LEFT JOIN Sales.vwDeliveryPlanHeader dp_h
		ON dp_h.Code = md_h.DlvPlanCode";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileDeliveryItemDetail
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileDeliveryItemDetail]
AS
	SELECT md_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileDeliveryItemDetail md_d
	LEFT JOIN Inventory.Item i
		ON i.Id = md_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = md_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = md_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileReceiveItemHeader
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileReceiveItemHeader]
AS
    SELECT mr_h.*,
        s.Initial AS SupInitial,
        e.Initial AS ReceiveInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mr_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileWarehouse.MobileReceiveItemHeader mr_h
    LEFT JOIN General.Supplier s
        ON s.Code = mr_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = mr_h.ReceiveBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mr_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mr_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mr_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mr_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileReceiveItemDetail
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileReceiveItemDetail]
AS
	SELECT rcv_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileReceiveItemDetail rcv_d
	LEFT JOIN Inventory.Item i
		ON i.Id = rcv_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = rcv_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = rcv_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileTransferStockHeader
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileTransferStockHeader]
AS
    SELECT mt_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mt_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status],
		ts_h.[Date],
		ts_h.WarehouseCodeFrom,
		ts_h.WarehouseInitialFrom,
		ts_h.WarehouseCodeTo,
		ts_h.WarehouseInitialTo
    FROM MobileWarehouse.MobileTransferStockHeader mt_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mt_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mt_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mt_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mt_h.RejectedBy
	LEFT JOIN Inventory.vwTransferStockHeader ts_h
		ON ts_h.Code = mt_h.TransferCode";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileTransferStockDetail
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileTransferStockDetail]
AS
	SELECT mt_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileTransferStockDetail mt_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mt_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mt_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mt_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view SystemManagement.vwCompany
            sql = @"CREATE VIEW [SystemManagement].[vwCompany]
AS
	SELECT c.*,
		u.Initial AS UpdatedInitial
	FROM SystemManagement.Company c
	LEFT JOIN SystemManagement.[User] u
		ON c.UpdatedBy = u.Id";
            migrationBuilder.Sql(sql);

            // Create view SystemManagement.vwRole
            sql = @"CREATE VIEW [SystemManagement].[vwRole]
AS
    SELECT r.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM SystemManagement.[Role] r
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = r.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = r.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view SystemManagement.vwUser
            sql = @"CREATE VIEW [SystemManagement].[vwUser]
AS
    SELECT a.*,
        r.[Name] As RoleName,
        e.Initial As EmployeeInitial,
        u.Initial AS UpdatedInitial
    FROM SystemManagement.[User] a
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = a.UpdatedBy
    LEFT JOIN General.Employee e
        ON a.EmployeeId = e.Id
    LEFT JOIN SystemManagement.[Role] r
        ON a.RoleId = r.Id";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_raiseerror
            sql = @"CREATE PROCEDURE [dbo].[sp_raiseerror]
AS  
	DECLARE @errorNumber INT = ERROR_NUMBER();
	DECLARE @errorLine INT = ERROR_LINE();
	DECLARE @errorMessage NVARCHAR(4000) = ERROR_MESSAGE();
	DECLARE @errorSeverity INT = ERROR_SEVERITY();
	DECLARE @errorState INT = ERROR_STATE();

	RAISERROR(@errorMessage, @errorSeverity, @errorState);";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_generate_autono
            sql = @"CREATE PROCEDURE [dbo].[sp_generate_autono]
	@code varchar(20),
	@date date = null,
	@newAutoNo varchar(20) = null OUTPUT
AS
BEGIN TRANSACTION

	DECLARE @result varchar(30) = ''

	IF ISNULL(@code, '') != ''
	BEGIN
		
		DECLARE @format varchar(32), @digitFormat varchar(32), @digit int,
				@myNumber varchar(20),
				@lastRunNo int
				
		/* Get format */
		SELECT @format = [Value]
		FROM SystemManagement.SystemParameter
		WHERE Code = @code
		
		IF @format IS NOT NULL
		BEGIN
		
			IF @date IS NULL
				SET @date = dbo.udf_current_local_time()

			/* Replace format for date */
			SET @format = REPLACE(@format, '{Y}', FORMAT(@date, 'yy'))
			SET @format = REPLACE(@format, '{M}', FORMAT(@date, 'MM'))
			SET @format = REPLACE(@format, '{D}', FORMAT(@date, 'dd'))

			SET @digitFormat = SUBSTRING(@format, PATINDEX('%{[0-9]}%', @format), 5)
			SET @digit = CONVERT(int, REPLACE(REPLACE(@digitFormat, '{', ''), '}', ''))
	
			/* Get sequence number */
			SELECT @lastRunNo = LastRunNo
			FROM SystemManagement.SequenceNumber
			WHERE Code = @code
			AND [Format] = @format

			IF @lastRunNo IS NULL
			BEGIN
				SET @lastRunNo = 0
			END

			SET @lastRunNo = @lastRunNo + 1
			
			SET @myNumber = '00000000000' + CONVERT(varchar, @lastRunNo)
			
			/* Replace format for sequence number */
			SET @result = REPLACE(@format, @digitFormat, RIGHT(@myNumber, @digit))

			/* Update history in SystemManagement.SequenceNumber */
			UPDATE SystemManagement.SequenceNumber
			SET LastRunNo = @lastRunNo
			WHERE Code = @code
			AND [Format] = @format

			/* Insert history in SystemManagement.SequenceNumber if no record affected */
			IF @@ROWCOUNT = 0
			BEGIN
				INSERT INTO SystemManagement.SequenceNumber (Code, [Format], LastRunNo)
				VALUES (@code, @format, @lastRunNo)
			END
		END

		/* Set output parameter */
		SET @newAutoNo = @result

	END
	
	/* return */
	SELECT @result AS value
	
COMMIT TRANSACTION";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_bsisdt_coa
            sql = @"CREATE PROCEDURE [dbo].[sp_get_bsisdt_coa]
	@type varchar(3),
	@code varchar(6)
AS
BEGIN
	DECLARE @looping int = 0

	--IF @type = 'BS'
	--BEGIN
	--	SELECT Code, 0 AS isCoa
	--	INTO #tmpSourceBs
	--	FROM Accounting.BalanceSheetFormat
	--	WHERE Code = @code
	--	UNION ALL
	--	SELECT Code, 1 AS isCoa
	--	FROM Accounting.COA
	--	WHERE Code = @code

	--	SELECT *
	--	INTO #tmpResultBs
	--	FROM #tmpSourceBs

	--	SELECT @looping = ISNULL(COUNT(*), 0)
	--	FROM #tmpSourceBs
	--	WHERE isCoa = 0

	--	WHILE @looping > 0
	--	BEGIN
	--		WITH cte_dtcode_bs AS (
	--			SELECT f.Code, 0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.ParentCode = src.Code
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_bsst AS (
	--			SELECT f.Code, 0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormatSubtotal f_st
	--				ON f_st.Code = src.Code
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.Code = f_st.SubCode
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_coa AS (
	--			SELECT c.Code, 1 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.COA c
	--				ON c.BsCode = src.Code
	--		)
	--		SELECT *
	--		INTO #tmpDetailBs
	--		FROM (
	--			SELECT * FROM cte_dtcode_bs
	--			UNION
	--			SELECT * FROM cte_dtcode_bsst
	--			UNION
	--			SELECT * FROM cte_dtcode_coa
	--			UNION
	--			SELECT * FROM #tmpResultBs
	--			WHERE isCoa = 1
	--		) TBA

	--		DELETE FROM #tmpResultBs

	--		INSERT INTO #tmpResultBs
	--			SELECT *
	--			FROM #tmpDetailBs
	--			WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceBs WHERE isCoa = 0)
			
	--		DROP TABLE #tmpDetailBs

	--		DELETE FROM #tmpSourceBs

	--		INSERT INTO #tmpSourceBs
	--			SELECT * FROM #tmpResultBs
			
	--		SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceBs WHERE isCoa = 0
	--	END

	--	SELECT DISTINCT Code AS Value
	--	FROM #tmpResultBs
	
	--	DROP TABLE #tmpSourceBs
	--	DROP TABLE #tmpResultBs
	--END

	--ELSE IF @type = 'ISS'
	IF @type = 'ISS'
	BEGIN
		SELECT Code, 0 AS isCoa
		INTO #tmpSourceIs
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type, 1)
		UNION ALL
		SELECT Code, 1 AS isCoa
		FROM Accounting.COA
		WHERE Code = @code

		SELECT *
		INTO #tmpResultIs
		FROM #tmpSourceIs

		SELECT @looping = ISNULL(COUNT(*), 0)
		FROM #tmpSourceIs
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT c.Code, 1 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.COA c
					ON c.IsCode = src.Code
			)
			SELECT *
			INTO #tmpDetailIs
			FROM (
				SELECT * FROM cte_dtcode_is
				UNION
				SELECT * FROM cte_dtcode_isst
				UNION
				SELECT * FROM cte_dtcode_coa
				UNION
				SELECT * FROM #tmpResultIs
				WHERE isCoa = 1
			) TBA

			DELETE FROM #tmpResultIs

			INSERT INTO #tmpResultIs
				SELECT *
				FROM #tmpDetailIs
				WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceIs WHERE isCoa = 0)
			
			DROP TABLE #tmpDetailIs

			DELETE FROM #tmpSourceIs

			INSERT INTO #tmpSourceIs
				SELECT * FROM #tmpResultIs
			
			SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceIs WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIs
	
		DROP TABLE #tmpSourceIs
		DROP TABLE #tmpResultIs
	END

	ELSE IF @type = 'ISD'
	BEGIN
		SELECT Code, 0 AS isCoa
		INTO #tmpSourceIsd
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type, 1)
		UNION ALL
		SELECT Code, 1 AS isCoa
		FROM Accounting.COA
		WHERE Code = @code

		SELECT *
		INTO #tmpResultIsd
		FROM #tmpSourceIsd

		SELECT @looping = ISNULL(COUNT(*), 0)
		FROM #tmpSourceIsd
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT c.Code, 1 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.COA c
					ON c.IsDetCode = src.Code
			)
			SELECT *
			INTO #tmpDetailIsd
			FROM (
				SELECT * FROM cte_dtcode_is
				UNION
				SELECT * FROM cte_dtcode_isst
				UNION
				SELECT * FROM cte_dtcode_coa
				UNION
				SELECT * FROM #tmpResultIsd
				WHERE isCoa = 1
			) TBA

			DELETE FROM #tmpResultIsd

			INSERT INTO #tmpResultIsd
				SELECT *
				FROM #tmpDetailIsd
				WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceIsd WHERE isCoa = 0)
			
			DROP TABLE #tmpDetailIsd

			DELETE FROM #tmpSourceIsd

			INSERT INTO #tmpSourceIsd
				SELECT * FROM #tmpResultIsd
			
			SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceIsd WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIsd
	
		DROP TABLE #tmpSourceIsd
		DROP TABLE #tmpResultIsd
	END
END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_dlv_plan_picking_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_dlv_plan_picking_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'ITEM'
AS
BEGIN

	IF @displayType = 'ITEM'
	BEGIN
		WITH cte_dlv_plan_src AS (
			SELECT dlv_plan_h.Code, dlv_plan_h.[Date], dlv_plan_h.VehicleId, dlv_plan_h.DriverId, dlv_plan_h.WarehouseCode,
				dlv_plan_h.Notes, dlv_plan_h.CreatedBy,
				dlv_plan_d.TransCode,
				CASE WHEN ISNULL(si_d.DOCode, '') = '' THEN dlv_plan_d.TransCode
					ELSE si_d.DOCode END AS DOCode
			FROM (
				SELECT *
				FROM Sales.DeliveryPlanHeader
				WHERE Code = @code
				AND Mark <> 'V'
			) dlv_plan_h
			LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
				ON dlv_plan_d.Code = dlv_plan_h.Code
			LEFT JOIN Sales.SalesInvoiceHeader si_h
				ON si_h.Code = dlv_plan_d.TransCode
			LEFT JOIN Sales.SalesInvoiceDetail si_d
				ON si_d.Code = si_h.Code
		)
		,cte_normal_src AS (
			SELECT cte.*,
				dlv_d.ItemId, dlv_d.Qty, dlv_d.UnitId
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesDeliveryDetail dlv_d
				ON dlv_d.Code = cte.DOCode
		)
		,cte_free_src AS (
			SELECT cte.*,
				dlv_d_fg.ItemId, dlv_d_fg.Qty, dlv_d_fg.UnitId
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesDeliveryDetailFreeGood dlv_d_fg
				ON dlv_d_fg.Code = cte.DOCode
		)
		,cte_union AS (
			SELECT cte_n.*,
				0 AS FreeQty
			FROM cte_normal_src cte_n
			UNION ALL
			SELECT cte_f.Code, cte_f.[Date], cte_f.VehicleId, cte_f.DriverId, cte_f.WarehouseCode,
				cte_f.Notes, cte_f.CreatedBy, cte_f.TransCode, cte_f.DOCode,
				cte_f.ItemId, 0 AS Qty, cte_f.UnitId,
				cte_f.Qty AS FreeQty
			FROM cte_free_src cte_f
		)
		,cte_calc_group AS (
			SELECT Code, [Date], VehicleId, DriverId, WarehouseCode, Notes, CreatedBy, ItemId, UnitId,
				SUM(Qty) AS Qty,
				SUM(FreeQty) AS FreeQty
			FROM cte_union
			GROUP BY Code, [Date], VehicleId, DriverId, WarehouseCode, Notes, CreatedBy, ItemId, UnitId
		)
		SELECT cte.*,
			v.VehicleNo,
			e_d.Initial AS DriverInitial, e_d.FirstName AS DriverFirstName, e_d.LastName AS DriverLastName,
			w.Initial AS WarehouseInitial, w.[Name] AS WarehouseName,
			e_c.Initial AS CreatedInitial,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName
		FROM cte_calc_group cte
		LEFT JOIN General.Vehicle v
			ON v.Id = cte.VehicleId
		LEFT JOIN General.Employee e_d
			ON e_d.Id = cte.DriverId
		LEFT JOIN Inventory.Warehouse w
			ON w.Code = cte.WarehouseCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = cte.CreatedBy
		LEFT JOIN Inventory.Item i
			ON i.Id = cte.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = cte.UnitId
		ORDER BY i.Initial, i.[Name], uom_c.UnitToConvert
	END

	ELSE IF @displayType = 'INVOICE'
	BEGIN
		WITH cte_dlv_plan_src AS (
			SELECT dlv_plan_h.Code,
				dlv_plan_d.TransCode,
				CASE WHEN ISNULL(si_d_do.Code, '') = '' THEN si_d_di.Code
					ELSE si_d_do.Code END AS InvCode
			FROM (
				SELECT *
				FROM Sales.DeliveryPlanHeader
				WHERE Code = @code
				AND Mark <> 'V'
			) dlv_plan_h
			LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
				ON dlv_plan_d.Code = dlv_plan_h.Code
			LEFT JOIN Sales.SalesInvoiceDetail si_d_do
				ON si_d_do.DOCode = dlv_plan_d.TransCode
			LEFT JOIN Sales.SalesInvoiceDetail si_d_di
				ON si_d_di.Code = dlv_plan_d.TransCode
		)
		,cte_src_with_cust AS (
			SELECT cte.*,
				si_h.CustCode,
				c.Initial AS CustInitial, c.[Name] AS CustName,
				CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
					ELSE ca_b.Address1 END AS CustAddress1
			FROM cte_dlv_plan_src cte
			INNER JOIN Sales.SalesInvoiceHeader si_h
				ON si_h.Code = cte.InvCode
			LEFT JOIN Sales.SalesOrderHeader so_h
				ON so_h.Code = si_h.SOCode
			LEFT JOIN General.Customer c
				ON c.Code = si_h.CustCode
			LEFT JOIN General.CustomerAddress ca_b
				ON ca_b.Code = si_h.CustCode
				AND ca_b.Id = so_h.BillingAddressId
				AND so_h.BillingAddressId IS NOT NULL
			LEFT JOIN General.CustomerAddress ca_d
				ON ca_d.Code = si_h.CustCode
				AND ca_d.IsDefault = 1
				AND so_h.BillingAddressId IS NULL
		)
		SELECT *
		FROM cte_src_with_cust
		ORDER BY InvCode
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_dlv_plan_packing_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_dlv_plan_packing_print_data]
	@code varchar(17)
AS
BEGIN

	WITH cte_dlv_plan_src AS (
		SELECT dlv_plan_h.Code, dlv_plan_h.[Date], dlv_plan_h.VehicleId, dlv_plan_h.DriverId, dlv_plan_h.WarehouseCode,
			dlv_plan_h.TotalVolume, dlv_plan_h.TotalWeight, dlv_plan_h.Notes, dlv_plan_h.CreatedBy,
			dlv_plan_d.TransCode,
			CASE WHEN ISNULL(si_d.DOCode, '') = '' THEN dlv_plan_d.TransCode
				ELSE si_d.DOCode END AS DOCode
		FROM (
			SELECT *
			FROM Sales.DeliveryPlanHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) dlv_plan_h
		LEFT JOIN Sales.DeliveryPlanDetail dlv_plan_d
			ON dlv_plan_d.Code = dlv_plan_h.Code
		LEFT JOIN Sales.SalesInvoiceHeader si_h
			ON si_h.Code = dlv_plan_d.TransCode
		LEFT JOIN Sales.SalesInvoiceDetail si_d
			ON si_d.Code = si_h.Code
	)
	,cte_normal_src AS (
		SELECT cte.*,
			dlv_d.ItemId, dlv_d.Qty, dlv_d.UnitId
		FROM cte_dlv_plan_src cte
		INNER JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = cte.DOCode
	)
	,cte_free_src AS (
		SELECT cte.*,
			dlv_d_fg.ItemId, dlv_d_fg.Qty, dlv_d_fg.UnitId
		FROM cte_dlv_plan_src cte
		INNER JOIN Sales.SalesDeliveryDetailFreeGood dlv_d_fg
			ON dlv_d_fg.Code = cte.DOCode
	)
	,cte_union AS (
		SELECT cte_n.*
		FROM cte_normal_src cte_n
		UNION ALL
		SELECT cte_f.*
		FROM cte_free_src cte_f
	)
	,cte_calc_group AS (
		SELECT cte.Code, cte.[Date], cte.VehicleId, cte.DriverId, cte.WarehouseCode, cte.TotalVolume, cte.TotalWeight,
			cte.Notes, cte.CreatedBy, cte.TransCode,
			cte.ItemId, cte.UnitId,
			do_h.CustCode,
			SUM(cte.Qty) AS Qty
		FROM cte_union cte
		LEFT JOIN Sales.SalesDeliveryHeader do_h
			ON do_h.Code = cte.DOCode
		GROUP BY cte.Code, cte.[Date], cte.VehicleId, cte.DriverId, cte.WarehouseCode, cte.TotalVolume, cte.TotalWeight,
			cte.Notes, cte.CreatedBy, cte.TransCode,
			cte.ItemId, cte.UnitId,
			do_h.CustCode
	)
	SELECT cte.*,
		v.VehicleNo,
		e_d.Initial AS DriverInitial, e_d.FirstName AS DriverFirstName, e_d.LastName AS DriverLastName,
		w.Initial AS WarehouseInitial, w.[Name] AS WarehouseName,
		e_c.Initial AS CreatedInitial,
		c.[Name] AS CustName,
		ca_d.Address1 AS CustAddress1, ca_d.ContactPerson AS CustContactPerson, ca_d.Phone AS CustPhone,
		i.Initial AS ItemInitial, i.[Name] AS ItemName,
		uom_c.UnitToConvert AS ItemUnitName
	FROM cte_calc_group cte
	LEFT JOIN General.Vehicle v
		ON v.Id = cte.VehicleId
	LEFT JOIN General.Employee e_d
		ON e_d.Id = cte.DriverId
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = cte.WarehouseCode
	LEFT JOIN General.Employee e_c
		ON e_c.Id = cte.CreatedBy
	LEFT JOIN General.Customer c
		ON c.Code = cte.CustCode
	LEFT JOIN General.CustomerAddress ca_d
		ON ca_d.Code = cte.CustCode
		AND ca_d.IsDefault = 1
	LEFT JOIN Inventory.Item i
		ON i.Id = cte.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = cte.UnitId
	ORDER BY cte.TransCode, i.Initial, i.[Name], uom_c.UnitToConvert

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_cb_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_cb_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT cb_h.Code, cb_h.VouCode, cb_h.[Type], cb_h.[Date], cb_h.CurrCode, ABS(cb_h.Amount) AS Amount, cb_h.ChequeNo, cb_h.Notes,
		CONCAT(cb_h.CoaCode, ' - ', c_1.[Name]) AS FullCoaName,
		CASE c_1.CBType
			WHEN 'C' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN KAS'
					WHEN 'C' THEN 'BUKTI PENGELUARAN KAS'
				END
			WHEN 'B' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN BANK'
					WHEN 'C' THEN 'BUKTI PENGELUARAN BANK'
				END
		END AS TitleCaption,
		cb_d.TransCode AS DetailTransCode, cb_d.Notes AS DetailNotes,
		CASE cb_h.[Type]
			WHEN 'C' THEN
				CASE cb_d.TypeAmount
					WHEN 'D' THEN cb_d.Amount
					WHEN 'C' THEN -cb_d.Amount
				END
			ELSE
				CASE cb_d.TypeAmount
					WHEN 'D' THEN -cb_d.Amount
					WHEN 'C' THEN cb_d.Amount
				END
		END AS DetailAmount,
		CONCAT(cb_d.CoaCode, ' - ', c_2.[Name]) AS DetailFullCoaName,
		e.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Finance.GeneralCashBankHeader
		WHERE Code = @code
		AND Mark <> 'V'
	) cb_h
	LEFT JOIN Finance.GeneralCashBankDetail cb_d
		ON cb_d.Code = cb_h.Code
	LEFT JOIN Accounting.COA c_1
		ON c_1.Code = cb_h.CoaCode
	LEFT JOIN Accounting.COA c_2
		ON c_2.Code = cb_d.CoaCode
	LEFT JOIN General.Employee e
		ON e.Id = cb_h.CreatedBy
	
END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_do_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_do_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER'
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT do_h.*,
			e_sls.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.ShippingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson,
			e_shp.Initial AS ShippedInitial
		FROM (
			SELECT *
			FROM Sales.SalesDeliveryHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) do_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = do_h.TransCode
			AND do_h.SrcTrans = 1
		LEFT JOIN General.Employee e_sls
			ON e_sls.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = do_h.CustCode
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = c.Code
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = c.Code
			AND ca_b.Id = c.ShippingAddressId
			AND c.BillingAddressId IS NOT NULL
		LEFT JOIN General.Employee e_shp
			ON e_shp.Id = do_h.ShippedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT do_d.Id, do_d.[LineNo], do_d.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, [LineNo], ItemId, Qty, UnitId
			FROM Sales.SalesDeliveryDetail
			WHERE Code = @code
		) do_d
		LEFT JOIN Inventory.Item i
			ON i.Id = do_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = do_d.UnitId
		UNION ALL
		SELECT do_d_fg.Id, do_d_fg.[LineNo], do_d_fg.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty
			FROM Sales.SalesDeliveryDetailFreeGood
			WHERE Code = @code
		) do_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = do_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = do_d_fg.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_ei_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_ei_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT ei_h.*,
			dbo.udf_num_to_words_id(ei_h.Amount, @centToWord) AS AmountInWord,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Expedition.ExpeditionInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) ei_h
		LEFT JOIN General.Supplier s
			ON s.Code = ei_h.SupCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = ei_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT ei_d.Code, ei_d.TransCode,
			CASE ei_h.SrcTrans
				WHEN 1 THEN rcv_h.[Date]
				WHEN 2 THEN do_h.[Date] END AS TransDate
		FROM (
			SELECT Code, TransCode
			FROM Expedition.ExpeditionInvoiceDetail
			WHERE Code = @code
		) ei_d
		LEFT JOIN Expedition.ExpeditionInvoiceHeader ei_h
			ON ei_h.Code = ei_d.Code
		LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
			ON rcv_h.Code = ei_d.TransCode
			AND ei_h.SrcTrans = 1
		LEFT JOIN Sales.SalesDeliveryHeader do_h
			ON do_h.Code = ei_d.TransCode
			AND ei_h.SrcTrans = 2
		ORDER BY ei_d.Code, ei_d.TransCode
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_pi_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_pi_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT pi_h.*,
			dbo.udf_num_to_words_id(pi_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_i.Initial AS IssuedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) pi_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = pi_h.POCode
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = pi_h.SupCode
		LEFT JOIN General.Employee e_i
			ON e_i.Id = pi_h.IssuedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pi_d.Id, pi_d.Code, pi_d.RcvCode,
			rcv_d.Id AS RcvDetailId, rcv_d.[LineNo] AS RcvDetailLineNo,
			rcv_d.Qty, rcv_d.UnitPrice, rcv_d.Disc,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, RcvCode
			FROM Purchasing.PurchaseInvoiceDetail
			WHERE Code = @code
		) pi_d
		LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
			ON rcv_h.Code = pi_d.RcvCode
		LEFT JOIN Purchasing.PurchaseReceiveDetail rcv_d
			ON rcv_d.Code = rcv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = rcv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = rcv_d.UnitId
		ORDER BY Sort, Id, RcvCode, RcvDetailId, RcvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_po_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_po_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT po_h.*,
			dbo.udf_num_to_words_id(po_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseOrderHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) po_h
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = po_h.SupCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = po_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT po_d.Id, po_d.[LineNo],
			po_d.Qty, po_d.UnitPrice, po_d.Disc,
			CASE WHEN po_d.[Type] = 0 THEN po_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN po_d.[Type] = 0 THEN po_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			po_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, TaxAmount, Total, [Type]
			FROM Purchasing.PurchaseOrderDetail
			WHERE Code = @code
		) po_d
		LEFT JOIN Inventory.Item i
			ON i.Id = po_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = po_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_pr_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_pr_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT pr_h.*,
			dbo.udf_num_to_words_id(pr_h.Total, @centToWord) AS TotalInWord,
			e_shp.Initial AS ShippedInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseReturnHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) pr_h
		LEFT JOIN General.Employee e_shp
			ON e_shp.Id = pr_h.ShippedBy
		LEFT JOIN General.Supplier s
			ON s.Code = pr_h.SupCode
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = pr_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pr_d.Id, pr_d.[LineNo],
			pr_d.Qty, pr_d.UnitPrice, pr_d.TaxAmount, pr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Purchasing.PurchaseReturnDetail
			WHERE Code = @code
		) pr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = pr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = pr_d.UnitId
		UNION ALL
		SELECT pr_d.Id, pr_d.[LineNo],
			pr_d.Qty, pr_d.UnitPrice, pr_d.TaxAmount, pr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Purchasing.PurchaseReturnDetailExchDiffItem
			WHERE Code = @code
		) pr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = pr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = pr_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

	ELSE IF @displayType = 'SUM-DETAIL'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Purchasing.PurchaseReturnDetail
		WHERE Code = @code
	END

	ELSE IF @displayType = 'SUM-EXCH'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Purchasing.PurchaseReturnDetailExchDiffItem
		WHERE Code = @code
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_rcv_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_rcv_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER'
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT rcv_h.*,
			e_req.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_rcv.Initial AS ReceiveInitial,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseReceiveHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) rcv_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = rcv_h.TransCode
			AND rcv_h.SrcTrans = 1
		LEFT JOIN General.Employee e_req
			ON e_req.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = rcv_h.SupCode
		LEFT JOIN General.Employee e_rcv
			ON e_rcv.Id = rcv_h.ReceiveBy
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = rcv_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT rcv_d.Id, rcv_d.[LineNo], rcv_d.Qty,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id, [LineNo], ItemId, Qty, UnitId, [Type]
			FROM Purchasing.PurchaseReceiveDetail
			WHERE Code = @code
		) rcv_d
		LEFT JOIN Inventory.Item i
			ON i.Id = rcv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = rcv_d.UnitId
		
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_si_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_si_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT si_h.*,
			dbo.udf_num_to_words_id(si_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @code
			AND Mark <> 'V'
		) si_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = si_h.SOCode
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = si_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = si_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = si_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.Code, si_d.DOCode,
			dlv_d.Id AS DlvDetailId, dlv_d.[LineNo] AS DlvDetailLineNo,
			dlv_d.Qty, dlv_d.UnitPrice, dlv_d.Disc, dlv_d.TaxAmount, dlv_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, DOCode
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Sales.SalesDeliveryHeader dlv_h
			ON dlv_h.Code = si_d.DOCode
		LEFT JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = dlv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d.UnitId
		UNION ALL
		SELECT 0, '', dlv_d_fg.Code,
			dlv_d_fg.DlvOrderDetailId, dlv_d_fg.[LineNo],
			dlv_d_fg.Qty, dlv_d_fg.UnitPrice, dlv_d_fg.UnitPrice, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesDeliveryDetailFreeGood
			WHERE EXISTS (
				SELECT Id,Code,DOCode
				FROM Sales.SalesInvoiceDetail
				WHERE Code = @code
				AND DOCode = Code
			)
		) dlv_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d_fg.UnitId
		ORDER BY Sort, Id, DOCode, DlvDetailId, DlvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_so_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_so_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT so_h.*,
			dbo.udf_num_to_words_id(so_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesOrderHeader 
			WHERE Code = @code
			AND Mark <> 'V'
		) so_h
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = so_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = so_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = so_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.[LineNo],
			si_d.Qty, si_d.UnitPrice, si_d.Disc, si_d.TaxAmount, si_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, TaxAmount, Total
			FROM Sales.SalesOrderDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Inventory.Item i
			ON i.Id = si_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = si_d.UnitId
		UNION ALL
		SELECT so_d_fg.Id, so_d_fg.[LineNo],
			so_d_fg.Qty, so_d_fg.UnitPrice, so_d_fg.UnitPrice, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesOrderDetailFreeGood
			WHERE Code = @code
		) so_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = so_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = so_d_fg.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_sr_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_sr_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT sr_h.*,
			dbo.udf_num_to_words_id(sr_h.Total, @centToWord) AS TotalInWord,
			e_sls.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN c.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson,
			w.Initial AS WarehouseInitial,
			e_cre.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Sales.SalesReturnHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) sr_h
		LEFT JOIN General.Employee e_sls
			ON e_sls.Id = sr_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = sr_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = sr_h.CustCode
			AND ca_b.Id = c.BillingAddressId
			AND c.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = sr_h.CustCode
			AND ca_d.IsDefault = 1
			AND c.BillingAddressId IS NULL
		LEFT JOIN Inventory.Warehouse w
			ON w.Code = sr_h.WarehouseCode
		LEFT JOIN General.Employee e_cre
			ON e_cre.Id = sr_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT sr_d.Id, sr_d.[LineNo],
			sr_d.Qty, sr_d.UnitPrice, sr_d.TaxAmount, sr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Sales.SalesReturnDetail
			WHERE Code = @code
		) sr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = sr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = sr_d.UnitId
		UNION ALL
		SELECT sr_d.Id, sr_d.[LineNo],
			sr_d.Qty, sr_d.UnitPrice, sr_d.TaxAmount, sr_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, TaxAmount, Total
			FROM Sales.SalesReturnDetailExchDiffItem
			WHERE Code = @code
		) sr_d
		LEFT JOIN Inventory.Item i
			ON i.Id = sr_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = sr_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

	ELSE IF @displayType = 'SUM-DETAIL'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Sales.SalesReturnDetail
		WHERE Code = @code
	END

	ELSE IF @displayType = 'SUM-EXCH'
	BEGIN
		SELECT SUM(Total) AS Total
		FROM Sales.SalesReturnDetailExchDiffItem
		WHERE Code = @code
	END

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_insert_curr_rate
            sql = @"CREATE PROCEDURE [dbo].[sp_insert_curr_rate]
	@dateFrom date,
	@dateTo date,
	@currCode varchar(3),
	@amount decimal(19, 6),
	@createdBy int
AS
BEGIN
	
	BEGIN TRY
		BEGIN TRANSACTION

			;WITH cte_rate_src AS (
				SELECT DATEADD(DAY, nbr - 1, @dateFrom) AS [date], @currCode AS currCode, @amount AS amount
				FROM (
					SELECT ROW_NUMBER() OVER ( ORDER BY c.object_id ) AS nbr
					FROM sys.columns c
				) nbrs
				WHERE nbr - 1 <= DATEDIFF(DAY, @dateFrom, @dateTo)
			)
	
			MERGE Accounting.CurrencyRate AS T
			USING cte_rate_src AS S
			ON (T.[Date] = S.[date] AND T.CurrCode = S.currCode)
			WHEN MATCHED THEN
				UPDATE SET T.Amount = S.amount,
						   T.UpdatedBy = @createdBy,
						   T.UpdatedDate = dbo.udf_current_local_time()
			WHEN NOT MATCHED BY TARGET THEN
				INSERT ([Date], CurrCode, Amount, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
				VALUES (S.[date], S.currCode, S.amount, @createdBy, dbo.udf_current_local_time(), @createdBy, dbo.udf_current_local_time());

		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		EXEC dbo.sp_raiseerror
	END CATCH

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_ts_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_ts_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT ts_h.Code, ts_h.[Date], ts_h.Notes,
		CASE ts_h.IsConsignee
			WHEN 1 THEN
				CASE ts_h.[Type]
					WHEN 'C' THEN 'Titipan Barang'
					WHEN 'RC' THEN 'Retur Titipan Barang'
				END
			ELSE
				CASE ts_h.[Type]
					WHEN 'DT' THEN 'Transfer Persediaan (Transfer Langsung)'
					WHEN 'IN' THEN 'Transfer Persediaan (Barang Masuk)'
					WHEN 'OUT' THEN 'Transfer Persediaan (Barang Keluar)'
				END
		END AS TitleCaption,
		ts_d.Qty,
		i.Initial AS ItemInitial, i.[Name] AS ItemName,
		uom_c.UnitToConvert AS ItemUnitName,
		w_f.Initial AS WarehouseFromInitial, w_f.[Name] AS WarehouseFromName,
		w_t.Initial AS WarehouseToInitial, w_t.[Name] AS WarehouseToName,
		e.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Inventory.TransferStockHeader
		WHERE Code = @code
		AND Mark <> 'V'
	) ts_h
	LEFT JOIN Inventory.TransferStockDetail ts_d
		ON ts_d.Code = ts_h.Code
	LEFT JOIN Inventory.Item i
		ON i.Id = ts_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = ts_d.UnitId
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts_h.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts_h.WarehouseCodeTo
	LEFT JOIN General.Employee e
		ON e.Id = ts_h.CreatedBy
	WHERE ts_d.ItemId IS NOT NULL
	ORDER BY i.Initial
	
END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_get_vo_cust_print_data
            sql = @"CREATE PROCEDURE [dbo].[sp_get_vo_cust_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT vo.Code, vo.[Date], vo.Notes,
		vo_c.CustCode, c.Initial AS CustInitial, c.[Name] AS CustName,
		ca_d.Address1 AS CustAddress1, ca_d.ContactPerson AS CustContactPerson,
		e_s.Initial AS SalesInitial, e_s.FirstName AS SalesFirstName, e_s.LastName AS SalesLastName,
		e_c.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Sales.VisitOrder
		WHERE Code = @code
		AND Mark <> 'V'
	) vo
	LEFT JOIN Sales.VisitOrderCustomer vo_c
		ON vo_c.Code = vo.Code
	LEFT JOIN General.Customer c
		ON c.Code = vo_c.CustCode
	LEFT JOIN General.CustomerAddress ca_d
		ON ca_d.Code = c.Code
		and ca_d.IsDefault = 1
	LEFT JOIN General.Employee e_s
		ON e_s.Id = vo.SalesmanId
	LEFT JOIN General.Employee e_c
		ON e_c.Id = vo.CreatedBy
	
END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_restore_cash_bank_transaction
            sql = @"CREATE PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
	@code varchar(17)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT d.Id, d.TransCode as TransCode, d.[Type], d.TransAmount, d.Src
	INTO #tmp_cb
	FROM Finance.GeneralCashBankHeader h
	JOIN Finance.GeneralCashBankDetail d ON h.Code = d.Code
	WHERE h.Code = @code

	-- Restore transaction
	DECLARE @id int
	DECLARE @type varchar(5), @transCode varchar(17), @src varchar(5)
	DECLARE @transAmount decimal(18, 2)
	
	DECLARE @idDetail int
	DECLARE @totalDetail Decimal(18, 2)
	DECLARE @totalHeader Decimal(18, 2)
	DECLARE @paidAmount Decimal(18, 2)
	DECLARE @proRate Decimal(18, 2)
	Declare @mark varchar(3)
	DECLARE @used Decimal(18, 2)

	WHILE EXISTS(SELECT * FROM #tmp_cb)
	BEGIN
		
		SELECT TOP 1 @id = Id, @type = [Type], @transCode = TransCode, @transAmount = TransAmount, @src = Src
		FROM #tmp_cb
		
		IF (@type = 'AR' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAR 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AR')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'
			
			UPDATE Sales.SalesInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, DoCode
			INTO #tmp_si
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @transCode

			DECLARE @doCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_si)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @doCode = DoCode FROM #tmp_si
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Sales.SalesDeliveryHeader SET PaidAmount = @proRate WHERE Code = @doCode 
					
				DELETE FROM #tmp_si
			END

			DROP TABLE #tmp_si

		END
		ELSE IF (@type = 'AP' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAP 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AP')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Purchasing.PurchaseInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'

			UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, RcvCode
			INTO #tmp_pi
			FROM Purchasing.PurchaseInvoiceDetail 
			WHERE Code = @transCode
				
			DECLARE @rcvCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_pi)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @rcvCode = RcvCode FROM #tmp_pi
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount = @proRate WHERE Code = @rcvCode 
					
				DELETE FROM #tmp_pi
			END

			DROP TABLE #tmp_pi

		END
		ELSE IF (@type = 'EPAP')
		BEGIN

			SELECT @totalHeader = Amount, @paidAmount = PaidAmount - @transAmount   
			FROM Expedition.ExpeditionInvoiceHeader 
			WHERE Code = @transCode

			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'

			UPDATE Expedition.ExpeditionInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode;

		END
		ELSE IF (@type = 'DPC')
		BEGIN

			UPDATE Sales.CreditMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'DPS')
		BEGIN

			UPDATE Purchasing.DebitMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF ((@type = 'RDPC' OR @type = 'SR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceCreditMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPC' OR @type = 'SR')
		BEGIN

			SET @used = ((SELECT Used FROM Sales.CreditMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Sales.CreditMemo SET Used = @used, Mark=@mark WHERE Code = @transCode

		END
		ELSE IF ((@type = 'RDPS' OR @type = 'PR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceDebitMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPS' OR @type = 'PR')
		BEGIN

			SET @used = ((SELECT Used FROM Purchasing.DebitMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Purchasing.DebitMemo SET Used = @used, Mark = 'A' WHERE Code = @transCode

		END

		DELETE #tmp_cb WHERE Id = @id

	END

	DROP TABLE #tmp_cb

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_restore_stock_mutation_from_adj
            sql = @"CREATE PROCEDURE [dbo].[sp_restore_stock_mutation_from_adj]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE Inventory.StockMutation Where Id = @Id
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_restore_stock_mutation_from_bb
            sql = @"CREATE PROCEDURE [dbo].[sp_restore_stock_mutation_from_bb]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE FROM Inventory.StockMutation WHERE Id = @Id
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_currency_sort
            sql = @"CREATE PROCEDURE [dbo].[sp_update_currency_sort]
	@beforeAfter int
AS
BEGIN

	DECLARE @orderBy varchar(5)
	DECLARE @query varchar(max) = ''

	--1 = after, 2 = before
	IF (@beforeAfter = 1)
	BEGIN
		SELECT @orderBy = 'asc'
	END
	ELSE
	BEGIN 
		SELECT @orderBy = 'desc'
	END

	EXECUTE('UPDATE c SET c.Sort = a.Num FROM (SELECT ROW_NUMBER() OVER(ORDER BY Sort ASC, UpdatedDate '+@orderBy+') AS Num, Code FROM General.Currency) a INNER JOIN General.Currency c ON a.Code = c.Code WHERE a.Num <> c.Sort')

END";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_po_rcv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_po_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @rcvQty decimal(18, 2)

	-- Get purchase order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_po
	FROM Purchasing.PurchaseOrderDetail po_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseOrderHeader po_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND po_h.Code = po_d.Code
	)
	AND [Type] = 0
	
	-- Return if purchase order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_po)
	BEGIN
		DROP TABLE #tmp_po
		RETURN
	END
	
	-- Get purchase receive data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseReceiveHeader pr_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND pr_h.Code = pr_d.Code
	)
	AND [Type] = 0
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT po.Code, po.ItemId, po.UnitId, po.Qty,
		ISNULL(pr.QtyRcv, 0) AS QtyRcv
	INTO #tmp_all
	FROM #tmp_po po
	LEFT JOIN #tmp_pr pr
		ON pr.ItemId = po.ItemId
		AND pr.UnitId = po.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_pr

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@rcvQty = SUM(QtyRcv)
	FROM #tmp_all

	-- Update PO header mark
	UPDATE Purchasing.PurchaseOrderHeader
	SET Mark = (
		CASE WHEN @rcvQty = 0 THEN 'A'
			WHEN @rcvQty >= @orderQty THEN 'CMP'
			ELSE 'PR' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update PO detail receive qty item
	UPDATE po_d
	SET po_d.QtyRcv = #tmp_all.QtyRcv
	FROM Purchasing.PurchaseOrderDetail po_d, #tmp_all
	WHERE po_d.Code = #tmp_all.Code
	AND po_d.ItemId = #tmp_all.ItemId
	AND po_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_pr_rcv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_pr_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @rtnQty decimal(18, 2), @rcvQty decimal(18, 2), @type int
	
	SELECT @type = [Type] FROM Purchasing.PurchaseReturnHeader rt_h WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

	-- Get purchase return data
	IF (@type = 2)
	BEGIN
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rt
		FROM Purchasing.PurchaseReturnDetail rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if purchase return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rt)
		BEGIN
			DROP TABLE #tmp_rt
			RETURN
		END
	
		-- Get purchase receive data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
		INTO #tmp_pr
		FROM Purchasing.PurchaseReceiveDetail pr_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReceiveHeader pr_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND pr_h.Code = pr_d.Code
		)
		AND [Type] = 0
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
			ISNULL(pr.QtyRcv, 0) AS QtyRcv
		INTO #tmp_all
		FROM #tmp_rt rt
		LEFT JOIN #tmp_pr pr
			ON pr.ItemId = rt.ItemId
			AND pr.UnitId = rt.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rt
		DROP TABLE #tmp_pr

		-- Calculate sum
		SELECT @rtnQty = SUM(Qty),
			@rcvQty = SUM(QtyRcv)
		FROM #tmp_all

		-- Update rt header mark
		UPDATE Purchasing.PurchaseReturnHeader
		SET Mark = (
			CASE WHEN @rcvQty = 0 THEN 'A'
				WHEN @rcvQty >= @rtnQty THEN 'CMP'
				ELSE 'PR' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail receive qty item
		UPDATE rt_d
		SET rt_d.QtyRcv = #tmp_all.QtyRcv
		FROM Purchasing.PurchaseReturnDetail rt_d, #tmp_all
		WHERE rt_d.Code = #tmp_all.Code
		AND rt_d.ItemId = #tmp_all.ItemId
		AND rt_d.UnitId = #tmp_all.UnitId

		-- Drop temp table
		DROP TABLE #tmp_all
	END
	ELSE IF (@type = 3)
	BEGIN
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rtx
		FROM Purchasing.PurchaseReturnDetailExchDiffItem rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if purchase return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rtx)
		BEGIN
			DROP TABLE #tmp_rtx
			RETURN
		END
	
		-- Get purchase receive data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
		INTO #tmp_prx
		FROM Purchasing.PurchaseReceiveDetail pr_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReceiveHeader pr_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND pr_h.Code = pr_d.Code
		)
		AND [Type] = 0
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rtx.Code, rtx.ItemId, rtx.UnitId, rtx.Qty,
			ISNULL(prx.QtyRcv, 0) AS QtyRcv
		INTO #tmp_allx
		FROM #tmp_rtx rtx
		LEFT JOIN #tmp_prx prx
			ON prx.ItemId = rtx.ItemId
			AND prx.UnitId = rtx.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rtx
		DROP TABLE #tmp_prx

		-- Calculate sum
		SELECT @rtnQty = SUM(Qty),
			@rcvQty = SUM(QtyRcv)
		FROM #tmp_allx

		-- Update rt header mark
		UPDATE Purchasing.PurchaseReturnHeader
		SET Mark = (
			CASE WHEN @rcvQty = 0 THEN 'A'
				WHEN @rcvQty >= @rtnQty THEN 'CMP'
				ELSE 'PR' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail receive qty item
		UPDATE rt_d
		SET rt_d.QtyRcv = #tmp_allx.QtyRcv
		FROM Purchasing.PurchaseReturnDetailExchDiffItem rt_d, #tmp_allx
		WHERE rt_d.Code = #tmp_allx.Code
		AND rt_d.ItemId = #tmp_allx.ItemId
		AND rt_d.UnitId = #tmp_allx.UnitId

		-- Drop temp table
		DROP TABLE #tmp_allx
	END
	

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_rt') IS NOT NULL
		DROP TABLE #tmp_rt
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_so_dlv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_so_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @dlvQty decimal(18, 2)

	-- Get sales order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_so
	FROM Sales.SalesOrderDetail so_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesOrderHeader so_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND so_h.Code = so_d.Code
	)
	
	-- Return if sales order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_so)
	BEGIN
		DROP TABLE #tmp_so
		RETURN
	END
	
	-- Get sales delivery data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail do_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesDeliveryHeader do_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND do_h.Code = do_d.Code
	)
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT so.Code, so.ItemId, so.UnitId, so.Qty,
		ISNULL(do.QtyDlv, 0) AS QtyDlv
	INTO #tmp_all
	FROM #tmp_so so
	LEFT JOIN #tmp_do do
		ON do.ItemId = so.ItemId
		AND do.UnitId = so.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_do

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@dlvQty = SUM(QtyDlv)
	FROM #tmp_all

	-- Update SO header mark
	UPDATE Sales.SalesOrderHeader
	SET Mark = (
		CASE WHEN @dlvQty = 0 THEN 'A'
			WHEN @dlvQty >= @orderQty THEN 'CMP'
			ELSE 'PS' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update SO detail delivery qty item
	UPDATE so_d
	SET so_d.QtyDlv = #tmp_all.QtyDlv
	FROM Sales.SalesOrderDetail so_d, #tmp_all
	WHERE so_d.Code = #tmp_all.Code
	AND so_d.ItemId = #tmp_all.ItemId
	AND so_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_so_free_dlv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_so_free_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @dlvQty decimal(18, 2)

	-- Get sales order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_so
	FROM Sales.SalesOrderDetailFreeGood so_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesOrderHeader so_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND so_h.Code = so_d.Code
	)
	
	-- Return if sales order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_so)
	BEGIN
		DROP TABLE #tmp_so
		RETURN
	END
	
	-- Get sales delivery data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetailFreeGood do_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesDeliveryHeader do_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND do_h.Code = do_d.Code
	)
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT so.Code, so.ItemId, so.UnitId, so.Qty,
		ISNULL(do.QtyDlv, 0) AS QtyDlv
	INTO #tmp_all
	FROM #tmp_so so
	LEFT JOIN #tmp_do do
		ON do.ItemId = so.ItemId
		AND do.UnitId = so.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_do

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@dlvQty = SUM(QtyDlv)
	FROM #tmp_all

	-- Update SO header mark
	UPDATE Sales.SalesOrderHeader
	SET Mark = (
		CASE WHEN @dlvQty = 0 THEN 'A'
			WHEN @dlvQty >= @orderQty THEN 'CMP'
			ELSE 'PS' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update SO detail delivery qty item
	UPDATE so_d
	SET so_d.QtyClosed = #tmp_all.QtyDlv
	FROM Sales.SalesOrderDetailFreeGood so_d, #tmp_all
	WHERE so_d.Code = #tmp_all.Code
	AND so_d.ItemId = #tmp_all.ItemId
	AND so_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_sr_dlv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_sr_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @returnQty decimal(18, 2), @dlvQty decimal(18, 2), @type int
	
	SELECT @type = [Type] FROM Sales.SalesReturnHeader rt_h WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
	IF (@type = 2)
	BEGIN
		-- Get sales return data
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rt
		FROM Sales.SalesReturnDetail rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if sales return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rt)
		BEGIN
			DROP TABLE #tmp_rt
			RETURN
		END
	
		-- Get sales delivery data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
		INTO #tmp_do
		FROM Sales.SalesDeliveryDetail do_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesDeliveryHeader do_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND do_h.Code = do_d.Code
		)
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
			ISNULL(do.QtyDlv, 0) AS QtyDlv
		INTO #tmp_all
		FROM #tmp_rt rt
		LEFT JOIN #tmp_do do
			ON do.ItemId = rt.ItemId
			AND do.UnitId = rt.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rt
		DROP TABLE #tmp_do

		-- Calculate sum
		SELECT @returnQty = SUM(Qty),
			@dlvQty = SUM(QtyDlv)
		FROM #tmp_all

		-- Update rt header mark
		UPDATE Sales.SalesReturnHeader
		SET Mark = (
			CASE WHEN @dlvQty = 0 THEN 'A'
				WHEN @dlvQty >= @returnQty THEN 'CMP'
				ELSE 'PS' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail delivery qty item
		UPDATE rt_d
		SET rt_d.QtyDlv = #tmp_all.QtyDlv
		FROM Sales.SalesReturnDetail rt_d, #tmp_all
		WHERE rt_d.Code = #tmp_all.Code
		AND rt_d.ItemId = #tmp_all.ItemId
		AND rt_d.UnitId = #tmp_all.UnitId

		-- Drop temp table
		DROP TABLE #tmp_all
	END
	ELSE IF (@type = 3)
	BEGIN
		-- Get sales return data
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rtx
		FROM Sales.SalesReturnDetailExchDiffItem rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if sales return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rtx)
		BEGIN
			DROP TABLE #tmp_rtx
			RETURN
		END
	
		-- Get sales delivery data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
		INTO #tmp_dox
		FROM Sales.SalesDeliveryDetail do_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesDeliveryHeader do_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND do_h.Code = do_d.Code
		)
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rtx.Code, rtx.ItemId, rtx.UnitId, rtx.Qty,
			ISNULL(dox.QtyDlv, 0) AS QtyDlv
		INTO #tmp_allx
		FROM #tmp_rtx rtx
		LEFT JOIN #tmp_dox dox
			ON dox.ItemId = rtx.ItemId
			AND dox.UnitId = rtx.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rtx
		DROP TABLE #tmp_dox

		-- Calculate sum
		SELECT @returnQty = SUM(Qty),
			@dlvQty = SUM(QtyDlv)
		FROM #tmp_allx

		-- Update rt header mark
		UPDATE Sales.SalesReturnHeader
		SET Mark = (
			CASE WHEN @dlvQty = 0 THEN 'A'
				WHEN @dlvQty >= @returnQty THEN 'CMP'
				ELSE 'PS' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail delivery qty item
		UPDATE rt_d
		SET rt_d.QtyDlv = #tmp_allx.QtyDlv
		FROM Sales.SalesReturnDetailExchDiffItem rt_d, #tmp_allx
		WHERE rt_d.Code = #tmp_allx.Code
		AND rt_d.ItemId = #tmp_allx.ItemId
		AND rt_d.UnitId = #tmp_allx.UnitId

		-- Drop temp table
		DROP TABLE #tmp_allx
	END

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_rt') IS NOT NULL
		DROP TABLE #tmp_rt
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_adj
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	SELECT d.*, h.WarehouseCode
	INTO #tmp_adj
	FROM Inventory.AdjustmentDetail d
	JOIN Inventory.AdjustmentHeader h on d.code = h.code
	WHERE d.Code = @code 

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk

	-- Delete stock mutation that doesn't have in adjustment item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'ADJ'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_adj
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in adjustment item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_adj.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_adj.ItemId,
		sm.UomId = #tmp_adj.UomId,
		sm.UnitId = #tmp_adj.UnitId,
		sm.Qty = #tmp_adj.QtyAdjust,
		sm.RefCode2 = NULL
	FROM Inventory.StockMutation sm, #tmp_adj
	WHERE sm.RefCode1 = #tmp_adj.Code
	AND sm.RefDetailId1 = #tmp_adj.Id
	AND sm.Src = 'ADJ'

	-- Insert stock mutation that doesn't have with adjustment item detail
	INSERT INTO Inventory.StockMutation
		SELECT 
			WarehouseCode as WarehouseCode, 
			@date as [Date], 
			ItemId as ItemId, 
			UomId as UomId, 
			UnitId as UnitId, 
			QtyAdjust as Qty,
			0 as NettPrice,
			UnitId as BaseUnit, --test
			0 as BaseQty,
			0 as BaseNettPrice,
			Code as RefCode1, 
			Id as RefDetailId,
			NULL as RefCode,
			'OH' as [Type],
			'ADJ' as Src
		FROM #tmp_adj adj
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = adj.Code
			AND sm.RefDetailId1 = adj.Id
			AND sm.Src = 'ADJ'
		) And adj.QtyAdjust <> 0
	-- Drop temp tables
	DROP TABLE #tmp_adj
	
	DECLARE @IsBaseUnit int
	DECLARE @BaseUnitQty int
	DECLARE @Seq int

	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id			
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation 
			SET BaseQty = ROUND(Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq), 0),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END
		
		-- update new warehouse quantity
		IF EXISTS (SELECT ID FROM Inventory.WarehouseQuantity WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode)
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
			END
		ELSE 
		BEGIN
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
				INSERT INTO Inventory.WarehouseQuantity (WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WarehouseCode, @ItemId, @BaseQty, 0, 0, 0, 0, dbo.udf_current_local_time())
			END
		END
		
		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_adj') IS NOT NULL
		DROP TABLE #tmp_adj
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_bb
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_bb]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	SELECT d.*, h.WarehouseCode
	INTO #tmp_bb
	FROM Inventory.BeginningBalanceDetail d
	JOIN Inventory.BeginningBalanceHeader h on d.code = h.code
	WHERE d.Code = @code 

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk

	-- Delete stock mutation that doesn't have in bb item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'BB'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_bb
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in bb item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_bb.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_bb.ItemId,
		sm.UomId = #tmp_bb.UomId,
		sm.UnitId = #tmp_bb.UnitId,
		sm.Qty = #tmp_bb.Qty,
		sm.NettPrice = #tmp_bb.UnitPrice,
		sm.RefCode2 = NULL
	FROM Inventory.StockMutation sm, #tmp_bb
	WHERE sm.RefCode1 = #tmp_bb.Code
	AND sm.RefDetailId1 = #tmp_bb.Id
	AND sm.Src = 'BB'

	-- Insert stock mutation that doesn't have with bb item detail
	INSERT INTO Inventory.StockMutation
		SELECT 
			WarehouseCode as WarehouseCode, 
			@date as [Date], 
			ItemId as ItemId, 
			UomId as UomId, 
			UnitId as UnitId, 
			Qty as Qty,
			UnitPrice as NettPrice,
			UnitId as BaseUnit, 
			0 as BaseQty,
			0 as BaseNettPrice,
			Code as RefCode1, 
			Id as RefDetailId,
			NULL as RefCode,
			'OH' as [Type],
			'BB' as Src
		FROM #tmp_bb bb
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = bb.Code
			AND sm.RefDetailId1 = bb.Id
			AND sm.Src = 'BB'
		) And bb.Qty <> 0
	-- Drop temp tables
	DROP TABLE #tmp_bb
	
	DECLARE @IsBaseUnit int
	DECLARE @BaseUnitQty int
	DECLARE @Seq int

	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId,BaseNettPrice = NettPrice WHERE Id = @Id			
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation 
			SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseNettPrice = NettPrice / (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END
		
		-- update new warehouse quantity
		IF EXISTS (SELECT ID FROM Inventory.WarehouseQuantity WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode)
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
			END
		ELSE 
		BEGIN
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
				INSERT INTO Inventory.WarehouseQuantity (WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WarehouseCode, @ItemId, @BaseQty, 0, 0, 0, 0, dbo.udf_current_local_time())
			END
		END
		
		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_bb') IS NOT NULL
		DROP TABLE #tmp_bb
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_do
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
AS
BEGIN TRY

	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesDeliveryHeader
	WHERE Code = @code

	SELECT do_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.Qty
			ELSE do_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = do_d.UomId
		AND uom_c.Id = do_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DO'

	-- Delete stock mutation that doesn't have in sales delivery item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_do
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales delivery item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_do.ItemId,
		sm.UomId = #tmp_do.UomId,
		sm.UnitId = #tmp_do.UnitId,
		sm.Qty = #tmp_do.Qty,
		sm.BaseUnit = #tmp_do.BaseUnit,
		sm.BaseQty = #tmp_do.BaseQty,
		sm.RefCode2 = @transCode
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)
	
	-- Free Goods
	SELECT do_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.Qty
			ELSE do_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_do_free
	FROM Sales.SalesDeliveryDetailFreeGood do_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = do_d.UomId
		AND uom_c.Id = do_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm_free
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DOF'

	-- Delete stock mutation that doesn't have in sales delivery free item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DOF'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_do_free
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales delivery free item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_do_free.ItemId,
		sm.UomId = #tmp_do_free.UomId,
		sm.UnitId = #tmp_do_free.UnitId,
		sm.Qty = #tmp_do_free.Qty,
		sm.BaseUnit = #tmp_do_free.BaseUnit,
		sm.BaseQty = #tmp_do_free.BaseQty,
		sm.RefCode2 = @transCode
	FROM Inventory.StockMutation sm, #tmp_do_free
	WHERE sm.RefCode1 = #tmp_do_free.Code
	AND sm.RefDetailId1 = #tmp_do_free.Id
	AND sm.Src = 'DOF'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DOF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DOF'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)
	DECLARE @srcTrans int

	SELECT @srcTrans = SrcTrans FROM Sales.SalesDeliveryHeader WHERE Code = @code

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF EXISTS(SELECT *FROM #tmp_ori_sm_free)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm_free)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm_free

			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm_free WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WHId, @ItemId, -@Qty, 0, -@Qty, 0, 0, dbo.udf_current_local_time())
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_do
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm
	DROP TABLE #tmp_do_free
	DROP TABLE #tmp_ori_sm_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm
	IF OBJECT_ID('tempdb.dbo.#tmp_do_free') IS NOT NULL
		DROP TABLE #tmp_do_free
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm_free') IS NOT NULL
		DROP TABLE #tmp_ori_sm_free


	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_po
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_po]
	@code varchar(17),
	@date date,
	@isVoid bit = 0
AS
BEGIN TRY
	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Purchasing.PurchaseOrderHeader
	WHERE Code = @code

	SELECT po_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.Qty
			ELSE po_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_po
	FROM Purchasing.PurchaseOrderDetail po_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = po_d.UomId
		AND uom_c.Id = po_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PO'

	-- Delete stock mutation that doesn't have in purchase order item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_po
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase order item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_po.ItemId,
		sm.UomId = #tmp_po.UomId,
		sm.UnitId = #tmp_po.UnitId,
		sm.Qty = #tmp_po.Qty,
		sm.BaseUnit = #tmp_po.BaseUnit,
		sm.BaseQty = #tmp_po.BaseQty
	FROM Inventory.StockMutation sm, #tmp_po
	WHERE sm.RefCode1 = #tmp_po.Code
	AND sm.RefDetailId1 = #tmp_po.Id
	AND sm.Src = 'PO'

	-- Insert stock mutation that doesn't have with purchase order item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
			Code, Id, NULL, 'OI', 'PO'
		FROM #tmp_po po
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = po.Code
			AND sm.RefDetailId1 = po.Id
			AND sm.Src = 'PO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int	
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, @Qty, 0, 0, 0, dbo.udf_current_local_time())
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	
	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po 
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
AS
BEGIN TRY
	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'

	SELECT pr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.Qty
			ELSE pr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_pr
	FROM Purchasing.PurchaseReturnDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase receive item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_pr.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_pr.ItemId,
		sm.UomId = #tmp_pr.UomId,
		sm.UnitId = #tmp_pr.UnitId,
		sm.Qty = #tmp_pr.Qty,
		sm.BaseUnit = #tmp_pr.BaseUnit,
		sm.BaseQty = #tmp_pr.BaseQty,
		sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'PR'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id,
			CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
			'PR'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'PR'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm		
			
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
			VALUES (@WHId, @ItemId, -@Qty, 0, 0, 0, 0, dbo.udf_current_local_time())
		END

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_rcv
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
AS
BEGIN TRY

	SELECT pr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.Qty
			ELSE pr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty,
		pr_d.NettPrice - pr_d.TaxAmount
		AS FinalNettPrice,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN
			pr_d.NettPrice - pr_d.TaxAmount
		ELSE
			(pr_d.NettPrice - pr_d.TaxAmount) / 
			(
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			)
		END AS BaseNettPrice
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	LEFT JOIN Purchasing.PurchaseReceiveHeader pr_h
		ON pr_h.Code = pr_d.Code
	WHERE pr_d.Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RCV'

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RCV'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase receive item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_pr.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_pr.ItemId,
		sm.UomId = #tmp_pr.UomId,
		sm.UnitId = #tmp_pr.UnitId,
		sm.Qty = #tmp_pr.Qty,
		sm.BaseUnit = #tmp_pr.BaseUnit,
		sm.BaseQty = #tmp_pr.BaseQty,
		sm.RefCode2 = CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END,
		sm.BaseNettPrice = #tmp_pr.BaseNettPrice,
		sm.NettPrice = #tmp_pr.FinalNettPrice
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, FinalNettPrice, BaseUnit, BaseQty, BaseNettPrice,
			Code, Id,
			CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END, 'OH',
			'RCV'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'RCV'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)
	DECLARE @srcTrans int
	DECLARE @UnitId int
	DECLARE @OldUnitId int


	SELECT @srcTrans = SrcTrans FROM Purchasing.PurchaseReceiveHeader

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode, @OldUnitId = UnitId FROM #tmp_ori_sm
			
			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId AND UnitId = @OldUnitId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId AND UnitId = @OldUnitId
		END
	END

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WHId, @ItemId, @Qty, 0, 0, 0, 0, dbo.udf_current_local_time())
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
		
	-- Drop temp tables
	DROP TABLE #tmp_pr
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_so
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_so]
	@code varchar(17),
	@date date,
	@isVoid bit = 0
AS
BEGIN TRY
	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesOrderHeader
	WHERE Code = @code

	SELECT so_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.Qty
			ELSE so_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_so
	FROM Sales.SalesOrderDetail so_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = so_d.UomId
		AND uom_c.Id = so_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SO'

	-- Delete stock mutation that doesn't have in sales order item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_so
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales order item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_so.ItemId,
		sm.UomId = #tmp_so.UomId,
		sm.UnitId = #tmp_so.UnitId,
		sm.Qty = #tmp_so.Qty,
		sm.BaseUnit = #tmp_so.BaseUnit,
		sm.BaseQty = #tmp_so.BaseQty
	FROM Inventory.StockMutation sm, #tmp_so
	WHERE sm.RefCode1 = #tmp_so.Code
	AND sm.RefDetailId1 = #tmp_so.Id
	AND sm.Src = 'SO'

	-- Insert stock mutation that doesn't have with sales order item detail
	INSERT INTO Inventory.StockMutation
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
		Code, Id,
		NULL, 'OO',
		'SO'
	FROM #tmp_so so
	WHERE NOT EXISTS (
		SELECT Id
		FROM Inventory.StockMutation sm
		WHERE sm.RefCode1 = so.Code
		AND sm.RefDetailId1 = so.Id
		AND sm.Src = 'SO'
	)

	-- Free Goods
	SELECT so_d.*,
	CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.UnitId
		ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
	CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.Qty
		ELSE so_d.Qty * (
			SELECT EXP(SUM(LOG(Conversion)))
			FROM Inventory.UoMConversion
			WHERE UomId = uom_c.UomId
			AND Seq <= uom_c.Seq
		) END AS BaseQty
	INTO #tmp_so_free
	FROM Sales.SalesOrderDetailFreeGood so_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = so_d.UomId
		AND uom_c.Id = so_d.UnitId
	WHERE Code = @code 

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm_free 
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SOF'

	-- Delete stock mutation that doesn't have in sales order free good item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SOF'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_so_free 
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales order free good item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_so_free.ItemId,
		sm.UomId = #tmp_so_free.UomId,
		sm.UnitId = #tmp_so_free.UnitId,
		sm.Qty = #tmp_so_free.Qty,
		sm.BaseUnit = #tmp_so_free.BaseUnit,
		sm.BaseQty = #tmp_so_free.BaseQty
	FROM Inventory.StockMutation sm, #tmp_so_free 
	WHERE sm.RefCode1 = #tmp_so_free.Code
	AND sm.RefDetailId1 = #tmp_so_free.Id
	AND sm.Src = 'SOF'

	-- Insert stock mutation that doesn't have with sales order free good item detail
	INSERT INTO Inventory.StockMutation
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
		Code, Id,
		NULL, 'OO',
		'SOF'
	FROM #tmp_so_free so
	WHERE NOT EXISTS (
		SELECT Id
		FROM Inventory.StockMutation sm
		WHERE sm.RefCode1 = so.Code
		AND sm.RefDetailId1 = so.Id
		AND sm.Src = 'SOF'
	)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int 
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF EXISTS(SELECT *FROM #tmp_ori_sm_free)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm_free)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm_free

			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm_free WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END 

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, 0, @Qty, 0, 0, dbo.udf_current_local_time())
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm
	DROP TABLE #tmp_so_free
	DROP TABLE #tmp_ori_sm_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm
	IF OBJECT_ID('tempdb.dbo.#tmp_so_free') IS NOT NULL
		DROP TABLE #tmp_so_free
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm_free') IS NOT NULL
		DROP TABLE #tmp_ori_sm_free

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_stock_mutation_from_sr
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20)
AS
BEGIN TRY

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.NettPrice
			ELSE sr_d.NettPrice / (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseNettPrice
	INTO #tmp_sr
	FROM Sales.SalesReturnDetail sr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = sr_d.UomId
		AND uom_c.Id = sr_d.UnitId
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in sales receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_sr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales receive item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_sr.ItemId,
		sm.UomId = #tmp_sr.UomId,
		sm.UnitId = #tmp_sr.UnitId,
		sm.Qty = #tmp_sr.Qty,
		sm.NettPrice = #tmp_sr.NettPrice,
		sm.BaseUnit = #tmp_sr.BaseUnit,
		sm.BaseQty = #tmp_sr.BaseQty,
		sm.BaseNettPrice = #tmp_sr.BaseNettPrice,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_sr
	WHERE sm.RefCode1 = #tmp_sr.Code
	AND sm.RefDetailId1 = #tmp_sr.Id
	AND sm.Src = 'SR'

	-- Insert stock mutation that doesn't have with sales receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, Code, Id,
			CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
			'SR'
		FROM #tmp_sr sr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = sr.Code
			AND sm.RefDetailId1 = sr.Id
			AND sm.Src = 'SR'
		)
	

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
			VALUES (@WHId, @ItemId, @Qty, 0, 0, 0, 0, dbo.udf_current_local_time())
		END

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_srx') IS NOT NULL
		DROP TABLE #tmp_srx
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_srx') IS NOT NULL
		DROP TABLE #tmp_srx
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create procedure dbo.sp_update_transfer_stock
            sql = @"CREATE PROCEDURE [dbo].[sp_update_transfer_stock]
    @code varchar(17),
    @date date,
    @IsComplete bit
AS
BEGIN TRY
	DECLARE @stockType varchar(8)
    DECLARE @whCodeFrom varchar(8)
    DECLARE @whCodeTo varchar(8)
	DECLARE @originTransferCode varchar(17)

	SELECT @stockType = [Type], @whCodeFrom = WarehouseCodeFrom, @whCodeTo = WarehouseCodeTo, @originTransferCode = OriginTransferCode
	FROM Inventory.TransferStockHeader
	WHERE Code = @code

    SELECT i_ts.*,
        CASE WHEN uom_c.IsBaseUnit = 1 THEN i_ts.UnitId
            ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
        CASE WHEN uom_c.IsBaseUnit = 1 THEN i_ts.Qty
            ELSE i_ts.Qty * (
                SELECT EXP(SUM(LOG(Conversion)))
                FROM Inventory.UoMConversion
                WHERE UomId = uom_c.UomId
                AND Seq <= uom_c.Seq
            ) END AS BaseQty
    INTO #tmp_its
    FROM [Inventory].[TransferStockDetail] i_ts
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.UomId = i_ts.UomId
        AND uom_c.Id = i_ts.UnitId
    WHERE Code = @code

	-- Delete stock mutation transfer stock item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src IN ('TS','CNEE')

    -- Update WarehouseQty
    DECLARE @itemID AS INT = 0
    DECLARE @qty AS DECIMAL = 0

    IF (@IsComplete = 0) --Active
    BEGIN

		IF (@stockType NOT IN ('DT','C','RC')) --InventoryIn & Out
		BEGIN
			DECLARE @loop INT = 0
			WHILE (@loop < 2)
			BEGIN
				-- Loop 0 = OH, Loop 1 = OT
				-- Insert stock mutation transfer stock item detail OH & OT
				INSERT INTO Inventory.StockMutation
					SELECT CASE WHEN @stockType = 'OUT' THEN @whCodeFrom 
								WHEN @stockType = 'IN' AND @loop = 0 THEN @whCodeTo 
								WHEN @stockType = 'IN' AND @loop = 1 THEN @whCodeFrom END, 
						@date, ItemId, UomId, UnitId, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN Qty * -1 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN Qty
							 WHEN @stockType = 'IN' AND @loop = 0 THEN Qty 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN Qty * -1 END, 
						0/*NettPrice */, BaseUnit, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN BaseQty * -1 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN BaseQty
							 WHEN @stockType = 'IN' AND @loop = 0 THEN BaseQty 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN BaseQty * -1 END,
						0/*BaseNettPrice */,Code, Id,
						CASE WHEN @stockType = 'OUT' THEN NULL WHEN @stockType = 'IN' THEN @originTransferCode END, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN 'OT'
							 WHEN @stockType = 'IN' AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN 'OT' END,
						'TS'
					FROM #tmp_its its
					--WHERE NOT EXISTS (
					--	SELECT Id
					--	FROM Inventory.StockMutation sm
					--	WHERE sm.RefCode1 = its.Code
					--	AND sm.RefDetailId1 = its.Id
					--	AND sm.Src = 'TS'
					--)

				SET @loop = @loop + 1
			END
		END

        IF (@stockType = 'OUT') --InventoryOut
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = dbo.udf_current_local_time() 
                    WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType = 'IN') --InventoryIn
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = dbo.udf_current_local_time() 
                WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() 
                    WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                END
                ELSE
                BEGIN
                    INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
                    VALUES (@whCodeTo, @ItemId, @qty, 0, 0, 0, 0, dbo.udf_current_local_time())
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType IN ('DT','C','RC')) --DirectTransfer & Consignee
        BEGIN
			-- Insert stock mutation transfer stock item detail warehouse from
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeFrom, @date, ItemId, UomId, UnitId, Qty * -1, 0/*NettPrice */, BaseUnit, BaseQty * -1, 0/*BaseNettPrice */,
					Code, Id,
					NULL, 'OH',
					CASE WHEN @stockType = 'DT' THEN 'TS'
						 WHEN @stockType IN ('C','RC') THEN 'CNEE' END
				FROM #tmp_its

			-- Insert stock mutation transfer stock item detail warehouse to
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeTo, @date, ItemId, UomId, UnitId, Qty, 0/*NettPrice */, BaseUnit, BaseQty, 0/*BaseNettPrice */,
					Code, Id,
					NULL, 'OH',
					CASE WHEN @stockType = 'DT' THEN 'TS'
						 WHEN @stockType IN ('C','RC') THEN 'CNEE' END
				FROM #tmp_its
				--WHERE NOT EXISTS (
				--	SELECT Id
				--	FROM Inventory.StockMutation sm
				--	WHERE sm.RefCode1 = its.Code
				--	AND sm.RefDetailId1 = its.Id
				--	AND sm.Src = 'TS'
				--)

            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
                        VALUES (@whCodeTo, @ItemId, @qty, 0, 0, 0, 0, dbo.udf_current_local_time())
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
    END
    ELSE --Void = rollback item
    BEGIN
        IF (@stockType = 'OUT')
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType = 'IN')
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType IN ('DT','C','RC'))
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
    END
        
    -- Drop temp tables
    DROP TABLE #tmp_its
 

END TRY
BEGIN CATCH
    -- Drop temp tables
    IF OBJECT_ID('tempdb.dbo.#tmp_its') IS NOT NULL
        DROP TABLE #tmp_its

    -- Raise error
    EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_Customer_CustomerAddress_BillingAddressId
            sql = @"ALTER TABLE [General].[Customer] NOCHECK CONSTRAINT [FK_Customer_CustomerAddress_BillingAddressId]";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_Customer_CustomerAddress_ShippingAddressId
            sql = @"ALTER TABLE [General].[Customer] NOCHECK CONSTRAINT [FK_Customer_CustomerAddress_ShippingAddressId]";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_GeneralCashBankDetail_CashBankType_Type 
            sql = @"ALTER TABLE [Finance].[GeneralCashBankDetail] NOCHECK CONSTRAINT [FK_GeneralCashBankDetail_CashBankType_Type]";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_User_Employee_EmployeeId
            sql = @"ALTER TABLE [Sales].[SalesOrderHeader] NOCHECK CONSTRAINT [FK_SalesOrderHeader_PaymentTerm_PaymentTermId]";
            migrationBuilder.Sql(sql);

            // Disabling constraints foreign key FK_User_Employee_EmployeeId
            sql = @"ALTER TABLE [SystemManagement].[User] NOCHECK CONSTRAINT [FK_User_Employee_EmployeeId]";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddress_Customer_Code",
                schema: "General",
                table: "CustomerAddress");

            migrationBuilder.DropTable(
                name: "AdjustmentDetailDiffUnit",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Attendance",
                schema: "HumanResource");

            migrationBuilder.DropTable(
                name: "BeginningBalanceAP",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceAR",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceCreditMemo",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceDebitMemo",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "BeginningBalanceDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ClosingMonth",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "COA",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "CurrencyRate",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "DeliveryPlanDetailItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DeliveryPlanUndeliveredItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DynamicReportTemplate",
                schema: "General");

            migrationBuilder.DropTable(
                name: "ExpeditionInvoiceDetail",
                schema: "Expedition");

            migrationBuilder.DropTable(
                name: "FixedAssetDepartment",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "FixedAssetHistory",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "GeneralCashBankDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GeneralJournalDetail",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "GeneralJournalHeader",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "IncomeStatementFormatSubtotal",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "Journal",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "MenuAction",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "MobileActivityLog",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileCostDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileCostImage",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileCustomer",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileDeliveryItemDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileItemRequestDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobilePaymentInvoice",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobilePaymentMethod",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileReceiveItemDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileTransferStockDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileVisitReason",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "PostingLog",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "PostingState",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "PromoDetailMultipleItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PromoDetailTier",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PromoSubject",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDebitMemo",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseReceiveDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetail",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetailExchDiffItem",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseReturnHeader",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "RoleMenu",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "RoleMenuAction",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "SalesDeliveryDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesDeliveryDetailFreeGood",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesInvoiceCreditMemo",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesmanMapTrackingHistory",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesmanSchedule",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesmanScheduleCustomer",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderDetailDiscount",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderDetailFreeGood",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnDetailExchDiffItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesTargetDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesTargetSubject",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SequenceNumber",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "StockMutation",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "SystemParameter",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "TransferStockDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "User",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "VisitOrderCustomer",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitOrderInvoice",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanDetailCustomer",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "WarehouseQuantity",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "AdjustmentDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "BeginningBalanceHeader",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "COAType",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "DeliveryPlanDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ExpeditionInvoiceHeader",
                schema: "Expedition");

            migrationBuilder.DropTable(
                name: "FixedAsset",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "CashBankType",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "IncomeStatementFormat",
                schema: "Accounting");

            migrationBuilder.DropTable(
                name: "MobileCostHeader",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileDeliveryItemHeader",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileItemRequestHeader",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetail",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "MobileOrderDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileReceiveItemHeader",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileTransferStockHeader",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "PromoDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DebitMemo",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceHeader",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Action",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "Menu",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "CreditMemo",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesDeliveryHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesInvoiceHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ItemGroupSubGroup",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "SalesTargetHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SystemParameterModule",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "AdjustmentHeader",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "AssetType",
                schema: "AssetManagement");

            migrationBuilder.DropTable(
                name: "GeneralCashBankHeader",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "DeliveryPlanHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "MobileOrderHeader",
                schema: "MobileCustomer");

            migrationBuilder.DropTable(
                name: "Item",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MobileOrderHeader",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "PurchaseReceiveHeader",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "TransferStockHeader",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PromoHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PurchaseOrderHeader",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "Supplier",
                schema: "General");

            migrationBuilder.DropTable(
                name: "Currency",
                schema: "General");

            migrationBuilder.DropTable(
                name: "Vehicle",
                schema: "General");

            migrationBuilder.DropTable(
                name: "ItemCategory",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Tax",
                schema: "General");

            migrationBuilder.DropTable(
                name: "UoMConversion",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MobileVisitLog",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "SalesOrderHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SupplierType",
                schema: "General");

            migrationBuilder.DropTable(
                name: "VehicleType",
                schema: "General");

            migrationBuilder.DropTable(
                name: "ItemGroup",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "UoM",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MobileReason",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "VisitOrder",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "Employee",
                schema: "General");

            migrationBuilder.DropTable(
                name: "SalesmanGroup",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "Warehouse",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "General");

            migrationBuilder.DropTable(
                name: "Area",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "CustomerAddress",
                schema: "General");

            migrationBuilder.DropTable(
                name: "CustomerType",
                schema: "General");

            migrationBuilder.DropTable(
                name: "PaymentTerm",
                schema: "General");

            // Drop function dbo.udf_string_split
            var sql = @"DROP FUNCTION [dbo].[udf_string_split]";
            migrationBuilder.Sql(sql);

            // Drop function dbo.udf_num_to_words_id
            sql = @"DROP FUNCTION [dbo].[udf_num_to_words_id]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwBeginningBalanceAP
            sql = @"DROP VIEW [Accounting].[vwBeginningBalanceAP]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwBeginningBalanceAR
            sql = @"DROP VIEW [Accounting].[vwBeginningBalanceAR]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwBeginningBalanceCreditMemo
            sql = @"DROP VIEW [Accounting].[vwBeginningBalanceCreditMemo]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwBeginningBalanceDebitMemo
            sql = @"DROP VIEW [Accounting].[vwBeginningBalanceDebitMemo]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwClosingMonth
            sql = @"DROP VIEW [Accounting].[vwClosingMonth]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwCOA
            sql = @"DROP VIEW [Accounting].[vwCOA]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwGeneralJournalHeader
            sql = @"DROP VIEW [Accounting].[vwGeneralJournalHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Accounting.vwIncomeStatementFormatSubtotal
            sql = @"DROP VIEW [Accounting].[vwIncomeStatementFormatSubtotal]";
            migrationBuilder.Sql(sql);

            // Drop view AssetManagement.vwAssetType
            sql = @"DROP VIEW [AssetManagement].[vwAssetType]";
            migrationBuilder.Sql(sql);

            // Drop view AssetManagement.vwFixedAsset
            sql = @"DROP VIEW [AssetManagement].[vwFixedAsset]";
            migrationBuilder.Sql(sql);

            // Drop view Expedition.vwExpeditionInvoiceHeader
            sql = @"DROP VIEW [Expedition].[vwExpeditionInvoiceHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwDebitMemo
            sql = @"DROP VIEW [Purchasing].[vwDebitMemo]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseInvoiceHeader
            sql = @"DROP VIEW [Purchasing].[vwPurchaseInvoiceHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseOrderHeader
            sql = @"DROP VIEW [Purchasing].[vwPurchaseOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseOrderDetail
            sql = @"DROP VIEW [Purchasing].[vwPurchaseOrderDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReceiveHeader
            sql = @"DROP VIEW [Purchasing].[vwPurchaseReceiveHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReceiveDetail
            sql = @"DROP VIEW [Purchasing].[vwPurchaseReceiveDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReturnHeader
            sql = @"DROP VIEW [Purchasing].[vwPurchaseReturnHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReturnDetail
            sql = @"DROP VIEW [Purchasing].[vwPurchaseReturnDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReturnDetailExchDiffItem
            sql = @"DROP VIEW [Purchasing].[vwPurchaseReturnDetailExchDiffItem]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwArea
            sql = @"DROP VIEW [Sales].[vwArea]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwCreditMemo
            sql = @"DROP VIEW [Sales].[vwCreditMemo]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwPromoHeader
            sql = @"DROP VIEW [Sales].[vwPromoHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwDeliveryPlanHeader
            sql = @"DROP VIEW [Sales].[vwDeliveryPlanHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesDeliveryHeader
            sql = @"DROP VIEW [Sales].[vwSalesDeliveryHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesDeliveryDetail
            sql = @"DROP VIEW [Sales].[vwSalesDeliveryDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesInvoiceHeader
            sql = @"DROP VIEW [Sales].[vwSalesInvoiceHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesmanGroup
            sql = @"DROP VIEW [Sales].[vwSalesmanGroup]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesmanSchedule
            sql = @"DROP VIEW [Sales].[vwSalesmanSchedule]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesmanScheduleCustomer
            sql = @"DROP VIEW [Sales].[vwSalesmanScheduleCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesOrderHeader
            sql = @"DROP VIEW [Sales].[vwSalesOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesOrderDetail
            sql = @"DROP VIEW [Sales].[vwSalesOrderDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesReturnHeader
            sql = @"DROP VIEW [Sales].[vwSalesReturnHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesReturnDetail
            sql = @"DROP VIEW [Sales].[vwSalesReturnDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesReturnDetailExchDiffItem
            sql = @"DROP VIEW [Sales].[vwSalesReturnDetailExchDiffItem]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesTargetHeader
            sql = @"DROP VIEW [Sales].[vwSalesTargetHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitOrder
            sql = @"DROP VIEW [Sales].[vwVisitOrder]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitOrderCustomer
            sql = @"DROP VIEW [Sales].[vwVisitOrderCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitOrderInvoice
            sql = @"DROP VIEW [Sales].[vwVisitOrderInvoice]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitPlanHeader
            sql = @"DROP VIEW [Sales].[vwVisitPlanHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitPlanDetail
            sql = @"DROP VIEW [Sales].[vwVisitPlanDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwVisitPlanDetailCustomer
            sql = @"DROP VIEW [Sales].[vwVisitPlanDetailCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwAP
            sql = @"DROP VIEW [Finance].[vwAP]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwAR
            sql = @"DROP VIEW [Finance].[vwAR]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwCashBankType
            sql = @"DROP VIEW [Finance].[vwCashBankType]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwDebitCreditPayment
            sql = @"DROP VIEW [Finance].[vwDebitCreditPayment]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwGeneralCashBankDetail
            sql = @"DROP VIEW [Finance].[vwGeneralCashBankDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwGeneralCashBankHeader
            sql = @"DROP VIEW [Finance].[vwGeneralCashBankHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwInterCashBankHeader
            sql = @"DROP VIEW [Finance].[vwInterCashBankHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwOutstandingCreditMemo
            sql = @"DROP VIEW [Finance].[vwOutstandingCreditMemo]";
            migrationBuilder.Sql(sql);

            // Drop view Finance.vwOutstandingDebitMemo
            sql = @"DROP VIEW [Finance].[vwOutstandingDebitMemo]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwApproval
            sql = @"DROP VIEW [General].[vwApproval]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwCustomer
            sql = @"DROP VIEW [General].[vwCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwCustomerType
            sql = @"DROP VIEW [General].[vwCustomerType]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwEmployee
            sql = @"DROP VIEW [General].[vwEmployee]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwPaymentTerm
            sql = @"DROP VIEW [General].[vwPaymentTerm]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwSupplier
            sql = @"DROP VIEW [General].[vwSupplier]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwSupplierType
            sql = @"DROP VIEW [General].[vwSupplierType]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwTax
            sql = @"DROP VIEW [General].[vwTax]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwVehicle
            sql = @"DROP VIEW [General].[vwVehicle]";
            migrationBuilder.Sql(sql);

            // Drop view General.vwVehicleType
            sql = @"DROP VIEW [General].[vwVehicleType]";
            migrationBuilder.Sql(sql);

            // Drop view HumanResource.vwAttendanceReport
            sql = @"DROP VIEW [HumanResource].[vwAttendanceReport]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwAdjustmentDetail
            sql = @"DROP VIEW [Inventory].[vwAdjustmentDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwAdjustmentHeader
            sql = @"DROP VIEW [Inventory].[vwAdjustmentHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwAdjustmentItem
            sql = @"DROP VIEW [Inventory].[vwAdjustmentItem]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwBeginningBalanceStockDetail
            sql = @"DROP VIEW [Inventory].[vwBeginningBalanceStockDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwBeginningBalanceStockHeader
            sql = @"DROP VIEW [Inventory].[vwBeginningBalanceStockHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItem
            sql = @"DROP VIEW [Inventory].[vwItem]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItemCategory
            sql = @"DROP VIEW [Inventory].[vwItemCategory]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItemGroup
            sql = @"DROP VIEW [Inventory].[vwItemGroup]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwTransferStockDetail
            sql = @"DROP VIEW [Inventory].[vwTransferStockDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwTransferStockHeader
            sql = @"DROP VIEW [Inventory].[vwTransferStockHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwUoM
            sql = @"DROP VIEW [Inventory].[vwUoM]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwWarehouse
            sql = @"DROP VIEW [Inventory].[vwWarehouse]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwWarehouseQuantity
            sql = @"DROP VIEW [Inventory].[vwWarehouseQuantity]";
            migrationBuilder.Sql(sql);

            // Drop view MobileCustomer.vwMobileOrderDetail
            sql = @"DROP VIEW [MobileCustomer].[vwMobileOrderDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileCustomer.vwMobileOrderHeader
            sql = @"DROP VIEW [MobileCustomer].[vwMobileOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileCostDetail
            sql = @"DROP VIEW [MobileSales].[vwMobileCostDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileCostHeader
            sql = @"DROP VIEW [MobileSales].[vwMobileCostHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileCustomer
            sql = @"DROP VIEW [MobileSales].[vwMobileCustomer]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileItemRequestHeader
            sql = @"DROP VIEW [MobileSales].[vwMobileItemRequestHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileItemRequestDetail
            sql = @"DROP VIEW [MobileSales].[vwMobileItemRequestDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileOrderHeader
            sql = @"DROP VIEW [MobileSales].[vwMobileOrderHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileOrderDetail
            sql = @"DROP VIEW [MobileSales].[vwMobileOrderDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobilePaymentInvoice
            sql = @"DROP VIEW [MobileSales].[vwMobilePaymentInvoice]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobilePaymentMethod
            sql = @"DROP VIEW [MobileSales].[vwMobilePaymentMethod]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileReason
            sql = @"DROP VIEW [MobileSales].[vwMobileReason]";
            migrationBuilder.Sql(sql);

            // Drop view MobileSales.vwMobileVisitLog
            sql = @"DROP VIEW [MobileSales].[vwMobileVisitLog]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileDeliveryItemHeader
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileDeliveryItemHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileDeliveryItemDetail
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileDeliveryItemDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileReceiveItemHeader
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileReceiveItemHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileReceiveItemDetail
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileReceiveItemDetail]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileTransferStockHeader
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileTransferStockHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileTransferStockDetail
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileTransferStockDetail]";
            migrationBuilder.Sql(sql);

            // Drop view SystemManagement.vwCompany
            sql = @"DROP VIEW [SystemManagement].[vwCompany]";
            migrationBuilder.Sql(sql);

            // Drop view SystemManagement.vwRole
            sql = @"DROP VIEW [SystemManagement].[vwRole]";
            migrationBuilder.Sql(sql);

            // Drop view SystemManagement.vwUser
            sql = @"DROP VIEW [SystemManagement].[vwUser]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_raiseerror
            sql = @"DROP PROCEDURE [dbo].[sp_raiseerror]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_generate_autono
            sql = @"DROP PROCEDURE [dbo].[sp_generate_autono]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_bsisdt_coa
            sql = @"DROP PROCEDURE [dbo].[sp_get_bsisdt_coa]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_dlv_plan_picking_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_dlv_plan_picking_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_dlv_plan_packing_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_dlv_plan_packing_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_cb_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_cb_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_do_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_do_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_ei_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_ei_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_pi_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_pi_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_po_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_po_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_pr_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_pr_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_rcv_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_rcv_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_si_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_si_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_so_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_so_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_sr_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_sr_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_insert_curr_rate
            sql = @"DROP PROCEDURE [dbo].[sp_insert_curr_rate]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_ts_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_ts_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_get_vo_cust_print_data
            sql = @"DROP PROCEDURE [dbo].[sp_get_vo_cust_print_data]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_restore_cash_bank_transaction
            sql = @"DROP PROCEDURE [dbo].[sp_restore_cash_bank_transaction]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_restore_stock_mutation_from_adj
            sql = @"DROP PROCEDURE [dbo].[sp_restore_stock_mutation_from_adj]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_restore_stock_mutation_from_bb
            sql = @"DROP PROCEDURE [dbo].[sp_restore_stock_mutation_from_bb]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_currency_sort
            sql = @"DROP PROCEDURE [dbo].[sp_update_currency_sort]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_po_rcv_qty
            sql = @"DROP PROCEDURE [dbo].[sp_update_po_rcv_qty]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_pr_rcv_qty
            sql = @"DROP PROCEDURE [dbo].[sp_update_pr_rcv_qty]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_so_dlv_qty
            sql = @"DROP PROCEDURE [dbo].[sp_update_so_dlv_qty]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_so_free_dlv_qty
            sql = @"DROP PROCEDURE [dbo].[sp_update_so_free_dlv_qty]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_sr_dlv_qty
            sql = @"DROP PROCEDURE [dbo].[sp_update_sr_dlv_qty]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_adj
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_bb
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_bb]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_do
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_do]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_po
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_po]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_rcv
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_so
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_so]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_stock_mutation_from_sr
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]";
            migrationBuilder.Sql(sql);

            // Drop procedure dbo.sp_update_transfer_stock
            sql = @"DROP PROCEDURE [dbo].[sp_update_transfer_stock]";
            migrationBuilder.Sql(sql);
        }
    }
}
