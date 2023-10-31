using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tests.Utilities.Extensions;
using Tests.Utilities.Helpers;
using UnityEngine.TestTools;
using Zenject;

namespace SceneManagement.Tests
{
    [TestFixture]
    public class SceneLoaderStringTests : SceneLoaderTests<string>
    {
        protected override string SceneNameDefault => null;
        protected override string SceneNameA => "A";
        protected override string SceneNameB => "B";
    }

    [TestFixture]
    public class SceneLoaderEnumTests : SceneLoaderTests<SceneLoaderEnumTests.SceneNames>
    {
        public enum SceneNames
        {
            Default,
            A,
            B
        }

        protected override SceneNames SceneNameDefault => SceneNames.Default;
        protected override SceneNames SceneNameA => SceneNames.A;
        protected override SceneNames SceneNameB => SceneNames.B;
    }

    [TestFixture]
    public class SceneLoaderStructTests : SceneLoaderTests<SceneLoaderStructTests.SceneName>
    {
        public readonly struct SceneName
        {
            private readonly string _sceneName;
            public SceneName(string sceneName) => _sceneName = sceneName;
            public override string ToString() => _sceneName;
        }

        protected override SceneName SceneNameDefault { get; } = default;
        protected override SceneName SceneNameA { get; } = new SceneName("A");
        protected override SceneName SceneNameB { get; } = new SceneName("B");
    }

    [TestFixture]
    public class SceneLoaderClassTests : SceneLoaderTests<SceneLoaderClassTests.SceneName>
    {
        public class SceneName
        {
            private readonly string _sceneName;
            public SceneName(string sceneName) => _sceneName = sceneName;
            public override string ToString() => _sceneName;
        }

        protected override SceneName SceneNameDefault { get; } = null;
        protected override SceneName SceneNameA { get; } = new SceneName("A");
        protected override SceneName SceneNameB { get; } = new SceneName("B");
    }

    public abstract class SceneLoaderTests<T> : ZenjectUnitTestFixture
    {
        private Mock<IUnitySceneManager> _sceneManagerMock;

        protected abstract T SceneNameDefault { get; }
        protected abstract T SceneNameA { get; }
        protected abstract T SceneNameB { get; }

        [SetUp]
        public void CommonInstall()
        {
            Container
                .Bind<SceneLoader<T>>()
                .WithId("SceneLoader_SceneManagerIsNull")
                .AsTransient()
                .WithArgumentsExplicit(InjectUtil.CreateArgListExplicit<IUnitySceneManager>(null));

            _sceneManagerMock = new Mock<IUnitySceneManager>();

            Container
                .Bind<SceneLoader<T>>()
                .WithId("SceneLoader_SceneManagerIsValid")
                .AsTransient()
                .WithArgumentsExplicit(InjectUtil.CreateArgListExplicit(_sceneManagerMock.Object));
        }

        [Test]
        public void SceneLoader_Throws_IfSceneManagerAdapterIsNull() =>
            Assert.Throws<ZenjectException>(() =>
                Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsNull"));

        [Test]
        public void SceneLoader_DoesntThrow_IfSceneManagerAdapterIsNotNull() =>
            Assert.DoesNotThrow(() => Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid"));

        [Test]
        public void SceneLoader_LoadThrows_IfSceneNameIsDefault()
        {
            var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");
            AssertHelpers.ThrowsAsync<ArgumentException>(() => sceneLoader.Load(default));
            AssertHelpers.ThrowsAsync<ArgumentException>(() => sceneLoader.Load(SceneNameDefault));
        }

        [Test]
        public void SceneLoader_LoadDoesntThrow_IfSceneNameIsValid()
        {
            var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");
            AssertHelpers.DoesNotThrowAsync(() => sceneLoader.Load(SceneNameA));
        }

        [UnityTest]
        public IEnumerator SceneLoader_CallsSceneManagerLoadOnce_IfSceneNameIsValid() =>
            UniTask.ToCoroutine(async () =>
            {
                var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameA.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                await sceneLoader.Load(SceneNameA);

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameA.ToString()), Times.Once());
            });

        [UnityTest]
        public IEnumerator
            SceneLoader_CallsSceneManagerLoadOnlyOnce_IfLoadingOfSameSceneIsRequestedInParallel() =>
            UniTask.ToCoroutine(async () =>
            {
                var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameA.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                var task1 = sceneLoader.Load(SceneNameA);
                var task2 = sceneLoader.Load(SceneNameA);

                await UniTask.WhenAll(task1, task2);

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameA.ToString()), Times.Once());
            });


        [UnityTest]
        public IEnumerator
            SceneLoader_CallsSceneManagerLoadOnceForEachLoadRequest_IfLoadingOfSameSceneIsRequestedInSequence() =>
            UniTask.ToCoroutine(async () =>
            {
                var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameA.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                await sceneLoader.Load(SceneNameA);
                await sceneLoader.Load(SceneNameA);

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameA.ToString()), Times.Exactly(2));
            });

        [UnityTest]
        public IEnumerator
            SceneLoader_CallsSceneManagerLoadOnceForEachLoadRequest_IfLoadingOfDifferentScenesIsRequestedInParallel() =>
            UniTask.ToCoroutine(async () =>
            {
                var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameA.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameB.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                var taskA = sceneLoader.Load(SceneNameA);
                var taskB = sceneLoader.Load(SceneNameB);
                await UniTask.WhenAll(taskA, taskB);

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameA.ToString()), Times.Once());

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameB.ToString()), Times.Once());
            });

        [UnityTest]
        public IEnumerator
            SceneLoader_CallsSceneManagerLoadOnceForEachLoadRequest_IfLoadingOfDifferentScenesIsRequestedInSequence() =>
            UniTask.ToCoroutine(async () =>
            {
                var sceneLoader = Container.ResolveId<SceneLoader<T>>("SceneLoader_SceneManagerIsValid");

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameA.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                _sceneManagerMock
                    .Setup(sceneManager => sceneManager.LoadSceneAsync(SceneNameB.ToString()))
                    .ReturnsAsync(delayFrameCount: 1);

                await sceneLoader.Load(SceneNameA);
                await sceneLoader.Load(SceneNameB);

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameA.ToString()), Times.Once());

                _sceneManagerMock.Verify(sceneManager =>
                    sceneManager.LoadSceneAsync(SceneNameB.ToString()), Times.Once());
            });
    }
}