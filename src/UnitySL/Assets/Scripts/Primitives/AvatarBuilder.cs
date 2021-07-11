using Assets.Scripts.Appearance;
using UnityEngine;

namespace Assets.Scripts.Primitives
{
    /// <summary>
    /// TODO: This class will replace the simple PolyMeshTest and build the entire, rigged, avatar based on the static information in AvatarAppearance.
    /// </summary>
    public class AvatarBuilder : MonoBehaviour
    {
        private void Start()
        {
            if (AvatarAppearance.AvatarXmlInfo == null)
            {
                AvatarAppearance.InitClass();
            }

            //foreach (var VARIABLE in AvatarAppearance.AvatarXmlInfo.)
            //{
                
            //}
        }
    }
}
