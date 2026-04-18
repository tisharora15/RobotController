-- Robot Controller API — initial schema
-- Runs automatically when Postgres container starts for the first time

CREATE TABLE IF NOT EXISTS robotcommand (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(50)  NOT NULL UNIQUE,
    description     VARCHAR(800),
    ismovecommand   BOOLEAN      NOT NULL DEFAULT FALSE,
    createddate     TIMESTAMP    NOT NULL DEFAULT NOW(),
    modifieddate    TIMESTAMP    NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS map (
    id           SERIAL PRIMARY KEY,
    columns      INTEGER      NOT NULL,
    rows         INTEGER      NOT NULL,
    name         VARCHAR(50)  NOT NULL,             
    description  VARCHAR(800),
    createddate  TIMESTAMP    NOT NULL DEFAULT NOW(),
    modifieddate TIMESTAMP    NOT NULL DEFAULT NOW(),
    issquare     BOOLEAN GENERATED ALWAYS AS (rows > 0 AND rows = columns) STORED                                
);

-- Seed data
INSERT INTO robotcommand (name, ismovecommand, description) VALUES
    ('MOVE',  TRUE,  'Move one step forward'),
    ('LEFT',  TRUE,  'Turn 90 degrees left'),
    ('RIGHT', TRUE,  'Turn 90 degrees right'),
    ('BACK',  TRUE,  'Move one step backward'),
    ('PING',  FALSE, 'Ping the robot for status')        
ON CONFLICT (name) DO NOTHING;           

INSERT INTO map (columns, rows, name, description) VALUES                  
    (10, 10, 'Default Map',  'Standard 10x10 grid'),
    (20, 20, 'Large Map',    'Spacious 20x20 grid'),
    (5,  10, 'Narrow Map',   'A narrow corridor map')
ON CONFLICT DO NOTHING;