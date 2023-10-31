using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class SceneManagerAdapter : IUnitySceneManager
    {
        public UniTask LoadSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName).ToUniTask();
    }
}