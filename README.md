# API de Citas Médicas — SQL Server + Controllers (.NET 10)

Monolito con **Clean Architecture**: una sola aplicación desplegable, pero con las dependencias apuntando siempre hacia adentro. El dominio no conoce a nadie; la API no conoce a la base de datos.

Es el mismo monolito de [monolito-con-minimal-api](https://github.com/LeonelHercules/monolito-con-minimal-api), con dos cambios:

1. **Base de datos: SQL Server** en lugar de PostgreSQL.
2. **API con Controllers** (`[ApiController]`) en lugar de Minimal APIs.

`Citas.Domain` y `Citas.Application` son prácticamente idénticos en los dos repos. Ese es el punto de la arquitectura: cambiar de base de datos y de estilo de API no tocó ni una regla de negocio.

```
CitasMedicas.sln
├── src/
│   ├── Citas.Domain            Entidades y reglas (sin paquetes)
│   ├── Citas.Application       Casos de uso con MediatR y FluentValidation
│   ├── Citas.Infrastructure    EF Core + SQL Server
│   └── Citas.Api               Controllers, manejo de errores, OpenAPI + Scalar
└── tests/
    └── Citas.Domain.Tests      Pruebas del dominio (xUnit)
```

## Requisitos

- Visual Studio 2026, con la carga de trabajo *ASP.NET and web development*.
- SDK de .NET 10, que viene con Visual Studio 2026.
- Un SQL Server. Cualquiera de los tres de la tabla de abajo.

## Cómo abrirlo y correrlo

1. Descomprimí el `.zip` y abrí `CitasMedicas.sln`. Visual Studio restaura los paquetes NuGet solo.
2. Elegí tu SQL Server (siguiente sección). Por defecto usa **LocalDB** y no hay que configurar nada.
3. Creá la migración inicial. En *Tools > NuGet Package Manager > Package Manager Console*:

   ```powershell
   Add-Migration Inicial -Project Citas.Infrastructure -StartupProject Citas.Api -OutputDir Persistence/Migrations
   ```

4. F5. Al arrancar en Development, la API crea la base `CitasDb` y aplica la migración. Se abre Scalar en `http://localhost:5080/scalar`.
5. Probá con `src/Citas.Api/Citas.Api.http`: son las 22 peticiones de la guía, con los mismos códigos esperados.
6. Pruebas: *Test > Run All Tests*.

## Qué SQL Server usar

| Opción | Cadena de conexión en `appsettings.json` |
| --- | --- |
| **LocalDB** (por defecto; viene con Visual Studio) | `Server=(localdb)\\MSSQLLocalDB;Database=CitasDb;Trusted_Connection=True;TrustServerCertificate=True` |
| **SQL Server en Docker** (`docker compose up -d`) | `Server=localhost,1433;Database=CitasDb;User Id=sa;Password=Citas#2026Dev;TrustServerCertificate=True` |
| **Tu SQL Server**, por ejemplo el de tu servidor | `Server=TU_SERVIDOR;Database=CitasDb;User Id=citas_app;Password=...;TrustServerCertificate=True` |

Para revisar si tenés LocalDB: `sqllocaldb info` en la terminal. Si no aparece `MSSQLLocalDB`, instalalo desde *Visual Studio Installer > Modify > Individual components > SQL Server Express LocalDB*.

Para ver la base desde Visual Studio: *View > SQL Server Object Explorer*.

**En producción**, la cadena de conexión no va en `appsettings.json`: va como variable de entorno `ConnectionStrings__Default`. Y la API no se conecta con `sa`, sino con un usuario propio que solo tenga permisos sobre `CitasDb`.

## Qué cambió respecto a la guía

### PostgreSQL → SQL Server (solo `Citas.Infrastructure`)

| | PostgreSQL (guía) | SQL Server (este proyecto) |
| --- | --- | --- |
| Paquete | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Microsoft.EntityFrameworkCore.SqlServer` |
| Nombres | `snake_case` con `EFCore.NamingConventions` | `PascalCase`, la convención de SQL Server |
| Fechas de la agenda | `timestamp without time zone` | `datetime2(0)` |
| Textos sin acentos | `varchar` | `varchar` con `IsUnicode(false)`; el resto queda en `nvarchar` |
| Check constraint | `fecha_hora_fin > fecha_hora_inicio` | `[FechaHoraFin] > [FechaHoraInicio]` |
| Errores transitorios | — | `EnableRetryOnFailure()` reintenta ante cortes de red |

Con SQL Server desaparece la trampa del `DateTime.Kind` que tenía Npgsql: `datetime2` acepta cualquier `DateTime`. Igual se mantiene la convención: la auditoría en UTC y la agenda en hora local de la clínica.

**Un detalle propio de SQL Server:** usar un `Guid` aleatorio como clave primaria agrupada (*clustered*) fragmenta el índice cuando la tabla crece mucho. Para este volumen no importa. En tablas de millones de filas conviene una clave no agrupada (`IsClustered(false)`) o un identificador secuencial.

### Minimal APIs → Controllers (solo `Citas.Api`)

| Minimal APIs (guía) | Controllers (este proyecto) |
| --- | --- |
| `Endpoints/MedicosEndpoints.cs` con `MapGroup` | `Controllers/MedicosController.cs` con `[Route("api/medicos")]` |
| `ISender` inyectado en cada lambda | `ISender` inyectado una vez en el constructor del controlador |
| `Results.Ok(...)`, `Results.CreatedAtRoute(...)` | `Ok(...)`, `CreatedAtAction(...)` |
| `app.MapMedicosEndpoints()` | `builder.Services.AddControllers()` + `app.MapControllers()` |
| Requests definidos dentro del archivo de endpoints | `Contracts/Requests.cs` |

Los controladores siguen siendo delgados: traducen HTTP a un mensaje de MediatR y nada más. Toda la lógica sigue en Application y en el dominio.

**Una diferencia importante:** con `[ApiController]` y nullable activado, MVC trata cada `string` no-nullable como `[Required]` y responde 400 por su cuenta, antes de llegar a FluentValidation. En `Program.cs` eso se desactiva con `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`, para que la validación viva en un solo lugar, igual que en la guía. `[ApiController]` sigue respondiendo 400 si el JSON está mal formado.

## Versiones

| Paquete | Versión | Nota |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore.*` | 10.0.15 | **No bajar de 10.0.15**: las anteriores resuelven `Microsoft.Data.SqlClient` 6.1.1, que arrastra un `System.Security.Cryptography.Pkcs` con vulnerabilidad conocida ([GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx)) |
| `MediatR` | 12.4.1 | **No actualizar a 13+**: desde la v13 la licencia es comercial |
| `FluentValidation.DependencyInjectionExtensions` | 12.2.0 | |
| `Scalar.AspNetCore` | 2.* | Toma la última 2.x |

## Errores comunes

| Síntoma | Causa | Solución |
| --- | --- | --- |
| `A network-related or instance-specific error` al arrancar | LocalDB no está instalado, o el servidor de la cadena no responde | `sqllocaldb info`, o revisá la cadena de conexión |
| `Login failed for user` | Usuario o contraseña incorrectos, o el usuario no tiene permiso para crear la base | En desarrollo, LocalDB o `sa`. En un servidor real, pedile al DBA que cree `CitasDb` |
| La API arranca pero no hay tablas | No creaste la migración (paso 3) | `Add-Migration Inicial ...` y volvé a arrancar |
| `The model for context 'AppDbContext' has pending changes` | Cambiaste una entidad o configuración sin migración | `Add-Migration <NombreDelCambio>` |
| `The certificate chain was issued by an authority that is not trusted` | SQL Server sin un certificado de confianza | `TrustServerCertificate=True` en desarrollo; en producción, un certificado válido |
| Aviso de licencia de MediatR en el log | Se actualizó MediatR a 13+ | Volvé a 12.4.1 |
