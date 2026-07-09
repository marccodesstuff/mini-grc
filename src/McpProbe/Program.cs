using System.Reflection;
using System.Linq;

var mcpAsm = Assembly.Load("ModelContextProtocol");
Console.WriteLine($"Assembly: {mcpAsm.GetName().Name} v{mcpAsm.GetName().Version}");

var all = mcpAsm.GetExportedTypes().ToArray();
Console.WriteLine("\n=== All exported types ===");
foreach (var t in all) Console.WriteLine(t.FullName);

Console.WriteLine("\n=== IServiceCollection extensions ===");
var svcExt = all.First(t => t.Name == "McpServerServiceCollectionExtensions");
foreach (var m in svcExt.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
    Console.WriteLine($"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");

Console.WriteLine("\n=== IMcpServerBuilder via LINQ ===");
var iface = all.FirstOrDefault(t => t.Name == "IMcpServerBuilder");
Console.WriteLine("found: " + iface?.FullName);
if (iface != null)
{
    foreach (var m in iface.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        Console.WriteLine($"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
}

Console.WriteLine("\n=== McpServerBuilderExtensions ===");
var ext = all.FirstOrDefault(t => t.Name == "McpServerBuilderExtensions");
Console.WriteLine("found: " + ext?.FullName);
if (ext != null)
{
    foreach (var m in ext.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        Console.WriteLine($"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
}
