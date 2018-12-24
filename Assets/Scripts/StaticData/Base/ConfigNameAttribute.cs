using System;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigNameAttribute : Attribute
{
    public ConfigNameAttribute(string name)
    {
        _name = name;
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }

    string _name;
}
