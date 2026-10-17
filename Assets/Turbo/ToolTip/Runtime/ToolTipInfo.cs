using UnityEngine;
using UnityEngine.EventSystems;

namespace Turbo.ToolTip
{
    public sealed class ToolTipInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string toolTipText;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipScreenSpaceUI.Instance == null)
                return;
            
            if (eventData.pointerCurrentRaycast.gameObject != gameObject)
                return;

            TooltipScreenSpaceUI.ShowTooltip_Static(GetTooltipText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipScreenSpaceUI.Instance == null)
                return;

            TooltipScreenSpaceUI.HideTooltip_Static();
        }
        
        private void OnDisable()
        {
            if (TooltipScreenSpaceUI.Instance == null)
                return;

            TooltipScreenSpaceUI.HideTooltip_Static();
        }

        private void OnDestroy()
        {
            if (TooltipScreenSpaceUI.Instance == null)
                return;

            TooltipScreenSpaceUI.HideTooltip_Static();
        }
        
        public void SetToolTipText(string newToolTipText)
        {
            toolTipText = newToolTipText;
        }

        private string GetTooltipText()
        {
            return toolTipText;
        }
    }
}

