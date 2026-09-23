# MoopelBackend

## GitHub Actions deployment scaffolding

This repository now includes separate GitHub Actions workflows for CI, image publishing, and environment-specific deployment:

- `build-and-test.yml` runs restore, build, unit tests, integration tests, and uploads TRX results.
- `build-image.yml` publishes `ghcr.io/moopelorg/moopelbackend` only after CI succeeds, tags images with the commit SHA, `latest` on `main`, includes release tags that point at the same commit, and uploads a Docker archive artifact for self-hosted deployments.
- `deploy-development.yml`, `deploy-staging.yml`, and `deploy-production.yml` reuse the same deployment workflow so each environment deploys the exact image built from the tested commit SHA on the self-hosted Windows runner without SSH.
- `docker-cleanup.yml` is currently a read-only GHCR retention audit so stale image candidates can be reviewed before any deletion automation is enabled.

To test development deployment on demand, manually run `deploy-development.yml` from the Actions tab on a non-`main` branch. It defaults to the selected branch HEAD commit SHA and can also redeploy any existing SHA-tagged image by setting the optional `image_tag` input.

### Required GitHub environments

Create these GitHub environments and configure any approval rules directly in GitHub:

- `development`
- `staging`
- `production`

Configure production reviewers in the `production` environment to require approval before that job proceeds.

### Required self-hosted runner setup

- Install the `moopel-server` self-hosted Windows runner on the Docker-capable machine that will actually run the container.
- Ensure Docker is available to the runner service account.
- Ensure the runner can keep the downloaded Docker artifact on disk long enough for the deployment job to load it.

### Optional environment variables

Add these GitHub environment variables when they apply to the self-hosted deployment runner:

- `HOST_PORT` - Host port to bind for the environment. Defaults to `8082` for development, `8083` for staging, and `8080` for production.
- `CONTAINER_PORT` - Container port exposed by the app. Defaults to `8080`.
- `CONTAINER_ENV_FILE_PATH` - Absolute path to the environment file already present on the self-hosted runner.
- `HEALTHCHECK_RETRIES` - Override for the deployment health retry count.
- `HEALTHCHECK_DELAY_SECONDS` - Override for the wait between health retries.
- `SMOKE_TEST_URL` - Optional URL to use for staging/production smoke tests. When omitted, the reusable workflow checks `/openapi/v1.json` through the bound host port.

Environment-specific application settings and secrets must stay outside source control. The deployment workflow only references runner-local files and environment configuration.

### Notes and assumptions

- CI separates unit and integration tests by the existing `ApiTests.Unit` and `ApiTests.Integration` namespaces because both suites currently live in the same test project.
- The app already exposes a lightweight health endpoint at `/`, so deployment health checks use that route.
- The deployment workflows assume the self-hosted runner and Docker engine share the same machine, so deployment is performed locally instead of over SSH.
- Files added under `.github/` will not appear automatically in Visual Studio Solution Explorer.