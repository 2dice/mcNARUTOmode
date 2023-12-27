using System;
using MinecraftConnection;

internal class Program
{
    static string address = "127.0.0.1";
    static ushort port = 25575;
    static string pass = "minecraft";
    static MinecraftCommands command = new MinecraftCommands(address, port, pass);
    private static void Main(string[] args)
    {
        // TODO:インスタンスの作成

        // メインループ処理
        while (true)
        {
            command.Wait(300);
            Console.WriteLine("Hello, World!");
        }
    }
}