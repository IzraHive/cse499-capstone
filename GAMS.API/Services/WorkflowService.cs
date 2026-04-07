namespace GAMS.API.Services;
public class WorkflowService : IWorkflowService
{
    private static readonly Dictionary<string, (string from, string to)[]> Rules = new()
    {
        ["SocialWorker"] = new[]
        {
            ("Submitted",    "Under Review"),
            ("Under Review", "Submitted to Head Office")
        },
        ["Admin"] = new[]
        {
            ("Submitted",                "Under Review"),
            ("Submitted to Head Office", "Under Head Office Review"),
            ("Under Head Office Review", "Approved"),
            ("Under Head Office Review", "Declined")
        },
        ["Finance"] = new[]
        {
            ("Approved", "Payment Issued")
        }
    };

    public bool IsValidTransition(string current, string next, string role)
    {
        if (!Rules.TryGetValue(role, out var list)) return false;
        return list.Any(r => r.from == current && r.to == next);
    }
}