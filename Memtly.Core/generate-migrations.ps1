param(
    [string]$MigrationName,
    [string]$ProviderFilter = "",
    [switch]$Fresh,
    [switch]$VerboseLogs
)

$dotNetVersion = "net10.0"
$providers = @(
    @{
        Name      = "Sqlite"
        Type      = "sqlite"
        ConnString = "Data Source=./_design_temp_sqlite.db"
    },
    @{
        Name      = "MySql"
        Type      = "mysql"
        ConnString = "Server=localhost;Database=_design_temp_mysql;Uid=root;Pwd=root;"
    },
    @{
        Name      = "SqlServer"
        Type      = "mssql"
        ConnString = "Server=localhost;Database=_design_temp_mssql;Trusted_Connection=True;"
    },
    @{
        Name      = "Postgres"
        Type      = "postgres"
        ConnString = "Host=localhost;Database=_design_temp_postgres;Username=postgres;Password=postgres;"
    }
)

Write-Host "Migration started for project: Memtly.Core" -ForegroundColor Green

if (-not $MigrationName) {
    Write-Host "Error: MigrationName is required" -ForegroundColor Red
    Write-Host "Usage: .\Memtly.Core\Memtly.Core\generate-migrations.ps1 -MigrationName 'InitializeDatabase'" -ForegroundColor Yellow
    exit 1
}

$startupSlnFile = Get-ChildItem -Path . -File -Filter "*.sln" | Select-Object -First 1
if (-not $startupSlnFile) {
    Write-Host "Error: Please run from the root directory which contains the .sln file" -ForegroundColor Red
    exit 1
}

$startupProjectFile = Get-ChildItem -Path . -Recurse -File -Filter "Memtly.Core.csproj" | Select-Object -First 1
if (-not $startupProjectFile) {
    Write-Host "Error: No .csproj found for Memtly.Core" -ForegroundColor Red
    exit 1
}

if ($Fresh) {
    Write-Host "FRESH START: Removing all existing migrations..." -ForegroundColor Red
    $providers | ForEach-Object {
        $providerProjectFileName = "Memtly.Core.Migrations.$($_.Name)"

        if (-not [string]::IsNullOrWhiteSpace($ProviderFilter) -and $_.Name -ne $ProviderFilter) {
            Write-Host "  (Skipped $($_.Name) - doesn't match specified provider '$($ProviderFilter)')" -ForegroundColor DarkGray
            return
        }

        $migrationProject = Get-ChildItem -Path . -Recurse -Directory -Filter $providerProjectFileName | Select-Object -First 1
        if (-not $migrationProject) {
            Write-Host "  (Skipped $($providerProjectFileName) - directory not found)" -ForegroundColor DarkGray
            return
        }

        if (Test-Path $migrationProject.FullName) {
            Get-ChildItem -Path $migrationProject.FullName -Recurse -File -Filter "*.cs" | Remove-Item -Force
            Write-Host "  ✓ Cleared migrations for $($_.Name)" -ForegroundColor Green
        }
    }
}

Write-Host "Cleaning up temporary files and databases..." -ForegroundColor Yellow

Get-ChildItem -Path . -Recurse -File -Filter "_design_temp_*.db" | Remove-Item -Force -ErrorAction SilentlyContinue
Write-Host "✓ Cleaned SQLite temp files" -ForegroundColor Green

try {
    mysql -e "DROP DATABASE IF EXISTS _design_temp_mysql;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped MySQL design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped MySQL - not accessible)" -ForegroundColor DarkGray
}

try {
    sqlcmd -Q "DROP DATABASE IF EXISTS _design_temp_mssql;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped SQL Server design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped SQL Server - not accessible)" -ForegroundColor DarkGray
}

try {
    psql -U postgres -c "DROP DATABASE IF EXISTS _design_temp_postgres;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Dropped Postgres design database" -ForegroundColor Green
    }
} catch {
    Write-Host "  (Skipped Postgres - not accessible)" -ForegroundColor DarkGray
}

Write-Host "Generating migration files..." -ForegroundColor Yellow

foreach ($provider in $providers) {
    $FinalMigrationName = "$($provider.Name)_$($MigrationName)"

    if (-not [string]::IsNullOrWhiteSpace($ProviderFilter) -and $provider.Name -ne $ProviderFilter) {
        Write-Host "  (Skipped $($provider.Name) - doesn't match specified provider '$($ProviderFilter)')" -ForegroundColor DarkGray
        continue
    }

    Write-Host "Generating migrations for $($provider.Name)..." -ForegroundColor Cyan

    $env:Memtly__Database__Type = $provider.Type
    $env:Memtly__Database__ConnectionString = $provider.ConnString

    $providerProjectFileName = "Memtly.Core.Migrations.$($provider.Name)"
    $providerProjectFile = Get-ChildItem -Path . -Recurse -File -Filter "$($providerProjectFileName).csproj" | Select-Object -First 1
    if (-not $providerProjectFile) {
        Write-Host "  ✗ No project found for $($providerProjectFileName).csproj" -ForegroundColor Red
        Write-Host "  Run: dotnet new classlib -n Memtly.Core.Migrations.$($provider.Name)" -ForegroundColor Yellow
        Write-Host ""
        continue
    }

    $verboseFlag = if ($VerboseLogs) { "--verbose" } else { "" }

    Write-Host "  Building $($providerProjectFile.Name)..." -ForegroundColor DarkGray
    dotnet build $providerProjectFile.FullName --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Build failed" -ForegroundColor Red
        Write-Host ""
        continue
    }

    $startupBin = Join-Path $startupProjectFile.DirectoryName "bin/Debug/$($dotNetVersion)"
    $migrationBin = Join-Path $providerProjectFile.DirectoryName "bin/Debug/$($dotNetVersion)"

    Get-ChildItem -Path $migrationBin -Filter "Memtly.Core.Migrations.$($provider.Name)*" |
        ForEach-Object {
            Copy-Item $_.FullName $startupBin -Force
            Write-Host "  Copied $($_.Name) to startup bin" -ForegroundColor DarkGray
        }

    dotnet ef migrations add $FinalMigrationName `
        --project $providerProjectFile.FullName `
        --startup-project $startupProjectFile.FullName `
        --output-dir .
        $verboseFlag

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Successfully generated: $FinalMigrationName" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed" -ForegroundColor Red
    }

    Write-Host ""
}

# Final cleanup
Remove-Item Env:Memtly__Database__Type -ErrorAction SilentlyContinue
Remove-Item Env:Memtly__Database__ConnectionString -ErrorAction SilentlyContinue

Get-ChildItem -Path . -Recurse -File -Filter "_design_temp_*.db" | Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "Done!" -ForegroundColor Green