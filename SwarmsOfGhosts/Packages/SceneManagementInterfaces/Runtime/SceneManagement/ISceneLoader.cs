using Cysharp.Threading.Tasks;

namespace SceneManagement
{
    public interface ISceneLoader<in T>
    {
        public UniTask Load(T sceneName);
    }
}