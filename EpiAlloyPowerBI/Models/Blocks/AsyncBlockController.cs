using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc;

namespace EpiAlloyPowerBI.Models.Blocks
{
    public abstract class AsyncBlockController<TContentData> : ActionControllerBase, IRenderTemplate<TContentData>
        where TContentData : IContentData
    {
        public abstract Task<ActionResult> Index(TContentData currentContent);
    }
}