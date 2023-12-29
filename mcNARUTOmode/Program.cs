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
    //Ninja Setup
    internal static readonly SandNinja sandNinja = new();
}

internal class Program
{
    private static void Main(string[] args)
    {
        // メインループ処理
        while (true)
        {
            Setup.command.Wait(30);
            Setup.player.UpdateStatus();
            Setup.sandNinja.PreventFallDamage();
            //Console.WriteLine(Setup.player.FallDistance);
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
        //スコアボードの作成(sand)
        Setup.command.SendCommand("scoreboard objectives add sandRightClicked minecraft.used:minecraft.sand \"sand right clicked\"");
        Setup.command.SendCommand("scoreboard players set @a sandRightClicked 0");
    }

    //update関数(座標等の取得
    internal void UpdateStatus()
    {
        UpdatePlayerPosition();
        UpdatePlayerMotion();
        UpdatePlayerFallDistance();
        UpdatePlayerSelectedItem();
        UpdatePlayerSelectedItemUsed();
        UpdateNearestEnemyPosition();
    }
    private void UpdatePlayerPosition()
    {
        //前回値の保持
        LastPosition = LatestPosition;

        try
        {
            //ここで稀に例外が発生(型が一致しない)
            Position pos = Setup.command.GetPlayerData(Setup.PlayerName).Position;
            // 整数座標に変換
            double x = Math.Floor(pos.X);
            double y = Math.Floor(pos.Y);
            double z = Math.Floor(pos.Z);
            // 更新したら前のインスタンスはGCで開放される
            LatestPosition = new Position(x, y, z);
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerPosition()Error: " + e.Message);
        }
    }

    private void UpdatePlayerMotion()
    {
        //TODO:Yデータだけでよければコマンドを2つ減らせる(xyz個別にサーバーに問い合わせているので
        var mot = Setup.command.GetPlayerData(Setup.PlayerName).Motion;
        // 更新したら前のインスタンスはGCで開放される
        Motion = new Motion(mot.X, mot.Y, mot.Z);
    }

    private void UpdatePlayerFallDistance()
    {
        try
        {
            //ここで稀に例外が発生(型が一致しない)
            FallDistance = Setup.command.GetPlayerData(Setup.PlayerName).FallDistance;
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerFallDistance()Error: " + e.Message);
        }
    }

    private void UpdatePlayerSelectedItem()
    {
        var result = Setup.command.SendCommand("data get entity " + Setup.PlayerName + " SelectedItem");
        //正規表現で""に囲まれた文字列の抽出
        var pickItem = new Regex("\"(.+?)\"");
        var match = pickItem.Match(result);
        SelectedItemName = match.Groups[1].Value;
    }

    private void UpdatePlayerSelectedItemUsed()
    {
        //2dice_K has 0 [sand right clicked]
        string result = "";
        switch (SelectedItemName)
        {
            case "minecraft:sand":
                result = Setup.command.SendCommand("scoreboard players get @p sandRightClicked");
                break;
            default:
                break;
        }
        //正規表現でスペースに囲まれた1桁の数字を抽出
        Regex regex = new Regex(@"\s(\d)\s");
        Match match = regex.Match(result);
        if (match.Groups[1].Value == "1")
        {
            SelectedItemIsUsed = true;
            Setup.command.SendCommand("scoreboard players set @a sandRightClicked 0");
        }
        else
        {
            SelectedItemIsUsed = false;
        }

    }

    private void UpdateNearestEnemyPosition()
    {
        //Spider has the following entity data: [-11.455525245656695d, 119.0d, 6.752002335797372d]
        string result = Setup.command.SendCommand("execute as @e[type=!player,type=!item,sort=nearest,limit=1] run data get entity @s Pos");
        // 正規表現で数値を抜き出す([]内の文字列を抽出)
        var regex = new Regex(@"\[(.+?)\]");
        Match match = regex.Match(result);
        string[] parts = match.Groups[1].Value.Split(',');
        double enemy_pos_x = Math.Floor(Double.Parse(parts[0].TrimEnd('d')));
        double enemy_pos_y = Math.Floor(Double.Parse(parts[1].TrimEnd('d')));
        double enemy_pos_z = Math.Floor(Double.Parse(parts[2].TrimEnd('d')));
        // 更新したら前のインスタンスはGCで開放される
        NearestEnemyPosition = new Position(enemy_pos_x, enemy_pos_y, enemy_pos_z);
    }
}

internal class Ninja
{
    protected string UseBlockName { get; set; } = "barrier";
    //コンストラクタ
    internal Ninja()
    {
        UseBlockName = "barrier";
    }

