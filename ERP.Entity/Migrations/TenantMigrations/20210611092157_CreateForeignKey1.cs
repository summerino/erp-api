using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateForeignKey1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SaleUnit",
                schema: "Sales",
                table: "PromoDetailTier",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true);

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
                name: "IX_UoMConversion_UomId",
                schema: "Inventory",
                table: "UoMConversion",
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
                name: "IX_Tax_CoaCode",
                schema: "General",
                table: "Tax",
                column: "CoaCode");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_TypeId",
                schema: "General",
                table: "Supplier",
                column: "TypeId");

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
                name: "IX_RoleMenu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAction_ActionId",
                schema: "SystemManagement",
                table: "MenuAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemGroupSubGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemGroupSubGroup",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategory_GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "GroupId");

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
                name: "IX_DeliveryPlanDetail_Code",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddress_Code",
                schema: "General",
                table: "CustomerAddress",
                column: "Code");

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
                name: "IX_AdjustmentHeader_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetailDiffUnit_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit",
                column: "UnitId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetail_AdjustmentHeader_Code",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "Code",
                principalSchema: "Inventory",
                principalTable: "AdjustmentHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetail_Item_ItemId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "Item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetail_UoM_UomId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetail_UoMConversion_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetail",
                column: "UnitId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentDetailDiffUnit_UoMConversion_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit",
                column: "UnitId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdjustmentHeader_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Area_AreaId1",
                schema: "General",
                table: "Customer",
                column: "AreaId1",
                principalSchema: "Sales",
                principalTable: "Area",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Area_AreaId2",
                schema: "General",
                table: "Customer",
                column: "AreaId2",
                principalSchema: "Sales",
                principalTable: "Area",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Area_AreaId3",
                schema: "General",
                table: "Customer",
                column: "AreaId3",
                principalSchema: "Sales",
                principalTable: "Area",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Area_AreaId4",
                schema: "General",
                table: "Customer",
                column: "AreaId4",
                principalSchema: "Sales",
                principalTable: "Area",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Area_AreaId5",
                schema: "General",
                table: "Customer",
                column: "AreaId5",
                principalSchema: "Sales",
                principalTable: "Area",
                principalColumn: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustomerType_TypeId",
                schema: "General",
                table: "Customer",
                column: "TypeId",
                principalSchema: "General",
                principalTable: "CustomerType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddress_Customer_Code",
                schema: "General",
                table: "CustomerAddress",
                column: "Code",
                principalSchema: "General",
                principalTable: "Customer",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanDetail_DeliveryPlanHeader_Code",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                column: "Code",
                principalSchema: "Sales",
                principalTable: "DeliveryPlanHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanHeader_Employee_DriverId",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "DriverId",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanHeader_Vehicle_VehicleId",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "VehicleId",
                principalSchema: "General",
                principalTable: "Vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanHeader_Warehouse_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanHeader",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanDetail_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "DlvPlanDetailId",
                principalSchema: "Sales",
                principalTable: "DeliveryPlanDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanHeader_Code",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "Code",
                principalSchema: "Sales",
                principalTable: "DeliveryPlanHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_Item_ItemId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "Item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_UoM_UomId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_UoMConversion_UnitId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "UnitId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_Warehouse_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_ItemCategory_CategoryId",
                schema: "Inventory",
                table: "Item",
                column: "CategoryId",
                principalSchema: "Inventory",
                principalTable: "ItemCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Tax_PurchaseTaxId",
                schema: "Inventory",
                table: "Item",
                column: "PurchaseTaxId",
                principalSchema: "General",
                principalTable: "Tax",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Tax_SalesTaxId",
                schema: "Inventory",
                table: "Item",
                column: "SalesTaxId",
                principalSchema: "General",
                principalTable: "Tax",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_UoM_UomId",
                schema: "Inventory",
                table: "Item",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_UoMConversion_UomBuyId",
                schema: "Inventory",
                table: "Item",
                column: "UomBuyId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_UoMConversion_UomSellId",
                schema: "Inventory",
                table: "Item",
                column: "UomSellId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategory_ItemGroup_GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "GroupId",
                principalSchema: "Inventory",
                principalTable: "ItemGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemGroupSubGroup_ItemGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemGroupSubGroup",
                column: "ItemGroupId",
                principalSchema: "Inventory",
                principalTable: "ItemGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuAction_Action_ActionId",
                schema: "SystemManagement",
                table: "MenuAction",
                column: "ActionId",
                principalSchema: "SystemManagement",
                principalTable: "Action",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuAction_Menu_MenuId",
                schema: "SystemManagement",
                table: "MenuAction",
                column: "MenuId",
                principalSchema: "SystemManagement",
                principalTable: "Menu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_Menu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu",
                column: "MenuId",
                principalSchema: "SystemManagement",
                principalTable: "Menu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_Role_RoleId",
                schema: "SystemManagement",
                table: "RoleMenu",
                column: "RoleId",
                principalSchema: "SystemManagement",
                principalTable: "Role",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenuAction_Action_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                column: "ActionId",
                principalSchema: "SystemManagement",
                principalTable: "Action",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenuAction_Menu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                column: "MenuId",
                principalSchema: "SystemManagement",
                principalTable: "Menu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenuAction_Role_RoleId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                column: "RoleId",
                principalSchema: "SystemManagement",
                principalTable: "Role",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMutation_Item_ItemId",
                schema: "Inventory",
                table: "StockMutation",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "Item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMutation_UoM_UomId",
                schema: "Inventory",
                table: "StockMutation",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMutation_UoMConversion_BaseUnit",
                schema: "Inventory",
                table: "StockMutation",
                column: "BaseUnit",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMutation_UoMConversion_UnitId",
                schema: "Inventory",
                table: "StockMutation",
                column: "UnitId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMutation_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "StockMutation",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplier_SupplierType_TypeId",
                schema: "General",
                table: "Supplier",
                column: "TypeId",
                principalSchema: "General",
                principalTable: "SupplierType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockDetail_Item_ItemId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "Item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockDetail_TransferStockHeader_Code",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "Code",
                principalSchema: "Inventory",
                principalTable: "TransferStockHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockDetail_UoM_UomId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockDetail_UoMConversion_UnitId",
                schema: "Inventory",
                table: "TransferStockDetail",
                column: "UnitId",
                principalSchema: "Inventory",
                principalTable: "UoMConversion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockHeader_Warehouse_WarehouseCodeFrom",
                schema: "Inventory",
                table: "TransferStockHeader",
                column: "WarehouseCodeFrom",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferStockHeader_Warehouse_WarehouseCodeTo",
                schema: "Inventory",
                table: "TransferStockHeader",
                column: "WarehouseCodeTo",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_UoMConversion_UoM_UomId",
                schema: "Inventory",
                table: "UoMConversion",
                column: "UomId",
                principalSchema: "Inventory",
                principalTable: "UoM",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee_EmployeeId",
                schema: "SystemManagement",
                table: "User",
                column: "EmployeeId",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_RoleId",
                schema: "SystemManagement",
                table: "User",
                column: "RoleId",
                principalSchema: "SystemManagement",
                principalTable: "Role",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Employee_DriverId",
                schema: "General",
                table: "Vehicle",
                column: "DriverId",
                principalSchema: "General",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_VehicleType_TypeId",
                schema: "General",
                table: "Vehicle",
                column: "TypeId",
                principalSchema: "General",
                principalTable: "VehicleType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseQuantity_Item_ItemId",
                schema: "Inventory",
                table: "WarehouseQuantity",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "Item",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseQuantity_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "WarehouseQuantity",
                column: "WarehouseCode",
                principalSchema: "Inventory",
                principalTable: "Warehouse",
                principalColumn: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentDetail_AdjustmentHeader_Code",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentDetail_Item_ItemId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentDetail_UoM_UomId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentDetail_UoMConversion_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentDetailDiffUnit_UoMConversion_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_AdjustmentHeader_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Area_AreaId1",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Area_AreaId2",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Area_AreaId3",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Area_AreaId4",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Area_AreaId5",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustomerAddress_BillingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustomerAddress_ShippingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustomerType_TypeId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddress_Customer_Code",
                schema: "General",
                table: "CustomerAddress");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanDetail_DeliveryPlanHeader_Code",
                schema: "Sales",
                table: "DeliveryPlanDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanHeader_Employee_DriverId",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanHeader_Vehicle_VehicleId",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanHeader_Warehouse_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanDetail_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_DeliveryPlanHeader_Code",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_Item_ItemId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_UoM_UomId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_UoMConversion_UnitId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPlanUndeliveredItem_Warehouse_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_ItemCategory_CategoryId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Tax_PurchaseTaxId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Tax_SalesTaxId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_UoM_UomId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_UoMConversion_UomBuyId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_UoMConversion_UomSellId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategory_ItemGroup_GroupId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemGroupSubGroup_ItemGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemGroupSubGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuAction_Action_ActionId",
                schema: "SystemManagement",
                table: "MenuAction");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuAction_Menu_MenuId",
                schema: "SystemManagement",
                table: "MenuAction");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_Menu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_Role_RoleId",
                schema: "SystemManagement",
                table: "RoleMenu");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenuAction_Action_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenuAction_Menu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenuAction");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenuAction_Role_RoleId",
                schema: "SystemManagement",
                table: "RoleMenuAction");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMutation_Item_ItemId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMutation_UoM_UomId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMutation_UoMConversion_BaseUnit",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMutation_UoMConversion_UnitId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMutation_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropForeignKey(
                name: "FK_Supplier_SupplierType_TypeId",
                schema: "General",
                table: "Supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockDetail_Item_ItemId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockDetail_TransferStockHeader_Code",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockDetail_UoM_UomId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockDetail_UoMConversion_UnitId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockHeader_Warehouse_WarehouseCodeFrom",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferStockHeader_Warehouse_WarehouseCodeTo",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_UoMConversion_UoM_UomId",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee_EmployeeId",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_RoleId",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Employee_DriverId",
                schema: "General",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_VehicleType_TypeId",
                schema: "General",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseQuantity_Item_ItemId",
                schema: "Inventory",
                table: "WarehouseQuantity");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseQuantity_Warehouse_WarehouseCode",
                schema: "Inventory",
                table: "WarehouseQuantity");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseQuantity_ItemId",
                schema: "Inventory",
                table: "WarehouseQuantity");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseQuantity_WarehouseCode",
                schema: "Inventory",
                table: "WarehouseQuantity");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_DriverId",
                schema: "General",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_TypeId",
                schema: "General",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_User_EmployeeId",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_RoleId",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_UoMConversion_UomId",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockHeader_WarehouseCodeFrom",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockHeader_WarehouseCodeTo",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockDetail_Code",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockDetail_ItemId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockDetail_UnitId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransferStockDetail_UomId",
                schema: "Inventory",
                table: "TransferStockDetail");

            migrationBuilder.DropIndex(
                name: "IX_Tax_CoaCode",
                schema: "General",
                table: "Tax");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_TypeId",
                schema: "General",
                table: "Supplier");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_BaseUnit",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_ItemId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_UnitId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_UomId",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_WarehouseCode",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_SequenceNumber_Code",
                schema: "SystemManagement",
                table: "SequenceNumber");

            migrationBuilder.DropIndex(
                name: "IX_SequenceNumber_Format",
                schema: "SystemManagement",
                table: "SequenceNumber");

            migrationBuilder.DropIndex(
                name: "IX_RoleMenuAction_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction");

            migrationBuilder.DropIndex(
                name: "IX_RoleMenuAction_MenuId",
                schema: "SystemManagement",
                table: "RoleMenuAction");

            migrationBuilder.DropIndex(
                name: "IX_RoleMenu_MenuId",
                schema: "SystemManagement",
                table: "RoleMenu");

            migrationBuilder.DropIndex(
                name: "IX_MenuAction_ActionId",
                schema: "SystemManagement",
                table: "MenuAction");

            migrationBuilder.DropIndex(
                name: "IX_ItemGroupSubGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemGroupSubGroup");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategory_GroupId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropIndex(
                name: "IX_Item_CategoryId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_PurchaseTaxId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_SalesTaxId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UomBuyId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UomId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UomSellId",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_Code",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_ItemId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_UnitId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_UomId",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanUndeliveredItem_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanUndeliveredItem");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanHeader_DriverId",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanHeader_VehicleId",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanHeader_WarehouseCode",
                schema: "Sales",
                table: "DeliveryPlanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanDetail_Code",
                schema: "Sales",
                table: "DeliveryPlanDetail");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAddress_Code",
                schema: "General",
                table: "CustomerAddress");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AreaId1",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AreaId2",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AreaId3",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AreaId4",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AreaId5",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_BillingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_ShippingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_TypeId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentHeader_WarehouseCode",
                schema: "Inventory",
                table: "AdjustmentHeader");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentDetailDiffUnit_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentDetail_Code",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentDetail_ItemId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentDetail_UnitId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropIndex(
                name: "IX_AdjustmentDetail_UomId",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.AlterColumn<string>(
                name: "SaleUnit",
                schema: "Sales",
                table: "PromoDetailTier",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
