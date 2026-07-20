Write-Host "Modifies the csproj and the appxmanifest, so that debugging wont interference with the installed original version."

$Project = ".\src\SilentNotes.Blazor\SilentNotes.csproj"
$Search  = "<ApplicationId>ch.martinstoeckli.silentnotes</ApplicationId>"
$Replace = "<ApplicationId>dev.martinstoeckli.silentnotes</ApplicationId>"

(Get-Content $Project) -replace $Search, $Replace | Set-Content -Path $Project -Encoding UTF8

$Manifest = ".\src\SilentNotes.Blazor\Platforms\Windows\Package.appxmanifest"

$Search  = "22846MartinStoeckli"
$Replace = "devMartinStoeckli"

(Get-Content $Manifest) -replace $Search, $Replace | Set-Content -Path $Manifest -Encoding UTF8

Read-Host -Prompt "Press Enter to exit"