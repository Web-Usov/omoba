using OpenMoba.Sim;

if (args is ["--smoke"])
{
    try
    {
        var composition = BootstrapHost.Create();
        if (string.IsNullOrWhiteSpace(composition.ComponentName))
        {
            Console.Error.WriteLine("Smoke composition failed: empty component name.");
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
