namespace SwarmsOfGhosts.App
{
    public interface IApplication
    {
        public void Quit();
    }

    public class GameApplication : IApplication
    {
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}