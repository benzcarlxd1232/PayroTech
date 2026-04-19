using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class SyncAllModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All columns and tables in this migration were already applied
            // by previous migrations (AddUIUXEnhancements, UpdateIDRequestStatusEnum,
            // AddIDRequestPrintTracking, AddVendorLogTable).
            // This migration is intentionally empty to satisfy the model snapshot sync.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to revert
        }
    }
}
