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

        protected void OnObjectUpdate(ObjectUpdateMessage message)
        {
            foreach (ObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                if (objectData.FullId == Guid.Empty
                && GameObjectByRegionAndId.ContainsKey(message.RegionHandle)
                && GameObjectByRegionAndId[message.RegionHandle].ContainsKey(objectData.LocalId))
                {
                    GameObject go = GameObjectByRegionAndId[message.RegionHandle][objectData.LocalId];
                    objectData.FullId = FullIdByGameObject[go];
                }

                if (GameObjectGoByFullId.ContainsKey(objectData.FullId))
                {
                    UpdateObject(objectData, message.RegionHandle);
                }
                else
                {
                    AddObject(objectData, message.RegionHandle);
                }
            }
        }

        protected void OnObjectUpdateCompressed(ObjectUpdateCompressedMessage message)
        {
            foreach (ObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                if (objectData.FullId == Guid.Empty
                    && GameObjectByRegionAndId.ContainsKey(message.RegionHandle)
                    && GameObjectByRegionAndId[message.RegionHandle].ContainsKey(objectData.LocalId))
                {
                    GameObject go = GameObjectByRegionAndId[message.RegionHandle][objectData.LocalId];
                    objectData.FullId = FullIdByGameObject[go];
                }

                if (GameObjectGoByFullId.ContainsKey(objectData.FullId))
                {
                    UpdateObject(objectData, message.RegionHandle);
                }
                else
                {
                    AddObject(objectData, message.RegionHandle);
                }
            }
        }

        protected void OnImprovedTerseObjectUpdate(ImprovedTerseObjectUpdateMessage message)
        {
            foreach (ImprovedTerseObjectUpdateMessage.ObjectData objectData in message.Objects)
            {
                Guid fullId = Guid.Empty;
                if (   GameObjectByRegionAndId.ContainsKey(message.RegionHandle)
                    && GameObjectByRegionAndId[message.RegionHandle].ContainsKey(objectData.LocalId))
                {
                    GameObject go = GameObjectByRegionAndId[message.RegionHandle][objectData.LocalId];
                    fullId = FullIdByGameObject[go];
                }

                if (GameObjectGoByFullId.ContainsKey(fullId))
                {
                    UpdateObject(objectData, fullId, message.RegionHandle);
                }
            }
        }

        #region ImprovedTerseObjectUpdate
        protected virtual void UpdateObject(ImprovedTerseObjectUpdateMessage.ObjectData objectData, Guid fullId, RegionHandle regionHandle)
        {
            UpdateObject(objectData, GameObjectGoByFullId[fullId], regionHandle);
        }

        protected virtual void UpdateObject(ImprovedTerseObjectUpdateMessage.ObjectData objectData, GameObject go, RegionHandle regionHandle)
        {
            if (objectData.MovementUpdate != null)
            {
                go.transform.localPosition = objectData.MovementUpdate.Position;
                go.transform.localRotation = objectData.MovementUpdate.Rotation;
            }
        }
        #endregion ImprovedTerseObjectUpdate

        #region FullUpdate
        protected virtual void AddObject(ObjectUpdateMessage.ObjectData objectData, RegionHandle regionHandle)
        {
            if (objectData.PCode != PCode.LEGACY_AVATAR)
            {
                return;
            }

            ViewerObjectPlaceholder placeholder = Placeholders.InstantiateTemplate();
            GameObjectGoByFullId[objectData.FullId] = placeholder.gameObject;
            FullIdByGameObject[placeholder.gameObject] = objectData.FullId;

            ObjectCount++;

            if (GameObjectByRegionAndId.ContainsKey(regionHandle) == false)
            {
                GameObjectByRegionAndId[regionHandle] = new Dictionary<uint, GameObject>();
                RegionCount++;
            }
            GameObjectByRegionAndId[regionHandle][objectData.LocalId] = placeholder.gameObject;

            UpdateObject(objectData, placeholder.gameObject, regionHandle);
        }

        protected virtual void UpdateObject(ObjectUpdateMessage.ObjectData objectData, RegionHandle regionHandle)
        {
            UpdateObject(objectData, GameObjectGoByFullId[objectData.FullId], regionHandle);
        }
        
        protected virtual void UpdateObject(ObjectUpdateMessage.ObjectData objectData, GameObject go, RegionHandle regionHandle)
        {
            go.GetComponent<ToolTipTarget>().Text = $"{objectData.PCode}\n{objectData.FullId}\n{objectData.NameValue}";

            if (   objectData.ParentId != 0
                && GameObjectByRegionAndId.ContainsKey(regionHandle)
                && GameObjectByRegionAndId[regionHandle].ContainsKey(objectData.ParentId))
            {
                if (GameObjectByRegionAndId[regionHandle][objectData.ParentId].transform != go.transform.parent)
                {
                    go.transform.SetParent(GameObjectByRegionAndId[regionHandle][objectData.ParentId].transform);
                }
            }

            if (objectData.MovementUpdate != null)
            {
                go.transform.localPosition = objectData.MovementUpdate.Position;
                go.transform.localRotation =  objectData.MovementUpdate.Rotation;
            }

            go.transform.localScale = objectData.Scale;

            ViewerObjectPlaceholder placeholder = go.GetComponent<ViewerObjectPlaceholder>();
            switch (objectData.PCode)
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
        #endregion FullUpdate

        protected void OnLogout()
        {
            foreach (GameObject go in GameObjectGoByFullId.Values)
            {
                Destroy(go);
            }

            GameObjectGoByFullId.Clear();
        }

        [Serializable] public class PlaceholderTemplate : Template<ViewerObjectPlaceholder> { }
    }
}
