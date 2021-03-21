using System;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.UI
{
    public class FloaterManager : MonoBehaviour
    {
        public enum FloaterType
        {
            People,
            Inventory,
            MiniMap,
            Chat,
            Search,
            SearchMini,
            DebugLog
        }

        public static FloaterManager Instance;

        [SerializeField] protected GameObject PeopleFloater;
        [SerializeField] protected GameObject InventoryFloater;
        [SerializeField] protected GameObject MiniMapFloater;
        [SerializeField] protected GameObject ChatFloater;
        [SerializeField] protected GameObject SearchFloater;
        [SerializeField] protected GameObject SearchMiniFloater;
        [SerializeField] protected GameObject DebugLogFloater;
        
        public void SetFloaterVisible (FloaterType type, bool isVisible)
        {
            GameObject floater = FloaterByType(type);
            if (floater != null)
            {
                floater.SetActive (isVisible);
                if (isVisible == true)
                {
                    floater.transform.SetAsLastSibling();
                }
            }
        }

        public void ToggleFloaterVisibility(string typeName)
        {
            FloaterType type = (FloaterType)Enum.Parse(typeof(FloaterType), typeName);
            ToggleFloaterVisibility(type);
        }

        public void ToggleFloaterVisibility(FloaterType type)
        {
            GameObject floater = FloaterByType(type);
            if (floater != null)
            {
                SetFloaterVisible(type, !floater.activeSelf);
            }
        }

        protected GameObject FloaterByType(FloaterType type)
        {
            switch (type)
            {
                case FloaterType.People:
                    return PeopleFloater;

                case FloaterType.Inventory:
                    return InventoryFloater;

                case FloaterType.MiniMap:
                    return MiniMapFloater;

                case FloaterType.Chat:
                    return ChatFloater;

                case FloaterType.Search:
                    return SearchFloater;

                case FloaterType.SearchMini:
                    return SearchMiniFloater;

                case FloaterType.DebugLog:
                    return DebugLogFloater;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void Start()
        {
            if (Instance != null && Instance != this)
            {
                Logger.LogError($"FloaterManager: More than one instance in scene, disabling {gameObject.name}.");
                enabled = false;
                return;
            }

            Instance = this;
        }
    }
}
