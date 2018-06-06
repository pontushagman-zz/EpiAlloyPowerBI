using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EpiAlloyPowerBI.Models.Blocks
{
    [ContentType(DisplayName = "PowerBI Block", GUID = "77bf9e39-8bd5-42cd-80b0-286551d5815e", Description = "Used to embed a report from Microsoft Power BI")]
    [ImageUrl("~/Static/gfx/PowerBI_Logo.png")]
    public class PowerBIBlock : SiteBlockData
    {

        [Display(Name = "Report ID", GroupName = SystemTabNames.Content, Order = 100)]
        public virtual string ReportId { get; set; }

        //[Display(Name = "Client ID", GroupName = "Advanced", Order = 100)]
        //public virtual string ClientId { get; set; }

        //[Display(Name = "Group ID", GroupName = "Advanced", Order = 200)]
        //public virtual string GroupId { get; set; }

        //[Display(Name = "Power BI username", GroupName = "Advanced", Order = 300)]
        //public virtual string PbiUsername { get; set; }

        //[Display(Name = "Power BI password", GroupName = "Advanced", Order = 400)]
        //public virtual string PbiPassword { get; set; }

    }
}