using MinecraftConnection;

namespace mcNARUTOmode;

internal abstract class Ninja
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
                Console.WriteLine("PreventFallDamage x:" + preventBlockPosition.X + ", y:" + preventBlockPosition.Y + 1 + ", z:" + preventBlockPosition.Z);
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
            //配置場所の1つ下がairかどうか確認(普通のジャンプで作らないように)
            string _result = Setup.Command.SendCommand($"execute if block {footholdBlockPosition.X} {footholdBlockPosition.Y - 1} {footholdBlockPosition.Z} air");
            if (_result == "Test passed")
            {
                Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z + 1} minecraft:{useBlockName}");
                footholdBlockSetFlag = true;
                Console.WriteLine("SetFootholdOnJump x:" + footholdBlockPosition.X + ", y:" + footholdBlockPosition.Y + ", z:" + footholdBlockPosition.Z);
            }
        }
    }

    protected bool defensiveWallSetFlag = false;
    internal virtual void AutoDefensiveWall()
    {
        //配置後移動したら破壊
        if (defensiveWallSetFlag == true &&
            (Setup.Player.LatestPosition.X != Setup.Player.LastPosition.X ||
            Setup.Player.LatestPosition.Y != Setup.Player.LastPosition.Y ||
            Setup.Player.LatestPosition.Z != Setup.Player.LastPosition.Z))
        {
            Setup.Command.SendCommand($"fill {Setup.Player.LastPosition.X - 3} {Setup.Player.LastPosition.Y} {Setup.Player.LastPosition.Z - 3} {Setup.Player.LastPosition.X + 3} {Setup.Player.LastPosition.Y + 1} {Setup.Player.LastPosition.Z + 3} minecraft:air replace minecraft:{useBlockName}");
            defensiveWallSetFlag = false;
        }

        //xz相対位置が3以下なら3以下の座標を5にしてテレポートさせて押し返し壁を設置
        int _relativeDistance_x = (int)Setup.Player.NearestEnemyPosition.X - (int)Setup.Player.LatestPosition.X;
        int _relativeDistance_y = (int)Setup.Player.NearestEnemyPosition.Y - (int)Setup.Player.LatestPosition.Y;
        int _relativeDistance_z = (int)Setup.Player.NearestEnemyPosition.Z - (int)Setup.Player.LatestPosition.Z;
        int _relativeOffsetDistance_x = 0;
        int _relativeOffsetDistance_z = 0;

        if (((Math.Abs(_relativeDistance_x)) < 4) && (Math.Abs(_relativeDistance_y) < 4 && (Math.Abs(_relativeDistance_z) < 4)))
        {
            //player座標からオフセットさせる距離を定義
            if (Math.Abs(_relativeOffsetDistance_x) > Math.Abs(_relativeOffsetDistance_z))
            {
                _relativeOffsetDistance_x = (_relativeDistance_x < 0) ? -5 : 5;
                _relativeOffsetDistance_z = _relativeDistance_z;
            }
            else
            {
                _relativeOffsetDistance_z = (_relativeDistance_z < 0) ? -5 : 5;
                _relativeOffsetDistance_x = _relativeDistance_x;
            }
            //敵をテレポートさせて押し返す
            int _x = (int)Setup.Player.LatestPosition.X - 3;
            int _y = (int)Setup.Player.LatestPosition.Y - 3;
            int _z = (int)Setup.Player.LatestPosition.Z - 3;
            Setup.Command.SendCommand($"execute as @e[x={_x},dx=6,y={_y},dy=6,z={_z},dz=6,type=!player,type=!item,sort=nearest,limit=1] run tp @s {(int)Setup.Player.LatestPosition.X + _relativeOffsetDistance_x} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.LatestPosition.Z + _relativeOffsetDistance_z}");
            var (_yaw, _pitch) = Utility.GetRotationFromRelative(_relativeDistance_x, _relativeDistance_y, _relativeDistance_z);
            //敵の方を向く
            //Setup.Command.SendCommand($"teleport @p {(int)Setup.Player.LatestPosition.X} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z} {_yaw} {_pitch}");
            //防御壁配置
            if ((_yaw < 22.5 && _yaw >= 0) || (_yaw > -22.5 && _yaw <= 0))
            {
                //南
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 3} {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 3} minecraft:{useBlockName}");
            }
            else if (_yaw >= 22.5 && _yaw < 67.5)
            {
                //南西
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 3} {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 3} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 2} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 2} {(int)Setup.Player.LatestPosition.X - 2} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 2} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 1} {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 1} minecraft:{useBlockName}");
            }
            else if (_yaw >= 67.5 && _yaw < 112.5)
            {
                //西
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 1} {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 1} minecraft:{useBlockName}");
            }
            else if (_yaw >= 112.5 && _yaw < 157.5)
            {
                //北西
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 1} {(int)Setup.Player.LatestPosition.X - 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 1} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 2} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 2} {(int)Setup.Player.LatestPosition.X - 2} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 2} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 3} {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 3} minecraft:{useBlockName}");
            }
            else if ((_yaw >= 157.5 && _yaw <= 180) || (_yaw >= -180 && _yaw < -157.5))
            {
                //北
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X - 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 3} {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 3} minecraft:{useBlockName}");
            }
            else if (_yaw >= -157.5 && _yaw < -112.5)
            {
                //北東
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 3} {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 3} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 2} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 2} {(int)Setup.Player.LatestPosition.X + 2} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 2} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 1} {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z - 1} minecraft:{useBlockName}");
            }
            else if (_yaw >= -112.5 && _yaw < -67.5)
            {
                //東
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z - 1} {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 1} minecraft:{useBlockName}");
            }
            else
            {
                //南東
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 1} {(int)Setup.Player.LatestPosition.X + 3} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 1} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 2} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 2} {(int)Setup.Player.LatestPosition.X + 2} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 2} minecraft:{useBlockName}");
                Setup.Command.SendCommand($"fill {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z + 3} {(int)Setup.Player.LatestPosition.X + 1} {(int)Setup.Player.LatestPosition.Y + 1} {(int)Setup.Player.LatestPosition.Z + 3} minecraft:{useBlockName}");
            }
            defensiveWallSetFlag = true;
            Console.WriteLine("AutoDefensiveWall");
        }
    }
    internal abstract void Attack();
    //抽象メソッド。子クラスで実装

}

