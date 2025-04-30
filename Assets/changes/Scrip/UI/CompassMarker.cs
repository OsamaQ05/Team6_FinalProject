using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CompassMarker : MonoBehaviour
    {
        [Tooltip("Main marker image")] public Image MainImage;

        [Tooltip("Canvas group for the marker")]
        public CanvasGroup CanvasGroup;

        [Header("Enemy element")] [Tooltip("Default color for the marker")]
        public Color DefaultColor;

        [Tooltip("Alternative color for the marker")]
        public Color AltColor;

        [Header("Direction element")] [Tooltip("Use this marker as a magnetic direction")]
        public bool IsDirection;

        [Tooltip("Text content for the direction")]
        public TMPro.TextMeshProUGUI TextContent;

        EnemyController m_EnemyController;
        CompassElement m_CompassElement;

        public void Initialize(CompassElement compassElement, string textDirection)
        {
            m_CompassElement = compassElement;
            
            if (IsDirection && TextContent)
            {
                TextContent.text = textDirection;
            }
            else
            {
                m_EnemyController = compassElement.transform.GetComponent<EnemyController>();

                if (m_EnemyController)
                {
                    m_EnemyController.onDetectedTarget += DetectTarget;
                    m_EnemyController.onLostTarget += LostTarget;

                    LostTarget();
                }
            }
            
            // Set initial visibility based on compass element's active state
            UpdateVisibility();
        }

        public void DetectTarget()
        {
            MainImage.color = AltColor;
        }

        public void LostTarget()
        {
            MainImage.color = DefaultColor;
        }
        
        public void UpdateVisibility()
        {
            // Only apply visibility changes for non-direction markers that have CanvasGroup
            if (!IsDirection && CanvasGroup != null && m_CompassElement != null)
            {
                CanvasGroup.alpha = m_CompassElement.gameObject.activeInHierarchy ? 1f : 0f;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (m_EnemyController != null)
            {
                m_EnemyController.onDetectedTarget -= DetectTarget;
                m_EnemyController.onLostTarget -= LostTarget;
            }
        }
    }
}