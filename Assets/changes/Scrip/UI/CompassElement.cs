using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class CompassElement : MonoBehaviour
    {
        [Tooltip("The marker on the compass for this element")]
        public CompassMarker CompassMarkerPrefab;

        [Tooltip("Text override for the marker, if it's a direction")]
        public string TextDirection;

        Compass m_Compass;
        bool m_IsRegistered = false;

        void Awake()
        {
            m_Compass = FindFirstObjectByType<Compass>();
            DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(m_Compass, this);

            var markerInstance = Instantiate(CompassMarkerPrefab);

            markerInstance.Initialize(this, TextDirection);
            
            // Register with the compass
            m_Compass.RegisterCompassElement(transform, markerInstance);
            m_IsRegistered = true;
        }

        void OnEnable()
        {
            // Update visibility when object becomes active
            if (m_Compass != null && m_IsRegistered)
            {
                m_Compass.UpdateCompassMarkerVisibility(transform);
            }
        }

        void OnDisable()
        {
            // Update visibility when object becomes inactive
            if (m_Compass != null && m_IsRegistered)
            {
                m_Compass.UpdateCompassMarkerVisibility(transform);
            }
        }

        void OnDestroy()
        {
            if (m_Compass != null && m_IsRegistered)
            {
                m_Compass.UnregisterCompassElement(transform);
                m_IsRegistered = false;
            }
        }
    }
}