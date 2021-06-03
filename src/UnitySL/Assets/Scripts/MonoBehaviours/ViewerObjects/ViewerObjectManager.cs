using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects;
using Assets.Scripts.MonoBehaviours.UI.ToolTips;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.ViewerObjects
{
    public class ViewerObjectManager : MonoBehaviour
    {
        [SerializeField] protected PlaceholderTemplate Placeholders;
        [SerializeField] protected int ObjectCount = 0;
        [SerializeField] protected int RegionCount = 0;

        protected static Dictionary<Guid, GameObject> GameObjectGoByFullId = new Dictionary<Guid, GameObject>();
        protected static Dictionary<GameObject, Guid> FullIdByGameObject = new Dictionary<GameObject, Guid>();
        protected static Dictionary<RegionHandle, Dictionary<UInt32, GameObject>> GameObjectByRegionAndId = new Dictionary<RegionHandle, Dictionary<uint, GameObject>>();
        
        private void OnEnable()
        {
            Placeholders.Initialize();

            EventManager.Instance.OnObjectUpdate += OnObjectUpdate;
            EventManager.Instance.OnObjectUpdateCompressed += OnObjectUpdateCompressed;
            EventManager.Instance.OnImprovedTerseObjectUpdate += OnImprovedTerseObjectUpdate;
            EventManager.Instance.OnLogout += OnLogout;
        }

        private void OnDisable()
        {
            EventManager.Instance.OnImprovedTerseObjectUpdate -= OnImprovedTerseObjectUpdate;
            EventManager.Instance.OnObjectUpdateCompressed -= OnObjectUpdateCompressed;
            EventManager.Instance.OnObjectUpdate -= OnObjectUpdate;
            EventManager.Instance.OnLogout -= OnLogout;

            GameObjectGoByFullId.Clear();
        }

        #region ObjectUpdate
        protected void OnObjectUpdate(ObjectUpdateMessage message)
        {
            foreach (ObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                //if (objectData.PCode == PCode.LEGACY_AVATAR) Logger.LogDebug("ViewerObjectManager.OnObjectUpdate", $"fullId={objectData.FullId}, localId={objectData.LocalId}");
                GameObject go = GetOrCreateGameObject(message.RegionHandle, objectData.FullId, objectData.LocalId, objectData.PCode);
                if (go == null)
                {
                    return;
                }

                go.GetComponent<ToolTipTarget>().Text = $"{objectData.PCode}\n{objectData.FullId}\n{objectData.NameValue}";

                UpdateParent(message.RegionHandle, objectData.ParentId, go);
                UpdateMovement(objectData.MovementUpdate, GameObjectByLocalId(message.RegionHandle, objectData.LocalId));
                go.transform.localScale = objectData.Scale;
                UpdatePlaceholderColour(objectData.PCode, go);
            }
        }

        protected void OnObjectUpdateCompressed(ObjectUpdateCompressedMessage message)
        {
            foreach (ObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                //if (objectData.PCode == PCode.LEGACY_AVATAR) Logger.LogDebug("ViewerObjectManager.OnObjectUpdateCompressed", $"fullId={objectData.FullId}, localId={objectData.LocalId}");
                GameObject go = GetOrCreateGameObject(message.RegionHandle, objectData.FullId, objectData.LocalId, objectData.PCode);
                if (go == null)
                {
                    return;
                }

                go.GetComponent<ToolTipTarget>().Text = $"{objectData.PCode}\n{objectData.FullId}\n{objectData.NameValue}";

                UpdateParent(message.RegionHandle, objectData.ParentId, go);
                UpdateMovement(objectData.MovementUpdate, GameObjectByLocalId(message.RegionHandle, objectData.LocalId));
                go.transform.localScale = objectData.Scale;
                UpdatePlaceholderColour(objectData.PCode, go);
            }
        }

        protected void OnImprovedTerseObjectUpdate(ImprovedTerseObjectUpdateMessage message)
        {
            foreach (ImprovedTerseObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                GameObject go = GameObjectByLocalId(message.RegionHandle, objectData.LocalId);
                if (go == null)
                {
                    return;
                }

                UpdateMovement(objectData.MovementUpdate, go);
            }
        }

        protected void UpdateParent(RegionHandle regionHandle, UInt32 parentId, GameObject go)
        {
            if (parentId == 0
                || GameObjectByRegionAndId.ContainsKey(regionHandle) == false
                || GameObjectByRegionAndId[regionHandle].ContainsKey(parentId) == false)
            {
                return;
            }

            if (GameObjectByRegionAndId[regionHandle][parentId].transform != go.transform.parent)
            {
                go.transform.SetParent(GameObjectByRegionAndId[regionHandle][parentId].transform);
            }
        }

        protected void UpdateMovement(ObjectUpdateMessage.MovementUpdate update, GameObject go)
        {
            if (update == null || go == null)
            {
                return;
            }

            go.transform.localPosition = update.Position;
            go.transform.localRotation = update.Rotation;

        }

        protected void UpdatePlaceholderColour(PCode pCode, GameObject go)
        {
            ViewerObjectPlaceholder placeholder = go.GetComponent<ViewerObjectPlaceholder>();
            switch (pCode)
            {
                case PCode.CUBE:
                case PCode.PRISM:
                case PCode.TETRAHEDRON:
                case PCode.PYRAMID:
                case PCode.CYLINDER:
                case PCode.CONE:
                case PCode.SPHERE:
                case PCode.TORUS:
                case PCode.CYLINDER_HEMI:
                case PCode.CONE_HEMI:
                case PCode.SPHERE_HEMI:
                case PCode.TORUS_HEMI:
                    placeholder.SetColour(Color.yellow);
                    break;

                case PCode.VOLUME:
                    placeholder.SetColour(Color.magenta);
                    break;

                case PCode.APP:
                    placeholder.SetColour(Color.black);
                    break;

                case PCode.LEGACY_AVATAR:
                    placeholder.SetColour(Color.blue);
                    break;

                case PCode.LEGACY_GRASS:
                    placeholder.SetColour(Color.green);
                    break;

                case PCode.TREE_NEW:
                case PCode.LEGACY_TREE:
                    ColorUtility.TryParseHtmlString("#964B00", out Color c);
                    placeholder.SetColour(c);
                    break;

                case PCode.LEGACY_PART_SYS:
                    placeholder.SetColour(Color.blue);
                    break;
                case PCode.LEGACY_ROCK:
                    placeholder.SetColour(Color.gray);
                    break;
                case PCode.LEGACY_TEXT_BUBBLE:
                    placeholder.SetColour(Color.cyan);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion ObjectUpdate

        protected void OnLogout()
        {
            foreach (GameObject go in GameObjectGoByFullId.Values)
            {
                Destroy(go);
            }

            GameObjectGoByFullId.Clear();
        }

        protected GameObject GameObjectByLocalId(RegionHandle regionHandle, UInt32 localId)
        {
            if (GameObjectByRegionAndId.ContainsKey(regionHandle) == false
                || GameObjectByRegionAndId[regionHandle].ContainsKey(localId) == false)
            {
                return null;
            }
            return GameObjectByRegionAndId[regionHandle][localId];
        }

        protected GameObject GetOrCreateGameObject(RegionHandle regionHandle, Guid fullId, UInt32 localId, PCode pCode)
        {
            GameObject go;
            if (fullId != Guid.Empty)
            {
                go = GameObjectGoByFullId.ContainsKey(fullId)
                    ? GameObjectGoByFullId[fullId]
                    : AddObject(regionHandle, fullId, localId, pCode);
            }
            else
            {
                go = GameObjectByLocalId(regionHandle, localId);
            }

            return go;
        }

        protected GameObject AddObject(RegionHandle regionHandle, Guid fullId, UInt32 localId, PCode pCode)
        {
            if (pCode != PCode.LEGACY_AVATAR)
            {
                return null;
            }

            Logger.LogDebug("ViewerObjectManager.AddObject", $"fullId={fullId}, localId={localId}");

            ViewerObjectPlaceholder placeholder = Placeholders.InstantiateTemplate();
            GameObjectGoByFullId[fullId] = placeholder.gameObject;
            FullIdByGameObject[placeholder.gameObject] = fullId;

            ObjectCount++;

            if (GameObjectByRegionAndId.ContainsKey(regionHandle) == false)
            {
                GameObjectByRegionAndId[regionHandle] = new Dictionary<uint, GameObject>();
                RegionCount++;
            }
            GameObjectByRegionAndId[regionHandle][localId] = placeholder.gameObject;

            return placeholder.gameObject;
        }

        [Serializable] public class PlaceholderTemplate : Template<ViewerObjectPlaceholder> { }
    }
}
