# Testing Conventions

## Test File Organization

Tests are organized into `#region` sections by endpoint or feature:
- Get My Items
- Create Item
- Get Item By Id
- Update Item
- Delete Item

## Test Method Naming

Pattern: `[Endpoint]_[ExpectedBehavior]_[Condition]`

Examples:
- `CreateAccount_ReturnsAccount_WhenValid`
- `MyAccounts_ReturnsList_WhenAuthenticated`
- `AccountById_ReturnsNotFound_WhenMissing`

## Test Structure

All tests follow Arrange/Act/Assert pattern with comments:

```csharp
// Arrange
await _apiFactory.ResetDatabaseAsync();
HttpClient client = _apiFactory.CreateClient();
await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

// Act
HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);
T? result = await response.Content.ReadFromJsonAsync<T>(_cancelToken);

// Assert
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
Assert.NotNull(result);
```

## Common Test Scenarios

1. **Authorization Tests**: Verify 401 Unauthorized when not authenticated
2. **Validation Tests**: Verify correct HTTP status codes (200, 201, 400, 404, 409)
3. **Not Found Tests**: Verify 404 when resource missing
4. **Security Tests**: Verify users cannot access other users' resources
5. **State Tests**: Verify changes persist (create then retrieve)

## Test Attributes

- `[Fact]` - Single test case
- `[Theory]` - Parameterized test with `[MemberData]`
- `[TestDocumentation]` - Documents expected HTTP status and purpose

## Test Data Sources

- `[MemberData(nameof(TestDataClass.TestSet), MemberType = typeof(TestDataClass))]`
- Test data classes provide valid/invalid credential or entity combinations

## Assertions

- HTTP status codes: `Assert.Equal(HttpStatusCode.XXX, response.StatusCode)`
- Response content: `Assert.NotNull(result)`, `Assert.Equal(expected, result.Property)`
- Collections: `Assert.NotEmpty()`, `Assert.Empty()`
- Edge cases: `Assert.True()` with boolean conditions

## Authentication Pattern

- `GenericAuthActions.CreateAuthenticatedClient()` - Sets auth token for subsequent requests
- `GenericAuthActions.Register()` - Creates user account
- `GenericAuthActions.Login()` - Gets auth token

## HTTP Verbs by Endpoint

- Get My Items: `client.GetAsync(Routes.X.MyItems)`
- Create: `client.PostAsJsonAsync(Routes.X.Create, request)`
- Get By Id: `client.GetAsync(Routes.X.Generate.ItemById(id))`
- Update: `client.PutAsJsonAsync(Routes.X.Generate.UpdateItem(id), request)`
- Delete: `client.DeleteAsync(Routes.X.Generate.DeleteItem(id))`

## Magic Numbers and Hardcoded Text

**Never use hardcoded magic numbers or text strings in tests:**

❌ **WRONG:**
```csharp
HttpResponseMessage response = await client.GetAsync($"group/{999999}", _cancelToken);
CreateGroupRequest request = new() { Name = "Projects" };
```

✅ **RIGHT:**
```csharp
// Use Routes.X.Generate functions for endpoints
HttpResponseMessage response = await client.GetAsync(Routes.Group.Generate.GroupById(9999), _cancelToken);

// Use TestData classes for request values
CreateGroupRequest request = GroupTestData.ValidCreateRequest;
```

**Rules:**
- All ID values must be extracted to TestData class constants
- All text/names must be extracted to TestData class constants
- All endpoint construction must use `Routes.X.Generate` methods, never hardcoded URLs
- Test data is centralized in `[Entity]TestData.cs` files following the pattern of `BankTestData.cs`
- `[MemberData]` attributes reference TheoryData collections from TestData classes for parameterized tests
