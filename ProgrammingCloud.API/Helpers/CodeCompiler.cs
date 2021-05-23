using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Helpers
{
    public class CodeCompiler
    {

        public static List<string> Compile(string sourceCode)
        {

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // define other necessary objects for compilation
            string assemblyName = Path.GetRandomFileName();

            //paths to all the framework.dll files
            var allPathsToFrameworkDllFiles = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            List<MetadataReference> references = new List<MetadataReference>();
            foreach (var dllPath in allPathsToFrameworkDllFiles)
            {
                references.Add(MetadataReference.CreateFromFile(dllPath));
            }

            // analyse and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // write IL code into memory
                EmitResult result = compilation.Emit(ms);
                var errors = new List<string>();
                if (!result.Success)
                {
                    // handle exceptions
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                                        
                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        errors.Add(diagnostic.ToString());
                    }
                    return errors;
                }
                else
                {
                    return errors;
                }
            }
        }
    }
}
