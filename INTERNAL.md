# DeltaEngine internal notes

This file is explicitly internal and documents migration details that are not
part of the user API.

`EngineFrameContext` carries input, elapsed time and delta time for world and
UI stages. The host projects only surface identity into the time-free
`EngineRenderFrame`; renderer services never receive the scheduling clock.

The legacy `DeltaEngine` module facade remains a compatibility composition
around `IEngineRenderService`. It is not a second renderer lifecycle. The
windowed adapter owns session/pipeline rollback and delegates resource
ownership to DeltaRender.

The current retained-XAML library adapter is quarantined in DeltaEditor.UiHost
until DeltaXAML consumers migrate to `DeltaXAML.Contract.UiDisplayList`.
Do not expose its library model as an Engine contract or create a second public
display-list model in this repository.

Shader and text runtime ownership remains downstream. Engine may package a
compatibility artifact during migration, but it must not compile shaders or
own atlas/GPU resources.
