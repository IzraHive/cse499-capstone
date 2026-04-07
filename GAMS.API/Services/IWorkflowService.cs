namespace GAMS.API.Services;
public interface IWorkflowService
{
    bool IsValidTransition(string currentStatus, string newStatus, string userRole);
}