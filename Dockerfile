# Runs the E2E suite headless in a container: docker build -t snipeit-uitests . && docker run --init --ipc=host --rm snipeit-uitests
FROM mcr.microsoft.com/dotnet/sdk:10.0.402-noble

ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright \
    Diagnostics__OutputDir=/src/artifacts

# The image's non-root "app" user owns the source tree and the browsers; only OS packages are installed as root.
RUN mkdir /src /ms-playwright && chown app:app /src /ms-playwright
USER app
WORKDIR /src

# Restore and install the browser before copying the sources, so code changes reuse these cached layers.
COPY --chown=app:app global.json Directory.Build.props .editorconfig SnipeIt.sln ./
COPY --chown=app:app tests/SnipeIt.UiTests/SnipeIt.UiTests.csproj tests/SnipeIt.UiTests/packages.lock.json tests/SnipeIt.UiTests/
RUN dotnet restore --locked-mode && dotnet build --no-restore -c Release
USER root
RUN pwsh tests/SnipeIt.UiTests/bin/Release/net10.0/playwright.ps1 install-deps chromium && rm -rf /var/lib/apt/lists/*
USER app
COPY --chown=app:app .github/scripts/install-browsers.sh .github/scripts/
RUN INSTALL_ARGS= .github/scripts/install-browsers.sh chromium

COPY --chown=app:app tests/ tests/
RUN dotnet build --no-restore -c Release

ENTRYPOINT ["dotnet", "test", "--no-build", "-c", "Release", "--filter", "TestCategory=E2E", "--blame-hang-timeout", "8m", \
    "--results-directory", "/src/TestResults", "--logger", "trx;LogFileName=e2e-docker.trx", \
    "--logger", "console;verbosity=normal"]
