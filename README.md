# AIと音声対話をするVRコンテンツ

## スクリプトの説明

### LookAtTargetOffset.cs
指定した物体に対して対象を振り向かせる

### VRMBlick.cs
BlendShapeを用いた、まばたきの実装

### HandCheck.cs
プレイヤーが大きく手を振ったかどうかの判定

### ChatGPTConnection.cs
ChatGPTAPIの設定(利用するモデルの指示、トークン上限などの指定、初期の命令など)

### Constants.cs
ChatGPTAPIとWhisperAPIで使用するAPIKeyの設定

### TextToSpeech.cs
Text-to-Speechの設定(APIkeyの設定,音声モデルの指定など)

### Textscript
一連の処理をまとめる（対話システム、表情変更、音声出力など）

