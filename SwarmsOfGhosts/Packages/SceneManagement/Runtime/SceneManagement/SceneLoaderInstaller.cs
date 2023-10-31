using Zenject;

namespace SceneManagement
{
    // Here we can only implement the installer as a C# class
    // as we can't specify template parameters from Unity Editor.
    public class SceneLoaderInstaller<T> : Installer<SceneLoaderInstaller<T>>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<SceneManagerAdapter>()
                .AsSingle();

            Container
                .BindInterfacesTo<SceneLoader<T>>()
                .AsSingle();
        }
    }
}