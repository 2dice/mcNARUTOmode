using System;
using System.Text.RegularExpressions;
using MinecraftConnection;

internal class Setup
{
    //Minecraft Setup
    static readonly string _address = "127.0.0.1";
    static readonly ushort _port = 25575;
    static readonly string _pass = "minecraft";
    internal static readonly MinecraftCommands Command = new(_address, _port, _pass);
    //Player Setup
    internal static readonly string PlayerName = "2dice_K";
    internal static readonly PlayerStatus Player = new();
    //Ninja Setup
    internal static readonly SandNinja SandNinja = new();
}

internal class Program
{
    private static void Main(string[] args)
    {
        // メインループ処理
        while (true)
        {
            Setup.Command.Wait(30);
            Setup.Player.UpdateStatus();
            Setup.SandNinja.PreventFallDamage();
            Setup.SandNinja.SetFootholdOnJump();
            //Console.WriteLine(Setup.Player.LatestPosition.Y - Setup.Player.LastPosition.Y);
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
        Setup.Command.SendCommand("scoreboard objectives add sandRightClicked minecraft.used:minecraft.sand \"sand right clicked\"");
        Setup.Command.SendCommand("scoreboard players set @a sandRightClicked 0");
    }

    //update関数(座標等の取得
    internal void UpdateStatus()
    {
        updatePlayerPosition();
        updatePlayerFallDistance();
        updatePlayerSelectedItem();
        updatePlayerSelectedItemUsed();
        updateNearestEnemyPosition();
    }
    private void updatePlayerPosition()
    {
        //前回値の保持
        LastPosition = LatestPosition;

        try
        {
            //ここで稀に例外が発生(型が一致しない)
            Position _pos = Setup.Command.GetPlayerData(Setup.PlayerName).Position;
            // 整数座標に変換
            double _x = Math.Floor(_pos.X);
            double _y = Math.Floor(_pos.Y);
            double _z = Math.Floor(_pos.Z);
            // 更新したら前のインスタンスはGCで開放される
            LatestPosition = new Position(_x, _y, _z);
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerPosition()Error: " + e.Message);
        }
    }

    private void updatePlayerFallDistance()
    {
        try
        {
            //ここで稀に例外が発生(型が一致しない)
            FallDistance = Setup.Command.GetPlayerData(Setup.PlayerName).FallDistance;
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerFallDistance()Error: " + e.Message);
        }
    }

    private void updatePlayerSelectedItem()
    {
        var _result = Setup.Command.SendCommand($"data get entity {Setup.PlayerName} SelectedItem");
        //正規表現で""に囲まれた文字列の抽出
        var _pickItem = new Regex("\"(.+?)\"");
        var _match = _pickItem.Match(_result);
        SelectedItemName = _match.Groups[1].Value;
    }

    private void updatePlayerSelectedItemUsed()
    {
        //2dice_K has 0 [sand right clicked]
        string _result = "";
        switch (SelectedItemName)
        {
            case "minecraft:sand":
                _result = Setup.Command.SendCommand("scoreboard players get @p sandRightClicked");
                break;
            default:
                break;
        }
        //正規表現でスペースに囲まれた1桁の数字を抽出
        Regex _regex = new Regex(@"\s(\d)\s");
        Match _match = _regex.Match(_result);
        if (_match.Groups[1].Value == "1")
        {
            SelectedItemIsUsed = true;
            Setup.Command.SendCommand("scoreboard players set @a sandRightClicked 0");
        }
        else
        {
            SelectedItemIsUsed = false;
        }

    }

    private void updateNearestEnemyPosition()
    {
        //Spider has the following entity data: [-11.455525245656695d, 119.0d, 6.752002335797372d]
        string _result = Setup.Command.SendCommand("execute as @e[type=!player,type=!item,sort=nearest,limit=1] run data get entity @s Pos");
        // 正規表現で数値を抜き出す([]内の文字列を抽出)
        var _regex = new Regex(@"\[(.+?)\]");
        Match _match = _regex.Match(_result);
        string[] _parts = _match.Groups[1].Value.Split(',');
        double _enemy_pos_x = Math.Floor(Double.Parse(_parts[0].TrimEnd('d')));
        double _enemy_pos_y = Math.Floor(Double.Parse(_parts[1].TrimEnd('d')));
        double _enemy_pos_z = Math.Floor(Double.Parse(_parts[2].TrimEnd('d')));
        // 更新したら前のインスタンスはGCで開放される
        NearestEnemyPosition = new Position(_enemy_pos_x, _enemy_pos_y, _enemy_pos_z);
    }
}

internal class Ninja
{
    protected string useBlockName { get; set; } = "barrier";
    //コンストラクタ
    internal Ninja()
    {
        useBlockName = "barrier";
    }

