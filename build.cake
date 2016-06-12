//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var rootDirectory = Argument("rootDirectory", ".");
var versionSuffix = Argument("versionSuffix", "");
var useMSBuild = Argument("useMSBuild", false);
var useDotNetCore = !useMSBuild;

DirectoryPath baseDirectory = new DirectoryPath(rootDirectory);
DirectoryPath artifactsDirectory = baseDirectory.Combine("artifacts");
DirectoryPath srcDirectory = baseDirectory.Combine("src");
DirectoryPath testDirectory = baseDirectory.Combine("test");

List<FilePath> srcFiles = GetFiles(srcDirectory + "/**/project.json").ToList();
List<FilePath> csprojFiles = GetFiles(srcDirectory + "/**/*.csproj").ToList();
List<FilePath> nuspecFiles = GetFiles(srcDirectory + "/**/*.nuspec").ToList();

List<FilePath> testFiles = GetFiles(testDirectory + "/**/project.json").ToList();
List<FilePath> testCsprojFiles = GetFiles(testDirectory + "/**/*.csproj").ToList();

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);
        CleanDirectories(srcDirectory + "/**/bin");
        CleanDirectories(srcDirectory + "/**/obj");
        CleanDirectories(testDirectory + "/**/bin");
        CleanDirectories(testDirectory + "/**/obj");
    });

Task("Build-DotNetCore")
    .WithCriteria(useDotNetCore)
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("\"" + srcDirectory.ToString() + "\"");
        srcFiles.ForEach(p => DotNetCoreBuild("\"" + p + "\""));
    });

Task("Build-MSBuild")
    .WithCriteria(useMSBuild)
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(srcFiles);
        csprojFiles.ForEach(p => MSBuild(p));
    });

Task("BuildTests-DotNetCore")
    .WithCriteria(useDotNetCore)
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreRestore("\"" + testDirectory.ToString() + "\"");
        testFiles.ForEach(p => DotNetCoreBuild("\"" + p + "\""));
    });

Task("Test-DotNetCore")
    .WithCriteria(useDotNetCore)
    .IsDependentOn("BuildTests")
    .Does(() =>
    {
        testFiles.ForEach(p => DotNetCoreTest("\"" + p + "\""));
    });

Task("Pack-DotNetCore")
    .WithCriteria(useDotNetCore)
    .IsDependentOn("Build")
    .Does(() =>
    {
        var packSettings = new DotNetCorePackSettings()
            {
                OutputDirectory = artifactsDirectory,
                VersionSuffix = versionSuffix
            };
        
        srcFiles.ForEach(p => DotNetCorePack("\"" + p + "\"", packSettings));
    });

Task("Pack-MsBuild")
    .WithCriteria(useMSBuild)
    .IsDependentOn("Build")
    .Does(() =>
    {
        var packSettings = new NuGetPackSettings()
            {
                OutputDirectory = artifactsDirectory,
                Version = "2.0.0" + versionSuffix
            };
        
        srcFiles.ForEach(p => NuGetPack(p, packSettings));
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("Build-DotNetCore")
    .IsDependentOn("Build-MSBuild");

Task("BuildTests")
    .IsDependentOn("BuildTests-DotNetCore");

Task("Test")
    .IsDependentOn("Test-DotNetCore");

Task("Pack")
    .IsDependentOn("Pack-DotNetCore")
    .IsDependentOn("Pack-MSBuild");

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);