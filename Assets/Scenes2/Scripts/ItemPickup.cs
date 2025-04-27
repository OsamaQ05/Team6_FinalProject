using Unity.FPS.Game;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

namespace Unity.FPS.Gameplay
{
    public class PlayerItemCarrier : MonoBehaviour
    {
        [Tooltip("Whether the player is currently carrying an item")]
        public bool IsCarryingItem { get; private set; } = false;
        
        [Tooltip("Reference to the TextMeshPro UI element")]
        public TextMeshProUGUI messageText;
        
        [Tooltip("How long to display messages (in seconds)")]
        public float messageDisplayTime = 3f;
        
        private float messageTimer = 0f;
        
        private void Start()
        {
            // Clear any initial text
            if (messageText != null)
            {
                messageText.text = "";
            }
        }
        
        public void PickupItem()
        {
            IsCarryingItem = true;
        }
        
        public void DropItem()
        {
            IsCarryingItem = false;
        }
        
        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageTimer = messageDisplayTime;
            }
        }
        
        private void Update()
        {
            // Handle timer for showing message
            if (messageTimer > 0)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0 && messageText != null)
                {
                    messageText.text = "";
                }
            }
        }
    }
    
    public class ItemPickup : Pickup
    {
        [Header("Item Parameters")]
        [Tooltip("Message to show when player can't pick up the item")]
        public string cannotPickupMessage = "You already have an Item, return it to its proper place before picking this one up.";
        
        protected override void OnPicked(PlayerCharacterController player)
        {
            // Try to get the PlayerItemCarrier component
            PlayerItemCarrier itemCarrier = player.GetComponent<PlayerItemCarrier>();
            
            // If player doesn't have the component or is not carrying an item
            if (itemCarrier == null || !itemCarrier.IsCarryingItem)
            {
                // Allow pickup
                if (itemCarrier != null)
                {
                    itemCarrier.PickupItem();
                }
                
                PlayPickupFeedback();
                Destroy(gameObject);
            }
            else
            {
                // Player is already carrying an item, show message
                itemCarrier.ShowMessage(cannotPickupMessage);
            }
        }
    }
}