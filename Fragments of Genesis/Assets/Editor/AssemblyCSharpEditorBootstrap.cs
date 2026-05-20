using UnityEditor;

// Ensures Unity generates Assembly-CSharp-Editor for projects that currently
// do not contain any Editor assembly scripts but are referenced by tooling.
internal static class AssemblyCSharpEditorBootstrap
{
    [InitializeOnLoadMethod]
    private static void Init()
    {
        // Intentionally empty.
    }
}

