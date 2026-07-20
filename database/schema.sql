CREATE TABLE Trainer (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    origin_city VARCHAR(100) NOT NULL
);

CREATE TABLE Pokemon (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    level INT NOT NULL CHECK (level >= 1 AND level <= 100),
    trainer_id INT NOT NULL,
    CONSTRAINT FK_Pokemon_Trainer FOREIGN KEY (trainer_id) REFERENCES Trainer(id)
);

CREATE TABLE Registration (
    id INT IDENTITY(1,1) PRIMARY KEY,
    pokemon_id INT NOT NULL,
    training_plan VARCHAR(50) NOT NULL CHECK (training_plan IN ('Ginasio Local', 'Liga Regional', 'Elite dos 4')),
    init_date DATE NOT NULL,
    end_date DATE NULL,
    status VARCHAR(20) NOT NULL CHECK (status IN ('Ativa', 'Cancelada', 'Concluida')),
    monthly_value DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_Registration_Pokemon FOREIGN KEY (pokemon_id) REFERENCES Pokemon(id)
);