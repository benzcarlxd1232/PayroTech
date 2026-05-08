-- Run this on your production database (WebMSSQL) to see what's already applied
SELECT [MigrationId], [ProductVersion] 
FROM [__EFMigrationsHistory] 
ORDER BY [MigrationId];
