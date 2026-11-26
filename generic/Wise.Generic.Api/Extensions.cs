public static class Extensions
{
  public static string Join(this string[] strings, string union)
  {
    var sb = new System.Text.StringBuilder();
    for (int i = 0; i < strings.Length; i++)
    {
      var value = strings[i];
      if (string.IsNullOrWhiteSpace(value)) continue;

      if (i != 0) sb.Append(union);
      sb.Append(strings[i]);
    }

    return sb.ToString();
  }
}