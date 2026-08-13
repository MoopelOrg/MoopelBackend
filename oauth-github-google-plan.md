# OAuth Implementation Plan for GitHub and Google

## Goal

Add sign-in with **GitHub** and **Google** to the existing Moopel backend while preserving the current **Moopel-issued JWT** model for API authorization.

## Current State

- The backend currently authenticates users with `username` + `password` in `MoopelApi\Controllers\AuthController.cs` and `MoopelApi\Services\AuthService.cs`.
- API authorization already uses bearer JWTs configured in `MoopelApi\Startup.cs`.
- Users already have an optional `Email` column and unique constraint in `MoopelApi\ConstantValues.cs`.
- There is no external identity provider configuration, callback flow, provider-user mapping, or account-linking support.

## Recommended Architecture

Keep **JWT bearer auth** as the only API authorization mechanism.

Use GitHub and Google only as **external identity providers**:

1. User clicks **Continue with Google** or **Continue with GitHub** in the frontend.
2. Backend starts OAuth challenge with the selected provider.
3. Provider redirects back to a backend callback endpoint.
4. Backend reads provider claims and identifies the external user.
5. Backend finds or creates the corresponding local Moopel user.
6. Backend issues the same Moopel JWT already used today.
7. Frontend stores that JWT and continues calling the API exactly as it does now.

This approach minimizes disruption because the rest of the application continues to trust only Moopel JWTs.

## Key Product Decisions

These must be decided before implementation because they affect the schema, endpoints, and user flow.

### 1. Account creation policy

Choose one:

- **Recommended:** auto-create a Moopel account on first successful provider login.
- Require the user to already have a Moopel account and then link the provider.
- Allow login only when the provider email matches an existing Moopel account.

**How to implement**

- If auto-create is chosen, callback logic creates a local `User` record when no linked account exists.
- If existing-account-only is chosen, callback returns a controlled failure when no local account can be linked.

### 2. Account linking policy

Choose one:

- **Recommended:** if a verified provider email matches an existing user email, link automatically.
- Require explicit logged-in linking from a settings page.
- Never auto-link; always create separate pending-link flow.

**How to implement**

- Auto-link requires verified-email checks and a unique provider-user mapping table.
- Explicit linking requires authenticated endpoints like `POST auth\link\google` and `POST auth\link\github`.

### 3. Username generation policy

OAuth providers may not return a username that fits local rules or is unique.

Choose one:

- **Recommended:** auto-generate a username and allow later editing.
- Force the user to choose a username before completing first login.

**How to implement**

- Generate from provider nickname/email prefix.
- Normalize to current username rules.
- Add suffixes until unique.

### 4. Email trust policy

Google usually provides verified email. GitHub may require extra API calls and email may be private.

Choose one:

- **Recommended:** trust only verified provider emails.
- Allow account creation without email for providers that do not return one.

**How to implement**

- For Google, read verified email claim from the provider payload.
- For GitHub, request email scope and, if needed, query the user email endpoint to find a verified primary email.

## Implementation Plan

## Phase 1: Add provider configuration

### Step 1. Add OAuth settings models

Create configuration classes for external providers, for example:

- `GoogleOAuthSettings`
- `GitHubOAuthSettings`
- `OAuthFrontendSettings` for redirect targets after successful login

Suggested fields:

- `ClientId`
- `ClientSecret`
- `CallbackPath`
- `Scopes`
- `Enabled`
- `FrontendSuccessRedirectUrl`
- `FrontendFailureRedirectUrl`

**How it would be done**

- Add new files under `MoopelApi\Models\Configuration`.
- Follow the existing `JwtSettings` pattern.
- Bind them from configuration in `Startup.cs`.

### Step 2. Add app configuration entries

Add new sections in `appsettings.json` and environment/secret storage.

**How it would be done**

- Add placeholder settings in `MoopelApi\appsettings.json`.
- Store real secrets in user secrets or deployment secret configuration.
- Do not commit real client secrets.

