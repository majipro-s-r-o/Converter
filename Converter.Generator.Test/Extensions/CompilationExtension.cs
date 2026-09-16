using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Extensions;

internal static class CompilationExtension
{
    /// <summary>
    /// Emits the compilation, loads it and calls <c>Main</c> on <see cref="TCompositor"/>.
    /// The type is looked up by its full name, because the loaded assembly contains its own copy
    /// of every compiled source file - only types coming from referenced assemblies are shared.
    /// </summary>
    internal static TOutput Run<TOutput, TCompositor>(this Compilation compilation)
    {
        using (var ms = new MemoryStream())
        {
            // write IL code into memory
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

                throw new AssertFailedException(
                    "Unable to compile generated code:" + Environment.NewLine +
                    string.Join(Environment.NewLine, errors));
            }

            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // create instance of the desired class and call the desired function

            Type type = assembly.GetType(typeof(TCompositor).FullName)
                        ?? throw new AssertFailedException($"There is no '{typeof(TCompositor).FullName}' in the compiled test case!");
            object obj = Activator.CreateInstance(type);

            return (TOutput)type.InvokeMember("Main",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                obj,
                []);
        }
    }
}
