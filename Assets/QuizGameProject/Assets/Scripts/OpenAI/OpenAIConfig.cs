using UnityEngine;

[CreateAssetMenu(fileName = "OpenAIConfig", menuName = "OpenAI/Configuration")]
public class OpenAIConfig : ScriptableObject
{
    public string ApiKey => apiKey;
} 