using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using KingdomExplorer.Catalog;
using KingdomExplorer.Models;

namespace KingdomExplorer.Tools;

/// <summary>
/// Documentation helper (TEAM 1): reads the compiled classes with reflection and prints
/// a Mermaid class diagram or an inheritance table, so the report always matches the code.
///   dotnet run -- --uml          class diagram of the Organism hierarchy + OrganismCatalog
///   dotnet run -- --uml full     every public class and interface
///   dotnet run -- --uml table    Markdown table: what each class inherits and adds
/// Paste the Mermaid text into https://mermaid.live to export a PNG or SVG for the report.
/// </summary>
public static class UmlGenerator
{
    private const BindingFlags Declared =
        BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.NonPublic;

    // ---------------- Mermaid class diagram ----------------

    public static string ToMermaid(bool full)
    {
        List<Type> types = SelectTypes(full);
        var sb = new StringBuilder();
        sb.AppendLine("classDiagram");
        foreach (Type t in types) AppendClass(sb, t);
        sb.AppendLine();
        foreach (string line in Relations(types)) sb.AppendLine(line);
        return sb.ToString();
    }

    private static List<Type> SelectTypes(bool full)
    {
        IEnumerable<Type> all = typeof(Organism).Assembly.GetTypes().Where(t =>
            t.IsPublic && !t.IsEnum && !(t.IsAbstract && t.IsSealed) &&   // skip static classes
            t.Name != "Program" &&
            t.GetCustomAttribute<CompilerGeneratedAttribute>() is null &&
            t.Namespace is { } ns && ns.StartsWith("KingdomExplorer") && ns != "KingdomExplorer.Tools");

        if (!full)
            all = all.Where(t => typeof(Organism).IsAssignableFrom(t) || t == typeof(OrganismCatalog));

        return all.OrderBy(Depth).ThenBy(t => t.Name).ToList();
    }

    private static void AppendClass(StringBuilder sb, Type t)
    {
        sb.AppendLine($"    class {t.Name} {{");
        if (t.IsInterface) sb.AppendLine("        <<interface>>");
        else if (t.IsAbstract) sb.AppendLine("        <<abstract>>");

        foreach (FieldInfo f in t.GetFields(Declared).Where(f => !f.Name.Contains('<') && !f.IsLiteral))
            sb.AppendLine($"        {Vis(f.IsPublic, f.IsFamily, f.IsPrivate)}{Name(f.FieldType)} {f.Name}{(f.IsStatic ? "$" : "")}");

        foreach (PropertyInfo p in t.GetProperties(Declared))
        {
            MethodInfo? acc = p.GetMethod ?? p.SetMethod;
            if (acc is null || acc.IsPrivate || acc.IsAssembly) continue;
            string mark = acc.IsStatic ? "$" : acc.IsAbstract && !t.IsInterface ? "*" : "";
            sb.AppendLine($"        {Vis(acc.IsPublic, acc.IsFamily || acc.IsFamilyOrAssembly, false)}{Name(p.PropertyType)} {p.Name}{mark}");
        }

        foreach (MethodInfo m in t.GetMethods(Declared).Where(m =>
                     !m.IsSpecialName && !m.IsPrivate && !m.IsAssembly && !m.Name.Contains('<')))
        {
            string args = string.Join(", ", m.GetParameters().Select(p => $"{Name(p.ParameterType)} {p.Name}"));
            string mark = m.IsStatic ? "$" : m.IsAbstract && !t.IsInterface ? "*" : "";
            sb.AppendLine($"        {Vis(m.IsPublic, m.IsFamily || m.IsFamilyOrAssembly, false)}{m.Name}({args}) {Name(m.ReturnType)}{mark}");
        }

        sb.AppendLine("    }");
    }

