$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | Select-Object -ExpandProperty FullName
& $compiler /nologo "/win32manifest:$PSScriptRoot/app.manifest" "/win32icon:$PSScriptRoot/LumeStudio.ico" /target:winexe /platform:x64 /main:LumeStudio /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll "/out:$PSScriptRoot/LumeRGB-Studio-v22.exe" $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }












