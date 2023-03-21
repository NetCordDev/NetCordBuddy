using Microsoft.CodeAnalysis;

namespace NetCordBuddy.Docs;

// Adopted from DocFX code
internal static class SymbolHelper
{
    public static string? GetId(ISymbol symbol)
    {
        if (symbol is IDynamicTypeSymbol)
            return "dynamic";

        var id = symbol.GetDocumentationCommentId();

        if (id is null)
        {
            if (symbol is IFunctionPointerTypeSymbol functionPointerTypeSymbol)
            {
                // Roslyn doesn't currently support doc comments for function pointer type symbols
                // This returns just the stringified symbol to ensure the source and target parts
                // match for reference item merging.

                return functionPointerTypeSymbol.ToString()!;
            }

            return null;
        }

        return id[2..];
    }

    public static bool IsAccessible(ISymbol symbol)
    {
        // TODO: should we include implicitly declared members like constructors? They are part of the API contract.
        if (symbol.IsImplicitlyDeclared && symbol.Kind is not SymbolKind.Namespace || !GetDisplayAccessibility(symbol).HasValue)
            return false;

        if (symbol is IMethodSymbol methodSymbol && methodSymbol.AssociatedSymbol != null)
            return false;

        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.Name == "value__")
        {
            var b = fieldSymbol.ContainingType.BaseType;
            if (b != null && !IsAccessible(b))
                return false;
        }

        return symbol.ContainingSymbol is null || IsAccessible(symbol.ContainingSymbol);

        static Accessibility? GetDisplayAccessibility(ISymbol symbol)
        {
            // Hide internal or private APIs
            return symbol.DeclaredAccessibility switch
            {
                Accessibility.NotApplicable => symbol is ITypeSymbol ? null : Accessibility.NotApplicable,
                Accessibility.Public => Accessibility.Public,
                Accessibility.Protected => Accessibility.Protected,
                Accessibility.ProtectedOrInternal => Accessibility.Protected,
                _ => null,
            };
        }
    }
}
