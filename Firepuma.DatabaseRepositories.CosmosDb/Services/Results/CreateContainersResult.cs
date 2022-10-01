namespace Firepuma.DatabaseRepositories.CosmosDb.Services.Results;

public class CreateContainersResult
{
    public List<SuccessfulContainerSummary> SuccessfulContainers { get; set; }
    public List<FailedContainerSummary> FailedContainers { get; set; }

    public class SuccessfulContainerSummary
    {
        public string ContainerId { get; set; }

        public SuccessfulContainerSummary(string containerId)
        {
            ContainerId = containerId;
        }
    }

    public class FailedContainerSummary
    {
        public string ContainerId { get; set; }
        public string ErrorMessage { get; set; }

        public FailedContainerSummary(string containerId, string errorMessage)
        {
            ContainerId = containerId;
            ErrorMessage = errorMessage;
        }
    }
}