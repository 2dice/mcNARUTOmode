namespace mcNARUTOmode;

// 汎用的に使用する処理を実装するクラス
internal static class Utility
{
    // プレイヤーと敵の相対位置を渡すと、yaw pitchの角度(degree)を返す関数
    // 使用例：敵のいる方向を向く
    // Setup.Command.SendCommand($"teleport @p {(int)Setup.Player.LatestPosition.X} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z} {_yaw} {_pitch}");
    internal static (float yaw, float pitch) GetRotationFromRelative(int x, int y, int z)
    {
        double xy = Math.Sqrt(x * x + z * z);
        double yaw = Math.Atan2(-x, z) * 180 / Math.PI;
        double pitch = Math.Atan2(-y, xy) * 180 / Math.PI;
        return ((float)yaw, (float)pitch);
    }
}
