# AspireMultirepo.Pedidos

Microservicio de **gestión de Pedidos**, implementado como prueba de concepto para validar la orquestación de **.NET Aspire en modo multirepo**. Forma parte de una solución compuesta por cuatro repositorios independientes: este microservicio, [AspireMultirepo.Clientes](https://github.com/RublenX/AspireMultirepo.Clientes), [AspireMultirepo.Orchestrator](https://github.com/RublenX/AspireMultirepo.Orchestrator) (AppHost de Aspire) y [AspireMultirepo.Portal](https://github.com/RublenX/AspireMultirepo.Portal) (frontend React).

> Este repositorio no se ejecuta de forma aislada en el flujo normal: es orquestado por `AspireMultirepo.Orchestrator`, que además de este microservicio levanta PostgreSQL, RabbitMQ y el microservicio de Clientes. Para ejecutar la solución completa, ver el README de ese repositorio.

## Stack técnico

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** + **Npgsql** (PostgreSQL)
- **RabbitMQ.Client** para consumo de eventos
- `IHttpClientFactory` con *service discovery* de Aspire para llamar al microservicio de Clientes
- **.NET Aspire ServiceDefaults** (service discovery, resiliencia de `HttpClient`, health checks, OpenTelemetry), aportados por `MicroserviciosConAspire.ServiceDefaults` del repositorio Orchestrator
- Swagger / OpenAPI (en entorno de desarrollo)

## Estructura del repositorio

```
src/
├── PedidosApi/            Web API: controlador y arranque
├── PedidosApplication/    Lógica de negocio: servicio de pedidos, DTOs, consumidor de eventos de Cliente
└── PedidosData/           Acceso a datos: DbContext de EF Core, entidad Pedido, repositorio y migraciones
```

## Modelo de datos

`Pedido`: `Id`, `IdCliente`, `Cliente` (nombre del cliente, desnormalizado), `Fecha`, `Total`.

## Endpoints (`api/Pedidos`)

| Verbo | Ruta | Descripción |
|---|---|---|
| GET | `/api/pedidos` | Lista todos los pedidos |
| GET | `/api/pedidos/{id}` | Obtiene un pedido por Id |
| POST | `/api/pedidos` | Crea un pedido |
| PUT | `/api/pedidos/{id}` | Actualiza un pedido |
| DELETE | `/api/pedidos/{id}` | Elimina un pedido |

## Integración con Clientes

Este microservicio combina dos formas de comunicación con `AspireMultirepo.Clientes`:

1. **HTTP síncrono** (`PedidoService`): al crear un pedido o cambiar el cliente de uno existente, se invoca `GET /api/cliente/{id}` mediante un `HttpClient` con nombre lógico `"Clientes"`, resuelto por *service discovery* de Aspire (`https+http://clientesapi`), para obtener y guardar el nombre del cliente desnormalizado en el pedido.
2. **Eventos asíncronos vía RabbitMQ** (`ClienteEventConsumer`, `BackgroundService`): se suscribe al exchange `clientes-exchange` (routing key `cliente.*`) mediante la cola `pedidos.cliente-events-queue`, para:
   - `cliente.actualizado` → actualizar el nombre de cliente en todos sus pedidos (`ActualizarNombreClienteAsync`).
   - `cliente.eliminado` → eliminar todos los pedidos de ese cliente (`EliminarPedidosPorClienteAsync`).

## Configuración

La cadena de conexión a PostgreSQL se resuelve desde `ConnectionStrings:DefaultConnection` y la configuración de RabbitMQ desde la sección `RabbitMq` (`Host`, `Port`, `User`, `Password`); ambas son inyectadas como variables de entorno por el AppHost cuando el servicio se ejecuta orquestado.

