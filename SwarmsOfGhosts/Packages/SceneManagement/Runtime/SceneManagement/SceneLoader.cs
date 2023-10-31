using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Utilities.Extensions;

namespace SceneManagement
{
    public class SceneLoader<T> : ISceneLoader<T>
    {
        private readonly Dictionary<T, AsyncLazy> _pendingScenes = new Dictionary<T, AsyncLazy>();

        private readonly IUnitySceneManager _unitySceneManager;

        private SceneLoader(IUnitySceneManager unitySceneManager)
        {
            if (unitySceneManager == null)
                throw new ArgumentNullException(nameof(unitySceneManager));

            _unitySceneManager = unitySceneManager;
        }

        public async UniTask Load(T sceneName)
        {
            if (sceneName.IsDefault())
                throw new ArgumentException("Incorrect scene name", nameof(sceneName));

            if (_pendingScenes.TryGetValue(sceneName, out var pendingTask))
            {
                await pendingTask.Task;
                return;
            }

            var lazyTask = _unitySceneManager
                .LoadSceneAsync(sceneName.ToString())
                .ToAsyncLazy();

            _pendingScenes.Add(sceneName, lazyTask);
            await lazyTask.Task;
            _pendingScenes.Remove(sceneName);
        }
    }
}