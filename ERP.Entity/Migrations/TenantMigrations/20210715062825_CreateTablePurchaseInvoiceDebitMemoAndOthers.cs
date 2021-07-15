using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTablePurchaseInvoiceDebitMemoAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDebitMemo",
                schema: "Purchasing",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DebitMemoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebitMemoAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                name: "SalesInvoiceCreditMemo",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CreditMemoCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    InvAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditMemoAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                name: "IX_SalesInvoiceHeader_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "IssuedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceHeader_SOCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "SOCode");

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
                name: "IX_SalesInvoiceCreditMemo_CreditMemoCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                column: "CreditMemoCode");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceCreditMemo_InvCode",
                schema: "Sales",
                table: "SalesInvoiceCreditMemo",
                column: "InvCode");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditMemo_Currency_CurrCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "CurrCode",
                principalSchema: "General",
                principalTable: "Currency",
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
                name: "FK_DebitMemo_Currency_CurrCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "CurrCode",
                principalSchema: "General",
                principalTable: "Currency",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_DebitMemo_Supplier_SupCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "SupCode",
                principalSchema: "General",
                principalTable: "Supplier",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail",
                column: "Code",
                principalSchema: "Purchasing",
                principalTable: "PurchaseInvoiceHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail",
                column: "RcvCode",
                principalSchema: "Purchasing",
                principalTable: "PurchaseReceiveHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceHeader_Currency_CurrCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "CurrCode",
                principalSchema: "General",
                principalTable: "Currency",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceHeader_Employee_IssuedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "IssuedBy",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "POCode",
                principalSchema: "Purchasing",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceHeader_Supplier_SupCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                column: "SupCode",
                principalSchema: "General",
                principalTable: "Supplier",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode",
                schema: "Sales",
                table: "SalesInvoiceDetail",
                column: "DOCode",
                principalSchema: "Sales",
                principalTable: "SalesDeliveryHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceDetail_SalesInvoiceHeader_Code",
                schema: "Sales",
                table: "SalesInvoiceDetail",
                column: "Code",
                principalSchema: "Sales",
                principalTable: "SalesInvoiceHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceHeader_Currency_CurrCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "CurrCode",
                principalSchema: "General",
                principalTable: "Currency",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceHeader_Customer_CustCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "CustCode",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceHeader_Employee_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "IssuedBy",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceHeader_SalesOrderHeader_SOCode",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                column: "SOCode",
                principalSchema: "Sales",
                principalTable: "SalesOrderHeader",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditMemo_Currency_CurrCode",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditMemo_Customer_CustCode",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_DebitMemo_Currency_CurrCode",
                schema: "Purchasing",
                table: "DebitMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_DebitMemo_Supplier_SupCode",
                schema: "Purchasing",
                table: "DebitMemo");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceDetail_PurchaseInvoiceHeader_Code",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceDetail_PurchaseReceiveHeader_RcvCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceHeader_Currency_CurrCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceHeader_Employee_IssuedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceHeader_PurchaseOrderHeader_POCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceHeader_Supplier_SupCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetail_SalesDeliveryHeader_DOCode",
                schema: "Sales",
                table: "SalesInvoiceDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceDetail_SalesInvoiceHeader_Code",
                schema: "Sales",
                table: "SalesInvoiceDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceHeader_Currency_CurrCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceHeader_Customer_CustCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceHeader_Employee_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceHeader_SalesOrderHeader_SOCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDebitMemo",
                schema: "Purchasing");

            migrationBuilder.DropTable(
                name: "SalesInvoiceCreditMemo",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceHeader_CurrCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceHeader_CustCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceHeader_IssuedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceHeader_SOCode",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceDetail_Code",
                schema: "Sales",
                table: "SalesInvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceDetail_DOCode",
                schema: "Sales",
                table: "SalesInvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceHeader_CurrCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceHeader_IssuedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceHeader_POCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceHeader_SupCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceDetail_Code",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceDetail_RcvCode",
                schema: "Purchasing",
                table: "PurchaseInvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_DebitMemo_CurrCode",
                schema: "Purchasing",
                table: "DebitMemo");

            migrationBuilder.DropIndex(
                name: "IX_DebitMemo_SupCode",
                schema: "Purchasing",
                table: "DebitMemo");

            migrationBuilder.DropIndex(
                name: "IX_CreditMemo_CurrCode",
                schema: "Sales",
                table: "CreditMemo");

            migrationBuilder.DropIndex(
                name: "IX_CreditMemo_CustCode",
                schema: "Sales",
                table: "CreditMemo");
        }
    }
}
