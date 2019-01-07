using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    public class RootController : Controller
    {
        // GET api/values
        [Route("~/")]
        [HttpGet]
        [Produces(ContentTypes.Root)]
        [ProducesResponseType(typeof(IDictionary<ApplicationRelationship, DocumentLink>), 200)]
        public IDictionary<ApplicationRelationship, DocumentLink> Get()
        {
            return new Dictionary<ApplicationRelationship, DocumentLink>()
            {  
                { 
                    ApplicationRelationship.Timesheets, new DocumentLink() 
                    { 
                        Method = Method.Get,
                        Type = ContentTypes.Timesheets,
                        Relationship = DocumentRelationship.Timesheets,
                        Reference = "/timesheets"
                    }   
                }
            };
        }
    }
}
