using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microsoft.eShopWeb.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Drops the DEFAULT constraints that earlier migrations left behind when they made these
    /// columns non-nullable with <c>AlterColumn(..., defaultValue: ...)</c>. The model declares
    /// none of them, and without them SQL Server rejects a row that omits a required value
    /// instead of silently storing an empty buyer, an empty address or catalog item 0 (#381).
    /// </summary>
    public partial class DropLeftoverDefaultConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AlterColumn always drops the column's current default constraint, whatever SQL
            // Server named it, and adds one back only when defaultValue is set.
            AlterStringColumns(migrationBuilder, defaultValue: null, oldDefaultValue: "");
            AlterCatalogItemIdColumn(migrationBuilder, defaultValue: null, oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            AlterStringColumns(migrationBuilder, defaultValue: "", oldDefaultValue: null);
            AlterCatalogItemIdColumn(migrationBuilder, defaultValue: 0, oldDefaultValue: null);
        }

        private static void AlterStringColumns(MigrationBuilder migrationBuilder, string defaultValue, string oldDefaultValue)
        {
            AlterStringColumn(migrationBuilder, "Catalog", "Description", maxLength: null, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "OrderItems", "ItemOrdered_PictureUri", maxLength: null, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "OrderItems", "ItemOrdered_ProductName", maxLength: 50, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "BuyerId", maxLength: 256, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "ShipToAddress_City", maxLength: 100, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "ShipToAddress_Country", maxLength: 90, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "ShipToAddress_State", maxLength: 60, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "ShipToAddress_Street", maxLength: 180, defaultValue, oldDefaultValue);
            AlterStringColumn(migrationBuilder, "Orders", "ShipToAddress_ZipCode", maxLength: 18, defaultValue, oldDefaultValue);
        }

        private static void AlterStringColumn(
            MigrationBuilder migrationBuilder,
            string table,
            string name,
            int? maxLength,
            string defaultValue,
            string oldDefaultValue)
        {
            var type = maxLength is null ? "nvarchar(max)" : $"nvarchar({maxLength})";
            migrationBuilder.AlterColumn<string>(
                name: name,
                table: table,
                type: type,
                maxLength: maxLength,
                nullable: false,
                defaultValue: defaultValue,
                oldClrType: typeof(string),
                oldType: type,
                oldMaxLength: maxLength,
                oldNullable: false,
                oldDefaultValue: oldDefaultValue);
        }

        private static void AlterCatalogItemIdColumn(MigrationBuilder migrationBuilder, int? defaultValue, int? oldDefaultValue)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ItemOrdered_CatalogItemId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: defaultValue,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false,
                oldDefaultValue: oldDefaultValue);
        }
    }
}
