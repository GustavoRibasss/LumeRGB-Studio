$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | Select-Object -ExpandProperty FullName
$resourceArgs = @("/resource:$PSScriptRoot/ThebestRGB.ico,ThebestRGB.ico")
$bundle = Join-Path $PSScriptRoot 'openrgb-bundle'
if (Test-Path -LiteralPath $bundle) {
  Get-ChildItem -LiteralPath $bundle -Recurse -File | ForEach-Object {
    $relative = $_.FullName.Substring($bundle.Length + 1).Replace('\','.')
    $resourceArgs += "/resource:$($_.FullName),OpenRGB.$relative"
  }
}
& $compiler /nologo "/win32manifest:$PSScriptRoot/app.manifest" "/win32icon:$PSScriptRoot/ThebestRGB.ico" /target:winexe /platform:x64 /main:ThebestRGB /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll "/out:$PSScriptRoot/ThebestRGB.exe" $sources $resourceArgs
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }












