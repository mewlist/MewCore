#nullable enable
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Mew.Core
{
    public struct MewUnityEarlyUpdate { }
    public struct MewUnityFixedUpdate { }
    public struct MewUnityPreUpdate { }
    public struct MewUnityUpdate { }
    public struct MewUnityPreLateUpdate { }
    public struct MewUnityPostLateUpdate { }
    public struct MewManualUpdate { }

    public static class MewLoopUnityInitializer
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            MewLoop.SetDefaultCollection<MewUnityUpdate>();
            Register<MewUnityEarlyUpdate, EarlyUpdate>();
            Register<MewUnityFixedUpdate, FixedUpdate>();
            Register<MewUnityPreUpdate, PreUpdate>();
            Register<MewUnityUpdate, Update>();
            Register<MewUnityPreLateUpdate, PreLateUpdate>();
            Register<MewUnityPostLateUpdate, PostLateUpdate>();
            Register<MewManualUpdate>();
        }

        private static void Register<T>()
        {
            MewLoop.Register<T>();
        }

        private static void Register<T,TPlayerLoop>()
        {
            Register<T>();

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var playerLoopSystem = new PlayerLoopSystem
            {
                type = typeof(T),
                updateDelegate = MewLoop.Update<T>
            };
            playerLoop = AddSubSystem<TPlayerLoop>(playerLoop, playerLoopSystem);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static PlayerLoopSystem AddSubSystem<T>(PlayerLoopSystem root, PlayerLoopSystem subSystem)
        {
            for (var i = 0; i < root.subSystemList.Length; i++)
            {
                if (root.subSystemList[i].type != typeof(T)) continue;
                root.subSystemList[i] = AddSubSystem(root.subSystemList[i], subSystem);
                break;
            }
            return root;
        }

        private static PlayerLoopSystem AddSubSystem(PlayerLoopSystem root, PlayerLoopSystem subSystem)
        {
            return new PlayerLoopSystem
            {
                type = root.type,
                updateDelegate = root.updateDelegate,
                updateFunction = root.updateFunction,
                loopConditionFunction = root.loopConditionFunction,
                subSystemList = root.subSystemList.Append(subSystem).ToArray()
            };
        }
    }
}
