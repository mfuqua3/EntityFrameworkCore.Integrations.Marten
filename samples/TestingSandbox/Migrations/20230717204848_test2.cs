using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TestingSandbox.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                schema: "public",
                table: "mt_doc_invoice",
                type: "varchar",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "mt_doc_invoice_idx_tenant_id",
                schema: "public",
                table: "mt_doc_invoice",
                column: "tenant_id")
                .Annotation("Npgsql:CreatedConcurrently", false)
                .Annotation("Npgsql:IndexMethod", "btree")
                .Annotation("Npgsql:IndexNullSortOrder", new[] { NullSortOrder.Unspecified });

            migrationBuilder.DropMartenTableFunctions(
                schemaQualifiedTableName: "public.mt_doc_invoice");

            migrationBuilder.CreateMartenTableFunctions(
                schemaQualifiedTableName: "public.mt_doc_invoice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "mt_doc_invoice_idx_tenant_id",
                schema: "public",
                table: "mt_doc_invoice");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "public",
                table: "mt_doc_invoice");

            migrationBuilder.DropMartenTableFunctions(
                schemaQualifiedTableName: "public.mt_doc_invoice");

            migrationBuilder.CreateMartenTableFunctions(
                schemaQualifiedTableName: "public.mt_doc_invoice");
        }
    }
}
