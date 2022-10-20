
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NicroWare.Pro.DemoSourceGen;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new NotifiSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not NotifiSyntaxReceiver nsr)
            return;

        foreach (var cls in nsr.Classes)
        {
            var sementicModel = context.Compilation.GetSemanticModel(cls.SyntaxTree);
            var symbol = sementicModel.GetDeclaredSymbol(cls) as ITypeSymbol;

            var mem = symbol.GetMembers();

            var allFields = mem.OfType<IFieldSymbol>();

            var source = $$"""
                using System;
                using System.ComponentModel;
                using System.Runtime.CompilerServices;

                namespace {{symbol.ContainingNamespace.ToDisplayString()}};

                public partial class {{symbol.Name}}
                {
                    public event PropertyChangedEventHandler? PropertyChanged;

                    private void OnPropertyChanged([CallerMemberName] string name = null!)
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                    }
                {{string.Join(Environment.NewLine, allFields.Select(x => $"    public {x.Type.ToString()} {x.Name.Substring(0, 1).ToUpper()}{x.Name.Substring(1)} {{ get => {x.Name}; set  {{ {x.Name} = value; OnPropertyChanged(); }}}}"))}}
                }
                """;

            context.AddSource(symbol.Name + ".g.cs", source);
        }
    }
}


public class NotifiSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; } = new();
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax cds && (cds.BaseList?.Types.Any(a => a.ToString() == "INotifyPropertyChanged") ?? false))
        {
            Classes.Add(cds);
        }
    }
}