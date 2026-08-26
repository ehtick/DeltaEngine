using DeltaEngine.ECS.Attributes;
using DeltaEngine.Integration;
using DeltaEngine.Runtime;
using DeltaEngine.EditorLib.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DeltaEngine.EditorLib.Compile;

internal sealed class CompilerModule : ICompilerModule
{
    private readonly CompileHelper _compileHelper;

    private AssemblyLoadContext? _context;
    private AssemblyLoadContext.ContextualReflectionScope _scope;

    private readonly HashSet<WeakReference<AssemblyLoadContext>> _oldAlcs = [];
    private readonly HashSet<Type> _components = [];

    public IAccessorsContainer? Accessors { get; private set; }
    public List<Type> Components => new(_components);

    public CompilerModule(IProjectPath projectPath)
    {
        _compileHelper = new CompileHelper(projectPath);
    }

    public void Recompile()
    {
        _context = NewLoadContext();
        Compile();

        _components.UnionWith(GetComponents());
        Accessors = Activator.CreateInstance(AccessorsContainerType()) as IAccessorsContainer ??
            throw new InvalidOperationException("Generated accessor container could not be created.");
    }

    private void Compile()
    {
        Debug.Assert(_context != null);
        LoadAssembly(_compileHelper.CompileScripts());
        HashSet<Type> components = new(GetComponents());
        LoadAssembly(_compileHelper.CompileAccessors(components));
    }

    private void LoadAssembly(ScriptCompilationResult result)
    {
        if (!result.Success)
        {
            string diagnostics = string.Join(
                Environment.NewLine,
                result.Diagnostics.Select(d => $"{d.Id}: {d.Message}"));
            throw new InvalidOperationException($"Script compilation failed.{Environment.NewLine}{diagnostics}");
        }

        using var assemblyStream = new MemoryStream(result.AssemblyBytes.ToArray(), writable: false);
        if (result.PdbBytes.IsEmpty)
        {
            var context = _context ?? throw new InvalidOperationException("Script load context is unavailable.");
            context.LoadFromStream(assemblyStream);
            return;
        }

        using var pdbStream = new MemoryStream(result.PdbBytes.ToArray(), writable: false);
        var contextWithSymbols = _context ?? throw new InvalidOperationException("Script load context is unavailable.");
        contextWithSymbols.LoadFromStream(assemblyStream, pdbStream);
    }

    private AssemblyLoadContext NewLoadContext()
    {
        UnloadContext();

        var loadContext = new AssemblyLoadContext("Scripting", true);
        _scope = loadContext.EnterContextualReflection();
        return loadContext;
    }

    private void UnloadContext()
    {
        _components.Clear();
        Accessors = null;
        _scope.Dispose();
        _scope = default;
        _context?.Unload();
        if (_context != null)
        {
            _oldAlcs.Add(new WeakReference<AssemblyLoadContext>(_context, false));
        }

        _context = null;
        _oldAlcs.RemoveWhere(r => !r.TryGetTarget(out _));
    }


    private static Type AccessorsContainerType()
    {
        var context = AssemblyLoadContext.CurrentContextualReflectionContext ??
            throw new InvalidOperationException("No contextual reflection context is active.");
        var contextAssemblies = context.Assemblies;
        var contextTypes = contextAssemblies.SelectMany(x => x.GetTypes());
        return contextTypes.FirstOrDefault(t => typeof(IAccessorsContainer).IsAssignableFrom(t)) ??
            throw new InvalidOperationException("Generated accessor container type was not found.");
    }

    private static IEnumerable<Type> GetComponents()
    {
        var context = AssemblyLoadContext.CurrentContextualReflectionContext ??
            throw new InvalidOperationException("No contextual reflection context is active.");
        var contextAssemblies = context.Assemblies;
        return contextAssemblies.Select(GetComponents).
            Concat(AssemblyLoadContext.Default.Assemblies.Select(GetComponents)).
            SelectMany(type => type);
    }

    private static IEnumerable<Type> GetComponents(Assembly assembly)
    {
        return assembly.GetTypes().
            Where(type => type.GetCustomAttribute<ComponentAttribute>() != null);
    }
}
