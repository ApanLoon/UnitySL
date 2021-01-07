using System;
using System.Collections.Generic;
using UnityEngine;

public class ViewerEffectsManager : MonoBehaviour
{
    [SerializeField] protected GameObject SpiralEffectPrefab;
    [SerializeField] protected GameObject LookAtEffectPrefab;

    protected Dictionary<Guid, GameObject> GameObjectByEffectId = new Dictionary<Guid, GameObject>();

    public void Start()
    {
        EventManager.Instance.OnViewerEffectMessage += OnViewerEffectMessage;
    }

    protected void OnViewerEffectMessage(ViewerEffectMessage message)
    {
        foreach (ViewerEffect viewerEffect in message.Effects)
        {
            GameObject go = null;
            switch (viewerEffect)
            {
                case ViewerEffectSpiral spiralEffect:
                    go = Instantiate(SpiralEffectPrefab, transform); // TODO: Use effect pool
                    go.transform.position = Region.CurrentRegion.GetLocalPosition(spiralEffect.PositionGlobal);
                    break;

                case ViewerEffectLookAt lookAtEffect:
                    go = Instantiate(LookAtEffectPrefab, transform); // TODO: Use effect pool
                    go.transform.position = Region.CurrentRegion.GetLocalPosition(lookAtEffect.TargetPosition);
                    break;
            }

            if (go == null)
            {
                Logger.LogWarning($"ViewerEffectsManager.OnViewerEffectMessage: ViewerEffect of type {viewerEffect.EffectType} is not implemented.");
                continue;
            }
            GameObjectByEffectId[viewerEffect.Id] = go;
            DestroyOnTime dot = go.GetComponent<DestroyOnTime>();
            if (dot != null)
            {
                dot.LifeTime = viewerEffect.Duration;
            }
        }
    }
}
