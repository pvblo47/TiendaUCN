# 🛒 Tienda UCN API
Proyecto correspondiente al taller de Backend del ramo de IDWM.


## Tecnologías Utilizadas

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0-3AC358?style=for-the-badge&logo=entityframework)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Serilog](https://img.shields.io/badge/Serilog-Logging-yellow?style=for-the-badge)

- **Framework:** ASP.NET Core 9.0
- **Versionado:** Git + Conventional Commits
- **Base de Datos:** SQLite
- **ORM:** Entity Framework Core
- **Autenticación:** JWT (JSON Web Tokens)
- **Hashing:** BCrypt.Net-Next
- **Mapeo de Datos:** Mapster
- **Logging:** Serilog
- **Mock Data:** Bogus
- **Envío de Correos:** Resend
- **Variables de Entorno:** DotNetEnv
- **Almacenamiento de Imágenes:** Cloudinary

## 👥 Integrantes del Equipo

- **Matias Peñailillo**
- **Pablo Bravo**

## Instalación y Configuración Local

### Requisitos Previos

- **.NET 9 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio Code**: [Download](https://code.visualstudio.com/)
- **Git**: [Download](https://git-scm.com/install/windows)
- **Postman**: [Download](https://www.postman.com/downloads/) (Recomendado para probar los endpoints)

### Instalar extensiones en VsCode
- **C# Dev Kit**
- **C#**
- **.NET Install Tool**
- **C# Extensions**
- **SQLite**

### 1. Clonar el Repositorio

Abre una terminal en el directorio donde desees almacenar este proyecto y ejecuta el siguiente comando:
```bash
git clone https://github.com/pvblo47/TiendaUCN.git
```

Navega a la carpeta del proyecto clonado
```bash
cd .\TiendaUCN\
```

Abre VsCode con el siguiente comando:
```bash
code .
```

### 2. Cambiar de rama (Opcional)

Abre la terminal en VsCode y cambia a la rama de desarrollo (si es necesario trabajar allí):
```bash
git checkout develop
```

### 3. Establecer las variables de entorno

Crear el archivo **.env**, desde la terminal en VsCode ejecuta este comando:
```bash
cp .env.example .env
```

Configurar las variables de **.env**:
```bash
DATA_BASE_URL= Data Source=<nombreBD>.db
RESEND_API_KEY=tu_resend_api_key
JWT_SECRET=your_jwt_secret_key
CLOUDINARY_CLOUD_NAME=your_cloud_name
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret
```
- Reemplace `<nombreBD>` por el nombre que tendrá su base de datos.
- Reemplace `RESEND_API_KEY` con su API key de resend; para ello puede obtener su API key en el siguiente enlace: [Resend - API keys](https://resend.com/api-keys).
- Reemplace `JWT_SECRET` con una clave secreta segura de al menos 32 caracteres.
- Reemplace `CLOUDINARY_CLOUD_NAME` con el nombre de su cloud de Cloudinary; puede encontrarlo en el dashboard de su cuenta en [Cloudinary](https://cloudinary.com/).
- Reemplace `CLOUDINARY_API_KEY` con su API key de Cloudinary; puede crearla en la sección de `settings > API Keys`.
- Reemplace `CLOUDINARY_API_SECRET` con su API secret de Cloudinary; puede extraerla de la API key creada.

### 4. Establecer las configuraciones en appsettings.json

Crear el archivo **appsettings.json**:
```bash
cp appsettings.example.json appsettings.json
```

**En caso de considerar necesario actualizar las siguientes variables en cada sección:**

**Token / Cookies:**
- Reemplace `ExpirationTimeInHours` con la cantidad de horas tras las cuales expirarán los tokens JWT.
- Reemplace `CookieExpirationDays` con la duración en días de la cookie del carrito para usuarios anónimos.

**VerificationCode:**
- Reemplace `ExpirationTimeInMinutes` con el tiempo en minutos para que expire el código de verificación.
- Reemplace `MaxFailedAttempts` con el número máximo de intentos fallidos permitidos, antes de que se bloquee la cuenta del usuario.
- Reemplace `WaitingTimeInMinutesAfterResendEmail` con el tiempo de espera en minutos antes de permitir el reenvio de un nuevo correo de verificación.

**EmailConfiguration**:
- Reemplace `From` con la dirección de salida, se recomienda `Tienda - UCN <onboarding@resend.dev>`. Ten en cuenta que, al usar el dominio de prueba, solo podrás enviar correos a la dirección con la que te registraste en Resend.

**Jobs (Hangfire):**
- Reemplace `CronJobDeleteUnconfirmedUsers` con la expresión cron que define la ejecución automática para eliminar usuarios no verificados. **Valor recomendado:** `30 20 * * *`.
- Reemplace `CronJobDeleteExpiredTokens` con la expresión cron que define la ejecución automática para eliminar tokens expirados. **Valor recomendado:** `30 20 * * *`.
- Reemplace `TimeZone` con la zona horaria utilizada para la ejecución de las tareas programadas. **Valor recomendado:** `Pacific SA Standard Time`.
- Reemplace `DaysToDeleteUnverifiedAccount` con la cantidad de días máximos permitidos antes de eliminar una cuenta no verificada.

**HangfireDashboard:**
- Reemplace `DashboardPath` con la ruta de acceso donde estará disponible el panel de control. **Valor recomendado:** `/hangfire`.

**Products:**
- Reemplace `FewUnitsAvailable` con el límite a partir del cual el stock se considera como "Pocas unidades disponibles".
- Reemplace `DefaultImageUrl` con la URL a usar si el producto no tiene fotos.

**User:**
*Datos del usuario administrador (`AdminUser`)*
- Reemplace `Name`, `Email`, `Rut`, `BirthDate`, `PhoneNumber`, `Gender` con los datos correspondientes.
- Reemplace `Password` con una contraseña alfanumérica segura.

*Contraseña para usuarios aleatorios*
- Reemplace `RandomUserPassword` con una contraseña base segura para los usuarios mockeados por el seeder.

### 5. Instalar Entity Framework
Si no tienes la herramienta instalada globalmente:
```bash
dotnet tool install --global dotnet-ef
```

### 6. Instalar Dependencias

```bash
dotnet restore
```

### 7. Compilar el Proyecto

```bash
dotnet build
```

### 8. Crear Base de datos

Nota: La aplicación aplica automáticamente las migraciones y rellena la base de datos (seeding) al iniciar, pero si quieres forzar la creación manual:
```bash
dotnet ef database update
```

### 9. Ejecutar el Proyecto

```bash
dotnet run
```

El servicio estará disponible en `http://localhost:5090` o en el puerto indicado en la consola.

### 10. Visualizar Base de datos

Abrir opciones en VsCode:
```bash
Shift + Ctrl + p
```
Buscar y presionar **SQLite: Open Database**

Finalmente abrir tu archivo **.db** (ej. `tiendaucn.db`).

---

## 🧪 Pruebas con Postman

Para probar la funcionalidad de los endpoints, puedes utilizar **Postman**:

1. Abre la aplicación de Postman.
2. Los archivos necesarios se encuentran en el directorio `docs/postman` del proyecto.
3. Importa la Colección y el Entorno yendo a `Import` > `Upload Files` (o arrastrando los archivos de `docs/postman`).
4. Selecciona el Entorno importado en la esquina superior derecha de Postman (esto configurará la `baseUrl` automáticamente).
5. **Autenticación**: Muchos endpoints están protegidos. Primero debes enviar una solicitud al endpoint de **Login** para obtener un Token JWT.
6. El Token obtenido se guardará en una variable de entorno para utilizarlo automáticamente en las siguientes solicitudes (o en caso contrario, ve a la pestaña **Authorization** de la solicitud que deseas probar (o en la configuración global de la colección), selecciona el tipo **Bearer Token** y pega tu token ahí).
7. ¡Ya estás listo para enviar solicitudes y probar la API!
