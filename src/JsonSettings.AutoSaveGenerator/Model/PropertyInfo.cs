using System.Collections.Generic;

namespace Nucs.JsonSettings.Autosave;

internal class PropertyInfo
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = new();
}
