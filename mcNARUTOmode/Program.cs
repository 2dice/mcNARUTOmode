using MinecraftConnection;

namespace mcNARUTOmode;

internal class Program
{
    private static void Main(string[] args)
    {
        // メインループ処理
        while (true)
        {
            Setup.Command.Wait(30);
            Setup.Player.UpdateStatus();
            switch (Setup.Player.SelectedItemName)
            {
                case "minecraft:sand":
                    Setup.SandNinja.PreventFallDamage();
                    Setup.SandNinja.SetFootholdOnJump();
                    Setup.SandNinja.Attack();
                    Setup.SandNinja.AutoDefensiveWall();
                    break;
                default:
                    break;
            }
        }
    }
}