internal class SandNinja : Ninja
{
    internal SandNinja()
    {
        useBlockName = "sand";
        //useBlockName = "glass";
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
                Console.WriteLine("PreventFallDamage x:" + preventBlockPosition.X + ", y:" + preventBlockPosition.Y + 1 + ", z:" + preventBlockPosition.Z);
            }
        }
        else if (Setup.Player.FallDistance == 0 && preventBlockSetFlag == true)
        {
            Setup.Command.SendCommand($"fill {preventBlockPosition.X - 1} {preventBlockPosition.Y} {preventBlockPosition.Z - 1} {preventBlockPosition.X + 1} {preventBlockPosition.Y + 1} {preventBlockPosition.Z + 1} minecraft:air[] destroy");
            preventBlockSetFlag = false;
        }
    }

    internal override void SetFootholdOnJump()
    {
        if ((Setup.Player.LatestPosition.X <= footholdBlockPosition.X - 2
            || Setup.Player.LatestPosition.X >= footholdBlockPosition.X + 2
            || Setup.Player.LatestPosition.Z <= footholdBlockPosition.Z - 2
            || Setup.Player.LatestPosition.Z >= footholdBlockPosition.Z + 2
            || Setup.Player.LatestPosition.Y != footholdBlockPosition.Y + 1)
            && footholdBlockSetFlag == true)
        {
            Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y - 1} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z + 1} minecraft:air[] destroy");
            footholdBlockSetFlag = false;
        }

        if (Setup.Player.LatestPosition.Y > Setup.Player.LastPosition.Y && footholdBlockSetFlag == false)
        {
            // 更新したら前のインスタンスはGCで開放される
            footholdBlockPosition = new Position(Setup.Player.LatestPosition.X, Setup.Player.LatestPosition.Y - 1, Setup.Player.LatestPosition.Z);
            //配置場所の1つ下がairかどうか確認
            string _result = Setup.Command.SendCommand($"execute if block {footholdBlockPosition.X} {footholdBlockPosition.Y - 1} {footholdBlockPosition.Z} air");
            if (_result == "Test passed")
            {
                Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y - 1} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y - 1} {footholdBlockPosition.Z + 1} minecraft:barrier");
                Setup.Command.SendCommand($"fill {footholdBlockPosition.X - 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z - 1} {footholdBlockPosition.X + 1} {footholdBlockPosition.Y} {footholdBlockPosition.Z + 1} minecraft:{useBlockName}");
                footholdBlockSetFlag = true;
                Console.WriteLine("SetFootholdOnJump x:" + footholdBlockPosition.X + ", y:" + footholdBlockPosition.Y + ", z:" + footholdBlockPosition.Z);
            }
        }
    }

    internal override void Attack()
    {
        //アイテムを使っていなければ何もしない
        if (Setup.Player.SelectedItemIsUsed == false)
        {
            return;
        }
        //相対位置が6を超えていたら何もしない
        int _relativeDistance_x = (int)Setup.Player.NearestEnemyPosition.X - (int)Setup.Player.LatestPosition.X;
        int _relativeDistance_y = (int)Setup.Player.NearestEnemyPosition.Y - (int)Setup.Player.LatestPosition.Y;
        int _relativeDistance_z = (int)Setup.Player.NearestEnemyPosition.Z - (int)Setup.Player.LatestPosition.Z;
        if (((Math.Abs(_relativeDistance_x)) > 6) || (Math.Abs(_relativeDistance_y) > 6 || (Math.Abs(_relativeDistance_z) > 6)))
        {
            return;
        }

        Console.WriteLine("Attack(sand) x:" + (int)Setup.Player.NearestEnemyPosition.X + ", y:" + (int)Setup.Player.NearestEnemyPosition.Y + ", z:" + (int)Setup.Player.NearestEnemyPosition.Z);

        //技名表示
        string title = "{\"text\":\"砂瀑送葬\",\"color\":\"dark_red\"}";
        Setup.Command.SendCommand($"title @a title {title}");
        int _x = (int)Setup.Player.LatestPosition.X - 6;
        int _y = (int)Setup.Player.LatestPosition.Y - 6;
        int _z = (int)Setup.Player.LatestPosition.Z - 6;
        //敵を1ブロック上にテレポート
        Setup.Command.SendCommand($"execute as @e[x={_x},dx=12,y={_y},dy=12,z={_z},dz=12,type=!player,type=!item,sort=nearest,limit=1] run tp @s {(int)Setup.Player.NearestEnemyPosition.X} {(int)Setup.Player.NearestEnemyPosition.Y + 1} {(int)Setup.Player.NearestEnemyPosition.Z}");
        //敵の足下に3x3の足場設置
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 1} {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z + 1} minecraft:{useBlockName}");
        //砂で囲う(1フレーム目)
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 2} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 1} {(int)Setup.Player.NearestEnemyPosition.X - 2} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z - 1} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 2} {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z - 2} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 2} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z + 1} {(int)Setup.Player.NearestEnemyPosition.X - 2} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z + 1} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z + 2} {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z + 2} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 2} {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z - 2} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X + 2} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 1} {(int)Setup.Player.NearestEnemyPosition.X + 2} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z - 1} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z + 2} {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z + 2} minecraft:{useBlockName}");
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X + 2} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z + 1} {(int)Setup.Player.NearestEnemyPosition.X + 2} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z + 1} minecraft:{useBlockName}");
        Setup.Command.Wait(400);
        //砂で囲う(2フレーム目)
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 1} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 1} {(int)Setup.Player.NearestEnemyPosition.X + 1} {(int)Setup.Player.NearestEnemyPosition.Y + 4} {(int)Setup.Player.NearestEnemyPosition.Z + 1} minecraft:{useBlockName}");
        //敵をkill
        Setup.Command.SendCommand($"kill @e[x={_x},dx=12,y={_y},dy=12,z={_z},dz=12,type=!player,type=!item,sort=nearest,limit=1]");
        Setup.Command.SendCommand($"playsound minecraft:block.sweet_berry_bush.break master @a {(int)Setup.Player.LatestPosition.X} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z} 10.0");
        Setup.Command.SendCommand($"playsound minecraft:block.shroomlight.break master @a {(int)Setup.Player.LatestPosition.X} {(int)Setup.Player.LatestPosition.Y} {(int)Setup.Player.LatestPosition.Z} 10.0");
        Setup.Command.Wait(500);
        //生成したブロックを削除
        Setup.Command.SendCommand($"fill {(int)Setup.Player.NearestEnemyPosition.X - 2} {(int)Setup.Player.NearestEnemyPosition.Y} {(int)Setup.Player.NearestEnemyPosition.Z - 2} {(int)Setup.Player.NearestEnemyPosition.X + 2} {(int)Setup.Player.NearestEnemyPosition.Y + 4} {(int)Setup.Player.NearestEnemyPosition.Z + 2} minecraft:air[] destroy");
        //血のエフェクト生成
        Setup.Command.Wait(300);
        Setup.Command.SendCommand($"particle minecraft:falling_lava {(int)Setup.Player.NearestEnemyPosition.X} {(int)Setup.Player.NearestEnemyPosition.Y + 2} {(int)Setup.Player.NearestEnemyPosition.Z} 1.2 1.2 1.2 0.0 3000 force");
    }
}
