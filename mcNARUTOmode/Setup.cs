using MinecraftConnection;

namespace mcNARUTOmode;

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
