# MoopelApi Full Project Review Checklist

Based on the file tree you pasted for `C:\Repos\MoopelBackend\MoopelApi`. :contentReference[oaicite:0]{index=0}

---

## 1. Startup and Configuration

### `Program.cs`

- [X] Confirm it only does high-level startup work.
- [X] Confirm it calls startup/configuration setup cleanly.
- [X] Remove temporary debug/test code.
- [X] Check for hardcoded delays or startup hacks.

### `Startup.cs`

- [X] Confirm all services are registered.
- [X] Confirm each interface maps to the correct implementation.
- [X] Confirm JWT/authentication setup is correct.
- [X] Confirm Swagger/OpenAPI setup is correct.
- [X] Confirm middleware order is correct.
- [ ] Confirm CORS settings are intentional.
- [ ] Confirm database initialization/seeding does not run accidentally in production.

### `appsettings.json`

- [X] No real secrets committed.
- [X] JWT settings are present.
- [X] Database connection settings are present.
- [X] Environment-specific values are moved to `appsettings.Development.json` or user secrets.

### `MoopelApi.csproj`

- [X] Package references are still needed.
- [X] Nullable/reference settings are intentional.
- [X] Analyzer/code style settings are intentional.
- [X] Docker/MSBuild targets are not doing anything dangerous on every build.

---

## 2. Global Project Files

### `ConstantValues.cs`

- [ ] Every table has all expected fields.
- [ ] Field names match the database diagram.
- [ ] Field names match DTO/request/service usage.
- [ ] Table aliases are unique.
- [ ] `SelectName`, `GetFields`, and generated SQL output are correct.
- [ ] Constraints are defined consistently.
- [ ] Foreign keys are in valid creation order.
- [ ] No duplicate or mismatched constants.

### `Routes.cs`

- [ ] Every controller route has a matching constant.
- [ ] Base paths are consistent.
- [ ] Create/update/delete routes are named consistently.
- [ ] Route parameters match controller method parameters.
- [ ] No plural/singular inconsistencies unless intentional.

### `GlobalExceptionMiddleware.cs`

- [ ] Maps custom exceptions correctly.
- [ ] Does not expose sensitive exception details in production.
- [ ] Logs useful information.
- [ ] Returns consistent JSON response shape.

### `GlobalSuppressions.cs`

- [ ] Suppressions are still justified.
- [ ] Remove old suppressions that are no longer needed.
- [ ] Prefer `.editorconfig` severities when enforcing rules globally.

---

## 3. Controllers

For each controller:

- [ ] Route attributes use `Routes` constants.
- [ ] Uses the correct service interface.
- [ ] Does not contain business/database logic.
- [ ] Accepts `CancellationToken` where useful.
- [ ] Forwards cancellation tokens to services.
- [ ] Validates route/body IDs when needed.
- [ ] Returns appropriate status codes.
- [ ] Uses `[Authorize]` where needed.
- [ ] Uses `[AllowAnonymous]` only where needed.
- [ ] Does not expose sensitive DTO fields.
- [ ] Method names match the route/action purpose.

### Controller Files

- [ ] `Controllers/AuthController.cs`
- [ ] `Controllers/BanksController.cs`
- [ ] `Controllers/FridgesController.cs`
- [ ] `Controllers/GroupsController.cs`
- [ ] `Controllers/ImageController.cs`
- [ ] `Controllers/NotesController.cs`
- [ ] `Controllers/ReportController.cs`
- [ ] `Controllers/SessionsController.cs`
- [ ] `Controllers/StoragesController.cs`
- [ ] `Controllers/TagController.cs`
- [ ] `Controllers/ThemesController.cs`
- [ ] `Controllers/TransactionsController.cs`
- [ ] `Controllers/UserController.cs`
- [ ] `Controllers/UserSettingsController.cs`
- [ ] `Controllers/WorkItemsController.cs`

---

## 4. Services

For each service:

