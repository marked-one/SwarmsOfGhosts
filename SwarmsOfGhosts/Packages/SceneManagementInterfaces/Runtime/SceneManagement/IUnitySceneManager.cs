using Cysharp.Threading.Tasks;

namespace SceneManagement
{
    public interface IUnitySceneManager
    {
        public UniTask LoadSceneAsync(string sceneName);
    }
}