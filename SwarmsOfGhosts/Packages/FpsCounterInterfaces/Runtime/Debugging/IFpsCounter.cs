using UniRx;

namespace Debugging
{
    public interface IFpsCounter
    {
        public IReadOnlyReactiveProperty<float> Fps { get; }
    }
}