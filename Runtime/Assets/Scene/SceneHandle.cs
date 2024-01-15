using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Mew.Core.Assets
{
    public class SceneHandle : ISceneHandle
    {
        public Scene Scene { get; }

        public SceneHandle(Scene scene)
        {
            Scene = scene;
        }

        public async ValueTask<Scene> GetScene()
        {
            return await Task.FromResult(Scene);
        }

        public bool Equals(Scene other)
        {
            return Scene.Equals(other);
        }
    }
}
