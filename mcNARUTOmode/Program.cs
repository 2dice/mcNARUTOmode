using MinecraftConnection;

namespace mcNARUTOmode;

internal class Program
{
    private static void Main(string[] args)
    {
        // メインループ処理
        while (true)
        {
            // ループ周期の設定
            Setup.Command.Wait(30);
            // プレイヤーの情報を更新
            Setup.Player.UpdateStatus();
            // 手に持っているアイテムに応じて各Ninjaの処理を実行
            switch (Setup.Player.SelectedItemName)
            {
                case "minecraft:sand":
                    Setup.SandNinja.PreventFallDamage();
                    Setup.SandNinja.SetFootholdOnJump();
                    Setup.SandNinja.Attack();
                    // 下記関数は敵座標が変わるため最後に実行
                    Setup.SandNinja.AutoDefensiveWall();
                    break;
                default:
                    break;
            }
        }
    }
}