- [ ] Implements every interface method.
- [ ] Method order matches the interface.
- [ ] No leftover `throw new NotImplementedException()`.
- [ ] Opens SQL connections with `await using`.
- [ ] Uses `OpenAsync(cancelToken)` where possible.
- [ ] Uses parameterized SQL only.
- [ ] Forwards `CancellationToken` to SQL calls.
- [ ] Uses transactions only when needed.
- [ ] Rolls back transactions on failure.
- [ ] Checks rows affected for update/delete operations.
- [ ] Throws meaningful custom exceptions.
- [ ] Uses consistent logging messages.
- [ ] Does not log secrets/passwords/tokens.
- [ ] Maps DTOs using `DtoReaders` instead of repeated reader code.
- [ ] Handles `null` SQL results correctly.

### Service Files

- [ ] `Services/AuthService.cs`
- [ ] `Services/BankService.cs`
- [ ] `Services/ContextService.cs`
- [ ] `Services/FridgeService.cs`
- [ ] `Services/GroupService.cs`
- [ ] `Services/ImageService.cs`
- [ ] `Services/NoteService.cs`
- [ ] `Services/NotificationService.cs`
- [ ] `Services/SessionService.cs`
- [ ] `Services/StorageService.cs`
- [ ] `Services/TagService.cs`
- [ ] `Services/ThemeService.cs`
- [ ] `Services/UserService.cs`
- [ ] `Services/WorkItemService.cs`

---

## 5. Service Interfaces

For each interface:

- [ ] Interface matches the service implementation.
- [ ] Method names are clear.
- [ ] Return types are consistent.
- [ ] Methods that read many rows accept `CancellationToken`.
- [ ] Methods that mutate data return either the updated DTO or `Task`, consistently.
- [ ] No unused methods.

### Interface Files

- [ ] `Services/Interfaces/IAuthService.cs`
- [ ] `Services/Interfaces/IBankService.cs`
- [ ] `Services/Interfaces/IContextService.cs`
- [ ] `Services/Interfaces/IFridgeService.cs`
- [ ] `Services/Interfaces/IGroupService.cs`
- [ ] `Services/Interfaces/IImageService.cs`
- [ ] `Services/Interfaces/INoteService.cs`
- [ ] `Services/Interfaces/INotificationService.cs`
- [ ] `Services/Interfaces/ISessionService.cs`
- [ ] `Services/Interfaces/IStorageService.cs`
- [ ] `Services/Interfaces/ITagService.cs`
- [ ] `Services/Interfaces/IThemeService.cs`
- [ ] `Services/Interfaces/IUserService.cs`
- [ ] `Services/Interfaces/IWorkItemService.cs`

---

## 6. DTO Models

For each DTO:

- [ ] Property names match database fields where appropriate.
- [ ] Nullable properties match database nullability.
- [ ] Dates use UTC naming where appropriate.
- [ ] Enums are used instead of raw strings where appropriate.
- [ ] Sensitive fields are not returned from read DTOs.
- [ ] DTOs are not accidentally being used as create/update requests.

### DTO Files

- [ ] `Models/Dto/BankAccount.cs`
- [ ] `Models/Dto/Fridge.cs`
- [ ] `Models/Dto/FridgeItem.cs`
- [ ] `Models/Dto/Group.cs`
- [ ] `Models/Dto/GroupTag.cs`
- [ ] `Models/Dto/GroupUser.cs`
- [ ] `Models/Dto/Image.cs`
- [ ] `Models/Dto/Login.cs`
- [ ] `Models/Dto/MoopelImage.cs`
- [ ] `Models/Dto/Note.cs`
- [ ] `Models/Dto/Session.cs`
- [ ] `Models/Dto/Storage.cs`
- [ ] `Models/Dto/StorageItem.cs`
- [ ] `Models/Dto/Tag.cs`
- [ ] `Models/Dto/Theme.cs`
- [ ] `Models/Dto/Transaction.cs`
- [ ] `Models/Dto/TransactionItem.cs`
- [ ] `Models/Dto/User.cs`
- [ ] `Models/Dto/UserSettings.cs`
- [ ] `Models/Dto/UserTag.cs`
- [ ] `Models/Dto/WorkItem.cs`

### Read DTO Files