    //設置位置保持用変数
    protected Position PreventBlockPosition = new(0, 0, 0);
    protected bool PreventBlockSetFlag = false;
    internal virtual void PreventFallDamage()
    {
        if (Setup.player.FallDistance >= 2 && PreventBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            PreventBlockPosition = new Position(Setup.player.LatestPosition.X, Setup.player.LatestPosition.Y - 2, Setup.player.LatestPosition.Z);
            //配置場所がairかどうか確認
            string result = Setup.command.SendCommand($"execute if block {PreventBlockPosition.X} {PreventBlockPosition.Y} {PreventBlockPosition.Z} air");
            if (result == "Test passed")
            {
                Setup.command.SendCommand($"fill {PreventBlockPosition.X - 1} {PreventBlockPosition.Y} {PreventBlockPosition.Z - 1} {PreventBlockPosition.X + 1} {PreventBlockPosition.Y + 1} {PreventBlockPosition.Z + 1} minecraft:{UseBlockName}");
                PreventBlockSetFlag = true;
            }
        }
        else if (Setup.player.FallDistance == 0 && PreventBlockSetFlag == true)
        {
            Setup.command.SendCommand($"fill {PreventBlockPosition.X - 1} {PreventBlockPosition.Y} {PreventBlockPosition.Z - 1} {PreventBlockPosition.X + 1} {PreventBlockPosition.Y + 1} {PreventBlockPosition.Z + 1} minecraft:air[] destroy");
            PreventBlockSetFlag = false;
        }
    }
}
internal class SandNinja : Ninja
{
    internal SandNinja()
    {
        UseBlockName = "sand";
    }
    internal override void PreventFallDamage()
    {
        if (Setup.player.FallDistance >= 2 && PreventBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            PreventBlockPosition = new Position(Setup.player.LatestPosition.X, Setup.player.LatestPosition.Y - 2, Setup.player.LatestPosition.Z);
            //配置場所がairかどうか確認
            string result = Setup.command.SendCommand($"execute if block {PreventBlockPosition.X} {PreventBlockPosition.Y} {PreventBlockPosition.Z} air");
            if (result == "Test passed")
            {
                Setup.command.SendCommand($"fill {PreventBlockPosition.X - 1} {PreventBlockPosition.Y} {PreventBlockPosition.Z - 1} {PreventBlockPosition.X + 1} {PreventBlockPosition.Y} {PreventBlockPosition.Z + 1} minecraft:barrier");
                Setup.command.SendCommand($"fill {PreventBlockPosition.X - 1} {PreventBlockPosition.Y + 1} {PreventBlockPosition.Z - 1} {PreventBlockPosition.X + 1} {PreventBlockPosition.Y + 1} {PreventBlockPosition.Z + 1} minecraft:{UseBlockName}");
                PreventBlockSetFlag = true;
            }
        }
        else if (Setup.player.FallDistance == 0 && PreventBlockSetFlag == true)
        {
            Setup.command.SendCommand($"fill {PreventBlockPosition.X - 1} {PreventBlockPosition.Y} {PreventBlockPosition.Z - 1} {PreventBlockPosition.X + 1} {PreventBlockPosition.Y + 1} {PreventBlockPosition.Z + 1} minecraft:air[] destroy");
            PreventBlockSetFlag = false;
        }
    }
}

