using System;
using MinecraftConnection;

internal class Setup
{
    //Minecraft Setup
    static readonly string address = "127.0.0.1";
    static readonly ushort port = 25575;
    static readonly string pass = "minecraft";
    internal static readonly MinecraftCommands command = new(address, port, pass);
    //Player Setup
    internal static readonly string PlayerName = "2dice_K";
    internal static readonly PlayerStatus player = new();
}

internal class Program
{
    private static void Main(string[] args)
    {
        // メインループ処理
        while (true)
        {
            Setup.command.Wait(300);
            Setup.player.UpdatePlayerStatus();
            Console.WriteLine(Setup.player.LatestPlayerPosition.X);
            Console.WriteLine(Setup.player.LatestPlayerPosition.Y);
            Console.WriteLine(Setup.player.LatestPlayerPosition.Z);
            Console.WriteLine(Setup.player.LastPlayerPosition.X);
            Console.WriteLine(Setup.player.LastPlayerPosition.Y);
            Console.WriteLine(Setup.player.LastPlayerPosition.Z);
        }
    }
}
internal class PlayerStatus
{
    //プレイヤー変数
    internal Position LatestPlayerPosition = new(0, 0, 0);
    internal Position LastPlayerPosition = new(0, 0, 0);
    internal Motion PlayerMotion = new(0, 0, 0);
    internal string SelectedItemName { get; set; } = "minecraft:sand";
    internal bool SelectedItemIsUsed { get; set; } = false;

    //敵変数
    internal Position NearestEnemyPosition = new(0, 0, 0);

    //コンストラクタ
    internal PlayerStatus()
    {
        Console.WriteLine("PlayerStatusInit!");
        //TODO:スコアボードの作成
    }

    //update関数(座標等の取得
    internal void UpdatePlayerStatus()
    {
        UpdatePlayerPosition();
    }
    private void UpdatePlayerPosition()
    {
        //前回値の保持
        LastPlayerPosition = LatestPlayerPosition;

        var pos = Setup.command.GetPlayerData(Setup.PlayerName).Position;
        // 整数座標に変換(末尾のdを削除して丸め)
        double x = Math.Floor(pos.X);
        double y = Math.Floor(pos.Y);
        double z = Math.Floor(pos.Z);
        // 更新前のインスタンスはGCで開放される
        LatestPlayerPosition = new Position(x, y, z);
    }
}

