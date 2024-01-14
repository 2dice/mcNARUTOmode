# mcNARUTOmode

## プロジェクトの概要
マインクラフトでNARUTOに出てくるキャラクターの術を再現するプログラムを作成します。

## 実行環境
このプロジェクトはtakunologyさんの[MinecraftConnection](https://github.com/takunology/MinecraftConnection/tree/main?tab=readme-ov-file)を活用して実装しています。  
環境構築はtakunologyさんの[こちらのサイト](https://zenn.dev/takunology/books/minecraft-programming-book)を参考にしてください。  
<br>
作成時の環境は以下の通りです。  
windows10  
Minecraft Java版 1.18.2  
Java 17.0.9  
Visual Studio 2022 Version 17.8.1  
.NET 6.0  
Minecraft Connection 2.1.0  

## プログラムの構成
### Setup.cs
server.propertiesで設定したpasswordとportをここで設定します。PlayerNameを各自のプレイヤー名に変更して実行してください。作成するNinjaクラスのインスタンスもここで作成しています。
### PlayerStatus.cs
プレイヤーの情報を集めて保持しておくクラスです。周期的にメインプログラムからUpdateStatusを呼び出して各情報を更新します。  
### Program.cs
メインプログラムです。指定したループ周期でループ処理を実行し、持っているアイテムに応じて各Ninjaインスタンスの処理を実行します。  
### Utility.cs
汎用的に使う処理の実装です。  
### Ninja.cs
Ninjaクラスは各Ninjaクラスの親クラスです。共通で使う処理を実装しています。  
SandNinjaクラスは砂ブロックを持っているときに実行する処理を実装しています。親クラスの処理をオーバーライドして実装しています。  

## ライセンス
このプロジェクトはMITライセンスです。[LICENSE.txt](/LICENSE.txt)を確認してください。