
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NicroWare.Pro.DemoSourceGen;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Initialize code here
        // Debugger.Launch();
        
        context.RegisterForSyntaxNotifications(() => new NotifiSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {

        if (!(context.SyntaxReceiver is NotifiSyntaxReceiver nsr))
            return;

        string ToPascalCase(string name)
            => name.Substring(0, 1).ToUpper() + name.Substring(1);

		foreach (var cls in nsr.Classes)
        {
            var sementicModel = context.Compilation.GetSemanticModel(cls.SyntaxTree);
            var symbol = sementicModel.GetDeclaredSymbol(cls) as ITypeSymbol;

            if (symbol == null)
                continue;

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

                    private void OnPropertyChanged([CallerMemberName] string fieldName = null!)
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
                    }
                {{ string.Join(Environment.NewLine, allFields.Select(x => $"    public {x.Type.ToString()} {ToPascalCase(x.Name)} {{ get => {x.Name}; set  {{ {x.Name} = value; OnPropertyChanged(); }}}}")) }}

                    public object GetValueForProperty(string name)
                        => name switch {
                {{ string.Join(Environment.NewLine, allFields.Select(x => $"            \"{ToPascalCase(x.Name)}\" => this.{x.Name},"))}}
                            _ => null,
                        };
                    
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
