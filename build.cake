//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var rootDirectory = Argument("rootDirectory", ".");
var versionSuffix = Argument("versionSuffix", "");

DirectoryPath baseDirectory = new DirectoryPath(rootDirectory);
DirectoryPath artifactsDirectory = baseDirectory.Combine("artifacts");
DirectoryPath srcDirectory = baseDirectory.Combine("src");
DirectoryPath testDirectory = baseDirectory.Combine("test");

List<FilePath> srcFiles = GetFiles(srcDirectory + "/**/project.json").ToList();
List<FilePath> testFiles = GetFiles(testDirectory + "/**/project.json").ToList();

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

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("\"" + srcDirectory.ToString() + "\"");
        srcFiles.ForEach(p => DotNetCoreBuild("\"" + p + "\""));
    });

Task("BuildTests")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreRestore("\"" + testDirectory.ToString() + "\"");
        testFiles.ForEach(p => DotNetCoreBuild("\"" + p + "\""));
    });

Task("Test")
    .IsDependentOn("BuildTests")
    .Does(() =>
    {
        testFiles.ForEach(p => DotNetCoreTest("\"" + p + "\""));
    });

Task("Pack")
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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);