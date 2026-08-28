using System;
using Delta.Engine.Integration;

namespace Delta.Engine.Runtime;

public interface IGraphicsModule : IDisposable
{
    IEngineRenderService Renderer { get; }

    (int width, int height) Size { get; set; }

    void Resize(int width, int height);

    void Execute();
}
