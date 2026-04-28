
function Launch-FwdNet {
    echo $PSScriptRoot

    $forceBuild = $false
    $useAot = $false
    $arguments = @()
    
    foreach ($a in $Args) {
        if ($arguments.Contains("--build")) {
            Write-Output "Force build"
            $forceBuild = $true
        } elseif ($arguments.Contains("--aot")) {
            Write-Output "Use AOT"
            $useAot = $true
        } else {
            $arguments += $a
        }
    }

    $tag = git -C $PSScriptRoot describe --tags --dirty --always 
    $buildVersion = Get-Content "$PSScriptRoot/build.version" -ErrorAction Ignore
    if ($buildVersion -ne $null) {
        Write-Output "Latest build version is $buildVersion"
    }

    $doBuild = $forceBuild
    if ($buildVersion -ne $tag) {
        $doBuild = $true
    }

    if ($doBuild) {
        Write-Output "Building binary with version $tag"
        if ($useAot) {
            dotnet publish "$PSScriptRoot/FwdNet.sln" /p:PublishAot=true
        } else {
            dotnet publish "$PSScriptRoot/FwdNet.sln"
        }
        Write-Output $tag > "$PSScriptRoot/build.version"
    }

    & "$PSScriptRoot\FwdNet\bin\Release\net10.0\win-x64\publish\FwdNet.exe" @arguments
}

Set-Alias -Name "fwd" -Value "Launch-FwdNet"
