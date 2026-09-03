using System.Reflection;

if (args is ["--smoke"])
{
    try
    {
        var simAssembly = Assembly.Load("OpenMoba.Sim");
        var bootstrapMarker = simAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "OpenMoba.Bootstrap");

        if (simAssembly.GetName().Name != "OpenMoba.Sim" || bootstrapMarker?.Value != "Sim")
        {
            Console.Error.WriteLine("Smoke composition failed: OpenMoba.Sim assembly metadata was not loaded.");
            return 1;
        }

        Console.WriteLine("""{"component":"OpenMoba.Server","mode":"smoke","status":"ok"}""");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 1;
    }
}

Console.Error.WriteLine("OpenMoba.Server bootstrap host. Pass --smoke for verification.");
return 1;
