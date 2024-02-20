# MewCore
Core Game Libraries for Unity

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewCore/blob/main/LICENSE)

## ドキュメント

https://mewlist.github.io/MewCore/

## 機能

* TaskQueue: 非同期関数を直列実行するためのライブラリ
* TaskInterval: 非同期関数を一定間隔で実行するためのライブラリ

## インストール

UPM 経由でインストールすることができます。
以下の git url を指定してください。
```
git@github.com:mewlist/MewCore.git
```

## TaskQueue

TaskQueue は Unity 開発における**非同期関数の直列処理**を行うためのライブラリです。
キューに入力した順で非同期関数が実行されます。
また、優先度付きキューの機能も備えており、重要なタスクを優先的に実行することができます。

### 主な特徴

* ***動的な関数追加***: ランタイムで非同期関数をタスクキューに追加することができます。これにより、変化する要件や状況に柔軟に対応可能です。
* ***優先度に基づく実行管理***: 各非同期関数に優先度を設定し、重要なタスクを優先的に処理します。これにより、重要な処理の遅延を防ぎます。
* ***直列処理と安全性***: 複数の非同期関数を順序立てて実行し、一つの関数が完了するまで次の関数の実行を待機します。これにより、UI 更新やゲームシーケンスの安全性が向上します。
* ***キューの最大サイズ***: キューに入力できるタスクの最大数を設定できます。これにより、キューにタスクが溜まりすぎることを防ぐことができます。
* ***シンプルな記述***: シンプルに記述できるように設計されています。

### 使用シナリオ

* ***UIの動的更新***: ゲーム内でのダイアログボックスやメニューの動的な表示・非表示をスムーズに制御する際に使用します。
* ***ゲームイベントのシーケンシング***: 物語進行やチュートリアルなど、順序立てられたイベントの管理に適しています。
* ***TaskQueue*** を使用することで、Unity 開発における非同期処理の複雑さを軽減し、より効果的かつ効率的なコード記述を可能にします。
* ***コマンドパターンへの適応***: 非同期処理を含めたコマンドパターンの実装に適しています。
* ***UI*** イベントのハンドリング: クリックなど UI の非同期イベントに対して並列実行を防ぐために使用します。

### サンプルコード

```csharp
class Sample : Monobehaviour
{
    TaskQueue taskQueue = new();

    void Start()
    {
        // destroyCancellationToken を渡すことで
        // MonoBehaviour が破棄されたタイミングで自動的に処理を停止し Dispose されます。
        taskQueue.DisposeWith(destroyCancellationToken);

        // TaskQueue に非同期関数を追加します。
        taskQueue.Enqueue(async cancellationToken =>
        {
            Debug.Log("Hello");
            await Task.Delay(1000, cancellationToken);
        });
        taskQueue.Enqueue(async cancellationToken =>
        {
            await Task.Delay(1000, cancellationToken);
            Debug.Log("Bye");
        });
    }
}
```

#### 実行結果

```
Hello
// 二秒後
Bye
```

### 優先度付きタスクの実行

Enqueue の第二引数に優先度を指定することで、優先度付きタスクを実行することができます。
```priotiry```値が小さい処理が優先されます。既定値は 0 です。

```csharp
taskQueue.Enqueue(async ct => { ... }, priority: 1);
taskQueue.Enqueue(async ct => { ... }, priority: 0); // このタスクが優先して処理される
```

### キュー最大サイズの設定

#### TaskQueueLimitType.Discard

最大サイズ 2 のキューに対して以下のようにタスクを追加し、最大数を超えた場合、最後に追加されたタスクが破棄されます。
追加されるタスクの優先度が高い場合は、より低い優先度のタスクが破棄されキューイングが行われます。

```csharp
taskQueue = new TaskQueue(TaskQueueLimitType.Discard, maxSize: 2);
taskQueue.Enqueue(async ct => { ... });
taskQueue.Enqueue(async ct => { ... });
taskQueue.Enqueue(async ct => { ... });　// このタスクが破棄される
```

