using UnityEngine;

namespace Debugging
{
    public class UnityTimeAdapter : IUnityTime
    {
        public float DeltaTime => Time.deltaTime;
    }
}