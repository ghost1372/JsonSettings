using System;

namespace Nucs.JsonSettings;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GenerateAutoSaveOnChangeAttribute : Attribute
{
}
