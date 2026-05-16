INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES
('a6b01013-97bf-4b45-9e11-1c0206253e41', 'Administrador', 'ADMINISTRADOR', '1e7f7e14-8c94-4d23-b0b8-1e8b9b4574a3'),
('c8749725-8d21-4daa-9742-47d32c7ff8b2', 'Operador', 'OPERADOR', '0ac385ea-4074-426f-a26b-a3b0ad85db62'),
('b3e43dfe-1234-4bbe-9e44-1011a1a1a1a1', 'Consulta', 'CONSULTA', 'fc7a8ede-2753-4e1d-803e-1d95e4b6d287');

INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
) VALUES
('b17343d7-8acd-42ae-b3d7-52ef5b1c78f1', 'admin@demo.local', 'ADMIN@DEMO.LOCAL', 'admin@demo.local', 'ADMIN@DEMO.LOCAL', true,
 'AQAAAAIAAYagAAAAEHN4J/Jn96dgTNwJsUvAlld6fHBkw/lSE4lKmXSpUwgQD8TrN1k98Z9C+mloK9BeMA==',
 '46b64626-c2a5-4ef9-9f75-bd1cd9cd0d31', 'c1a41d57-7c26-4fc6-a96e-252c5a35d81f', false, false, false, 0),
('3f1a4b6f-9c9b-4cb1-b05e-3d9604a89c2f', 'operador@demo.local', 'OPERADOR@DEMO.LOCAL', 'operador@demo.local', 'OPERADOR@DEMO.LOCAL', true,
 'AQAAAAIAAYagAAAAELz1ZrcfNOWuGWdreX22KmCY6tucYcYFnU0C7cY8T3I0/0Gmn1UdB7MUKiLtZzifvA==',
 '3b1a59e1-7562-4d84-9bf1-0f4b6f6f684e', '7a2c4f8d-bd2f-4c08-9f0b-793d98f6a3df', false, false, false, 0),
('d5a2905f-4faf-4c0d-8a0e-0d87b379ba41', 'consulta@demo.local', 'CONSULTA@DEMO.LOCAL', 'consulta@demo.local', 'CONSULTA@DEMO.LOCAL', true,
 'AQAAAAIAAYagAAAAEPy2bch9NtoGTKGKqmAmUi4rT/zrFgK7VDP/psI/YjLFwPrDaCwPZuRZ2DjBuw9EpA==',
 '58c1a4aa-2f08-4f3e-9b47-5f07a68041f1', '2c240f8f-36ad-4d60-a433-8fb5c0612906', false, false, false, 0);

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES
('b17343d7-8acd-42ae-b3d7-52ef5b1c78f1', 'a6b01013-97bf-4b45-9e11-1c0206253e41'),
('3f1a4b6f-9c9b-4cb1-b05e-3d9604a89c2f', 'c8749725-8d21-4daa-9742-47d32c7ff8b2'),
('d5a2905f-4faf-4c0d-8a0e-0d87b379ba41', 'b3e43dfe-1234-4bbe-9e44-1011a1a1a1a1');

INSERT INTO "Categorias" ("Id", "Nombre", "Activo", "FechaCreacion", "CreadoPor") VALUES
(1, 'Bebidas', true, NOW(), 'admin@demo.local'),
(2, 'Snacks', true, NOW(), 'admin@demo.local');

INSERT INTO "Productos" (
    "Id", "Nombre", "Descripcion", "Precio", "Stock", "CategoriaId", "Activo", "FechaCreacion", "CreadoPor"
) VALUES
(1, 'Agua mineral 600ml', 'Agua sin gas', 1.20, 200, 1, true, NOW(), 'admin@demo.local'),
(2, 'Jugo de naranja 1L', 'Jugo natural', 3.50, 40, 1, true, NOW(), 'admin@demo.local'),
(3, 'Galletas integrales', 'Paquete 250g', 2.80, 60, 2, true, NOW(), 'admin@demo.local');
