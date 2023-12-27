using System;
using System.Text.RegularExpressions;
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
            Setup.player.UpdateStatus();
            Console.WriteLine(Setup.player.SelectedItemName);
        }
    }
}
internal class PlayerStatus
{
    //プレイヤー変数
    internal Position LatestPosition = new(0, 0, 0);
    internal Position LastPosition = new(0, 0, 0);
    internal Motion Motion = new(0, 0, 0);
    internal double FallDistance { get; set; } = 0.0;
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
    internal void UpdateStatus()
    {
        UpdatePlayerPosition();
        UpdatePlayerMotion();
        UpdatePlayerFallDistance();
        UpdatePlayerSelectedItem();
    }
    private void UpdatePlayerPosition()
    {
        //前回値の保持
        LastPosition = LatestPosition;

        var pos = Setup.command.GetPlayerData(Setup.PlayerName).Position;
        // 整数座標に変換
        double x = Math.Floor(pos.X);
        double y = Math.Floor(pos.Y);
        double z = Math.Floor(pos.Z);
        // 更新前のインスタンスはGCで開放される
        LatestPosition = new Position(x, y, z);
    }

    private void UpdatePlayerMotion()
    {
        var mot = Setup.command.GetPlayerData(Setup.PlayerName).Motion;
        // 更新前のインスタンスはGCで開放される
        Motion = new Motion(mot.X, mot.Y, mot.Z);
    }

    private void UpdatePlayerFallDistance()
    {
        FallDistance = Setup.command.GetPlayerData(Setup.PlayerName).FallDistance;
    }

    private void UpdatePlayerSelectedItem()
    {
        var result = Setup.command.SendCommand("data get entity " + Setup.PlayerName + " SelectedItem");
        //正規表現で""に囲まれた文字列の抽出
        var pickItem = new Regex("\"(.+?)\"");
        var match = pickItem.Match(result);
        SelectedItemName = match.Groups[1].Value;
    }
}

