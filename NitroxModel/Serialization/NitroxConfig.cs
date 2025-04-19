using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxModel.Serialization;

public static class NitroxConfig
{
    private static readonly Dictionary<string, MemberInfo> typeCache = [];
    private static readonly Dictionary<string, object> unserializedMembersWarnOnceCache = [];

    private static readonly char[] newlineChars = Environment.NewLine.ToCharArray();
    private static readonly object locker = new();

    public static IDictionary<string, string> Parse(Stream stream)
    {
        using StreamReader reader = new(stream, leaveOpen: true, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096);
        return Parse(reader);
    }

    public static IDictionary<string, string> Parse(StreamReader stream)
    {
        Dictionary<string, string> result = [];
        char[] keyValueSeparator = ['='];
        int lineNum = 0;

        while (stream.ReadLine() is { } readLine)
        {
            lineNum++;
            if (readLine.Length < 1 || readLine[0] == '#')
            {
                continue;
            }

            if (readLine.Contains('='))
            {
                string[] keyValuePair = readLine.Split(keyValueSeparator, 2);
                result.Add(keyValuePair[0], keyValuePair[1]);
            }
            else
            {
                throw new Exception($"Incorrect format detected on line {lineNum}:{Environment.NewLine}{readLine}");
            }
        }
        return result;
    }

