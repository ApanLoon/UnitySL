
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.ViewerObjects
{
    public class ViewerObjectPlaceholder : MonoBehaviour
    {
        public void SetColour(Color colour)
        {
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material.color = colour;
            }
        }
    }
}
