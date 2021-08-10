using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateMobileSalesRelatedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HumanResource");

            migrationBuilder.EnsureSchema(
                name: "MobileSales");

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
                    CheckInLat = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CheckInLng = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CheckInImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CheckInNotes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CheckOut = table.Column<DateTime>(type: "datetime", nullable: true),
                    CheckOutLat = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CheckOutLng = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CheckOutImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CheckOutNotes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    TotalHours = table.Column<decimal>(type: "decimal(19,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileCustomer",
                schema: "MobileSales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
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
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
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
                    Lat = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Lng = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
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
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NotesFailCollect = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SrcTrans = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
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
                    VisitLogCode = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false),
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
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
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
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
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
                    Qty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
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
                name: "IX_Attendance_EmployeeId",
                schema: "HumanResource",
                table: "Attendance",
                column: "EmployeeId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_Customer_CustCode",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_Employee_SalesBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesBy",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_PaymentTerm_PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "PaymentTermId",
                principalSchema: "General",
                principalTable: "PaymentTerm",
                principalColumn: "Id");

            // Update foreign key SystemManagement.User
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
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE SystemManagement.[User] WITH NOCHECK ADD CONSTRAINT
	FK_User_Employee_EmployeeId FOREIGN KEY
	(
	EmployeeId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE SystemManagement.[User]
	NOCHECK CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE SystemManagement.[User] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Update foreign key Sales.SalesOrderHeader
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
ALTER TABLE Sales.SalesOrderHeader
    DROP CONSTRAINT FK_SalesOrderHeader_PaymentTerm_PaymentTermId
GO
ALTER TABLE General.PaymentTerm SET(LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Sales.SalesOrderHeader WITH NOCHECK ADD CONSTRAINT
    FK_SalesOrderHeader_PaymentTerm_PaymentTermId FOREIGN KEY
    (
    PaymentTermId

    ) REFERENCES General.PaymentTerm
    (
    Id
    ) ON UPDATE  NO ACTION

     ON DELETE  NO ACTION


GO
ALTER TABLE Sales.SalesOrderHeader
    NOCHECK CONSTRAINT FK_SalesOrderHeader_PaymentTerm_PaymentTermId
GO
ALTER TABLE Sales.SalesOrderHeader SET(LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_Customer_CustCode",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_Employee_SalesBy",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_PaymentTerm_PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropTable(
                name: "Attendance",
                schema: "HumanResource");

            migrationBuilder.DropTable(
                name: "MobileCustomer",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailDiscount",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetailFreeGood",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobilePaymentInvoice",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileVisitReason",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderDetail",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileOrderHeader",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileVisitLog",
                schema: "MobileSales");

            migrationBuilder.DropTable(
                name: "MobileReason",
                schema: "MobileSales");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_CustCode",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_SalesBy",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
