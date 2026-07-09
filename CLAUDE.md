# CLAUDE.md

Este fichero proporciona contexto a Claude Code (claude.ai/code) para trabajar con el código de este repositorio.

## Contexto

Este es el repositorio del microservicio **Pedidos**, parte de un PoC de .NET Aspire en modo multirepo. La arquitectura global está documentada en `../CLAUDE.md` (carpeta padre). Este repo debe estar como carpeta hermana de `Orchestrator/` y `Clientes/` porque `PedidosApi.csproj` tiene una `ProjectReference` a `../../../Orchestrator/src/MicroserviciosConAspire.ServiceDefaults/`.

Este servicio **no se ejecuta de forma autónoma** en el desarrollo habitual — lo lanza el AppHost de Aspire del repo `Orchestrator`, que inyecta la cadena de conexión a PostgreSQL, las credenciales de RabbitMQ y la URL de ClientesApi como variables de entorno/service discovery.

## Estructura de la solución

```
MicroservicioPedidos.slnx
src/
  PedidosApi/           # ASP.NET Core Web API (.NET 10)
  PedidosApplication/   # Lógica de aplicación: servicios, mensajería, DTOs (librería de clases, .NET 10)
  PedidosData/          # Capa de datos EF Core (librería de clases, .NET 10)
```

`PedidosApi` solo depende de `PedidosApplication` y `ServiceDefaults`. `PedidosApplication` depende de `PedidosData` y contiene toda la lógica de negocio, el consumidor RabbitMQ y la integración HTTP con ClientesApi.

## Comandos de compilación y ejecución

```bash
# Compilar la solución
dotnet build MicroservicioPedidos.slnx

# Ejecutar de forma autónoma (requiere variables de entorno manuales; preferible usar el AppHost de Aspire)
dotnet run --project src/PedidosApi/PedidosApi.csproj
```

## Migraciones de EF Core

El proyecto de inicio para las migraciones es `PedidosApi`; el proyecto de migraciones es `PedidosData`.

```bash
# Añadir una nueva migración
dotnet ef migrations add <NombreMigración> --project src/PedidosData --startup-project src/PedidosApi

# Aplicar migraciones manualmente (se aplican automáticamente en Development al arrancar)
dotnet ef database update --project src/PedidosData --startup-project src/PedidosApi
```

Las migraciones están en `src/PedidosData/Migrations/`. En entorno `Development`, `db.Database.Migrate()` se llama automáticamente en `Program.cs` al arrancar.

## Puntos arquitectónicos clave

- **DTO de entrada**: el controlador recibe `PedidoRequest { IdCliente, Fecha, Total }` — el campo `Cliente` (nombre desnormalizado) **no forma parte del request**; siempre se resuelve internamente mediante una llamada HTTP a ClientesApi (`GET /api/cliente/{id}`).
- **Resolución del nombre del cliente**: `PedidoService.GetNombreCliente` usa `JsonDocument` para leer la propiedad `"nombre"` (minúsculas) de la respuesta JSON de ClientesApi. Depende de que ASP.NET Core serialice en camelCase por defecto; si se cambia la serialización de ClientesApi, hay que actualizar esta lectura.
- **`ClienteEventConsumer`** (`BackgroundService`): al arrancar declara el exchange, la cola `pedidos.cliente-events-queue` y el binding con pattern `cliente.*`. Para resolver `IPedidoService` (scoped) desde el hosted service (singleton), usa `IServiceScopeFactory` por cada mensaje — patrón obligatorio para evitar errores de lifetime.
- **ACK manual**: el consumidor confirma (`BasicAck`) o rechaza y reencola (`BasicNack`) los mensajes explícitamente; `autoAck` está desactivado.
- **Contrato de eventos** (`PedidosApplication.Events.ClienteEventContract`): exchange `clientes-exchange` (topic), cola `pedidos.cliente-events-queue`, pattern `cliente.*`. Este contrato está **duplicado** en el repo de Clientes — ambas copias deben mantenerse sincronizadas manualmente si cambian el exchange, las routing keys o el payload.
- `builder.AddServiceDefaults()` y `app.MapDefaultEndpoints()` provienen del proyecto compartido `MicroserviciosConAspire.ServiceDefaults` del repo Orchestrator.
