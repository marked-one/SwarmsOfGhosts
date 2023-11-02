using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

namespace Debugging
{
    public class FpsCounter : IFpsCounter, ITickable
    {
        private readonly IUnityTime _time;
        private readonly float _measurePeriod;

        private readonly Queue<float> _deltaTimeValues = new Queue<float>();
        private float _deltaTimesSum;

        private readonly ReactiveProperty<float> _fps = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<float> Fps => _fps;

        [Inject]
        private FpsCounter(IUnityTime time, float measurePeriodSeconds)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            _time = time;
            _measurePeriod = measurePeriodSeconds;
        }

        public void Tick()
        {
            var deltaTime = _time.DeltaTime;
            var deltaTimeSum = _deltaTimesSum + deltaTime;

            while (deltaTimeSum > _measurePeriod && _deltaTimeValues.Count > 0)
            {
                var oldestDeltaTime = _deltaTimeValues.Dequeue();
                deltaTimeSum -= oldestDeltaTime;
            }

            _deltaTimeValues.Enqueue(deltaTime);
            _deltaTimesSum = _deltaTimeValues.Sum(); // Avoid float degradation

            var ticksCountForSmoothPeriod = _deltaTimeValues.Count;
            _fps.Value = ticksCountForSmoothPeriod / _deltaTimesSum;
        }
    }
}