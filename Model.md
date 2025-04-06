## Identity Tables (auto-managed by ASP.NET Identity)

These are the default tables (names may vary slightly based on configuration):

| Table Name               | Description |
|--------------------------|-------------|
| `AspNetUsers`            | Stores user accounts (Buyers, Sellers, Admins). Extend this to add role-specific info. |
| `AspNetRoles`            | Stores role definitions (Buyer, Seller, Admin). |
| `AspNetUserRoles`        | Many-to-many mapping between users and roles. |
| `AspNetUserClaims`       | Stores claims for users. |
| `AspNetRoleClaims`       | Stores claims for roles. |
| `AspNetUserLogins`       | External login info (e.g., Google, Facebook). |
| `AspNetUserTokens`       | Auth tokens, refresh tokens, etc. |

## Real Estate Platform Tables

### 1. **Properties**

| Column              | Type               | Description                             |
|---------------------|--------------------|-----------------------------------------|
| Id                  | Guid / int         | Primary key                             |
| Title               | string             | Listing title                           |
| Description         | string             | Full description                        |
| Price               | decimal            | Price of property                       |
| Area                | float              | Size in m²                              |
| PropertyType        | enum/string        | House, Apartment, etc.                  |
| Address             | string             | Full address                            |
| City                | string             | City name                               |
| Latitude            | double             | For maps/geolocation                    |
| Longitude           | double             | For maps/geolocation                    |
| CreatedAt           | DateTime           | Timestamp                               |
| UpdatedAt           | DateTime?          | Timestamp                               |
| OwnerId             | string (FK)        | Foreign key to `AspNetUsers`           |
| IsActive            | bool               | Active/inactive listing                 |
| IsVerified          | bool               | Verified by admin                       |

---

### 2. **PropertyImages**

| Column        | Type       | Description                |
|---------------|------------|----------------------------|
| Id            | int        | Primary key                |
| PropertyId    | FK         | Foreign key to `Properties`|
| ImageUrl      | string     | Image path or URL          |
| IsMain        | bool       | Main image flag            |

---

### 3. **Favorites**

| Column      | Type   | Description |
|-------------|--------|-------------|
| Id          | int    | Primary key |
| UserId      | FK     | User who favorited the property |
| PropertyId  | FK     | Favorited property |
| CreatedAt   | DateTime | Timestamp |

---

### 4. **Messages**

| Column      | Type     | Description                          |
|-------------|----------|--------------------------------------|
| Id          | int      | Primary key                          |
| SenderId    | string   | FK to `AspNetUsers`                 |
| ReceiverId  | string   | FK to `AspNetUsers`                 |
| PropertyId  | int?     | Optional reference to the listing    |
| Content     | string   | Message body                         |
| SentAt      | DateTime | Timestamp                            |
| IsRead      | bool     | Read/unread status                   |


## Relationships

- One `AspNetUser` can own many `Properties`.
- One `Property` can have many `PropertyImages`.
- One `AspNetUser` can favorite many properties.
- One `AspNetUser` can message multiple users.
