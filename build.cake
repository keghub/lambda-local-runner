#addin "Cake.ExtendedNuGet"
#addin "nuget:?package=NuGet.Core"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"

var target = Argument("Target", "Test");

FilePath SolutionFile = MakeAbsolute(File("LambdaLocalRunner.sln"));

var testFolder = SolutionFile.GetDirectory().Combine("tests");
var outputFolder = SolutionFile.GetDirectory().Combine("outputs");
var testOutputFolder = outputFolder.Combine("tests");
var coverageOutputFile = testOutputFolder.CombineWithFilePath("coverage.dcvr");
var dotCoverFolder = MakeAbsolute(Context.Tools.Resolve("dotcover.exe").GetDirect‌​ory());


Setup(context => 
{
    CleanDirectory(outputFolder);
});

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(SolutionFile.FullPath);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Debug"
    };

    DotNetCoreBuild(SolutionFile.FullPath, settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
{
    Information($"Looking for test projects in {testFolder.FullPath}");

    var testProjects = GetFiles($"{testFolder}/**/*.csproj");

    var dotCoverSettings = new DotCoverCoverSettings()
                                    .WithFilter("+:EMG.*")
                                    .WithFilter("-:Tests.*");

    foreach (var project in testProjects)
    {
        Information($"Testing {project.FullPath}");

        var testResultFile = testOutputFolder.CombineWithFilePath(project.GetFilenameWithoutExtension() + ".trx");
        var coverageResultFile = testOutputFolder.CombineWithFilePath(project.GetFilenameWithoutExtension() + ".dvcr");
        
        Verbose($"Saving test results on {testResultFile.FullPath}");

        var settings = new DotNetCoreTestSettings
        {
            NoBuild = true,
            NoRestore = true,
            Logger = $"trx;LogFileName={testResultFile.FullPath}"
        };

        DotCoverCover(context => 
        {
            context.DotNetCoreTest(project.FullPath, settings);
        }, coverageResultFile, dotCoverSettings);

        if (BuildSystem.IsRunningOnTeamCity)
        {
            TeamCity.ImportData("mstest", testResultFile);
        }
    }

    var coverageFiles = GetFiles($"{testOutputFolder}/*.dvcr");
    DotCoverMerge(coverageFiles, coverageOutputFile);
    DeleteFiles(coverageFiles);

    if (BuildSystem.IsRunningOnTeamCity)
    {
        TeamCity.ImportDotCoverCoverage(coverageOutputFile, dotCoverFolder);
    }
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    var packSettings = new DotNetCorePackSettings 
    {
        Configuration = "Release",
        OutputDirectory = outputFolder
    };

    DotNetCorePack(SolutionFile.FullPath, packSettings);
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var apiKey = EnvironmentVariable("NuGetApiKey");

    var settings = new DotNetCoreNuGetPushSettings
    {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = apiKey
    };

    var files = GetFiles($"{outputFolder}/*.nupkg");

    foreach (var file in files)
    {
        var fileName = file.GetFilename();

        try
        {
            Information($"Pushing {fileName}");

            DotNetCoreNuGetPush(file.FullPath, settings);
            Information($"{fileName} pushed!");
        }
        catch (CakeException)
        {
            Warning($"{fileName} already published, removing from artifacts");
            DeleteFile(file);
        }
    }
});

RunTarget(target);