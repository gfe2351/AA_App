using System.Reflection;
using AA_App.Data;

namespace AA_App.Tasks;

public class Column
{
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; } = "text";      // text | number | date | select
    public string[] Options { get; set; } = Array.Empty<string>();

    public static Column T(string key, string title) => new Column { Key = key, Title = title };
    public static Column N(string key, string title) => new Column { Key = key, Title = title, Type = "number" };
    public static Column D(string key, string title) => new Column { Key = key, Title = title, Type = "date" };
    public static Column S(string key, string title, params string[] options)
        => new Column { Key = key, Title = title, Type = "select", Options = options };
}

public class Row
{
    private readonly Dictionary<string, string> _values;
    private readonly Dictionary<string, string> _titles;

    public Row(Dictionary<string, string> values, Dictionary<string, string> titles)
    {
        _values = values;
        _titles = titles;
    }

    private string TitleOf(string key) => _titles.TryGetValue(key, out var t) ? t : key;

    public string Opt(string key) => _values.TryGetValue(key, out var s) ? (s ?? "").Trim() : "";

    public string Str(string key)
    {
        var s = Opt(key);
        if (s.Length == 0) throw new FormatException($"Поле «{TitleOf(key)}» не заполнено");
        return s;
    }

    public int Int(string key)
    {
        if (!int.TryParse(Opt(key), out var n)) throw new FormatException($"Поле «{TitleOf(key)}» должно быть целым числом");
        return n;
    }
}

public class Block
{
    public string Kind { get; set; }                // info | ok | err | table
    public string Text { get; set; }
    public string[] Headers { get; set; }
    public List<string[]> Rows { get; set; }
}

public class TaskResult
{
    public List<Block> Blocks { get; } = new();

    public void Info(string text) => Blocks.Add(new Block { Kind = "info", Text = text });
    public void Ok(string text) => Blocks.Add(new Block { Kind = "ok", Text = text });
    public void Err(string text) => Blocks.Add(new Block { Kind = "err", Text = text });
    public void Table(string title, string[] headers, List<string[]> rows)
        => Blocks.Add(new Block { Kind = "table", Text = title, Headers = headers, Rows = rows });

    public string Summary()
    {
        var s = string.Join(" | ", Blocks.Where(b => b.Kind != "table").Select(b => b.Text.Replace("\n", " ")));
        return s.Length > 600 ? s.Substring(0, 600) + "…" : s;
    }
}

public class TaskDef
{
    public int Number { get; set; }
    public string Section { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Column[] Columns { get; set; } = Array.Empty<Column>();
    public List<string[]> Sample { get; set; } = new();
    public Func<List<Row>, TaskResult> Run { get; set; }
}

public class TaskPageModel
{
    public TaskDef Def { get; set; }
    public List<string[]> Rows { get; set; }
    public TaskResult Result { get; set; }
    public List<TaskRun> History { get; set; } = new();
}

/// <summary>Показывает модификаторы доступа членов класса через рефлексию.</summary>
public static class AccessInfo
{
    public static readonly string[] Headers = { "Член", "Вид", "Модификатор", "В наследнике", "Снаружи" };

    public static string Modifier(MemberInfo m) => m switch
    {
        FieldInfo f => Of(f.IsPublic, f.IsPrivate, f.IsFamily, f.IsAssembly, f.IsFamilyOrAssembly, f.IsFamilyAndAssembly),
        MethodBase mb => Of(mb.IsPublic, mb.IsPrivate, mb.IsFamily, mb.IsAssembly, mb.IsFamilyOrAssembly, mb.IsFamilyAndAssembly),
        PropertyInfo p => Modifier((MemberInfo)(p.GetMethod ?? p.SetMethod)),
        _ => "?"
    };

    private static string Of(bool pub, bool priv, bool fam, bool asm, bool famOrAsm, bool famAndAsm)
    {
        if (pub) return "public";
        if (famOrAsm) return "protected internal";
        if (famAndAsm) return "private protected";
        if (fam) return "protected";
        if (asm) return "internal";
        if (priv) return "private";
        return "?";
    }

    public static string Kind(MemberInfo m) => m switch
    {
        FieldInfo => "поле",
        PropertyInfo => "свойство",
        ConstructorInfo => "конструктор",
        MethodInfo => "метод",
        _ => "член"
    };

    public static string TypeModifier(Type t) => t.IsPublic ? "public" : t.IsNotPublic ? "internal" : "вложенный";

    public static bool InDerived(string mod) => mod != "private" && mod != "?";
    public static bool Outside(string mod) => mod == "public";

    public static List<string[]> Table(Type t, Func<MemberInfo, bool> filter, bool declaredOnly = true)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        if (declaredOnly) flags |= BindingFlags.DeclaredOnly;

        return t.GetMembers(flags)
            .Where(m => (m is FieldInfo || m is MethodInfo || m is PropertyInfo)
                        && !m.Name.StartsWith("<")
                        && !(m is MethodInfo mi && mi.IsSpecialName)
                        && filter(m))
            .OrderBy(m => m.Name)
            .Select(m =>
            {
                var mod = Modifier(m);
                return new[] { t.Name + "." + m.Name, Kind(m), mod, InDerived(mod) ? "Да" : "Нет", Outside(mod) ? "Да" : "Нет" };
            })
            .ToList();
    }
}