    //設置位置保持用変数
    protected Position preventBlockPosition = new(0, 0, 0);
    protected bool preventBlockSetFlag = false;
    internal virtual void PreventFallDamage()
    {
        if (Setup.Player.FallDistance >= 2 && preventBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            preventBlockPosition = new Position(Setup.Player.LatestPosition.X, Setup.Player.LatestPosition.Y - 2, Setup.Player.LatestPosition.Z);
            //配置場所がairかどうか確認
            string _result = Setup.Command.SendCommand($"execute if block {preventBlockPosition.X} {preventBlockPosition.Y} {preventBlockPosition.Z} air");
            if (_result == "Test passed")
            {
                Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z + 1} minecraft:{useBlockName}");
                preventBlockSetFlag = true;
            }
        }
        else if (Setup.Player.FallDistance == 0 && preventBlockSetFlag == true)
        {
            Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z + 1} minecraft:air[] destroy");
            preventBlockSetFlag = false;
        }
    }

    protected Position footholdBlockPosition = new(0, 0, 0);
    protected bool footholdBlockSetFlag = false;
    internal virtual void SetFootholdOnJump()
    {
        if ((Setup.Player.LatestPosition.X <= footholdBlockPosition.X - 2
            || Setup.Player.LatestPosition.X >= footholdBlockPosition.X + 2
            || Setup.Player.LatestPosition.Z <= footholdBlockPosition.Z - 2
            || Setup.Player.LatestPosition.Z >= footholdBlockPosition.Z + 2
            || Setup.Player.LatestPosition.Y != footholdBlockPosition.Y + 1)
            && footholdBlockSetFlag == true)
        {
            Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z + 1} minecraft:air[] destroy");
            footholdBlockSetFlag = false;
        }

        if (Setup.Player.LatestPosition.Y > Setup.Player.LastPosition.Y && footholdBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            footholdBlockPosition = new Position(Setup.Player.LatestPosition.X, Setup.Player.LatestPosition.Y - 1, Setup.Player.LatestPosition.Z);
            //配置場所がairかどうか確認
            string _result = Setup.Command.SendCommand($"execute if block {footholdBlockPosition.X} {footholdBlockPosition.Y} {footholdBlockPosition.Z} air");
            if (_result == "Test passed")
            {
                Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z + 1} minecraft:{useBlockName}");
                footholdBlockSetFlag = true;
            }
        }
    }
}
internal class SandNinja : Ninja
{
    internal SandNinja()
    {
        //useBlockName = "sand";
        useBlockName = "glass";
    }
    internal override void PreventFallDamage()
    {
        if (Setup.Player.FallDistance >= 2 && preventBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            preventBlockPosition = new Position(Setup.Player.LatestPosition.X, Setup.Player.LatestPosition.Y - 2, Setup.Player.LatestPosition.Z);
            //配置場所がairかどうか確認
            string _result = Setup.Command.SendCommand($"execute if block {preventBlockPosition.X} {preventBlockPosition.Y} {preventBlockPosition.Z} air");
            if (_result == "Test passed")
            {
                Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y} {preventBlockPosition.Z + 1} minecraft:barrier");
                Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z + 1} minecraft:{useBlockName}");
                preventBlockSetFlag = true;
            }
        }
        else if (Setup.Player.FallDistance == 0 && preventBlockSetFlag == true)
        {
            Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z + 1} minecraft:air[] destroy");
            preventBlockSetFlag = false;
        }
    }
}

