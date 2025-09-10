#!/usr/bin/env pwsh
#
# Script para build e criação do pacote NuGet do EncineCarlos.Obfuscator
#

param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$PackOnly,
    [string]$OutputPath = ".\artifacts"
)

# Cores para output
$Red = [ConsoleColor]::Red
$Green = [ConsoleColor]::Green
$Yellow = [ConsoleColor]::Yellow
$Blue = [ConsoleColor]::Blue

function Write-ColoredText {
    param(
        [string]$Text,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )
    $currentColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Host $Text
    $Host.UI.RawUI.ForegroundColor = $currentColor
}

function Exit-WithError {
    param([string]$Message)
    Write-ColoredText "❌ ERRO: $Message" $Red
    exit 1
}

function Write-Step {
    param([string]$Message)
    Write-ColoredText "🔄 $Message" $Blue
}

function Write-Success {
    param([string]$Message)
    Write-ColoredText "✅ $Message" $Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColoredText "⚠️  $Message" $Yellow
}

# Banner
Write-ColoredText @"
╔══════════════════════════════════════════════════════════════╗
║                   EncineCarlos.Obfuscator                    ║
║                    Package Build Script                      ║
╚══════════════════════════════════════════════════════════════╝
"@ $Blue

Write-Host ""

# Verificar se estamos na raiz do projeto
if (-not (Test-Path "Obfuscator\Obfuscator.csproj")) {
    Exit-WithError "Este script deve ser executado a partir da raiz do projeto"
}

# Criar diretório de output
if (-not (Test-Path $OutputPath)) {
    Write-Step "Criando diretório de output: $OutputPath"
    New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
}

try {
    if (-not $PackOnly) {
        # Limpar builds anteriores
        Write-Step "Limpando builds anteriores..."
        dotnet clean --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            Exit-WithError "Falha ao limpar o projeto"
        }

        # Restaurar dependências
        Write-Step "Restaurando dependências do NuGet..."
        dotnet restore
        if ($LASTEXITCODE -ne 0) {
            Exit-WithError "Falha ao restaurar dependências"
        }

        # Executar testes (se não foi pulado)
        if (-not $SkipTests) {
            Write-Step "Executando testes..."
            dotnet test --configuration $Configuration --no-restore --verbosity minimal
            if ($LASTEXITCODE -ne 0) {
                Exit-WithError "Os testes falharam"
            }
            Write-Success "Todos os testes passaram!"
        } else {
            Write-Warning "Testes foram pulados"
        }

        # Build do projeto
        Write-Step "Compilando o projeto em modo $Configuration..."
        dotnet build Obfuscator\Obfuscator.csproj --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) {
            Exit-WithError "Falha na compilação"
        }
        Write-Success "Compilação concluída com sucesso!"
    }

    # Criar pacote NuGet
    Write-Step "Criando pacote NuGet..."
    dotnet pack Obfuscator\Obfuscator.csproj --configuration $Configuration --output $OutputPath --no-build
    if ($LASTEXITCODE -ne 0) {
        Exit-WithError "Falha ao criar o pacote NuGet"
    }

    # Listar pacotes criados
    Write-Success "Pacote NuGet criado com sucesso!"
    Write-Host ""
    Write-ColoredText "📦 Pacotes criados:" $Green
    Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | ForEach-Object {
        Write-Host "   • $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)"
    }

    # Verificar se há símbolos
    $symbolPackages = Get-ChildItem -Path $OutputPath -Filter "*.snupkg"
    if ($symbolPackages) {
        Write-Host ""
        Write-ColoredText "🔍 Pacotes de símbolos:" $Green
        $symbolPackages | ForEach-Object {
            Write-Host "   • $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)"
        }
    }

    Write-Host ""
    Write-ColoredText "🎉 Build e empacotamento concluídos com sucesso!" $Green
    Write-Host ""
    Write-ColoredText "Próximos passos:" $Yellow
    Write-Host "1. Revisar o pacote criado em: $OutputPath"
    Write-Host "2. Testar o pacote localmente se necessário"
    Write-Host "3. Publicar no NuGet.org com: dotnet nuget push <package.nupkg> --api-key <your-api-key> --source https://api.nuget.org/v3/index.json"

} catch {
    Exit-WithError "Erro inesperado: $($_.Exception.Message)"
}
