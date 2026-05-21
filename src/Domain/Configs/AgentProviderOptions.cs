namespace Domain.Configs
{
    public class AgentProviderOptions
    {
        /// <summary>Azure AI Foundry / Azure OpenAI endpoint URL. Required for AzureOpenAI provider.</summary>
        public string Endpoint { get; set; }
    }
}
