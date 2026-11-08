using System;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Diagnostics;

/// <summary>
/// A conversion the generator recognized as its own but can not write. It carries the diagnostic it
/// is reported as, so the compilation fails naming that conversion, instead of the catch all
/// <see cref="ConverterDiagnostics.UnhandledError"/>. Thrown while a pair is travelled, it ends that
/// pair only and the rest are still travelled, so every refused pair is reported in one build.
/// </summary>
internal sealed class ConversionException : Exception
{
    public DiagnosticDescriptor Descriptor { get; }

    public object[] MessageArguments { get; }

    public ConversionException(DiagnosticDescriptor descriptor, params object[] messageArguments)
        : base(string.Format(descriptor.MessageFormat.ToString(), messageArguments))
    {
        Descriptor = descriptor;
        MessageArguments = messageArguments;
    }
}
