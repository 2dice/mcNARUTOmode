using MinecraftConnection;
using System.Text.RegularExpressions;

namespace mcNARUTOmode;

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

    protected bool useRangeValueFlag = false;
    private void updateNearestEnemyPosition()
    {
        //2種類の範囲をローテーションでサーチ(一度補足した対象が範囲外に出ないと更新されないため)
        int _x, _y, _z, _dx, _dy, _dz;
        if (useRangeValueFlag)
        {
            _x = (int)Setup.Player.LatestPosition.X - 6;
            _y = (int)Setup.Player.LatestPosition.Y - 6;
            _z = (int)Setup.Player.LatestPosition.Z - 6;
            _dx = _dy = _dz = 12;
            useRangeValueFlag = false;
        }
        else
        {
            _x = (int)Setup.Player.LatestPosition.X - 3;
            _y = (int)Setup.Player.LatestPosition.Y - 3;
            _z = (int)Setup.Player.LatestPosition.Z - 3;
            _dx = _dy = _dz = 6;
            useRangeValueFlag = true;

        }
        //Spider has the following entity data: [-11.455525245656695d, 119.0d, 6.752002335797372d]
        string _result = Setup.Command.SendCommand($"execute as @e[x={_x},dx={_dx},y={_y},dy={_dy},z={_z},dz={_dz},type=!player,type=!item,sort=nearest,limit=1] run data get entity @s Pos");
        if (_result != "")
        {
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
        else if (useRangeValueFlag == false)
        {
            //広範囲サーチで見つからなかった場合に仮の値を設定(狭範囲の場合は更新しない)
            NearestEnemyPosition = new Position(Setup.Player.LatestPosition.X + 100, Setup.Player.LatestPosition.Y + 100, Setup.Player.LatestPosition.Z + 100);
        }
    }
}