Example shape:

```json
{
  "OAuth": {
    "Google": {
      "Enabled": true,
      "ClientId": "",
      "ClientSecret": "",
      "CallbackPath": "/auth/google/callback"
    },
    "GitHub": {
      "Enabled": true,
      "ClientId": "",
      "ClientSecret": "",
      "CallbackPath": "/auth/github/callback"
    }
  }
}
```

### Step 3. Add package references

Install the ASP.NET Core authentication handlers required for provider login.

Expected packages:

- `Microsoft.AspNetCore.Authentication.Google`
- a GitHub OAuth handler package if not already available in the target framework/ecosystem

**How it would be done**

- Update `MoopelApi\MoopelApi.csproj`.
- Use the official ASP.NET Core provider for Google.
- For GitHub, choose the provider package that best matches the project’s framework and conventions.

## Phase 2: Extend authentication startup

### Step 4. Register cookie-based temporary external auth state

OAuth challenge/callback flows typically need a temporary sign-in scheme during redirect handling.

**How it would be done**

- In `MoopelApi\Startup.cs`, keep JWT bearer as the API auth scheme.
- Add an external cookie scheme used only during OAuth handshake.
- Add Google and GitHub handlers under `AddAuthentication(...)`.

Important design point:

- JWT stays the scheme used by `[Authorize]` on API requests.
- Cookie auth is only for the provider redirect handshake, not for long-term app auth.

### Step 5. Register Google handler

**How it would be done**

- Configure client ID, client secret, callback path, and scopes.
- Request email/profile scopes.
- Map provider claims needed for downstream identity resolution.

Expected mapped values:

- provider user id
- email
- email verified flag
- display name
- avatar url

### Step 6. Register GitHub handler

**How it would be done**

- Configure client ID, client secret, callback path, and scopes.
- Request user identity and email access.
- Ensure verified email can be retrieved when not present by default.

Expected mapped values:

- provider user id
- login name
- verified email if available
- display name
- avatar url

## Phase 3: Add database support for external identities

### Step 7. Add a user external login table definition

Create a new table to map Moopel users to provider identities.

Suggested table name:

- `UserExternalLogins`

Suggested columns:

- `UserExternalLoginId` (identity PK)
- `UserId` (FK to `Users`)
- `Provider` (e.g. `Google`, `GitHub`)
- `ProviderUserId`
- `ProviderEmail`
- `ProviderDisplayName`
- `CreatedAtUtc`
- `LastLoginUtc`

Suggested uniqueness rules:

- unique on (`Provider`, `ProviderUserId`)
- optional filtered uniqueness on (`Provider`, `ProviderEmail`) only if the data is reliable enough

**How it would be done**

- Add `DatabaseField` definitions and `DatabaseTable` definition in `MoopelApi\ConstantValues.cs`.
- Follow the existing `UsersTable` pattern.

### Step 8. Add migration / schema creation script updates

Wherever the database schema is created or maintained, add the new external login table.

**How it would be done**

- Update the project’s schema creation path to include the new table.
- Add foreign key to `Users`.
- Ensure deletes behave correctly, likely cascade or explicit cleanup.

### Step 9. Add DTO and reader support

Create a DTO to represent external logins if service code needs to read/write them cleanly.

**How it would be done**

- Add a DTO under `MoopelApi\Models\Dto`.
- Update `MoopelApi\Helpers\DtoReaders.cs` with a reader helper.

## Phase 4: Add service-layer support

### Step 10. Add provider and external-login domain models

Create types that describe provider identities and callback results.

Suggested models:

- `ExternalLoginProvider` enum or constants
- `ExternalLoginInfo`
- `ExternalLoginResult`

Suggested fields in `ExternalLoginInfo`:

- `Provider`
- `ProviderUserId`
- `Email`
- `EmailVerified`
- `DisplayName`
- `UsernameHint`
- `AvatarUrl`

