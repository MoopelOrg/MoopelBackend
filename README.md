# MoopelBackend

## GitHub Actions deployment scaffolding

This repository now includes separate GitHub Actions workflows for CI, image publishing, and environment-specific deployment:

- `build-and-test.yml` runs restore, build, unit tests, integration tests, and uploads TRX results.
- `build-image.yml` publishes `ghcr.io/moopelorg/moopelbackend` only after CI succeeds and tags images with the commit SHA, `latest` on `main`, and release tags that point at the same commit.
- `deploy-development.yml`, `deploy-staging.yml`, and `deploy-production.yml` reuse the same deployment workflow so each environment deploys the exact image built from the tested commit SHA.
- `docker-cleanup.yml` is currently a read-only GHCR retention audit so stale image candidates can be reviewed before any deletion automation is enabled.

### Required GitHub environments

Create these GitHub environments and configure any approval rules directly in GitHub:

- `development`
- `staging`
- `production`

Configure production reviewers in the `production` environment to require approval before that job proceeds.

### Required environment secrets

Add these secrets to each GitHub environment that will deploy:

- `DEPLOY_HOST`
- `DEPLOY_USER`
- `DEPLOY_SSH_KEY`
- `DEPLOY_KNOWN_HOSTS`
- `GHCR_USERNAME`
- `GHCR_READ_TOKEN`

`GHCR_READ_TOKEN` should be a package-read token that can pull the published container image from GHCR on the target host.

### Optional environment variables

Add these GitHub environment variables when they apply to the target host:

- `DEPLOY_PORT` - SSH port for the deployment host. Defaults to `22`.
- `CONTAINER_ENV_FILE_PATH` - Absolute path to the environment file already present on the host.
- `DOCKER_RUN_ARGS` - Additional `docker run` arguments such as published ports, networks, or volumes.
- `SMOKE_TEST_URL` - Optional URL to use for staging/production smoke tests. When omitted, the reusable workflow checks `/openapi/v1.json` inside the container.

Environment-specific application settings and secrets must stay outside source control. The deployment workflow only references host-side files and environment configuration.

### Notes and assumptions

- CI separates unit and integration tests by the existing `ApiTests.Unit` and `ApiTests.Integration` namespaces because both suites currently live in the same test project.
- The app already exposes a lightweight health endpoint at `/`, so deployment health checks use that route.
- The deployment workflows assume each environment deploys to a Docker-capable host that can be reached over SSH.
- Files added under `.github/` will not appear automatically in Visual Studio Solution Explorer.