using MinecraftConnection;
using System.Text.RegularExpressions;

namespace mcNARUTOmode;

// プレイヤーの情報を集めて保持しておくクラス
internal class PlayerStatus
{
    // プレイヤー変数
    // プレイヤーの最新座標
    internal Position LatestPosition = new(0, 0, 0);
    // プレイヤーが前回実行時にいた座標
    internal Position LastPosition = new(0, 0, 0);
    // プレイヤーが落下した距離
    internal double FallDistance { get; set; } = 0.0;
    // 手に持っているアイテムの名前
    internal string SelectedItemName { get; set; } = "minecraft:sand";
    // 手に持っているアイテムを使ったかどうか
    internal bool SelectedItemIsUsed { get; set; } = false;

    // 敵変数
    // プレイヤーに最も近い敵の座標
    internal Position NearestEnemyPosition = new(0, 0, 0);

    // コンストラクタ
    internal PlayerStatus()
    {
        Console.WriteLine("PlayerStatusInit!");
        // アイテムを使ったかどうかの検知用にスコアボードを作成(砂ブロック)
        Setup.Command.SendCommand("scoreboard objectives add sandRightClicked minecraft.used:minecraft.sand \"sand right clicked\"");
        Setup.Command.SendCommand("scoreboard players set @a sandRightClicked 0");
    }

    // ステータスの更新関数(座標等のステータスを取得)
    internal void UpdateStatus()
    {
        updatePlayerPosition();
        updatePlayerFallDistance();
        updatePlayerSelectedItem();
        updatePlayerSelectedItemUsed();
        updateNearestEnemyPosition();
    }

    // プレイヤーの座標を取得する関数
    private void updatePlayerPosition()
    {
        // 前回値の保持
        LastPosition = LatestPosition;
        try
        {
            // ここで稀に例外が発生(型が一致しない)
            Position _pos = Setup.Command.GetPlayerData(Setup.PlayerName).Position;
            // 整数座標に変換
            double _x = Math.Floor(_pos.X);
            double _y = Math.Floor(_pos.Y);
            double _z = Math.Floor(_pos.Z);
            LatestPosition = new Position(_x, _y, _z);
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerPosition()Error: " + e.Message);
        }
    }

    // プレイヤーの落下距離を取得する関数
    private void updatePlayerFallDistance()
    {
        try
        {
            // ここで稀に例外が発生(型が一致しない)
            FallDistance = Setup.Command.GetPlayerData(Setup.PlayerName).FallDistance;
        }
        catch (Exception e)
        {
            Console.WriteLine("UpdatePlayerFallDistance()Error: " + e.Message);
        }
    }

    // 手に持っているアイテム名を取得する関数
    private void updatePlayerSelectedItem()
    {
        var _result = Setup.Command.SendCommand($"data get entity {Setup.PlayerName} SelectedItem");
        // 正規表現で""に囲まれた文字列を抽出
        var _pickItem = new Regex("\"(.+?)\"");
        var _match = _pickItem.Match(_result);
        SelectedItemName = _match.Groups[1].Value;
    }

    // 手に持っているアイテムを使用したかどうかを検出する関数
    private void updatePlayerSelectedItemUsed()
    {
        string _result = "";
        switch (SelectedItemName)
        {
            case "minecraft:sand":
                // スコアボードを読み取るコマンドを実行
                // レスポンス例：
                // 2dice_K has 0 [sand right clicked]
                _result = Setup.Command.SendCommand("scoreboard players get @p sandRightClicked");
                break;
            default:
                break;
        }
        // 正規表現でスペースに囲まれた1桁の数字を抽出
        Regex _regex = new Regex(@"\s(\d)\s");
        Match _match = _regex.Match(_result);
        // スコアボードの使用回数が1なら値を検出してゼロクリア
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

    // プレイヤーに最も近い敵の座標を取得する関数
    protected bool useRangeValueFlag = false;
    private void updateNearestEnemyPosition()
    {
        // 自分の座標に対して検索範囲を指定
        // 2種類の範囲をローテーションでサーチ(一度補足した対象が範囲外に出ないと最も近い敵が更新されなかったため)
        int _x, _y, _z, _dx, _dy, _dz;
        if (useRangeValueFlag)
        {
            // プレイヤー座標に対し-6から+6(-6+12)の範囲をサーチ
            _x = (int)Setup.Player.LatestPosition.X - 6;
            _y = (int)Setup.Player.LatestPosition.Y - 6;
            _z = (int)Setup.Player.LatestPosition.Z - 6;
            _dx = _dy = _dz = 12;
            useRangeValueFlag = false;
        }
        else
        {
            // プレイヤー座標に対し-3から+3(-3+6)の範囲をサーチ
            _x = (int)Setup.Player.LatestPosition.X - 3;
            _y = (int)Setup.Player.LatestPosition.Y - 3;
            _z = (int)Setup.Player.LatestPosition.Z - 3;
            _dx = _dy = _dz = 6;
            useRangeValueFlag = true;

        }
        // 最も近い敵のデータを取得するコマンドを実行
        // レスポンス例：
        // Spider has the following entity data: [-11.455525245656695d, 119.0d, 6.752002335797372d]
        string _result = Setup.Command.SendCommand($"execute as @e[x={_x},dx={_dx},y={_y},dy={_dy},z={_z},dz={_dz},type=!player,type=!item,sort=nearest,limit=1] run data get entity @s Pos");
        // 範囲内に敵がいた場合
        if (_result != "")
        {
            // 正規表現で数値を抜き出す([]内の文字列を抽出)
            var _regex = new Regex(@"\[(.+?)\]");
            Match _match = _regex.Match(_result);
            string[] _parts = _match.Groups[1].Value.Split(',');
            double _enemy_pos_x = Math.Floor(Double.Parse(_parts[0].TrimEnd('d')));
            double _enemy_pos_y = Math.Floor(Double.Parse(_parts[1].TrimEnd('d')));
            double _enemy_pos_z = Math.Floor(Double.Parse(_parts[2].TrimEnd('d')));
            NearestEnemyPosition = new Position(_enemy_pos_x, _enemy_pos_y, _enemy_pos_z);
        }
        // いなかった場合、かつ検索範囲が広い場合(狭い場合で見つからなかった場合は意図的に値を更新せず前回値のままにする)
        else if (useRangeValueFlag == false)
        {
            // 仮の値(十分遠く)を設定
            NearestEnemyPosition = new Position(Setup.Player.LatestPosition.X + 100, Setup.Player.LatestPosition.Y + 100, Setup.Player.LatestPosition.Z + 100);
        }
    }
}
