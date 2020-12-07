namespace CleanupUserProfile
{
    internal static class Helpers
    {
        public static T WithFlag<T>(
            this T value,
            T add) =>
            (T)(object)((int)(object)value | (int)(object)add);

        public static T WithoutFlag<T>(
            this T value,
            T remove) =>
            (T)(object)((int)(object)value & ~(int)(object)remove);
    }
}
