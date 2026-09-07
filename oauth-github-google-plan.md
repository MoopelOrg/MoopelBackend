# Two-Factor Enforcement and OAuth Cost Plan

## 1) Two-factor enforcement plan

### Goal

Force MFA for all standard user accounts, with Microsoft Authenticator as the recommended free option. This reduces account takeover risk while keeping the setup simple and low cost.

### Recommended approach

Use time-based one-time passwords (TOTP) via Microsoft Authenticator.

Why this is a good fit:
- Microsoft Authenticator is free.
- It works with TOTP and supports an easy enrollment flow.
- It does not require a paid identity platform for basic MFA.
- It fits a custom JWT-based app like Moopel well.

### Enforced MFA model

Recommended policy:
- All non-guest users must enable MFA before full access is granted.
- Guests stay guest-only and never get full app access.
- Admins also require MFA.
- High-risk actions (password changes, account recovery, linking OAuth accounts) require re-authentication or step-up MFA.

### MFA setup flow

1. User signs in normally with username/password or OAuth.
2. If `TwoFactorEnabled == false`, app redirects to an MFA enrollment page.
3. Backend generates a TOTP secret and stores a protected/encrypted secret in the user record or a separate table.
4. App shows a QR code for Microsoft Authenticator.
5. User scans the QR code in Microsoft Authenticator.
6. User enters a verification code from the app.
7. Backend validates the code and marks the account as MFA-enabled.
8. On future login, the user must provide a valid MFA code after the username/password or OAuth flow.

### Recommended backend changes

Add these fields to the user identity model:
- `TwoFactorEnabled` (bool)
- `TwoFactorSecretEncrypted` (string or binary encrypted field)
- `TwoFactorBackupCodes` (optional array or separate table)
- `MfaLastVerifiedUtc` (optional for session-level enforcement)

Also add these tables / entities if needed:
- `UserMfaSecrets`
- `MfaBackupCodes`

### Recommended logic

On login:
- If user has MFA enabled, require a second factor after primary login succeeds.
- If the code is missing or invalid, reject the login with a dedicated error like `mfa_required` or `invalid_mfa_code`.
- Return a short-lived MFA session or challenge token that is valid only for temporary MFA completion.

On password change:
- Require MFA challenge before completing the password update.

On OAuth account linking:
- Require current step-up verification if the user is already logged in.

### Enforcement strategy

Use a layered approach:
- Force MFA at login for all normal users.
- Force MFA for sensitive flows such as password changes, email updates, account recovery, and OAuth linking.
- Keep guest login separate and not allowed to receive full user privileges.

### Free MFA options

The simplest free choice is:
- Microsoft Authenticator: free
- Google Authenticator: free
- Authy: free for basic use

If you want the easiest enterprise-friendly path and no extra service cost, choose Microsoft Authenticator.

### Notes for this codebase

This app currently uses a custom JWT auth pattern in `AuthService` and `Startup.cs`. That means MFA should be added as:
- a required second-step check in the login pipeline
- a server-side verification step before JWT issuance for MFA-enabled accounts
- a `TwoFactorEnabled` gate that can be checked during login and account-sensitive operations

This keeps the rest of the API unchanged while enforcing MFA at the auth boundary.

---

## 2) OAuth plan cost estimate

### Provider costs

For standard OAuth with Google and GitHub, the provider fees are typically:
- Google OAuth: $0 for normal app usage under standard quotas
- GitHub OAuth App: $0 for standard public/private apps under normal usage

So for a basic app, the direct OAuth provider cost is:
- Estimated provider cost: $0

### Implementation cost

The engineering cost depends on the scope.

#### Minimal MVP (Google + GitHub, no complex linking)
- Backend OAuth setup and callback handling: $1,000–$3,000
- Database changes and external-login mapping: $500–$1,500
- Frontend login flow: $500–$2,000
- Test coverage and bug fixes: $500–$1,500
- Total MVP estimate: $2,500–$8,000

#### Full robust rollout
- OAuth support for multiple providers
- Auto-linking by verified email
- Manual account linking
- MFA enforcement
- Login UX polish and extra edge-case handling
- Total estimate: $5,000–$15,000+

### Operational cost

If you use only TOTP MFA with Microsoft Authenticator:
- MFA app cost: $0
- Hosting/infra cost: $0–$50/month depending on platform usage
- Secret management and deployment: usually included in normal hosting costs

### Recommended budget

For a small app like this, a realistic budget is:
- OAuth + MFA implementation: $3,500–$8,000 total
- Ongoing provider cost: $0
- Ongoing MFA app cost: $0

### Final recommendation

Use:
- Microsoft Authenticator for MFA: free
- Google OAuth + GitHub OAuth: free for typical app usage
- Budget for engineering time, not provider fees

That makes the real cost a development effort cost, not a platform fee.

---

## 3) Recommended rollout order

1. Add MFA enforcement for all normal accounts.
2. Use Microsoft Authenticator as the default TOTP app.
3. Add Google OAuth next.
4. Add GitHub OAuth after Google is stable.
5. Keep the OAuth plan behind environment flags until production secrets are ready.
6. Turn on required MFA before allowing broad production use.

## 4) Bottom line

- MFA with Microsoft Authenticator: free and recommended.
- OAuth provider cost: effectively $0 for normal usage.
- Main cost is engineering and testing, not OAuth fees.
- Best-fit architecture for this app: keep JWTs, add TOTP MFA, and use Google/GitHub only as identity providers that issue your existing local JWT after validation.
