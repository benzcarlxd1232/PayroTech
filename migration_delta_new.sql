IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505180530_AddPayrollFrequency'
)
BEGIN
    ALTER TABLE [Companies] ADD [PayrollFrequency] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505180530_AddPayrollFrequency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260505180530_AddPayrollFrequency', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    CREATE TABLE [TwoFactorCodes] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Code] nvarchar(10) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TwoFactorCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TwoFactorCodes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    CREATE INDEX [IX_TwoFactorCodes_UserId_IsUsed] ON [TwoFactorCodes] ([UserId], [IsUsed]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506182453_Add2FAAndLockout'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506182453_Add2FAAndLockout', N'9.0.0');
END;

COMMIT;
GO