    private static List<string> Relations(List<Type> types)
    {
        var set = new HashSet<Type>(types);
        var lines = new List<string>();

        foreach (Type t in types)
        {
            if (t.BaseType is not null && set.Contains(t.BaseType))
                lines.Add($"    {t.BaseType.Name} <|-- {t.Name}");

            Type[] inherited = t.BaseType?.GetInterfaces() ?? Type.EmptyTypes;
            foreach (Type i in t.GetInterfaces().Except(inherited))
                if (set.Contains(i)) lines.Add($"    {i.Name} <|.. {t.Name}");

            var owned = t.GetFields(Declared).Where(f => !f.Name.Contains('<')).Select(f => (f.Name, f.FieldType))
                .Concat(t.GetProperties(Declared).Select(p => (p.Name, p.PropertyType)));

            var seen = new HashSet<Type>();
            foreach ((string member, Type memberType) in owned)
            {
                (Type elem, bool many) = Unwrap(memberType);
                if (!set.Contains(elem) || elem == t || seen.Contains(elem)) continue;
                if (elem.IsAssignableFrom(t) || t.IsAssignableFrom(elem)) continue;
                seen.Add(elem);
                lines.Add(many
                    ? $"    {t.Name} \"1\" o-- \"*\" {elem.Name} : {member}"
                    : $"    {t.Name} --> {elem.Name} : {member}");
            }
        }
        return lines;
    }

    // ---------------- Markdown inheritance table ----------------

    public static string ToMarkdownTable()
    {
        var types = typeof(Organism).Assembly.GetTypes()
            .Where(t => t.IsPublic && typeof(Organism).IsAssignableFrom(t))
            .OrderBy(Depth).ThenBy(t => t.Name).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("| Class | Kind | Inherits from | Inherited members | Adds or overrides |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (Type t in types)
        {
            var inherited = new List<string>();
            for (Type? b = t.BaseType; b is not null && b != typeof(object); b = b.BaseType)
                inherited.AddRange(PublicMembers(b).Select(m => m.Name));

            var added = PublicMembers(t).Select(m =>
                m.Name + (m.IsOverride ? " (override)" : m.IsAbstract ? " (abstract)" : ""));

            string baseName = t == typeof(Organism) || t.BaseType is null ? "-" : t.BaseType.Name;
            sb.AppendLine($"| {t.Name} | {(t.IsAbstract ? "abstract" : "concrete")} | {baseName} | " +
                          $"{Join(inherited.Distinct())} | {Join(added)} |");
        }
        return sb.ToString();
    }

    private record Member(string Name, bool IsOverride, bool IsAbstract);

    private static List<Member> PublicMembers(Type t)
    {
        var list = new List<Member>();
        foreach (PropertyInfo p in t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
        {
            MethodInfo? g = p.GetMethod;
            list.Add(new Member(p.Name, g is not null && g.GetBaseDefinition().DeclaringType != g.DeclaringType, g?.IsAbstract ?? false));
        }
        foreach (MethodInfo m in t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                     .Where(m => !m.IsSpecialName))
            list.Add(new Member(m.Name + "()", m.GetBaseDefinition().DeclaringType != m.DeclaringType, m.IsAbstract));
        return list;
    }

    private static string Join(IEnumerable<string> items)
    {
        string text = string.Join(", ", items);
        return text.Length > 0 ? text : "-";
    }

    // ---------------- helpers ----------------

    private static int Depth(Type t)
    {
        int d = 0;
        for (Type? b = t.BaseType; b is not null && b != typeof(object); b = b.BaseType) d++;
        return d;
    }

    private static string Vis(bool isPublic, bool isProtected, bool isPrivate) =>
        isPublic ? "+" : isProtected ? "#" : isPrivate ? "-" : "~";

    private static (Type, bool) Unwrap(Type t)
    {
        if (t.IsArray) return (t.GetElementType()!, true);
        if (t.IsGenericType && Nullable.GetUnderlyingType(t) is null)
            return (t.GetGenericArguments()[0], true);
        return (t, false);
    }

    private static string Name(Type t)
    {
        if (t == typeof(void)) return "void";
        if (t == typeof(string)) return "string";
        if (t == typeof(bool)) return "bool";
        if (t == typeof(int)) return "int";
        if (Nullable.GetUnderlyingType(t) is { } inner) return Name(inner) + "?";
        if (t.IsArray) return Name(t.GetElementType()!) + "[]";
        if (t.IsGenericType)
            return t.Name[..t.Name.IndexOf('`')] + "~" + string.Join(",", t.GetGenericArguments().Select(Name)) + "~";
        return t.Name;
    }
}
