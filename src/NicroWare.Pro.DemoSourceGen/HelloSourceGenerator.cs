
using Microsoft.CodeAnalysis;

namespace NicroWare.Pro.DemoSourceGen;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		// Initialize code here
	}

	public void Execute(GeneratorExecutionContext context)
	{
		// Generating extra code here
	}
}