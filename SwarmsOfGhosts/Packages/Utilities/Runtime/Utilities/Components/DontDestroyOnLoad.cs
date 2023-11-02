using System;
using UnityEngine;

namespace Utilities.Components
{
    [DisallowMultipleComponent]
    public sealed class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            ValidateRoot(notRootAction: Destroy, rootAction: () =>
            {
                DontDestroyOnLoad(gameObject);
            });
        }

#if UNITY_EDITOR
        private void Reset() => ValidateRoot(notRootAction: DestroyImmediate);
#endif

        private void ValidateRoot(Action<UnityEngine.Object> notRootAction, Action rootAction = null)
        {
            if (transform.parent == null)
            {
                rootAction?.Invoke();
            }
            else
            {
                Debug.LogError("DontDestroyOnLoad only works for root GameObjects");
                notRootAction?.Invoke(this);
            }
        }
    }
}