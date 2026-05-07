# 🛒 TiendaUCN API

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0-3AC358?style=for-the-badge&logo=entityframework)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Serilog](https://img.shields.io/badge/Serilog-Logging-yellow?style=for-the-badge)

La **API de TiendaUCN** es el backend del ecosistema de la tienda universitaria. Está construida usando **ASP.NET Core (.NET 9.0)** y utiliza una base de datos **SQLite** manejada a través de **Entity Framework Core**.

## 👥 Integrantes del Equipo

- **Matias Peñailillo**
- **Pablo Bravo**

## 💻 Tecnologías y Paquetes Principales

Este proyecto utiliza un stack moderno y actualizado para asegurar rendimiento, escalabilidad y un desarrollo ágil:

- **.NET 9.0 ASP.NET Core**: Framework principal.
- **Entity Framework Core (SQLite)**: ORM y base de datos relacional ligera, ideal para desarrollo rápido y portabilidad.
- **JWT (JSON Web Tokens)**: Protección y autenticación de Endpoints.
- **BCrypt.Net-Next**: Hashing algorítmico robusto y seguro de contraseñas.
- **Mapster**: Mapeo y transformación de entidades de dominio a DTOs con alto rendimiento.
- **Serilog**: Sistema de logueo estructurado, permitiendo trazar operaciones y errores detalladamente.
- **Bogus**: Generación de datos falsos de forma estocástica y masiva (útil en los Seeders).
- **Resend**: Integración para el envío sencillo y moderno de correos electrónicos.
- **DotNetEnv**: Carga automática de variables de entorno desde el archivo `.env`.

## ⚙️ Configuración y Puesta en Marcha

1. **Variables de entorno**: Copia o renombra el archivo `.env.example` a `.env` en la raíz del proyecto y rellena las variables de entorno necesarias (conexiones, secretos, api keys, etc.).
2. **Configuración adicional**: Asegúrate de revisar y/o modificar los parámetros predeterminados en `appsettings.json` o usa `appsettings.example.json` para guiarte.
3. **Migraciones / Base de Datos**: Si utilizas migraciones de EF Core, actualiza al último esquema de base de datos local corriendo:
   ```bash
   dotnet ef database update
   ```
4. **Ejecutar en Desarrollo**: En la raíz de tu proyecto ejecuta:
   ```bash
   dotnet run
   ```

Una vez levantado el servidor (normalmente en `http://localhost:5090` o donde se especifique en los logs), podrás interactuar con la API a través de las rutas expuestas o la especificación de OpenAPI.
