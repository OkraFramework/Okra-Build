# Configuration

$projects = "MEFExtensions.DependencyInjection",
            "Okra.Core",
            "Okra.DependencyInjection.MEF",
            "Okra.Platform.UniversalWindows",
            "Okra.Platform.XamarinForms"
            
$destinationArtifacts = ".\artifacts"
$logFiles = ".\logs"

# Clean the artifacts and log folders

If (Test-Path $destinationArtifacts) { Remove-Item $destinationArtifacts -Recurse }
If (Test-Path $logFiles) { Remove-Item $logFiles -Recurse }

New-Item -Force -ItemType Directory -Path $destinationArtifacts | Out-Null
New-Item -Force -ItemType Directory -Path $logFiles | Out-Null

# Iterate through all the projects

ForEach ($project in $projects)
{
    Write-Host "  " $project -NoNewline
    
    $projectDirectory = Join-Path ".." $project
    $buildFile = Join-Path $projectDirectory "build.ps1"
    $sourceArtifacts = Join-Path $projectDirectory "artifacts\*"
    $logFile = Join-Path $logFiles ($project + ".log")
    
    If (Test-Path $buildFile)
    {
        $process = Start-Process Powershell -Argument $buildFile -Wait -NoNewWindow -RedirectStandardOutput $logFile -PassThru
        
        If (!$process.ExitCode)
        {
            Copy-Item $sourceArtifacts $destinationArtifacts
            Write-Host " : success" -ForegroundColor Green
        }
        Else
        {
            Write-Host " : failed" -ForegroundColor Red
        }
    }
    Else
    {
        Write-Host " : missing" -ForegroundColor Red    
    }
}