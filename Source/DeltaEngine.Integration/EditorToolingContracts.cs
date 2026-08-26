using System;
using System.Collections.Generic;

namespace DeltaEngine.Integration;

// Text composition is deliberately separate from physical key packets. A
// platform/editor adapter may implement this seam when IME composition is
// available without making the renderer or Engine core own text input state.
public interface IEngineImeCompositionSink
{
    void BeginComposition();
    void UpdateComposition(string text, int selectionStart, int selectionLength);
    void CommitComposition(string text);
    void CancelComposition();
}

public readonly record struct EngineComponentFieldDescriptor(
    string Id,
    string DisplayName,
    string ValueType,
    bool CanWrite);

public sealed record EngineComponentSchema(
    string ComponentTypeId,
    string DisplayName,
    IReadOnlyList<EngineComponentFieldDescriptor> Fields);

public readonly record struct EngineComponentValue(
    string ComponentTypeId,
    string FieldId,
    object? Value,
    long Version);

public readonly record struct EngineComponentEdit(
    string ComponentTypeId,
    string FieldId,
    object? Value,
    long ExpectedVersion);

public readonly record struct EngineComponentRegistration(
    string ComponentTypeId,
    EngineComponentSchema Schema,
    Func<object> CreateValue);

public interface IEngineComponentCatalog
{
    IReadOnlyList<EngineComponentRegistration> Registrations { get; }
}

public interface IEngineEntityComponentBinding
{
    bool TryRead(EngineEntityId entity, string componentTypeId, string fieldId, out EngineComponentValue value);
    bool TryWrite(EngineEntityId entity, in EngineComponentEdit edit, out string? writeError);
}

public readonly record struct EngineEntitySelection(EngineEntityId Entity, long Version);
