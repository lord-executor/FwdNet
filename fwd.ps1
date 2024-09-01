
function Launch-FwdNet {
    echo $PSScriptRoot

    $forceBuild = $false
    if ($Args.Contains("--build")) {
        Write-Output "Force build"
        # When the filtered $Args only contains a single item after filtering, then PowerShell just
        # assigns the single value to the result instead of an _array_ of that single value. Specifying
        # [array] as the variable type avoids that but does require a temporary variable as it doesn't
        # work when directly re-assigning $Args
        [array] $filtered = $Args | Where-Object { $_ -ne "--build" }
        $Args = $filtered
        $forceBuild = $true
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
        dotnet publish "$PSScriptRoot/FwdNet.sln"
        Write-Output $tag > "$PSScriptRoot/build.version"
    }

    & "$PSScriptRoot\FwdNet\bin\Release\net8.0\win-x64\publish\FwdNet.exe" @Args
}

Set-Alias -Name "fwd" -Value "Launch-FwdNet"
