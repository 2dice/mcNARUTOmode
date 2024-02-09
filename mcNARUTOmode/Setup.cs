using MinecraftConnection;

namespace mcNARUTOmode;

// 全体で使用するインスタンスを定義するクラス
internal class Setup
{
    // Minecraft Setup
    // server.propertiesで設定したpasswordとportをここで設定する
    static readonly string _address = "127.0.0.1";
    static readonly ushort _port = 25575;
    static readonly string _pass = "minecraft";
    internal static readonly MinecraftCommands Command = new(_address, _port, _pass);

    // Player Setup
    // PlayerNameをマイクラで設定した自分のプレイヤー名に変更すること
    internal static readonly string PlayerName = "2dice_K";
    internal static readonly PlayerStatus Player = new();

    // Ninja Setup
    // 使用するNinjaクラスのインスタンスを作成
    internal static readonly SandNinja SandNinja = new();
}
