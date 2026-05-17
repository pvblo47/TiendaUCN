# Diagrama Entidad-Relación (ER) - TiendaUCN

A continuación se presenta el diagrama Entidad-Relación generado a partir de los modelos definidos en `src/Domain/Models` y `src/Domain/Product`.

```mermaid
erDiagram
    PRODUCT {
        int Id PK
        string Name
        string Description
        int Price
        int Stock
        int BrandId FK
        int CategoryId FK
        bool IsActive
        DateTime CreatedAt
        bool IsDeleted
    }
    BRAND {
        int Id PK
        string Name
        string Description
        bool IsDeleted
    }
    CATEGORY {
        int Id PK
        string Name
        string Description
        bool IsDeleted
    }
    IMAGE {
        int Id PK
        string ImageUrl
        string PublicId
        int ProductId FK
    }
    CART {
        int Id PK
        int TotalPrice
        string BuyerId
        int UserId FK "Nullable"
    }
    CARTITEM {
        int Id PK
        int Quantity
        int CartId FK
        int ProductId FK
    }
    ORDER {
        int Id PK
        string Code
        DateTime TransactionDate
        int TotalPrice
        int UserId FK
    }
    ORDERITEM {
        int Id PK
        int Quantity
        string NameAtMoment
        string DescriptionAtMoment
        int UnitPriceAtMoment
        string ImageUrlAtMoment
        int SubtotalPrice
        int OrderId FK
    }
    USER {
        int Id PK
        string Name
        string Email
        bool EmailConfirmed
        string Rut
        string PhoneNumber
        DateTime BirthDate
        string Gender
        string PasswordHash
        int RoleId FK
        DateTime CreatedAt
        bool IsDeleted
    }
    ROLE {
        int Id PK
        string Name
        bool IsDeleted
    }
    VERIFICATIONCODE {
        int Id PK
        string Code
        DateTime Expiry
        int FailedAttempts
        DateTime DateToResend
        int UserId FK
    }

    %% Relaciones
    BRAND ||--o{ PRODUCT : "has many"
    CATEGORY ||--o{ PRODUCT : "has many"
    PRODUCT ||--o{ IMAGE : "has many"
    
    CART ||--o{ CARTITEM : "has many"
    PRODUCT ||--o{ CARTITEM : "in"
    USER |o--o| CART : "owns (1 or 0)"
    
    ORDER ||--o{ ORDERITEM : "has many"
    USER ||--o{ ORDER : "places"
    
    ROLE ||--o{ USER : "assigned to"
    USER ||--|| VERIFICATIONCODE : "has"
```

## Detalles de las Relaciones
- **Product & Brand / Category**: Un `Product` pertenece a una única `Brand` y a una única `Category`. Una `Brand` o `Category` puede tener muchos `Product`.
- **Product & Image**: Un `Product` puede tener muchas `Image`.
- **Cart & CartItem**: Un `Cart` contiene muchos `CartItem`.
- **CartItem & Product**: Un `CartItem` hace referencia a un `Product`.
- **User & Cart**: Un `User` puede tener asociado un `Cart` (la relación es opcional del lado del carrito usando `UserId` nullable o `BuyerId` para no autenticados).
- **Order & OrderItem**: Una `Order` contiene múltiples `OrderItem`. Los `OrderItem` guardan un *snapshot* de los datos del producto al momento de la compra (nombre, precio, etc.).
- **User & Order**: Un `User` puede tener muchas `Order`.
- **User & Role**: Un `User` tiene un `Role`. Un `Role` puede ser asignado a muchos `User`.
- **User & VerificationCode**: Un `User` tiene asociado un `VerificationCode` para procesos como la validación del correo electrónico.