- [ ] `Models/Dto/Read/FridgeRead.cs`
- [ ] `Models/Dto/Read/StorageRead.cs`
- [ ] `Models/Dto/Read/TransactionRead.cs`
- [ ] `Models/Dto/Read/UserRead.cs`

---

## 7. Request Models

For each request model:

- [ ] Required fields are non-nullable.
- [ ] Optional fields are nullable.
- [ ] IDs are not included in body models when they come from the route.
- [ ] Create requests contain only creation fields.
- [ ] Update requests contain only editable fields.
- [ ] Validation attributes are added where useful.

### Creation Requests

- [ ] `Models/Requests/Creation/BankAccountCreateRequest.cs`
- [ ] `Models/Requests/Creation/FridgeCreateRequest.cs`
- [ ] `Models/Requests/Creation/FridgeItemCreateRequest.cs`
- [ ] `Models/Requests/Creation/GroupCreateRequest.cs`
- [ ] `Models/Requests/Creation/NoteCreationRequest.cs`
- [ ] `Models/Requests/Creation/StorageCreateRequest.cs`
- [ ] `Models/Requests/Creation/TagCreateRequest.cs`
- [ ] `Models/Requests/Creation/ThemeCreateRequest.cs`
- [ ] `Models/Requests/Creation/TransactionCreateRequest.cs`
- [ ] `Models/Requests/Creation/TransactionItemCreateRequest.cs`
- [ ] `Models/Requests/Creation/UserSettingsCreateRequest.cs`
- [ ] `Models/Requests/Creation/WorkItemCreateRequest.cs`
- [ ] `Models/Requests/Creation/WorkItemLinkRequest.cs`

### Update Requests

- [ ] `Models/Requests/Update/BankAccountUpdateRequest.cs`
- [ ] `Models/Requests/Update/FridgeItemUpdateRequest.cs`
- [ ] `Models/Requests/Update/FridgeUpdateRequest.cs`
- [ ] `Models/Requests/Update/GroupUpdateRequest.cs`
- [ ] `Models/Requests/Update/NoteUpdateRequest.cs`
- [ ] `Models/Requests/Update/StorageUpdateRequest.cs`
- [ ] `Models/Requests/Update/TagUpdateRequest.cs`
- [ ] `Models/Requests/Update/ThemeUpdateRequest.cs`
- [ ] `Models/Requests/Update/TransactionItemUpdateRequest.cs`
- [ ] `Models/Requests/Update/TransactionUpdateRequest.cs`
- [ ] `Models/Requests/Update/UserSettingsUpdateRequest.cs`
- [ ] `Models/Requests/Update/UserUpdateRequest.cs`
- [ ] `Models/Requests/Update/WorkItemUpdateRequest.cs`

---

## 8. Configuration Models

### `Models/Configuration/AppSettings.cs`

- [ ] Represents actual config sections.
- [ ] No unused properties.

### `Models/Configuration/DatabaseConnection.cs`

- [ ] Matches `appsettings.json`.
- [ ] Does not contain secrets directly in source.

### `Models/Configuration/DatabaseInfo.cs`

- [ ] Generates SQL correctly.
- [ ] Handles field sizes correctly.
- [ ] Handles `VARBINARY(MAX)` and `NVARCHAR(MAX)` correctly.
- [ ] Handles nullable/default/identity/primary key fields correctly.

### `Models/Configuration/DeploymentInfo.cs`

- [ ] Confirm it is still useful.
- [ ] Values are environment-specific.

### `Models/Configuration/JwtSettings.cs`

- [ ] Contains issuer, audience, key, and expiration if needed.
- [ ] Used consistently in startup/auth code.

### `Models/Configuration/BearerSecuritySchemeTransformer.cs`

- [ ] Swagger auth works correctly.
- [ ] No duplicate Swagger auth setup elsewhere.

---

## 9. Helpers

### `Helpers/ApiHelper.cs`

- [ ] Methods are actually used.
- [ ] No controller-specific logic hidden here.

### `Helpers/DtoReaders.cs`

- [ ] Every DTO reader uses the correct column names.
- [ ] Handles nullable DB fields safely.
- [ ] Does not depend on fragile column order.
- [ ] Works with aliased joined queries.

