Write-Host "Reverses the modifications done with StartDevMode.ps1"

$Project = ".\src\SilentNotes.Blazor\SilentNotes.csproj"
$Search  = "<ApplicationId>dev.martinstoeckli.silentnotes</ApplicationId>"
$Replace = "<ApplicationId>ch.martinstoeckli.silentnotes</ApplicationId>"

(Get-Content $Project) -replace $Search, $Replace | Set-Content -Path $Project -Encoding UTF8

$Manifest = ".\src\SilentNotes.Blazor\Platforms\Windows\Package.appxmanifest"

$Search  = "devMartinStoeckli"
$Replace = "22846MartinStoeckli"

(Get-Content $Manifest) -replace $Search, $Replace | Set-Content -Path $Manifest -Encoding UTF8

Read-Host -Prompt "Press Enter to exit"