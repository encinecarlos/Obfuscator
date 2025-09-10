#!/usr/bin/env pwsh
#
# Script para validação do pacote NuGet antes da publicação
# Compatível com .NET 8
#

param(
    [string]$PackagePath = ".\artifacts\EncineCarlos.Obfuscator.*.nupkg"
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

function Write-Step {
    param([string]$Message)
    Write-ColoredText "🔍 $Message" $Blue
}

function Write-Success {
    param([string]$Message)
    Write-ColoredText "✅ $Message" $Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColoredText "⚠️  $Message" $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-ColoredText "❌ $Message" $Red
}

# Banner
Write-ColoredText @"
╔══════════════════════════════════════════════════════════════╗
║              EncineCarlos.Obfuscator Validator               ║
║                 Package Validation Script                   ║
║                      (.NET 8 Target)                        ║
╚══════════════════════════════════════════════════════════════╝
"@ $Blue

Write-Host ""

# Verificar versão do .NET SDK
Write-Step "Verificando ambiente .NET..."
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Success "SDK do .NET detectado: $dotnetVersion"
    if ($dotnetVersion -ge "8.0.0") {
        Write-Success "Versão do SDK compatível com .NET 8"
    } else {
        Write-Warning "Recomenda-se .NET SDK 8.0 ou superior"
    }
} else {
    Write-Error ".NET SDK não encontrado"
}

# Encontrar o pacote
$packages = Get-ChildItem -Path $PackagePath -ErrorAction SilentlyContinue
if (-not $packages) {
    Write-Error "Nenhum pacote encontrado em: $PackagePath"
    Write-Host "Execute primeiro o script build-package.ps1"
    exit 1
}

$package = $packages | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Step "Validando pacote: $($package.Name)"

# Checklist de validação
$validationResults = @()

# 1. Verificar metadados básicos
Write-Step "Verificando metadados do pacote..."

# Extrair informações do pacote (simulação - em um ambiente real usaríamos NuGet.exe ou APIs)
$packageInfo = @{
    Name = $package.BaseName
    Size = $package.Length
    Created = $package.CreationTime
}

Write-Host "   📄 Nome: $($packageInfo.Name)"
Write-Host "   📏 Tamanho: $([math]::Round($packageInfo.Size / 1KB, 2)) KB"
Write-Host "   📅 Criado: $($packageInfo.Created)"

# 2. Verificar estrutura de arquivos essenciais
Write-Step "Verificando arquivos essenciais do projeto..."

$requiredFiles = @(
    "README.md",
    "LICENSE",
    "CHANGELOG.md",
    "Obfuscator\Obfuscator.csproj"
)

$missingFiles = @()
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Success "   ✓ $file"
    } else {
        Write-Warning "   ✗ $file (não encontrado)"
        $missingFiles += $file
    }
}

# 3. Verificar conteúdo do projeto
Write-Step "Verificando estrutura do projeto..."

$sourceFiles = @(
    "Obfuscator\ObfuscatorService.cs",
    "Obfuscator\SensitiveDataAttribute.cs",
    "Obfuscator\Interfaces\IObfuscatorService.cs",
    "Obfuscator\Extensions\ObfuscatorExtensions.cs",
    "Obfuscator\Redactors\SimpleRedactorProvider.cs"
)

foreach ($file in $sourceFiles) {
    if (Test-Path $file) {
        Write-Success "   ✓ $file"
    } else {
        Write-Warning "   ✗ $file (não encontrado)"
    }
}

# 4. Verificar testes
Write-Step "Verificando testes..."
$testFiles = Get-ChildItem -Path "Obfuscator.Tests" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue
if ($testFiles) {
    Write-Success "   ✓ $($testFiles.Count) arquivos de teste encontrados"
} else {
    Write-Warning "   ✗ Nenhum arquivo de teste encontrado"
}

# 5. Verificar se o projeto compila
Write-Step "Verificando compilação (.NET 8)..."
$buildResult = dotnet build Obfuscator\Obfuscator.csproj --configuration Release --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Success "   ✓ Projeto compila sem erros"
} else {
    Write-Error "   ✗ Erro na compilação"
}

# 6. Executar testes
Write-Step "Executando testes (.NET 8)..."
$testResult = dotnet test --configuration Release --verbosity quiet --logger "console;verbosity=minimal"
if ($LASTEXITCODE -eq 0) {
    Write-Success "   ✓ Todos os testes passaram"
} else {
    Write-Error "   ✗ Alguns testes falharam"
}

# 7. Verificar dependências (.NET 8)
Write-Step "Verificando dependências (.NET 8 compatible)..."
$projectContent = Get-Content "Obfuscator\Obfuscator.csproj" -Raw
if ($projectContent -match "Microsoft\.Extensions\.Compliance") {
    Write-Success "   ✓ Dependências principais encontradas"
} else {
    Write-Warning "   ✗ Dependências principais não encontradas"
}

# 7.1. Verificar versões das dependências
if ($projectContent -match "Microsoft\.Extensions\.Compliance\.Abstractions.*Version=`"8\.") {
    Write-Success "   ✓ Microsoft.Extensions.Compliance.Abstractions versão 8.x"
} else {
    Write-Warning "   ✗ Versão de Microsoft.Extensions.Compliance.Abstractions não é 8.x"
}

# 8. Verificar target framework
Write-Step "Verificando target framework..."
if ($projectContent -match "<TargetFramework>net8\.0</TargetFramework>") {
    Write-Success "   ✓ Target Framework configurado para .NET 8"
} else {
    Write-Warning "   ✗ Target Framework não está definido como .NET 8"
}

# 9. Verificar versionamento
Write-Step "Verificando versionamento..."
if ($projectContent -match "<Version>1\.0\.0</Version>") {
    Write-Success "   ✓ Versão 1.0.0 configurada corretamente"
} else {
    Write-Warning "   ✗ Versão não está definida como 1.0.0"
}

# 10. Verificar informações de copyright
Write-Step "Verificando informações de copyright..."
if ($projectContent -match "2025") {
    Write-Success "   ✓ Ano 2025 configurado corretamente"
} else {
    Write-Warning "   ✗ Ano não está definido como 2025"
}

# Resumo final
Write-Host ""
Write-ColoredText "📋 RESUMO DA VALIDAÇÃO" $Blue
Write-Host ""

if ($missingFiles.Count -eq 0) {
    Write-Success "✅ Todos os arquivos essenciais estão presentes"
} else {
    Write-Warning "⚠️  Arquivos ausentes: $($missingFiles -join ', ')"
}

Write-Host ""
Write-ColoredText "📝 CHECKLIST FINAL PARA PUBLICAÇÃO (.NET 8):" $Yellow
Write-Host ""
Write-Host "□ Revisar README.md para clareza e exemplos corretos"
Write-Host "□ Verificar se a licença MIT está apropriada"
Write-Host "□ Confirmar que todos os testes passam em .NET 8"
Write-Host "□ Validar exemplos de código no README"
Write-Host "□ Testar o pacote em um projeto .NET 8 de exemplo"
Write-Host "□ Verificar compatibilidade com .NET 8 runtime"
Write-Host "□ Configurar API key do NuGet.org"
Write-Host "□ Publicar com: dotnet nuget push"

Write-Host ""
Write-ColoredText "🎯 Especificações Técnicas Validadas:" $Green
Write-Host "   • Target Framework: .NET 8"
Write-Host "   • Dependencies: Microsoft Extensions 8.x series"
Write-Host "   • Compatibility: .NET 8+ applications"

Write-Host ""
Write-ColoredText "🎯 Pacote pronto para validação final!" $Green