### `Helpers/FormatHelper.cs`

- [ ] Formatting methods are consistent.
- [ ] No business rules hidden here.

### `Helpers/RegexHelper.cs`

- [ ] Regex patterns are correct.
- [ ] Regexes are compiled only if useful.
- [ ] Email/password rules are intentional.

### `Helpers/SqlHelper.cs`

- [ ] SQL generation is safe.
- [ ] No string interpolation with user input.
- [ ] Reusable SQL patterns are correct.

### `Helpers/SqlReaderHelper.cs`

- [ ] Handles missing/null fields safely.
- [ ] Does not silently hide reader bugs.
- [ ] Has helpers for nullable values.

---

## 10. Exceptions

### Exception Files

- [ ] `Exceptions/ApiException.cs`
- [ ] `Exceptions/ForbiddenException.cs`
- [ ] `Exceptions/NotFoundException.cs`

### Exception Review

- [ ] Services throw custom exceptions instead of generic ones where appropriate.
- [ ] Global middleware maps these exceptions correctly.
- [ ] Exception messages are user-safe.
- [ ] Permission failures use `ForbiddenException`.
- [ ] Missing records use `NotFoundException`.

---

## 11. Database Notes and Docs

### `Notes/DatabaseDiagram.dbml`

- [ ] Matches `ConstantValues.cs`.
- [ ] Matches DTOs.
- [ ] Matches service SQL queries.
- [ ] Contains all tables.
- [ ] Contains all relationships.
- [ ] Contains all unique constraints.
- [ ] Cascade behavior is intentional.

### `Notes/TerminalCommands.txt`

- [ ] Commands still work.
- [ ] Remove outdated commands.

### `Notes/WhatIWantToAdd.txt`

- [ ] Convert useful items into issues/tasks.
- [ ] Remove completed items.

### `Notes/WhatNeedsToChange.txt`

- [ ] Convert useful items into issues/tasks.
- [ ] Remove completed items.

---

## 12. Docker and Local Database

### `Dockerfile`

- [ ] Still builds successfully.
- [ ] Uses the correct .NET version.
- [ ] Does not copy unnecessary files.
- [ ] Uses the correct entrypoint.

### `sql-docker-compose.yml`

- [ ] SQL Server starts correctly.
- [ ] Password is not a real production password.
- [ ] Container name is intentional.
- [ ] Port `1433` is intentional.
- [ ] Init script waits for SQL Server correctly.
- [ ] Database is created if missing.
- [ ] Recreate behavior is intentional.

---

## 13. Security Pass

- [ ] No secrets in source files.
- [ ] No JWT keys committed.
- [ ] Passwords are always hashed.
- [ ] Login failures are handled safely.
- [ ] Auth endpoints do not leak user info.
- [ ] User IDs from requests are checked against the current user/context.
- [ ] Group access is validated before group-owned data is returned.
- [ ] Delete/update endpoints check ownership/permission.
- [ ] Images/files validate content type and size.
- [ ] Swagger is not overly exposed in production.

---

## 14. SQL Correctness Pass

For every SQL query:

- [ ] Uses parameters.
- [ ] Uses correct table aliases.
- [ ] Uses correct field constants.
- [ ] Uses correct joins.
- [ ] Uses correct `WHERE` clause.
- [ ] Update/delete checks `rowsAffected == 1` where appropriate.
- [ ] Insert returns created ID using `OUTPUT INSERTED...`.
- [ ] No accidental double execution.
- [ ] No `ExecuteScalarAsync()` followed by `ExecuteNonQueryAsync()` on the same insert.
- [ ] Transactions are committed only after all needed operations succeed.
- [ ] Transactions are rolled back on failure.
- [ ] Cancellation token is forwarded.

---

## 15. Logging Pass

For every controller/service method:

- [ ] Has a useful log at the start of important operations.
- [ ] Logs IDs where helpful.
- [ ] Does not log passwords.
- [ ] Does not log JWTs.
- [ ] Does not log connection strings.
- [ ] Does not log image binary data.
- [ ] Uses structured logging.

Example:

```csharp
_logger.LogInformation("Updating group [{GroupId}]", groupId);