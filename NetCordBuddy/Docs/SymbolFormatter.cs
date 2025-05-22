using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace NetCordBuddy.Docs;

// Adopted from https://github.com/dotnet/docfx/blob/main/src/Microsoft.DocAsCode.Dotnet/SymbolFormatter.cs
internal static partial class SymbolFormatter
{
    private static readonly SymbolDisplayFormat s_nameFormat = new(
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeExplicitInterface,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.AllowDefaultLiteral | SymbolDisplayMiscellaneousOptions.UseSpecialTypes,
        extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod);

    private static readonly SymbolDisplayFormat s_nameWithTypeFormat = s_nameFormat
        .AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
        .WithTypeQualificationStyle(SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static readonly SymbolDisplayFormat s_qualifiedNameFormat = s_nameWithTypeFormat
        .WithTypeQualificationStyle(SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static readonly SymbolDisplayFormat s_namespaceFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static readonly SymbolDisplayFormat s_methodNameFormat = s_nameFormat
        .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut);

    private static readonly SymbolDisplayFormat s_methodNameWithTypeFormat = s_nameWithTypeFormat
        .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut);

    private static readonly SymbolDisplayFormat s_methodQualifiedNameFormat = s_qualifiedNameFormat
        .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut);

    private static readonly SymbolDisplayFormat s_linkItemNameWithTypeFormat = new(
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

    private static readonly SymbolDisplayFormat s_linkItemQualifiedNameFormat = s_linkItemNameWithTypeFormat
        .WithTypeQualificationStyle(SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    public static string GetName(ISymbol symbol)
    {
        return GetNameParts(symbol).ToDisplayString();
    }

    public static ImmutableArray<SymbolDisplayPart> GetNameParts(
        ISymbol symbol, bool nullableReferenceType = true, bool overload = false)
    {
        return GetDisplayParts(symbol, nullableReferenceType, overload, symbol.Kind switch
        {
            SymbolKind.NamedType => s_nameWithTypeFormat,
            SymbolKind.Namespace => s_namespaceFormat,
            SymbolKind.Method => s_methodNameFormat,
            _ => s_nameFormat,
        });
    }

    public static string GetNameWithType(ISymbol symbol)
    {
        return GetNameWithTypeParts(symbol).ToDisplayString();
    }

    public static ImmutableArray<SymbolDisplayPart> GetNameWithTypeParts(
        ISymbol symbol, bool nullableReferenceType = true, bool overload = false)
    {
        return GetDisplayParts(symbol, nullableReferenceType, overload, symbol.Kind switch
        {
            SymbolKind.Namespace => s_namespaceFormat,
            SymbolKind.Method => s_methodNameWithTypeFormat,
            _ => s_nameWithTypeFormat,
        });
    }

    public static string GetQualifiedName(ISymbol symbol)
    {
        return GetQualifiedNameParts(symbol).ToDisplayString();
    }

    public static ImmutableArray<SymbolDisplayPart> GetQualifiedNameParts(
        ISymbol symbol, bool nullableReferenceType = true, bool overload = false)
    {
        return GetDisplayParts(symbol, nullableReferenceType, overload, symbol.Kind switch
        {
            SymbolKind.Namespace => s_namespaceFormat,
            SymbolKind.Method => s_methodQualifiedNameFormat,
            _ => s_qualifiedNameFormat,
        });
    }

    private static ImmutableArray<SymbolDisplayPart> GetDisplayParts(
        ISymbol symbol, bool nullableReferenceType, bool overload, SymbolDisplayFormat format)
    {
        if (!nullableReferenceType)
            format = format.RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        if (overload)
            format = format.RemoveMemberOptions(SymbolDisplayMemberOptions.IncludeParameters);

        try
        {
            var result = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.ToDisplayParts(symbol, format);

            if (overload && symbol.Kind is SymbolKind.Method && ((IMethodSymbol)symbol).MethodKind is MethodKind.Conversion)
                return GetCastOperatorOverloadDisplayParts(result);

            return result;
        }
        catch (InvalidOperationException)
        {
            return [];
        }

        static ImmutableArray<SymbolDisplayPart> GetCastOperatorOverloadDisplayParts(ImmutableArray<SymbolDisplayPart> parts)
        {
            // Convert from "explicit operator Bar" to "explicit operator", for lack of disabling return type in SymbolDisplay.
            var endIndex = parts.Length;
            while (--endIndex >= 0)
            {
                var part = parts[endIndex];
                if (part.Kind is SymbolDisplayPartKind.Keyword && part.ToString() is "operator" or "checked")
                    break;
            }
            return [.. parts.Take(endIndex + 1)];
        }
    }

    private static SymbolDisplayFormat WithTypeQualificationStyle(this SymbolDisplayFormat format, SymbolDisplayTypeQualificationStyle style)
    {
        return new(
            format.GlobalNamespaceStyle,
            style,
            format.GenericsOptions,
            format.MemberOptions,
            format.DelegateStyle,
            format.ExtensionMethodStyle,
            format.ParameterOptions,
            format.PropertyStyle,
            format.LocalOptions,
            format.KindOptions,
            format.MiscellaneousOptions);
    }
}