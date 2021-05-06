using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Viewer;
using Assets.Scripts.Regions;
using UnityEngine;

public class ViewerEffectsManager : MonoBehaviour
{
    [SerializeField] protected GameObject SpiralEffectPrefab;
    [SerializeField] protected GameObject LookAtEffectPrefab;
    [SerializeField] protected GameObject PointAtEffectPrefab;

    protected Dictionary<Guid, GameObject> GameObjectByEffectId = new Dictionary<Guid, GameObject>();

    public void OnEnable()
    {
        EventManager.Instance.OnViewerEffectMessage += OnViewerEffectMessage;
    }
    public void OnDisable()
    {
        EventManager.Instance.OnViewerEffectMessage -= OnViewerEffectMessage;
    }

    protected void OnViewerEffectMessage(ViewerEffectMessage message)
    {
        Region region = Agent.CurrentPlayer?.Region;
        if (region == null)
        {
            return;
        }

        foreach (ViewerEffect viewerEffect in message.Effects)
        {
            GameObject go = null;
            switch (viewerEffect)
            {
                case ViewerEffectSpiral spiralEffect:
                    go = Instantiate(SpiralEffectPrefab, transform); // TODO: Use effect pool
                    go.transform.position = region.GetLocalPosition(spiralEffect.PositionGlobal);
                    break;

                case ViewerEffectLookAt lookAtEffect:
                    go = Instantiate(LookAtEffectPrefab, transform); // TODO: Use effect pool
                    go.transform.position = region.GetLocalPosition(lookAtEffect.TargetPosition);
                    break;

                case ViewerEffectPointAt pointAtEffect:
                    go = Instantiate(PointAtEffectPrefab, transform); // TODO: Use effect pool
                    go.transform.position = region.GetLocalPosition(pointAtEffect.TargetPosition);
                    // TODO: Set the source position to the location of the source avatar?
                    break;
            }

            if (go == null)
            {
                Logger.LogWarning("ViewerEffectsManager.OnViewerEffectMessage", $"ViewerEffect of type {viewerEffect.EffectType} is not implemented.");
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
