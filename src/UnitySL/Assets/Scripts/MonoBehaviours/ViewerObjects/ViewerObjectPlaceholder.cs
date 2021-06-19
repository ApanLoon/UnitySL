
using Assets.Scripts.Types.OctTrees;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.ViewerObjects
{
    public class ViewerObjectPlaceholder : MonoBehaviour, IPositionable
    {
        public Vector3 Position => transform.position;

        public void SetColour(Color colour)
        {
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material.color = colour;
            }
        }
    }
}
