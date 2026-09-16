using Microsoft.CodeAnalysis.Text;

namespace Majipro.Converter.Generator.Generating;

/// <summary>One file, ready to be handed to the compiler.</summary>
internal readonly struct GeneratedSource
{
    public string FileName { get; }

    public SourceText Text { get; }

    public GeneratedSource(string fileName, SourceText text)
    {
        FileName = fileName;
        Text = text;
    }
}
