//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var rootDirectory = Argument("rootDirectory", ".");
var versionSuffix = Argument("versionSuffix", "");
var useMSBuild = Argument("useMSBuild", false);
var useDotNetCore = !useMSBuild;

DirectoryPath baseDirectory = new DirectoryPath(rootDirectory);
DirectoryPath artifactsDirectory = baseDirectory.Combine("artifacts");
DirectoryPath srcDirectory = baseDirectory.Combine("src");
DirectoryPath testDirectory = baseDirectory.Combine("test");

List<FilePath> srcFiles = GetFiles(srcDirectory + "/**/*.csproj").ToList();
List<FilePath> nuspecFiles = GetFiles(srcDirectory + "/**/*.nuspec").ToList();

List<FilePath> testFiles = GetFiles(testDirectory + "/**/*.csproj").ToList();

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
        var settings = new DotNetCoreBuildSettings()
        {
            Configuration = configuration
        };

        srcFiles.ForEach(p => DotNetCoreRestore("\"" + p + "\""));
        srcFiles.ForEach(p => DotNetCoreBuild("\"" + p + "\"", settings));
    });

Task("Build-MSBuild")
    .WithCriteria(useMSBuild)
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var settings = new MSBuildSettings()
        {
            Configuration = configuration
        };
        
        NuGetRestore(srcFiles);
        srcFiles.ForEach(p => MSBuild(p, settings));
    });

Task("BuildTests-DotNetCore")
    .WithCriteria(useDotNetCore)
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
    {
        testFiles.ForEach(p => DotNetCoreRestore("\"" + p + "\""));
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
                VersionSuffix = versionSuffix,
                Configuration = configuration
            };
        
        srcFiles.ForEach(p => DotNetCorePack("\"" + p + "\"", packSettings));
    });

Task("Pack-MSBuild")
    .WithCriteria(useMSBuild)
    .IsDependentOn("Build")
    .Does(() =>
    {
        var packSettings = new NuGetPackSettings()
            {
                OutputDirectory = artifactsDirectory,
                Version = "2.0.0" + "-" + versionSuffix,
                Symbols = true
            };
        
        nuspecFiles.ForEach(p => NuGetPack(p, packSettings));
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