using System;
using System.IO;
using System.Threading.Tasks;

using NuGet.Versioning;

using Xunit;

namespace BaGet.Protocol.Samples.Tests;

public class Sample01_Download
{
    [Fact]
    public async Task DownloadPackage()
    {
        // Downloads a package file (.nupkg)
        var client = new NuGetClient("https://api.nuget.org/v3/index.json");

        string packageId = "Newtonsoft.Json";
        var packageVersion = new NuGetVersion("12.0.1");

        await using Stream packageStream = await client.DownloadPackageAsync(packageId, packageVersion);
        Console.WriteLine($"Downloaded package {packageId} {packageVersion}");
    }

    [Fact]
    public async Task DownloadPackageManifest()
    {
        // Downloads a package manifest (.nuspec)
        var client = new NuGetClient("https://api.nuget.org/v3/index.json");

        string packageId = "Newtonsoft.Json";
        var packageVersion = new NuGetVersion("12.0.1");

        await using Stream manifestStream = await client.DownloadPackageManifestAsync(packageId, packageVersion);
        Console.WriteLine($"Downloaded package {packageId} {packageVersion}'s nuspec");
    }
}
