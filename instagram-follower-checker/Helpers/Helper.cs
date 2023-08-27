namespace instagram_follower_checker;

public static class Helper
{
    /// <summary>
    /// checks if a string is empty (null or "")
    /// </summary>
    /// <param name="s">the string to check</param>
    /// <returns>true, when string is empty. otherwise false</returns>
    public static bool IsEmpty(this string? s)
    {
        if (s == null)
            return true;

        if (s.Trim() == "")
            return true;

        return false;
    }
}