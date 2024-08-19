namespace Utils
{
  public static class NumberUtils
  {
    /// <summary>
    /// Format a number with a suffix (K, M, B, etc.)
    /// Starts formatting from 100,000.
    /// Also adds comma seperators.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string FormatNumberWithSuffix(float number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000).ToString("N0") + " B";
        if (number >= 1_000_000)
            return (number / 1_000_000).ToString("N0") + " M";
        if (number >= 100_000)
            return (number / 1_000).ToString("N0") + " K";
        return number.ToString("N0");
    }
  }
}