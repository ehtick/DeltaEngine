using DeltaEngine.Assets.Defaults;
using DeltaEngine.Assets;
using DeltaEngine.ECS;
using DeltaEngine.ECS.Components;
using DeltaEngine.Runtime;
using DeltaEngine.EditorLib.Loader;
using DeltaMaths;
using System.Diagnostics;

try
{
    string directoryPath = ProjectCreator.GetExecutableDirectory();
    var projectPath = new EditorPaths(directoryPath);
    ProjectCreator.CreateProject(projectPath);
    using var ctx = RuntimeContextFactory.CreateWindowedContext(projectPath);
    using var eng = new Runtime(ctx);

    //VCShader.Init();
    DefaultsImporter<MeshData>.Import(Path.Combine(Directory.GetCurrentDirectory(), "Import", "Models"));
    //MaterialsImporter.Import(Path.Combine(Directory.GetCurrentDirectory(), "Import", "Shaders"));

    var camera = IRuntimeContext.Current.SceneManager.CurrentScene.AddEntity();
    camera.Entity.Add<Transform>();
    camera.Entity.Add<Camera>();
    camera.Entity.Get<Transform>() = new Transform()
    {
        rotation = quaternion.identity,
        scale = new float3(1),
        position = new float3(0, 0, -5),
    };
    var cam = camera.Entity.Get<Camera>();
    camera.Entity.Get<Camera>() = new Camera();
    cam = camera.Entity.Get<Camera>();

    var render = IRuntimeContext.Current.SceneManager.CurrentScene.AddEntity();
    render.Entity.Add<Transform>();
    render.Entity.Add<Render>();

    render.Entity.Get<Transform>() = new Transform()
    {
        rotation = quaternion.identity,
        scale = new float3(1),
        position = float3.zero
    };

    render.Entity.Get<Render>() = new Render()
    {
        material = IRuntimeContext.Current.AssetImporter.GetAllAssets<MaterialData>()[0],
        mesh = IRuntimeContext.Current.AssetImporter.GetAllAssets<MeshData>()[0],
    };


    eng.Context.Running = true;

    while (true)
    {
        eng.Run();
        Thread.Yield();
    }
}
catch (Exception e) when (e is IOException or InvalidOperationException or UnauthorizedAccessException or DllNotFoundException)
{
    Console.WriteLine(e);
}
Console.ReadLine();
