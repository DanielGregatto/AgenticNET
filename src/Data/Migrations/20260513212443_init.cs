using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AgenticNET");

            migrationBuilder.CreateTable(
                name: "ANET_Conversation",
                schema: "AgenticNET",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANET_Conversation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ANET_Product",
                schema: "AgenticNET",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ImageFileName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANET_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ANET_ConversationMessage",
                schema: "AgenticNET",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANET_ConversationMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ANET_ConversationMessage_ANET_Conversation_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: "AgenticNET",
                        principalTable: "ANET_Conversation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ANET_ConversationTurn",
                schema: "AgenticNET",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssistantMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TraceSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANET_ConversationTurn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ANET_ConversationTurn_ANET_Conversation_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: "AgenticNET",
                        principalTable: "ANET_Conversation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ANET_Conversation_UserId",
                schema: "AgenticNET",
                table: "ANET_Conversation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ANET_ConversationMessage_ConversationId",
                schema: "AgenticNET",
                table: "ANET_ConversationMessage",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ANET_ConversationTurn_ConversationId",
                schema: "AgenticNET",
                table: "ANET_ConversationTurn",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ANET_Product_Sku",
                schema: "AgenticNET",
                table: "ANET_Product",
                column: "Sku");

            migrationBuilder.CreateIndex(
                name: "IX_ANET_Product_Slug",
                schema: "AgenticNET",
                table: "ANET_Product",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ANET_ConversationMessage",
                schema: "AgenticNET");

            migrationBuilder.DropTable(
                name: "ANET_ConversationTurn",
                schema: "AgenticNET");

            migrationBuilder.DropTable(
                name: "ANET_Product",
                schema: "AgenticNET");

            migrationBuilder.DropTable(
                name: "ANET_Conversation",
                schema: "AgenticNET");
        }
    }
}
