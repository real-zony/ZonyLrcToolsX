using FreeSql.DataAnnotations;

namespace ZonyLrcTools.Api.Domain.Settings;

[Table(Name = "Settings")]
public class Setting
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public Guid Id { get; set; }

    public string Key { get; protected set; } = null!;

    public string? Value { get; set; }

    public SettingValueType ValueType { get; protected set; }

    public Setting(string key, SettingValueType valueType)
    {
        Key = key;
        ValueType = valueType;
    }
}

public enum SettingValueType
{
    String = 1,
    Int = 2,
    Bool = 3,
    Double = 4
}