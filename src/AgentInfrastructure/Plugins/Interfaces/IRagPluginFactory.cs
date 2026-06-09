namespace AgentInfrastructure.Plugins.Interfaces
{
    public interface IRagPluginFactory
    {
        IRAGPlugin Create(string indexKey);
    }
}
