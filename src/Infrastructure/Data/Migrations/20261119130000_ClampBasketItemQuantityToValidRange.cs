using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

#nullable disable

namespace Microsoft.eShopWeb.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Clamps stored basket quantities into 0..<see cref="BasketItem.MaxQuantity"/>, the range
    /// the basket and checkout pages accept. Rows above the max were saved before the cap
    /// existed (#360); negative rows came from the AddQuantity overflow fixed in #354.
    /// </summary>
    public partial class ClampBasketItemQuantityToValidRange : Migration
    {
        // Captured as a literal rather than read from BasketItem.MaxQuantity: a migration must
        // keep doing what it did when it was written, even if the constant changes later.
        private const int MaxQuantity = 999;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"UPDATE [BasketItems] SET [Quantity] = {MaxQuantity} WHERE [Quantity] > {MaxQuantity};");

            // A zero-quantity line is valid, and the next basket update removes it.
            migrationBuilder.Sql(
                "UPDATE [BasketItems] SET [Quantity] = 0 WHERE [Quantity] < 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the original quantities are not recoverable, and every
            // clamped value is valid under the earlier schema too.
        }
    }
}