**How it would be done**

- Add models in `MoopelApi\Models`.
- Keep them provider-agnostic so controller logic stays clean.

### Step 11. Extend `IAuthService`

Add methods for external login flow.

Suggested methods:

- `Task<LoginResult?> LoginWithExternalProvider(ExternalLoginInfo loginInfo, RequestContext context, CancellationToken cancelToken)`
- `Task<User> CreateUserFromExternalLogin(ExternalLoginInfo loginInfo, CancellationToken cancelToken)`
- `Task LinkExternalLogin(int userId, ExternalLoginInfo loginInfo, CancellationToken cancelToken)`

**How it would be done**

- Update `MoopelApi\Services\Interfaces\IAuthService.cs`.
- Implement in `MoopelApi\Services\AuthService.cs`.

### Step 12. Add service methods to query and persist external logins

Add methods to find external accounts and save them.

Suggested queries:

- find by `(Provider, ProviderUserId)`
- find by verified email
- insert external login row
- update `LastLoginUtc`

**How it would be done**

- Add SQL operations in `AuthService` or a dedicated external-login service.
- Follow the existing ADO.NET pattern already used by the project.

### Step 13. Add username generation logic

External providers may not return a valid or unique username.

**How it would be done**

- Reuse existing username validation rules from `RegexHelper` and `ValidationRegex`.
- Create a helper that:
  1. picks a base name from provider data
  2. normalizes invalid characters
  3. truncates to the allowed length
  4. appends suffixes until unique

### Step 14. Add local user creation from provider login

When no linked user exists, create a local Moopel user if policy allows.

**How it would be done**

- Insert a `Users` row with:
  - generated username
  - empty or unusable password hash strategy decided explicitly
  - email if verified
  - standard role
  - normal timestamps
- Immediately insert a `UserExternalLogins` row.

Important implementation detail:

- Do **not** fake a normal password if the account is OAuth-only.
- Decide whether password login is disabled for these users until they separately set a password.

### Step 15. Add account linking logic

If an existing user should be linked instead of created, persist the provider mapping.

**How it would be done**

- If verified email matches an existing user and policy allows auto-linking:
  - insert external login row tied to that user
- If logged-in user explicitly links a provider:
  - require auth
  - verify provider identity
  - reject if already linked to another user

### Step 16. Reuse current JWT generation

After external login resolves to a local `User`, issue the current Moopel JWT.

**How it would be done**

- Reuse `GenerateToken(user)` in `AuthService`.
- Return the existing `LoginResult` shape so the frontend gets the same auth payload as normal login.

## Phase 5: Add controller endpoints

### Step 17. Extend route constants

Add route entries for provider start and callback flows.

Suggested routes:

- `auth/google/start`
- `auth/google/callback`
- `auth/github/start`
- `auth/github/callback`
- optional `auth/link/google`
- optional `auth/link/github`

**How it would be done**

- Update `MoopelApi\Routes.cs` with constants matching the existing route style.

### Step 18. Add challenge endpoints in `AuthController`

Add endpoints that start the provider redirect flow.

**How it would be done**

- In `MoopelApi\Controllers\AuthController.cs`, add GET endpoints that call `Challenge(...)`.
- Include a `returnUrl` or state parameter so success/failure can redirect the frontend cleanly.

### Step 19. Add callback endpoints in `AuthController`

Add endpoints that handle provider callback, build `ExternalLoginInfo`, call `IAuthService`, and then return or redirect with JWT.

**How it would be done**

- Authenticate the temporary external cookie/session.
- Extract provider claims.
- Build a provider-agnostic `ExternalLoginInfo`.
- Call the new auth service method.
- If successful:
  - issue Moopel JWT
  - redirect to frontend success page with token delivery strategy
- If failed:
  - redirect to frontend failure page with a safe error code

### Step 20. Choose token delivery strategy