    public static void LoadIntoObject<TConfig>(string filePath, TConfig config) where TConfig : class
    {
        lock (locker)
        {
            Type type = typeof(TConfig);
            Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary<TConfig>();
            using StreamReader reader = new(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
            HashSet<MemberInfo> unserializedMembers = new(typeCachedDict.Values);

            foreach (KeyValuePair<string, string> pair in NitroxConfig.Parse(reader))
            {
                // Ignore case for property names in file.
                if (!typeCachedDict.TryGetValue(pair.Key.ToLowerInvariant(), out MemberInfo member))
                {
                    Log.Warn($"Property or field {pair.Key} does not exist on type {type.FullName}!");
                    continue;
                }
                unserializedMembers.Remove(member); // This member was serialized in the file

                if (!SetMemberValue(config, member, pair.Value))
                {
                    (Type type, object value) logData = member switch
                    {
                        FieldInfo field => (field.FieldType, field.GetValue(config)),
                        PropertyInfo prop => (prop.PropertyType, prop.GetValue(config)),
                        _ => (typeof(string), "")
                    };
                    Log.Warn($@"Property ""({logData.type.Name}) {member.Name}"" has an invalid value {StringifyValue(pair.Value)}. Using default value: {StringifyValue(logData.value)}");
                }
            }

            if (unserializedMembers.Count != 0)
            {
                string[] unserializedProps = unserializedMembers
                                             .Select(m =>
                                             {
                                                 object value = null;
                                                 if (m is FieldInfo field)
                                                 {
                                                     value = field.GetValue(config);
                                                 }
                                                 else if (m is PropertyInfo prop)
                                                 {
                                                     value = prop.GetValue(config);
                                                 }

                                                 if (unserializedMembersWarnOnceCache.TryGetValue(m.Name, out object cachedValue))
                                                 {
                                                     if (Equals(value, cachedValue))
                                                     {
                                                         return null;
                                                     }
                                                 }
                                                 unserializedMembersWarnOnceCache[m.Name] = value;
                                                 return $" - {m.Name}: {value}";
                                             })
                                             .Where(i => i != null)
                                             .ToArray();
                if (unserializedProps.Length > 0)
                {
                    Log.Warn($"{Path.GetFileName(filePath)} is using default values for the missing properties:{Environment.NewLine}{string.Join(Environment.NewLine, unserializedProps)}");
                }
            }
        }
    }

    public static void CreateFile<TConfig>(string filePath) where TConfig : class, new()
    {
        lock (locker)
        {
            Type type = typeof(TConfig);
            TConfig config = new();
            Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary<TConfig>();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new ArgumentException(nameof(filePath)));
                using StreamWriter stream = new(new FileStream(filePath, FileMode.Create, FileAccess.Write), Encoding.UTF8);
                WritePropertyDescription(type, stream);

                foreach (string name in typeCachedDict.Keys)
                {
                    MemberInfo member = typeCachedDict[name];

                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                    {
                        WritePropertyDescription(member, stream);
                        WriteProperty(field, field.GetValue(config), stream);
                    }

                    PropertyInfo property = member as PropertyInfo;
                    if (property != null)
                    {
                        WritePropertyDescription(member, stream);
                        WriteProperty(property, property.GetValue(config), stream);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.Error($"Config file {Path.GetFileName(filePath)} exists but is a hidden file and cannot be modified, config file will not be updated. Please make file accessible");
            }
        }
    }

    private static Dictionary<string, MemberInfo> GetTypeCacheDictionary<T>() where T : class
    {
        Type type = typeof(T);
        if (typeCache.Count == 0)
        {
            IEnumerable<MemberInfo> members = type.GetFields()
                                                  .Where(f => f.Attributes != FieldAttributes.NotSerialized && !f.IsLiteral)
                                                  .Concat(type.GetProperties()
                                                              .Where(p => p.CanWrite)
                                                              .Cast<MemberInfo>());

            try
            {
                foreach (MemberInfo member in members)
                {
                    typeCache.Add(member.Name.ToLowerInvariant(), member);
                }
            }
            catch (ArgumentException e)
            {
                Log.Error(e, $"Type {type.FullName} has properties that require case-sensitivity to be unique which is unsuitable for .properties format.");
                throw;
            }
        }

        return typeCache;
    }

    private static string StringifyValue(object value) => value switch
    {
        string _ => $@"""{value}""",
        null => @"""""",
        _ => value.ToString()
    };

    private static bool SetMemberValue(object instance, MemberInfo member, string valueFromFile)
    {
        object ConvertFromStringOrDefault(Type typeOfValue, out bool isDefault, object defaultValue = null)
        {
            try
            {
                object newValue = TypeDescriptor.GetConverter(typeOfValue).ConvertFrom(null!, CultureInfo.InvariantCulture, valueFromFile);
                isDefault = false;
                return newValue;
            }
            catch (Exception)
            {
                isDefault = true;
                return defaultValue;
            }
        }

        bool usedDefault;
        switch (member)
        {
            case FieldInfo field:
                field.SetValue(instance, ConvertFromStringOrDefault(field.FieldType, out usedDefault, field.GetValue(instance)));
                return !usedDefault;
            case PropertyInfo prop:
                prop.SetValue(instance, ConvertFromStringOrDefault(prop.PropertyType, out usedDefault, prop.GetValue(instance)));
                return !usedDefault;
            default:
                throw new Exception($"Serialized member must be field or property: {member}.");
        }
    }

    private static void WriteProperty<TMember>(TMember member, object value, StreamWriter stream) where TMember : MemberInfo
    {
        stream.Write(member.Name);
        stream.Write('=');
        stream.WriteLine(Convert.ToString(value, CultureInfo.InvariantCulture));
    }

    private static void WritePropertyDescription(MemberInfo member, StreamWriter stream)
    {
        PropertyDescriptionAttribute attribute = member.GetCustomAttribute<PropertyDescriptionAttribute>();
        if (attribute != null)
        {
            foreach (string line in attribute.Description.Split(newlineChars))
            {
                stream.Write("# ");
                stream.WriteLine(line);
            }
        }
    }
}

public abstract class NitroxConfig<T> where T : NitroxConfig<T>, new()
{
    public abstract string FileName { get; }

    public static T Load(string saveDir)
    {
        T config = new();
        config.Deserialize(saveDir);
        return config;
    }

    public void Deserialize(string saveDir)
    {
        NitroxConfig.LoadIntoObject(Path.Combine(saveDir, FileName), this);
    }

    public void Serialize(string saveDir)
    {
        NitroxConfig.CreateFile<T>(Path.Combine(saveDir, FileName));
    }

    /// <summary>
    ///     Ensures updates are properly persisted to the backing config file without overwriting user edits.
    /// </summary>
    public UpdateDisposable Update(string saveDir)
    {
        return new UpdateDisposable(this, saveDir);
    }

    public readonly struct UpdateDisposable : IDisposable
    {
        private string SaveDir { get; }
        private NitroxConfig<T> Config { get; }

        public UpdateDisposable(NitroxConfig<T> config, string saveDir)
        {
            config.Deserialize(saveDir);
            SaveDir = saveDir;
            Config = config;
        }

        public void Dispose() => Config.Serialize(SaveDir);
    }
}
