using DeltaEngine.Generation.Models;
using DeltaEngine.Generation.Core;

using DeltaEngine.Generation.Templates;
internal class SystemCallInvokeTemplate(SystemCallModel model, string worldParameterName) : Template<SystemCallModel>(model)
{
    public override string ToString()
    {
        bool isStatic = Model.MethodSymbol.IsStatic;
        string arguments = isStatic ? string.Empty : $"{Model.System.ParameterModifierString} this, ";
        return $"{Model.SystemCallMethodName}({arguments}{worldParameterName});";
    }
}
