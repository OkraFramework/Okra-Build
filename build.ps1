# Import Modules

Import-Module -Name ".\.build\scripts\Invoke-NuGet.psm1"

# Install Sake

If (!(Test-Path ".\packages\Sake.0.2.2"))
{
    Install-NuGet
    Install-NuGetPackage Sake 0.2.2 .\packages
}

# Run build steps

.\packages\Sake.0.2.2\tools\Sake.exe -I .build\shade -f .build\shade\makefile.shade @args

If ($LastExitCode)
{
    throw("Sake build process failed")
}