This is a required implementation decision.

Choose one:

- **Recommended for SPA/mobile simplicity:** redirect to frontend with a short-lived one-time code, then exchange code for JWT via API.
- Simpler but less ideal: redirect with JWT in the URL fragment.

**How it would be done**

- Best practice:
  1. callback creates one-time auth completion record
  2. callback redirects frontend with code
  3. frontend posts code to backend
  4. backend returns `LoginResult`
- This avoids putting the JWT directly in query parameters.

## Phase 6: Handle provider-specific claim retrieval

### Step 21. Implement Google claims mapping

**How it would be done**

- Use provider claims made available by the Google handler.
- Extract subject ID, email, verified flag, display name, and avatar.
- Fail clearly if required fields are absent.

### Step 22. Implement GitHub claims mapping and email lookup

GitHub often needs extra handling for email.

**How it would be done**

- Read provider user id and login.
- Request email scope.
- If email claim is missing, call GitHub’s user email API endpoint using the provider access token supplied during authentication.
- Pick the primary verified email.
- If no verified email is available, follow the selected policy:
  - reject login, or
  - allow account without email

## Phase 7: Add explicit account management support

### Step 23. Decide whether OAuth-only users can set a password later

This affects account recovery and future login flexibility.

**Recommended:** allow OAuth-created users to later add a password from an authenticated settings flow.

**How it would be done**

- Add a secure endpoint for setting/changing password.
- Only permit for the authenticated user or admins, consistent with current policies.

### Step 24. Add link/unlink endpoints if needed

If the product should support multiple providers per account, add explicit management endpoints.

Suggested endpoints:

- `POST auth/link/google`
- `POST auth/link/github`
- `DELETE auth/link/{provider}`

**How it would be done**

- Require current JWT auth.
- Re-run provider challenge for proof of identity.
- Prevent unlinking the last viable login method unless another login method exists.

## Phase 8: Frontend integration work

### Step 25. Add provider login buttons

**How it would be done**

- Add UI actions for Google and GitHub on the login screen.
- These buttons navigate to backend challenge endpoints.

### Step 26. Add callback completion handling

**How it would be done**

- Add frontend route/page that receives success or failure redirect.
- If using one-time code exchange, frontend sends that code to backend and receives `LoginResult`.
- Store JWT exactly as the current login flow does.

### Step 27. Add first-login UX if username/email is incomplete

This may be needed when provider data is insufficient.

**How it would be done**

- If backend reports `username_required` or `email_required`, show a completion form.
- Finalize account creation or linking through a dedicated endpoint.

## Phase 9: Security and operational hardening

### Step 28. Register GitHub and Google apps

**How it would be done**

- In Google Cloud Console, create OAuth credentials and configure authorized redirect URIs.
- In GitHub, create an OAuth App and configure callback URL.
- Create separate settings for local, staging, and production environments.

### Step 29. Store secrets securely

**How it would be done**

- Local development: use .NET user secrets or environment variables.
- Hosted environments: use deployment secret store / platform secret management.
- Never keep production client secrets in `appsettings.json`.

### Step 30. Validate redirect URI and state handling

**How it would be done**

- Allow only approved frontend redirect targets.
- Do not accept arbitrary open redirects from caller input.
- Use framework-supported state and correlation protection built into the OAuth handlers.

### Step 31. Review logging

OAuth errors need observability, but secrets and tokens must never be logged.

**How it would be done**

- Log provider name, success/failure state, and internal reason codes.
- Avoid logging access tokens, JWTs, client secrets, or raw provider payloads unnecessarily.

### Step 32. Review abuse and lockout impacts

OAuth reduces password brute-force exposure but introduces account-linking abuse risks.

**How it would be done**

- Add explicit checks before auto-linking by email.
- Reject conflicting provider mappings.
- Ensure deactivated local users cannot re-enter via provider login.

## Phase 10: Testing plan

