using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Tests.Utilities.Helpers
{
    public static class AssertHelpers
    {
        public static async void ThrowsAsync<TExpectedException>(Func<UniTask> code)
            where TExpectedException : Exception
        {
            try
            {
                await code.Invoke();
            }
            catch (TExpectedException)
            {
                return;
            }

            Assert.Fail("Expected an exception of type " +
                        typeof(TExpectedException).FullName +
                        " but none was thrown."  );
        }

        public static async void DoesNotThrowAsync(Func<UniTask> code)
        {
            try
            {
                await code.Invoke();
            }
            catch (Exception)
            {
                Assert.Fail("Expected no exceptions but an exception of type " +
                            typeof(Exception).FullName +
                            " was thrown."  );
            }
        }
    }
}