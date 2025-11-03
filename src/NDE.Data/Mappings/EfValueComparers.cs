namespace NDE.Data.Mappings;

using System.Linq;

using Microsoft.EntityFrameworkCore.ChangeTracking;

public static class EfValueComparers
{
  public static readonly ValueComparer<Dictionary<string, string>> DictStringString =
    new ValueComparer<Dictionary<string, string>>(
        (a, b) => DictEquals(a, b),
        v => DictHash(v ?? new Dictionary<string, string>()),
        v => DictSnapshot(v)
    );

  public static readonly ValueComparer<List<string>> ListOfString =
    new ValueComparer<List<string>>(
      (a, b) => ListEquals(a ?? new List<string>(), b ?? new List<string>()),
      v => ListHash(v ?? new List<string>()),
      v => ListSnapshot(v)
    );

  public static readonly ValueComparer<float[]> ArrayOfFloat =
    new ValueComparer<float[]>(
        (c1, c2) => (c1 ?? Array.Empty<float>()).SequenceEqual(c2 ?? Array.Empty<float>()),
        c => (c ?? Array.Empty<float>()).Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => (c ?? Array.Empty<float>()).ToArray());

  private static bool ListEquals(List<string> a, List<string> b)
  {
    if (ReferenceEquals(a, b)) return true;
    if (a.Count != b.Count) return false;
    for (int i = 0; i < a.Count; i++)
    {
      var sa = a[i] ?? string.Empty;
      var sb = b[i] ?? string.Empty;
      if (!string.Equals(sa, sb, StringComparison.Ordinal)) return false;
    }
    return true;
  }

  private static int ListHash(List<string> v)
  {
    var hash = new HashCode();
    foreach (var s in v)
    {
      hash.Add(s ?? string.Empty, StringComparer.Ordinal);
    }
    return hash.ToHashCode();
  }

  private static List<string> ListSnapshot(List<string>? v)
    => v == null ? new List<string>() : new List<string>(v);

  public static readonly ValueComparer<Dictionary<string, int>> DictStringInt =
    new ValueComparer<Dictionary<string, int>>(
      (a, b) => DictEquals(a, b),
      v => DictHash(v ?? new Dictionary<string, int>()),
      v => DictSnapshot(v)
    );

  private static bool DictEquals(Dictionary<string, int>? a, Dictionary<string, int>? b)
  {
    if (ReferenceEquals(a, b)) return true;
    if (a is null || b is null) return false;
    if (a.Count != b.Count) return false;

    foreach (var pair in a)
    {
      if (!b.TryGetValue(pair.Key, out var vb) || vb != pair.Value)
        return false;
    }
    return true;
  }

  private static int DictHash(Dictionary<string, int> v)
  {
    var hash = new HashCode();
    foreach (var pair in v.OrderBy(p => p.Key, StringComparer.Ordinal))
    {
      hash.Add(pair.Key ?? string.Empty, StringComparer.Ordinal);
      hash.Add(pair.Value);
    }
    return hash.ToHashCode();
  }

  private static Dictionary<string, int> DictSnapshot(Dictionary<string, int>? v)
    => v == null ? new Dictionary<string, int>() : new Dictionary<string, int>(v);

  private static bool DictEquals(Dictionary<string, string>? a, Dictionary<string, string>? b)
  {
    if (ReferenceEquals(a, b)) return true;
    if (a is null || b is null) return false;
    if (a.Count != b.Count) return false;

    foreach (var pair in a)
    {
      if (!b.TryGetValue(pair.Key, out var vb) || vb != pair.Value)
        return false;
    }
    return true;
  }

  private static int DictHash(Dictionary<string, string> v)
  {
    var hash = new HashCode();
    foreach (var pair in v.OrderBy(p => p.Key, StringComparer.Ordinal))
    {
      hash.Add(pair.Key ?? string.Empty, StringComparer.Ordinal);
      hash.Add(pair.Value ?? string.Empty, StringComparer.Ordinal);
    }
    return hash.ToHashCode();
  }

  private static Dictionary<string, string> DictSnapshot(Dictionary<string, string>? v)
      => v == null ? new Dictionary<string, string>() : new Dictionary<string, string>(v);
}
