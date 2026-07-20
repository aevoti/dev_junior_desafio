IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE TABLE [Trainers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(320) NOT NULL,
        [City] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_Trainers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE TABLE [TrainingPlans] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [MonthlyPrice] decimal(10,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_TrainingPlans] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE TABLE [Pokemons] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Level] int NOT NULL,
        [TrainerId] int NOT NULL,
        CONSTRAINT [PK_Pokemons] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Pokemon_Level] CHECK ([Level] BETWEEN 1 AND 100),
        CONSTRAINT [CK_Pokemon_Type] CHECK ([Type] IN ('Normal', 'Fire', 'Water', 'Grass', 'Electric', 'Ice', 'Fighting', 'Poison', 'Ground', 'Flying', 'Psychic', 'Bug', 'Rock', 'Ghost', 'Dragon', 'Dark', 'Steel', 'Fairy')),
        CONSTRAINT [FK_Pokemons_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int NOT NULL IDENTITY,
        [PokemonId] int NOT NULL,
        [TrainingPlanId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [MonthlyPrice] decimal(10,2) NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_Pokemons_PokemonId] FOREIGN KEY ([PokemonId]) REFERENCES [Pokemons] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_TrainingPlans_TrainingPlanId] FOREIGN KEY ([TrainingPlanId]) REFERENCES [TrainingPlans] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'MonthlyPrice', N'Name') AND [object_id] = OBJECT_ID(N'[TrainingPlans]'))
        SET IDENTITY_INSERT [TrainingPlans] ON;
    EXEC(N'INSERT INTO [TrainingPlans] ([Id], [Description], [MonthlyPrice], [Name])
    VALUES (1, N''Treinos básicos'', 50.0, N''Ginásio Local''),
    (2, N''Treinos intermediários + batalhas simuladas'', 120.0, N''Liga Regional''),
    (3, N''Preparação completa para a Liga'', 300.0, N''Elite dos 4'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'MonthlyPrice', N'Name') AND [object_id] = OBJECT_ID(N'[TrainingPlans]'))
        SET IDENTITY_INSERT [TrainingPlans] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrollments_TrainingPlanId] ON [Enrollments] ([TrainingPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Enrollment_PokemonId_Open] ON [Enrollments] ([PokemonId]) WHERE [EndDate] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pokemons_TrainerId] ON [Pokemons] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Trainers_Email] ON [Trainers] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720121134_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720121134_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720190209_AddEnrollmentTrainerSnapshot'
)
BEGIN
    ALTER TABLE [Enrollments] ADD [TrainerId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720190209_AddEnrollmentTrainerSnapshot'
)
BEGIN

                    UPDATE e
                    SET e.TrainerId = p.TrainerId
                    FROM [Enrollments] e
                    INNER JOIN [Pokemons] p ON p.Id = e.PokemonId;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720190209_AddEnrollmentTrainerSnapshot'
)
BEGIN
    CREATE INDEX [IX_Enrollments_TrainerId] ON [Enrollments] ([TrainerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720190209_AddEnrollmentTrainerSnapshot'
)
BEGIN
    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Trainers_TrainerId] FOREIGN KEY ([TrainerId]) REFERENCES [Trainers] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720190209_AddEnrollmentTrainerSnapshot'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720190209_AddEnrollmentTrainerSnapshot', N'8.0.11');
END;
GO

COMMIT;
GO

