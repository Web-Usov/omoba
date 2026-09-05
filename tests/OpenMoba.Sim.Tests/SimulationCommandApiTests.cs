using System.Diagnostics;
using System.Security;

namespace OpenMoba.Sim.Tests;

public sealed class SimulationCommandApiTests
{
    [Theory]
    [InlineData("public sealed record ExternalCommand : SimulationCommand;", "CS0534")]
    [InlineData("""
        public sealed record ExternalCommand : SimulationCommand
        {
            public ExternalCommand(SimulationCommand original) : base(original) { }
        }
        """, "CS0534")]
    [InlineData("""
        public sealed record ExternalCommand : SimulationCommand
        {
            public ExternalCommand(SimulationCommand original) : base(original) { }
            private protected override void RestrictToFoundationCommands() { }
        }
        """, "CS0115")]
    public async Task External_Consumer_Cannot_Define_Concrete_Commands(string declaration, string diagnostic)
    {
        // Отдельный assembly без InternalsVisibleTo: сначала доказываем, что
        // public API доступна consumer, затем проверяем отказ именно компилятора.
        var directory = Path.Combine(Path.GetTempPath(), $"OpenMoba.CommandApi.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var assemblyPath = SecurityElement.Escape(typeof(SimulationCommand).Assembly.Location);
            await File.WriteAllTextAsync(Path.Combine(directory, "Consumer.csproj"), $$"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <OutputType>Exe</OutputType>
                  </PropertyGroup>
                  <ItemGroup>
                    <Reference Include="OpenMoba.Sim">
                      <HintPath>{{assemblyPath}}</HintPath>
                    </Reference>
                  </ItemGroup>
                </Project>
                """);
            var sourcePath = Path.Combine(directory, "Program.cs");
            const string supportedConsumer = """
                using OpenMoba.Sim;

                var simulation = new SimulationInstance(new SimulationConfig(11));
                SimulationCommand create = new CreateEntityCommand();
                SimulationCommand createCopy = create with { };
                if (createCopy != create || object.ReferenceEquals(createCopy, create))
                    throw new System.Exception("Create record semantics changed.");
                simulation.Submit(createCopy);
                if (simulation.Tick.Value != 0 || simulation.CaptureSnapshot().ActiveEntities.Count != 0)
                    throw new System.Exception("Submit mutated simulation.");
                var created = (EntityCreatedOutcome)simulation.Advance().Outcomes[0];
                var destroy = new DestroyEntityCommand(default) with { EntityId = created.EntityId };
                SimulationCommand destroyBase = destroy;
                if ((destroyBase with { }) != destroy)
                    throw new System.Exception("Destroy record semantics changed.");
                simulation.Submit(destroy);
                if (simulation.Advance().Outcomes[0] is not EntityDestroyedOutcome ||
                    simulation.Tick.Value != 2 || simulation.CaptureSnapshot().ActiveEntities.Count != 0)
                    throw new System.Exception("Foundation lifecycle changed.");
                """;
            await File.WriteAllTextAsync(sourcePath, supportedConsumer);
            var supported = await RunDotnet(directory, "run", "--configuration", "Release");
            Assert.True(supported.ExitCode == 0, supported.Output);

            await File.AppendAllTextAsync(sourcePath, Environment.NewLine + declaration);
            var unsupported = await RunDotnet(directory, "build", "--no-restore", "--configuration", "Release");
            Assert.NotEqual(0, unsupported.ExitCode);
            Assert.Contains(diagnostic, unsupported.Output);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<(int ExitCode, string Output)> RunDotnet(string directory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"External consumer verification timed out in {directory}.");
        }
        return (process.ExitCode, await stdout + await stderr);
    }
}
