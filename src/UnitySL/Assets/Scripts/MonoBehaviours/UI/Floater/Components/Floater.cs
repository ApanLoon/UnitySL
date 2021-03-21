using Assets.Scripts.MonoBehaviours.UI;
using UnityEngine;

public class Floater : MonoBehaviour
{
    [SerializeField] protected FloaterManager.FloaterType FloaterType;

    public void OnClose()
    {
        FloaterManager.Instance.SetFloaterVisible(FloaterType, false);
    }
}
