function Install-NuGet
{
    If (!(Test-Path .\.nuget\nuget.exe))
    {
        New-Item .\.nuget -type directory -Force | Out-Null
        Invoke-WebRequest 'http://dist.nuget.org/win-x86-commandline/v3.1.0-beta/nuget.exe' -OutFile '.\.nuget\nuget.exe'
    }

    .\.nuget\NuGet.exe update -self
}

function Install-NuGetPackage
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$True, Position = 1)][string]$PackageName,
        [Parameter(Mandatory=$True, Position = 2)][string]$PackageVersion,
        [Parameter(Mandatory=$True, Position = 3)][string]$OutputDirectory
    )

    .\.nuget\NuGet.exe install $PackageName -OutputDirectory $OutputDirectory -Version $PackageVersion
}

function Add-NuGetPackage
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$True, Position = 1)][string]$NuspecFileName,
        [Parameter(Mandatory=$True, Position = 2)][string]$OutputDirectory
    )

    # Call NuGet pack

    .\.nuget\NuGet.exe pack $NuspecFileName -Prop Configuration=Release -Output $OutputDirectory -Symbols
    
    # Throw a terminating exception if NuGet packaging failed

    if ($LastExitCode -ne 0)
    {
        throw "NuGet packaging failed."
    }
}

function Push-NuGetPackage
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$True, Position = 1)][string]$PackageFileName
    )

    .\.nuget\NuGet.exe push $PackageFileName
}

function Restore-NuGetPackages
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$False)][string]$SolutionFileName
    )

    .nuget\nuget restore $SolutionFileName
}
