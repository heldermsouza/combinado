using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combinado.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Onboarding_CaixaParceiroCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "caixa",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caixa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    padrao = table.Column<bool>(type: "boolean", nullable: false),
                    caixa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_categoria_caixa_caixa_id",
                        column: x => x.caixa_id,
                        principalSchema: "public",
                        principalTable: "caixa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parceiro",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    numero_whatsapp = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    email_primario = table.Column<bool>(type: "boolean", nullable: false),
                    convidado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vinculado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    caixa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parceiro", x => x.id);
                    table.ForeignKey(
                        name: "fk_parceiro_caixa_caixa_id",
                        column: x => x.caixa_id,
                        principalSchema: "public",
                        principalTable: "caixa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_categoria_caixa_slug",
                schema: "public",
                table: "categoria",
                columns: new[] { "caixa_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parceiro_caixa_id",
                schema: "public",
                table: "parceiro",
                column: "caixa_id");

            migrationBuilder.CreateIndex(
                name: "ux_parceiro_numero_whatsapp",
                schema: "public",
                table: "parceiro",
                column: "numero_whatsapp",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categoria",
                schema: "public");

            migrationBuilder.DropTable(
                name: "parceiro",
                schema: "public");

            migrationBuilder.DropTable(
                name: "caixa",
                schema: "public");
        }
    }
}
