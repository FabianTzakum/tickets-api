# Tickets API

API REST profesional construida con ASP.NET Core para la gestión de tickets de soporte técnico.


## Objetivo del proyecto

Tickets API simula un sistema interno de soporte donde administradores y agentes pueden gestionar clientes, tickets, comentarios, prioridades y estados de atención.

## Tecnologías utilizadas

- ASP.NET Core
- .NET 10
- C#
- SQL Server Express
- Entity Framework Core
- JWT Authentication
- Swagger / Swashbuckle
- Visual Studio 2026
- BCrypt para hashing de contraseñas

## Funcionalidades principales

- Autenticación con JWT.
- Protección de endpoints por roles.
- Gestión de usuarios.
- Gestión de clientes.
- Gestión de tickets.
- Comentarios por ticket.
- Cambio de estado de tickets.
- Dashboard operativo.
- Seed automático de datos demo.
- Respuestas API estructuradas.
- Swagger configurado para pruebas.

## Usuario demo

El sistema crea datos demo automáticamente al iniciar si la base de datos está vacía.

Email: admin@tickets.local
Password: Admin12345
Rol: Admin

También se crea un agente demo:

Email: agent@tickets.local
Password: Agent12345
Rol: SupportAgent
Flujo básico de prueba

## Iniciar sesión
POST /api/auth/login

Body:

{
  "email": "admin@tickets.local",
  "password": "Admin12345"
}

## Autorizar Swagger

Copiar el accessToken, presionar Authorize y pegar el token.


## Roles disponibles

| Rol | Valor | Descripción |
|---|---:|---|
| Admin | 1 | Acceso completo |
| SupportAgent | 2 | Gestión de clientes y tickets |
| Client | 3 | Usuario cliente |

## Estados de ticket

| Estado | Valor |
|---|---:|
| Open | 1 |
| InProgress | 2 |
| WaitingClient | 3 |
| Resolved | 4 |
| Closed | 5 |

## Prioridades de ticket

| Prioridad | Valor |
|---|---:|
| Low | 1 |
| Medium | 2 |
| High | 3 |
| Critical | 4 |

## Configuración de base de datos

La cadena de conexión está configurada para SQL Server Express:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TicketsApi;Trusted_Connection=True;TrustServerCertificate=True;"
}
