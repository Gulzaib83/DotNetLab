using Microsoft.AspNetCore.Authorization;

namespace LabAPI.Filters
{
    public class ResourceOwnershipRequirement : IAuthorizationRequirement
    {
        public string ResourceId { get; }

        public ResourceOwnershipRequirement(string resourceId)
        {
            ResourceId = resourceId;
        }

        public ResourceOwnershipRequirement()
        {
        }
    }
}
