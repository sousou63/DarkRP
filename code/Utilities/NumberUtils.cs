namespace Utils
{
  public static class NumberUtils
  {
    /// <summary>
    /// Format a number with a suffix (K, M, B, etc.)
    /// Starts formatting from 100,000.
    /// Also adds comma separators.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string FormatNumberWithSuffix(float number)
    {
        return number switch
        {
            >= 1_000_000_000 => (number / 1_000_000_000).ToString("0.#") + " B",
            >= 1_000_000 => (number / 1_000_000).ToString("0.#") + " M",
            >= 100_000 => (number / 1_000).ToString("0.#") + " K",
            _ => number.ToString("N0")
        };
    }
  }
}