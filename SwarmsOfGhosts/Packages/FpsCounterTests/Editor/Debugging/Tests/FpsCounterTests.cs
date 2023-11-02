using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Debugging.Tests
{
    [TestFixture]
    public class FpsCounterWithNegativeMeasurePeriodTests : FpsCounterNonPositiveMeasurePeriodTests
    {
        protected override float MeasurePeriodSeconds => -1f;
    }

    [TestFixture]
    public class FpsCounterWithZeroMeasurePeriodTests : FpsCounterNonPositiveMeasurePeriodTests
    {
        protected override float MeasurePeriodSeconds => 0f;
    }

    public abstract class FpsCounterNonPositiveMeasurePeriodTests : FpsCounterTests
    {
        [Test]
        public void FpsCounter_ReturnsSingleFrameFpsForAnyTick_Always()
        {
            Random.InitState(100);

            var fpsCounter = Container.ResolveId<FpsCounter>(TimeIsValid);
            var expectedFps = 0f;
            fpsCounter.Fps
                .Subscribe(value => Assert.AreEqual(expectedFps, value, Epsilon))
                .AddTo(Subscriptions);

            var ticks = 1000;
            for (var i = 0; i < ticks; i++)
            {
                var deltaTime = Random.Range(0.01f, 0.5f);
                expectedFps = 1 / deltaTime;

                UnityTimeMock
                    .Setup(time => time.DeltaTime)
                    .Returns(deltaTime);

                fpsCounter.Tick();
            }
        }
    }

    [TestFixture]
    public class FpsCounterWithPositiveMeasurePeriodTests : FpsCounterTests
    {
        protected override float MeasurePeriodSeconds => 1f;

        [Test]
        public void FpsCounter_ReturnsSmoothedFpsForAnyTick_Always()
        {
            Random.InitState(100);

            var fpsCounter = Container.ResolveId<FpsCounter>(TimeIsValid);
            var expectedFps = 0f;
            fpsCounter.Fps
                .Subscribe(value => Assert.AreEqual(expectedFps, value, Epsilon))
                .AddTo(Subscriptions);

            var ticks = 1000;
            var deltaTimeQueue = new Queue<float>();
            var deltaTimes = 0f;
            for (var i = 0; i < ticks; i++)
            {
                var deltaTime = Random.Range(0.01f, 0.5f);
                deltaTimes += deltaTime;

                while (deltaTimes > MeasurePeriodSeconds && deltaTimeQueue.Count > 0)
                {
                    var oldestDeltaTime = deltaTimeQueue.Dequeue();
                    deltaTimes -= oldestDeltaTime;
                }

                deltaTimeQueue.Enqueue(deltaTime);
                deltaTimes = deltaTimeQueue.Sum();

                var ticksCount = deltaTimeQueue.Count;
                expectedFps = ticksCount / deltaTimes;

                UnityTimeMock
                    .Setup(time => time.DeltaTime)
                    .Returns(deltaTime);

                fpsCounter.Tick();
            }
        }
    }

    public abstract class FpsCounterTests : ZenjectUnitTestFixture
    {
        private const string _timeIsNull = "FpsCounter_TimeIsNull";
        protected const string TimeIsValid = "FpsCounter_TimeIsValid";

        protected float Epsilon;

        protected Mock<IUnityTime> UnityTimeMock { get; private set; }
        protected CompositeDisposable Subscriptions { get; private set; }
        protected abstract float MeasurePeriodSeconds { get; }

        [SetUp]
        public void CommonInstall()
        {
            Container
                .Bind<FpsCounter>()
                .WithId(_timeIsNull)
                .AsTransient()
                .WithArguments((IUnityTime)null, MeasurePeriodSeconds);

            UnityTimeMock = new Mock<IUnityTime>();

            Container
                .Bind<FpsCounter>()
                .WithId(TimeIsValid)
                .AsTransient()
                .WithArguments(UnityTimeMock.Object, MeasurePeriodSeconds);

            Subscriptions = new CompositeDisposable();
            Epsilon = Mathf.Epsilon;
        }

        [Test]
        public void FpsCounter_ThrowsArgumentNullException_IfTimeIsNull() =>
            Assert.Throws<ZenjectException>(() => Container.ResolveId<FpsCounter>(_timeIsNull));

        [Test]
        public void FpsCounter_DoesntThrow_IfTimeIsValid() =>
            Assert.DoesNotThrow(() => Container.ResolveId<FpsCounter>(TimeIsValid));

        [Test]
        public void FpsCounter_ReturnsSingleFrameFpsForFirstTick_Always()
        {
            var deltaTime = 0.5f;

            UnityTimeMock
                .Setup(time => time.DeltaTime)
                .Returns(deltaTime);

            var fpsCounter = Container.ResolveId<FpsCounter>(TimeIsValid);
            var expectedFps = 1 / deltaTime;
            fpsCounter.Fps
                .Skip(1)
                .Subscribe(value => Assert.AreEqual(expectedFps, value, Epsilon))
                .AddTo(Subscriptions);

            fpsCounter.Tick();
        }

        [Test]
        public void FpsCounter_CallsTimeDeltaTimeOnlyOnceEachTick_Always()
        {
            var ticks = 1000;
            var fpsCounter = Container.ResolveId<FpsCounter>(TimeIsValid);
            for (var i = 0; i < ticks; i++)
                fpsCounter.Tick();

            UnityTimeMock.Verify(time => time.DeltaTime, Times.Exactly(ticks));
        }

        [TearDown]
        public void CommonDispose() => Subscriptions.Dispose();
    }
}