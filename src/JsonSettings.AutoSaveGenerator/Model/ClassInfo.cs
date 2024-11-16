using System.Collections.Generic;

namespace Nucs.JsonSettings.Autosave;

internal class ClassInfo
{
    public string Accessibility { get; set; } = "public";
    public string ClassName { get; set; }
    public string Namespace { get; set; }
    public List<PropertyInfo> Properties { get; set; } = new();
    public List<string> BaseTypes { get; set; } = new();
}
