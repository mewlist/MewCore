# MewCore
Core Game Libraries for Unity

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewCore/blob/main/LICENSE)

## インストール

UPM 経由でインストールすることができます。
以下の git url を指定してください。
```
git@github.com:mewlist/MewCore.git
```

## TaskQueue

TaskQueue は、Unity 開発における非同期処理を簡素化し、効率的に扱うためのライブラリです。このライブラリは、動的に変化する非同期関数の管理と、優先度に基づいた実行順序の決定を可能にします。

### 主な特徴

動的な関数追加: ランタイムで非同期関数をタスクキューに追加することができます。これにより、変化する要件や状況に柔軟に対応可能です。

優先度に基づく実行管理: 各非同期関数に優先度を設定し、重要なタスクを優先的に処理します。これにより、重要な処理の遅延を防ぎます。

直列処理と安全性: 複数の非同期関数を順序立てて実行し、一つの関数が完了するまで次の関数の実行を待機します。これにより、UI 更新やゲームシーケンスの安全性が向上します。

シンプルな記述: TaskQueue は、非同期関数の実行を簡単に記述できるように設計されています。

### 使用シナリオ

UIの動的更新: ゲーム内でのダイアログボックスやメニューの動的な表示・非表示をスムーズに制御する際に使用します。

ゲームイベントのシーケンシング: 物語進行やチュートリアルなど、順序立てられたイベントの管理に適しています。

TaskQueue を使用することで、Unity 開発における非同期処理の複雑さを軽減し、より効果的かつ効率的なコード記述を可能にします。

コマンドパターンへの適応: 非同期処理を含めたコマンドパターンの実装に適しています。

UI イベントのハンドリング: クリックなど UI の非同期イベントに対して並列実行を防ぐために使用します。

### サンプルコード

```csharp
class Sample : Monobehaviour
{
    void Start()
    {
        // TaskQueue のインスタンスを生成します。
        var taskQueue = new TaskQueue();
        // TaskQueue の実行を開始します。
        // destroyCancellationToken を渡すことで
        // MonoBehaviour が破棄されたタイミングで自動的に処理を停止し Dispose されます。
        taskQueue.Start(destroyCancellationToken);

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
数値が高ければ高いほど処理が優先されます。既定値は 0 です。

```csharp
taskQueue.Enqueue(async ct => { ... }, priority: 1);
```

### PlayerLoop の指定

Queue を処理する PlayerLoop のタイミングを指定することができます。
以下のタイミング型が定義されています。

| タイミング                   | 説明 |
|-------------------------|------|
| `MewUnityEarlyUpdate`   | Unityのフレーム更新の最初の段階で呼ばれます。この時点で、最初のイベント処理や入力の更新が行われます。 |
| `MewUnityFixedUpdate`   | 物理演算の更新が行われるタイミングです。Unityエンジンにおける固定フレームレートでの処理に対応します。 |
| `MewUnityPreUpdate`     | `Update` メソッドの前に実行される処理です。シーンの状態更新やアニメーションの更新などが含まれます。 |
| `MewUnityUpdate`        | 主にゲームロジックの更新に使用される、通常の `Update` メソッドのタイミングです。 |
| `MewUnityPreLateUpdate` | `LateUpdate` の前に実行される処理です。一部のカメラやアニメーションの後処理が行われる可能性があります。 |
| `MewUnityPostLateUpdate` | フレームの最後に実行される処理で、レンダリングの前準備やカメラの最終更新が含まれます。 |

PlayerLoop のタイミングを指定するには、コンストラクタにタイミング型を指定します。
例えば MewUnityFixedUpdate を指定ることで、フレームスキップが発生した場合にキューの処理が遅延することを防くことができます。

```csharp
var fixedUpdateTaskQueue = new TaskQueue<MewUnityFixedUpdate>();
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

| Timer の種類 | 説明                        |
|--------------|---------------------------|
| `IntervalTimerType.SystemTime` | システム時間を使用します。             |
| `IntervalTimerType.UnityTime` | Unity の Time.time を使用します。 |
| `IntervalTimerType.UnityUnscaledTime` | Time.unscaledTime を使用します。|

Time.timeScale の影響を受けずに処理を実行する例。

```csharp
TaskInterval
    .Create(1000 /* ms */, TestTaskAsync, IntervalTimerType.UnityUnscaledTime)
    .Start(destroyCancellationToken);
```

### PlayerLoop の指定

タスクを処理する PlayerLoop のタイミングを指定することができます。
以下のタイミング型が定義されています。

| タイミング                   | 説明 |
|-------------------------|------|
| `MewUnityEarlyUpdate`   | Unityのフレーム更新の最初の段階で呼ばれます。この時点で、最初のイベント処理や入力の更新が行われます。 |
| `MewUnityFixedUpdate`   | 物理演算の更新が行われるタイミングです。Unityエンジンにおける固定フレームレートでの処理に対応します。 |
| `MewUnityPreUpdate`     | `Update` メソッドの前に実行される処理です。シーンの状態更新やアニメーションの更新などが含まれます。 |
| `MewUnityUpdate`        | 主にゲームロジックの更新に使用される、通常の `Update` メソッドのタイミングです。 |
| `MewUnityPreLateUpdate` | `LateUpdate` の前に実行される処理です。一部のカメラやアニメーションの後処理が行われる可能性があります。 |
| `MewUnityPostLateUpdate` | フレームの最後に実行される処理で、レンダリングの前準備やカメラの最終更新が含まれます。 |

PlayerLoop のタイミングを指定するには、TaskIntervalに対してタイミング型を指定します。
例えば MewUnityFixedUpdate を指定ることで、フレームスキップが発生した場合にも安定したゲーム時間でのタスク実行を行い遅延することを防くことができます。

```csharp
TaskInterval<MewUnityFixedUpdate>
    .Create(1000 /* ms */, TestTaskAsync, IntervalTimerType.UnityUnscaledTime)
    .Start(destroyCancellationToken);
```

