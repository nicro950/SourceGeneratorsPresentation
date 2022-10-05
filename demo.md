## Introductions

```csharp
	public void Execute(GeneratorExecutionContext context)
	{
		// Generating extra code here
		var main = context.Compilation.GetEntryPoint(context.CancellationToken);

		var source = $$"""
			using System;

			namespace {{main.ContainingNamespace}};

			public static partial class {{main.ContainingType.Name}}
			{
				static partial void HelloFrom(string name) 
					=> Console.WriteLine($"Generator says: Hi '{name}'");
			}
			""";

		context.AddSource(main.ContainingType.Name + ".g.cs", source);
	}
```

```xml
<ItemGroup>
    <ProjectReference Include="..\PathTo\SourceGenerator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
</ItemGroup>
```

```csharp
public class Notifier : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public Notifier(string firstName, string lastName)
	{
		this.firstName = firstName;
		this.lastName = lastName;
	}

	private string firstName;
	public string FirstName 
	{
		get => firstName;
		set
		{
			firstName = value;
			OnPropertyChanged();
		}
	}

	private string lastName;
	public string LastName
	{
		get => lastName;
		set
		{
			lastName = value;
			OnPropertyChanged();
		}
	}

	private void OnPropertyChanged([CallerMemberName] string name = null!)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
```

```csharp
public void Initialize(GeneratorInitializationContext context)
{
    // Initialize code here
    Debugger.Launch();
    context.RegisterForSyntaxNotifications(() => new NotifiSyntaxReceiver());
}

///


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
```

```csharp
if (context.SyntaxReceiver is not NotifiSyntaxReceiver nsr)
    return;

foreach (var cls in nsr.Classes)
{
    var sementicModel = context.Compilation.GetSemanticModel(cls.SyntaxTree);
    var symbol = sementicModel.GetDeclaredSymbol(cls) as ITypeSymbol;

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
    }
    """;

    context.AddSource(symbol.Name + ".g.cs", source);
}
```

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
```


```csharp

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                {{ string.Join(Environment.NewLine, allFields.Select(x => $"    public {x.Type.ToString()} {x.Name.Substring(0, 1).ToUpper()}{x.Name.Substring(1)} {{ get => {x.Name}; set  {{ {x.Name} = value; OnPropertyChanged(); }}}}")) }}
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

```