#### TaskQueueLimitType.SwapLast

最大サイズ 2 のキューに対して以下のようにタスクを追加し、最大数を超えた場合、最後のタスクを入れ替えます。
追加されるタスクより優先度が高いタスクでキューが構成される場合は、入れ替えは行われません。

```csharp
taskQueue = new TaskQueue(TaskQueueLimitType.SwapLast, maxSize: 2);
taskQueue.Enqueue(async ct => { ... });
taskQueue.Enqueue(async ct => { ... });　// このタスクが破棄される
taskQueue.Enqueue(async ct => { ... });　
```

## TaskInterval

TaskInterval は、Unity 開発で一定間隔による特定の処理の実行を容易にするライブラリです。このライブラリは、非同期関数を定期的に実行し、複数の非同期処理が並行して実行されることを防ぎます。

### 主な特徴

- **非同期関数の定期実行**: 指定された間隔ごとに非同期関数を自動的に実行します。これにより、非同期処理を含むタスクの管理が容易になります。

- **処理時間への柔軟な対応**: 非同期関数の実行に時間がかかり、指定した間隔を超えた場合でも、処理をスキップするか、遅れても実行を継続することができます。これは一般的な定期実行処理とは異なり、よりリアルタイムの動作環境に適応します。

- **並行実行の防止**: 一度に一つの非同期処理のみが実行され、複数の処理が同時に実行されることはありません。これにより、タスクの実行が予測可能で安全になります。

- **安定した間隔での実行**: 同期関数を使用すれば、安定した間隔で関数を実行することも可能です。

### 使用シナリオ

- **ゲーム内の定期的な更新**: ゲームの状態やオブジェクトの状態を一定間隔で更新する際に利用します。

- **バックグラウンド処理**: ネットワーク通信やデータの読み込みなど、バックグラウンドでの定期的な処理に適しています。

- **UIの定期的な更新**: ユーザーインターフェースの要素を定期的に更新する際にも利用できます。

TaskInterval を使用することで、Unity 開発における定期的な処理の実装がより柔軟かつ効率的になり、並行実行による問題を防ぎます。

### サンプルコード
    
```csharp
public class Sample : MonoBahaviour
{
    private void Awake()
    {
        // 一秒ごとに TestTaskAsync を実行する TaskInterval を生成します。
        // destroyCancellationToken を渡すことで
        // MonoBehaviour が破棄されたタイミングで自動的に処理を停止し Dispose されます。
        TaskInterval
            .Create(TimeSpan.FromSeconds(1), TestTaskAsync)
            .Start(destroyCancellationToken);
    }

    private float time;
    private async Task TestTaskAsync(CancellationToken ct)
    {
        var currentTime = Time.time;
        Debug.Log($"{currentTime - time}");            
        time = currentTime;
        await Task.Delay(100, ct);
    }
}
```

#### 実行結果

```
0.9996152
1.000825
1.000599
0.9999266
1.000448
0.9925194
...
```

### 使用する Timer の指定

Create の第３引数に Timer の種類を指定することで、使用する Timer を変更することができます。
Unity の Time.timeScale の影響を受けないようにしたい場合は、```IntervalTimerType.UnityTime``` 以外の値を指定するとよいでしょう。

| Timer の種類 | 説明                          |
|--------------|-----------------------------|
| `IntervalTimerType.SystemTime` | システム時間を使用します。               |
| `IntervalTimerType.UnityTime` | Unity の Time.time を使用します。(***規定値***) |
| `IntervalTimerType.UnityUnscaledTime` | Time.unscaledTime を使用します。   |

Time.timeScale の影響を受けずに処理を実行する例。

```csharp
TaskInterval
    .Create(1000 /* ms */, TestTaskAsync, IntervalTimerType.UnityUnscaledTime)
    .Start(destroyCancellationToken);
```
