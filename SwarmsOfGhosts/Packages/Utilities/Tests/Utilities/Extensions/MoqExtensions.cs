using Cysharp.Threading.Tasks;
using Moq.Language;
using Moq.Language.Flow;

namespace Tests.Utilities.Extensions
{
    public static class MoqExtensions
    {
        public static IReturnsResult<TMock> ReturnsAsync<TMock>(this IReturns<TMock, UniTask> mock, int delayFrameCount)
            where TMock : class => DelayedResult(mock, delayFrameCount);

        private static IReturnsResult<TMock> DelayedResult<TMock>(
            IReturns<TMock, UniTask> mock, int delayFrameCount)
            where TMock : class
        {
            var completionSource = new UniTaskCompletionSource();

            UniTask
                .DelayFrame(delayFrameCount)
                .ContinueWith(() => completionSource.TrySetResult());

            return mock.Returns(completionSource.Task);
        }
    }
}