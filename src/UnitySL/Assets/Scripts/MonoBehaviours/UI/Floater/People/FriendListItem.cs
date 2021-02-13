using Assets.Scripts.Agents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MonoBehaviours.UI.Floater.People
{
    public class FriendListItem : MonoBehaviour
    {
        [SerializeField] protected Color OnlineTextColour;
        [SerializeField] protected Color OfflineTextColour;
        [SerializeField] protected TMP_Text NameText;
        [SerializeField] protected Toggle OnLineToPermissionToggle;
        [SerializeField] protected Toggle MapToPermissionToggle;
        [SerializeField] protected Toggle ModifyToPermissionToggle;
        [SerializeField] protected Toggle OnLineFromPermissionToggle;
        [SerializeField] protected Toggle MapFromPermissionToggle;
        [SerializeField] protected Toggle ModifyFromPermissionToggle;

        public void Set (string name, Relationship relationship)
        {
            NameText.text = name;
            NameText.fontStyle = relationship.IsOnline ? FontStyles.Bold  : FontStyles.Normal;
            NameText.color     = relationship.IsOnline ? OnlineTextColour : OfflineTextColour;

            OnLineToPermissionToggle.isOn   = relationship.IsRightGrantedTo(Relationship.Rights.OnlineStatus);
            MapToPermissionToggle.isOn      = relationship.IsRightGrantedTo(Relationship.Rights.MapLocation);
            ModifyToPermissionToggle.isOn   = relationship.IsRightGrantedTo(Relationship.Rights.ModifyObjects);

            OnLineFromPermissionToggle.isOn = relationship.IsRightGrantedFrom(Relationship.Rights.OnlineStatus);
            MapFromPermissionToggle.isOn    = relationship.IsRightGrantedFrom(Relationship.Rights.MapLocation);
            ModifyFromPermissionToggle.isOn = relationship.IsRightGrantedFrom(Relationship.Rights.ModifyObjects);
        }

        public void SetName(string name)
        {
            NameText.text = name;
        }
    }
}
