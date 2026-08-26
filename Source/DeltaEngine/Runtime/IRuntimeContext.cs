using System;

namespace Delta.Engine.Runtime;

public interface IRuntimeContext : IDisposable
{
    internal IRuntimeContext? PreviousContext { get; set; }
    public bool Running { get; set; }
    public IAssetCollection AssetImporter { get; }
    public IProjectPath ProjectPath { get; }
    public ISceneManager SceneManager { get; }
    public IGraphicsModule GraphicsModule { get; }

    private static IRuntimeContext? _current;
    public static IRuntimeContext Current
    {
        get => _current ?? throw new InvalidOperationException("No runtime context is active.");
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            value.PreviousContext = _current;
            _current = value;
        }
    }

    internal static void RestoreCurrent(IRuntimeContext context)
    {
        if (ReferenceEquals(_current, context))
        {
            _current = context.PreviousContext;
        }

        context.PreviousContext = null;
    }
}
