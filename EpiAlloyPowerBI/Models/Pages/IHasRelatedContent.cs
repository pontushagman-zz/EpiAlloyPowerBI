using EPiServer.Core;

namespace EpiAlloyPowerBI.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
