namespace mcNARUTOmode;

internal static class Utility
{
    internal static (float yaw, float pitch) GetRotationFromRelative(int x, int y, int z)
    {
        double xy = Math.Sqrt(x * x + z * z);
        double yaw = Math.Atan2(-x, z) * 180 / Math.PI;
        double pitch = Math.Atan2(-y, xy) * 180 / Math.PI;
        return ((float)yaw, (float)pitch);
    }
}