### Step 33. Add unit tests for auth service decisions

Cover:

- existing linked user logs in successfully
- verified email auto-links to existing user
- no link + auto-create creates user
- duplicate/conflicting provider mapping is rejected
- deactivated user cannot log in through provider
- username generation handles collisions

**How it would be done**

- Add tests near `ApiTests\Unit\AuthServiceTests.cs`.
- Mock or isolate provider-independent service logic where possible.

### Step 34. Add integration tests for new controller routes

Cover:

- challenge endpoints return expected redirect/challenge behavior
- callback success returns or redirects to completion flow
- callback failure returns controlled error behavior
- JWT returned after successful external login is accepted by protected endpoints

**How it would be done**

- Add integration coverage under `ApiTests\Integration\AuthControllerTests.cs`.
- For provider callbacks, simulate the authenticated external principal rather than relying on real third-party calls.

### Step 35. Add regression tests for existing login

Current username/password login must remain intact.

**How it would be done**

- Re-run and extend existing auth tests to confirm local login, guest login, register, and `me` behavior are unchanged.

## Phase 11: Documentation and rollout

### Step 36. Document local setup

**How it would be done**

- Update `README.md` or auth setup documentation with:
  - provider app registration steps
  - required secrets
  - callback URLs
  - local frontend/backend startup notes

### Step 37. Document operational runbook

**How it would be done**

- Document how to rotate secrets.
- Document how to disable one provider without breaking the other.
- Document expected failure cases like missing verified GitHub email.

### Step 38. Roll out incrementally

**Recommended rollout order**

1. Add backend support behind config flags.
2. Enable in local development.
3. Enable Google first.
4. Enable GitHub after email-handling is confirmed.
5. Enable account linking features later if not required for first release.

## Suggested File-Level Change List

Expected backend files to change or be added:

- `MoopelApi\Startup.cs`
- `MoopelApi\MoopelApi.csproj`
- `MoopelApi\appsettings.json`
- `MoopelApi\Routes.cs`
- `MoopelApi\Controllers\AuthController.cs`
- `MoopelApi\Services\Interfaces\IAuthService.cs`
- `MoopelApi\Services\AuthService.cs`
- `MoopelApi\ConstantValues.cs`
- `MoopelApi\Helpers\DtoReaders.cs`
- `MoopelApi\Models\Configuration\*.cs` for OAuth settings
- `MoopelApi\Models\Dto\*.cs` for external login DTOs if needed
- `MoopelApi\Models\*.cs` for provider-agnostic external login models
- `ApiTests\Unit\AuthServiceTests.cs`
- `ApiTests\Integration\AuthControllerTests.cs`
- `README.md` or other setup docs

## Recommended MVP Scope

If the goal is the fastest safe delivery, the MVP should be:

1. Google + GitHub login only
2. Auto-create users on first login
3. Auto-link only by **verified** email
4. Moopel JWT issued after provider success
5. No unlink flow yet
6. No multi-step username chooser unless provider data is unusable

That gives the shortest path to value while fitting the existing architecture.

## Risks and Edge Cases

- GitHub may not provide a usable verified email without extra handling.
- Provider username/display name may violate local username requirements.
- Auto-linking by email can be dangerous if email verification is not enforced.
- OAuth-only accounts need a clear policy for password reset and fallback login.
- Redirect handling can introduce open redirect or token leakage issues if implemented loosely.

## Acceptance Criteria

The implementation is complete when all of the following are true:

- A user can start Google login and receive a valid Moopel JWT.
- A user can start GitHub login and receive a valid Moopel JWT.
- Existing local username/password login still works unchanged.
- Linked external identities are persisted and reused on later logins.
- Duplicate/conflicting account links are rejected safely.
- Deactivated users remain blocked even if provider authentication succeeds.
- Configuration can enable/disable Google and GitHub independently.
- Secrets are not stored in committed config.
- Tests cover the main success and failure flows.
