using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLapTrinhWeb.Migrations
{
    /// <inheritdoc />
    public partial class budget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TransactionId",
                table: "Budgets",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Transactions_TransactionId",
                table: "Budgets",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Transactions_TransactionId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TransactionId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Budgets");

            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions",
                column: "BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "BudgetId");
        }
    }
}
