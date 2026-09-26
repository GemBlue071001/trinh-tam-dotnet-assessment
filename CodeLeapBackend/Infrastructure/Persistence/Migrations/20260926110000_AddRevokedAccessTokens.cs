using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations;

[DbContext(typeof(ECommerceDbContext))]
[Migration("20260926110000_AddRevokedAccessTokens")]
public partial class AddRevokedAccessTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "revoked_access_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                jwt_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_revoked_access_tokens", token => token.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_revoked_access_tokens_jwt_id",
            table: "revoked_access_tokens",
            column: "jwt_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_revoked_access_tokens_expires_at",
            table: "revoked_access_tokens",
            column: "expires_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "revoked_access_tokens");
    }
}
