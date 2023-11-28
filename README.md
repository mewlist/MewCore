# MewCore
Core Game Libraries for Unity

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewCore/blob/main/LICENSE)

## Documents

https://mewlist.github.io/MewCore/

## Readme (日本語)

[Readme_ja.md](./README_ja.md)

## Installation

It can be installed via UPM.
Please specify the following git URL.
```
git@github.com:mewlist/MewCore.git
```

## TaskQueue

TaskQueue is a library that simplifies and efficiently handles asynchronous processes in Unity development. This library enables the management of dynamically changing asynchronous functions and the determination of execution order based on priority.

### Main Features
Dynamic Function Addition: Allows adding asynchronous functions to the task queue at runtime. This enables flexible response to changing requirements and situations.

Priority-Based Execution Management: Sets priorities for each asynchronous function and processes important tasks preferentially. This prevents delays in critical processes.

Serial Processing and Safety: Executes multiple asynchronous functions in order, waiting for one function to complete before starting the next. This improves safety in UI updates and game sequencing.

Simple Description: TaskQueue is designed to simplify the description of executing asynchronous functions.

### Use Scenarios

Dynamic UI Updates: Used for smooth control of dynamic display and hiding of dialog boxes and menus in the game.

Game Event Sequencing: Suitable for managing ordered events such as story progression and tutorials.

Command Pattern Adaptation: Suitable for implementing the command pattern, including asynchronous processes.

UI Event Handling: Used to prevent concurrent execution in response to asynchronous UI events such as clicks.

### Sample Code

```csharp
class Sample : Monobehaviour
{
    void Start()
    {
        // Create an instance of TaskQueue.
        var taskQueue = new TaskQueue();
        // Start executing TaskQueue.
        // Passing destroyCancellationToken will automatically stop the process and dispose of it when the MonoBehaviour is destroyed.
        taskQueue.Start(destroyCancellationToken);

        // Add asynchronous functions to TaskQueue.
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

#### Execution Result

```
Hello
// 2sec later
Bye
```

### Executing Priority Tasks

You can execute priority tasks by specifying the priority as the second argument in Enqueue.
The lower the number, the more prioritized the processing. The default value is 0.

```csharp
taskQueue.Enqueue(async ct => { ... }, priority: 1);
```

### Specifying PlayerLoop

You can specify the PlayerLoop timing for processing the queue.
The following timing types are defined.

| Timing                   | Description |
|-------------------------|------|
| `MewUnityEarlyUpdate`   | Called at the beginning of Unity's frame update. At this stage, initial event processing and input updates occur. |
| `MewUnityFixedUpdate`   | The timing for physics updates. Corresponds to fixed-frame-rate processing in Unity Engine. |
| `MewUnityPreUpdate`     | Processing executed before the Update method. Includes scene state updates and animation updates. |
| `MewUnityUpdate`        | The normal Update method timing, mainly used for updating game logic. |
| `MewUnityPreLateUpdate` | Processing executed before LateUpdate. Some post-processing for cameras and animations may occur. |
| `MewUnityPostLateUpdate` | Processing at the end of the frame, including rendering preparation and final camera updates. |

To specify PlayerLoop timing, specify the timing type in the constructor.
For example, specifying MewUnityFixedUpdate can prevent queue processing delays in case of frame skips.

```csharp
var fixedUpdateTaskQueue = new TaskQueue<MewUnityFixedUpdate>();
```


## TaskInterval

TaskInterval is a library that facilitates the execution of specific processes at regular intervals in Unity development. This library allows for periodic execution of asynchronous functions and prevents multiple asynchronous processes from running concurrently.

### Main Features

- **Regular Execution of Asynchronous Functions**: Automatically executes asynchronous functions at specified intervals, making it easier to manage tasks that include asynchronous processes.

- **Flexible Response to Processing Time**: If the execution of an asynchronous function takes longer than the specified interval, it can either skip the process or continue executing it delayed. This differs from typical periodic execution and adapts better to real-time operating environments.

- **Prevention of Concurrent Execution**: Only one asynchronous process is executed at a time, preventing multiple processes from running simultaneously. This makes task execution predictable and safe.

- **Stable Interval Execution**: If synchronous functions are used, it is also possible to execute functions at stable intervals.

### Use Scenarios

- **Regular Updates Within the Game**: Used for regularly updating the state of the game or objects at set intervals.

- **Background Processes**: Suitable for regular background processes such as network communication and data loading.

- **Regular UI Updates**: Can also be used for regularly updating user interface elements.

Using TaskInterval makes the implementation of regular processes in Unity development more flexible and efficient, and prevents issues due to concurrent execution.

### Sample Code

```csharp
public class Sample : MonoBahaviour
{
    private void Awake()
    {
        // Create a TaskInterval that executes TestTaskAsync every second.
        // Passing destroyCancellationToken will automatically stop the process and dispose of it when the MonoBehaviour is destroyed.
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

#### Execution Result

```
0.9996152
1.000825
1.000599
0.9999266
1.000448
0.9925194
...
```

### Specifying Timer Type

You can change the timer used by specifying the type of timer as the third argument in Create.

| Timer Type | Description             |
|--------------|-------------------------|
| `IntervalTimerType.SystemTime` | Uses system time.       |
| `IntervalTimerType.UnityTime` | Uses Unity's Time.time. |
| `IntervalTimerType.UnityUnscaledTime` | Time.unscaledTime.      |

	
Example of executing a process unaffected by Time.timeScale.

```csharp
TaskInterval
    .Create(1000 /* ms */, TestTaskAsync, IntervalTimerType.UnityUnscaledTime)
    .Start(destroyCancellationToken);
```

### Specifying PlayerLoop

You can specify the PlayerLoop timing for processing tasks.
The following timing types are defined.

| Timing                   | Description |
|-------------------------|------|
| `MewUnityEarlyUpdate`   | Called at the beginning of Unity's frame update. At this stage, initial event processing and input updates occur. |
| `MewUnityFixedUpdate`   | The timing for physics updates. Corresponds to fixed-frame-rate processing in Unity Engine. |
| `MewUnityPreUpdate`     | Processing executed before the Update method. Includes scene state updates and animation updates. |
| `MewUnityUpdate`        | The normal Update method timing, mainly used for updating game logic. |
| `MewUnityPreLateUpdate` | Processing executed before LateUpdate. Some post-processing for cameras and animations may occur. |
| `MewUnityPostLateUpdate` | Processing at the end of the frame, including rendering preparation and final camera updates. |

To specify PlayerLoop timing, specify the timing type for TaskInterval.
For example, specifying MewUnityFixedUpdate can ensure stable game-time task execution even in case of frame skips, preventing delays.

```csharp
TaskInterval<MewUnityFixedUpdate>
    .Create(1000 /* ms */, TestTaskAsync, IntervalTimerType.UnityUnscaledTime)
    .Start(destroyCancellationToken);
```


