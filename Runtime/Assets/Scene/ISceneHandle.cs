using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public interface ISceneHandle : IEquatable<Scene>
    {
        ValueTask<Scene> GetScene();
    }
}
