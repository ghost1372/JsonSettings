﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Nucs.JsonSettings.Autosave;

[Generator]
public class AutoSaveGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classesWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetClassWithAttribute(ctx))
            .Where(static classWithAttribute => classWithAttribute is not null);

        context.RegisterSourceOutput(classesWithAttribute, (context, classInfo) =>
        {
            var generatedCode = GenerateClassWithAutoSave(classInfo, context);
            context.AddSource($"{classInfo!.ClassName}_AutoSave.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    private static ClassInfo? GetClassWithAttribute(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null)
            return null;

        bool hasTargetAttribute = classSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "AutoSaveAttribute" ||
                         attr.AttributeClass?.ToDisplayString() == "AutoSave.AutoSaveAttribute");

        if (!hasTargetAttribute)
            return null;

        var accessibility = classSymbol.DeclaredAccessibility.ToString().ToLower();

        // Collect property information
        var properties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => new PropertyInfo
            {
                Name = p.Identifier.Text,
                Type = semanticModel.GetTypeInfo(p.Type).Type?.ToDisplayString() ?? "object",
                IsVirtual = p.Modifiers.Any(SyntaxKind.VirtualKeyword),
                IsOverride = p.Modifiers.Any(SyntaxKind.OverrideKeyword),
                DefaultValue = p.Initializer?.Value.ToString(),
                Attributes = p.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Select(a => a.ToString())
                    .ToList()
            })
            .ToList();
        
        var baseTypes = GetBaseTypes(classSymbol);

        // Return class info with properties and base types
        return new ClassInfo
        {
            Accessibility = accessibility,
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            Properties = properties,
            BaseTypes = baseTypes
        };
    }

    private static List<string> GetBaseTypes(INamedTypeSymbol classSymbol)
    {
        var baseTypes = new List<string>();
        var currentBase = classSymbol.BaseType;

        while (currentBase != null && currentBase.ToDisplayString() != "object")
        {
            baseTypes.Add(currentBase.ToDisplayString());
            currentBase = currentBase.BaseType;
        }

        foreach (var iface in classSymbol.Interfaces)
        {
            baseTypes.Add(iface.ToDisplayString());
        }

        return baseTypes;
    }

    private static string GenerateClassWithAutoSave(ClassInfo classInfo, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        // Add required usings
        sb.AppendLine("using Nucs.JsonSettings.Modulation;");
        sb.AppendLine();

        // File-scoped namespace
        sb.AppendLine($"namespace {classInfo.Namespace};");
        sb.AppendLine();

        // Start class declaration with base types if any
        var baseTypeList = classInfo.BaseTypes;
        baseTypeList.Remove("Nucs.JsonSettings.JsonSettings");
        baseTypeList.Remove("JsonSettings");
        var baseTypesString = classInfo.BaseTypes.Count > 0 ? " : " + string.Join(", ", baseTypeList) : string.Empty;
        sb.AppendLine($"{classInfo.Accessibility} partial class {classInfo.ClassName}{baseTypesString}");
        sb.AppendLine("{");

        foreach (var prop in classInfo.Properties)
        {
            string fieldName = char.ToLower(prop.Name[0]) + prop.Name.Substring(1);

            if (prop.Name.ToLower().Equals("version") && baseTypeList.Contains("Nucs.JsonSettings.Modulation.IVersionable"))
            {
                continue;
            }

            // Check for invalid naming convention
            if (char.IsUpper(prop.Name[0]) &&
                !prop.Name.StartsWith("_") &&
                !prop.Name.StartsWith("m_"))
            {
                // Create a diagnostic error
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SG001",
                        "Invalid Property Name",
                        $"The property name '{prop.Name}' must start with 'Lowercase Char', '_', or 'm_'.",
                        "Naming",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true), Location.None);

                // Report the diagnostic
                context.ReportDiagnostic(diagnostic);
            }

            // Generate property
            sb.AppendLine($"    {(prop.Name.ToLower().Equals("filename") ? "public override" : "public")}" +
                $" {prop.Type} {prop.Name.FirstCharToUpper()}");
            sb.AppendLine("    {");
            sb.AppendLine($"        get => {fieldName};");
            sb.AppendLine("        set");
            sb.AppendLine("        {");
            sb.AppendLine($"            if ({fieldName} != value)"); // Add the condition here
            sb.AppendLine("            {");
            sb.AppendLine($"                {fieldName} = value;");
            sb.AppendLine($"                OnPropertyChanged(nameof({prop.Name}));"); // Ensure OnPropertyChanged() is called
            sb.AppendLine("                Save();"); // Call Save() only if value changed
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private class ClassInfo
    {
        public string Accessibility { get; set; } = "public";
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public List<PropertyInfo> Properties { get; set; } = new();
        public List<string> BaseTypes { get; set; } = new();
    }

    private class PropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
        public string? DefaultValue { get; set; }
        public List<string> Attributes { get; set; } = new();
    }
}
public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        input = ConvertToPropertyName(input);
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };
    }

    public static string ConvertToPropertyName(string inputName)
    {
        // Remove common prefixes
        if (inputName.StartsWith("_"))
            inputName = inputName.Substring(1);
        else if (inputName.StartsWith("m_"))
            inputName = inputName.Substring(2);

        return inputName;
    }
}