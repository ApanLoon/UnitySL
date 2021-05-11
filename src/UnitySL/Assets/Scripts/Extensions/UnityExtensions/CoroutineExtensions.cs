using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Extensions.UnityExtensions
{
    public static class CoroutineExtensions
    {
        public class CoroutineRunner : MonoBehaviour { }
        private static CoroutineRunner coroutineRunner;
        public static Coroutine RunCoroutine(this IEnumerator ienum)
        {
            if (coroutineRunner == null)
            {
                coroutineRunner = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
                coroutineRunner.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
                coroutineRunner.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
            }
            return coroutineRunner.StartCoroutine(ienum);
        }
    }
}
