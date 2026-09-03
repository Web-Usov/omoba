using System.Xml.Linq;

namespace OpenMoba.ArchitectureTests;

public sealed class ProjectDependencyGraphTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void Sim_Targets_Net8()
    {
        var project = LoadProject("src/OpenMoba.Sim/OpenMoba.Sim.csproj");
        Assert.Equal("net8.0", GetTargetFramework(project));
    }

    [Fact]
    public void SharedProjects_Keep_Expected_Tfms()
    {
        Assert.Equal("net8.0", GetTargetFramework(LoadProject("src/OpenMoba.Contracts/OpenMoba.Contracts.csproj")));
        Assert.Equal("net8.0", GetTargetFramework(LoadProject("src/OpenMoba.ModApi.Contracts/OpenMoba.ModApi.Contracts.csproj")));
        Assert.Equal("net8.0", GetTargetFramework(LoadProject("src/OpenMoba.Sim/OpenMoba.Sim.csproj")));
        Assert.Equal("net10.0", GetTargetFramework(LoadProject("src/OpenMoba.Server/OpenMoba.Server.csproj")));
        Assert.Equal("net10.0", GetTargetFramework(LoadProject("src/OpenMoba.Cli/OpenMoba.Cli.csproj")));
        Assert.Equal("net8.0", GetTargetFramework(LoadProject("src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj")));
    }

    [Fact]
    public void Sim_Does_Not_Reference_Client_Or_Server()
    {
        var references = GetProjectReferences("src/OpenMoba.Sim/OpenMoba.Sim.csproj");
        Assert.DoesNotContain(references, path => path.Contains("OpenMoba.Client.Godot", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, path => path.Contains("OpenMoba.Server", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Sim_Transitive_Graph_Has_No_Godot_Dependency()
    {
        var reachable = GetTransitiveProjectGraph("src/OpenMoba.Sim/OpenMoba.Sim.csproj");

        Assert.DoesNotContain(
            reachable,
            relativePath => relativePath.Contains("OpenMoba.Client.Godot", StringComparison.OrdinalIgnoreCase));

        foreach (var relativePath in reachable)
        {
            var project = LoadProject(relativePath);
            Assert.False(UsesGodotSdk(project), $"{relativePath} uses Godot.NET.Sdk");
            Assert.DoesNotContain(GetPackageReferences(project), name => IsGodotPackage(name));
            Assert.DoesNotContain(GetAssemblyReferences(project), name => IsGodotAssembly(name));
        }
    }

    [Fact]
    public void Client_References_Contracts_But_Not_Sim()
    {
        var references = GetProjectReferences("src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj");
        Assert.Contains(references, path => path.Replace('\\', '/').EndsWith("OpenMoba.Contracts/OpenMoba.Contracts.csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, path => path.Contains("OpenMoba.Sim", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Server_References_Sim()
    {
        var references = GetProjectReferences("src/OpenMoba.Server/OpenMoba.Server.csproj");
        Assert.Contains(references, path => path.Replace('\\', '/').EndsWith("OpenMoba.Sim/OpenMoba.Sim.csproj", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Client_Uses_Pinned_Godot_Sdk()
    {
        var project = LoadProject("src/OpenMoba.Client.Godot/OpenMoba.Client.Godot.csproj");
        Assert.Equal("Godot.NET.Sdk/4.7.2", GetSdk(project));
    }

    private static HashSet<string> GetTransitiveProjectGraph(string relativeProjectPath)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();
        queue.Enqueue(Normalize(relativeProjectPath));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (var reference in GetProjectReferences(current))
            {
                var absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.Combine(RepositoryRoot, current))!, reference));
                var relative = Path.GetRelativePath(RepositoryRoot, absolute);
                queue.Enqueue(Normalize(relative));
            }
        }

        return visited;
    }

    private static IReadOnlyList<string> GetProjectReferences(string relativeProjectPath)
    {
        var project = LoadProject(relativeProjectPath);
        return project
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => include!)
            .ToArray();
    }

    private static IReadOnlyList<string> GetPackageReferences(XDocument project)
    {
        return project
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include") ?? element.Element("Include")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray();
    }

    private static IReadOnlyList<string> GetAssemblyReferences(XDocument project)
    {
        return project
            .Descendants("Reference")
            .Select(element => (string?)element.Attribute("Include") ?? element.Element("Include")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray();
    }

    private static XDocument LoadProject(string relativeProjectPath)
    {
        var path = Path.Combine(RepositoryRoot, relativeProjectPath);
        Assert.True(File.Exists(path), $"Missing project file: {relativeProjectPath}");
        return XDocument.Load(path);
    }

    private static string GetTargetFramework(XDocument project)
    {
        var tfm = project.Descendants("TargetFramework").Select(element => element.Value).FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(tfm), "TargetFramework is missing");
        return tfm!;
    }

    private static string GetSdk(XDocument project)
    {
        var sdk = project.Root?.Attribute("Sdk")?.Value;
        Assert.False(string.IsNullOrWhiteSpace(sdk), "Project Sdk is missing");
        return sdk!;
    }

    private static bool UsesGodotSdk(XDocument project)
    {
        var sdk = project.Root?.Attribute("Sdk")?.Value ?? string.Empty;
        return sdk.Contains("Godot.NET.Sdk", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGodotPackage(string packageName)
    {
        return packageName.Contains("Godot", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGodotAssembly(string assemblyName)
    {
        return assemblyName.Contains("Godot", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OpenMoba.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root containing OpenMoba.sln.");
    }
}
