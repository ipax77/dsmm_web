using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DSmm;
using Microsoft.Extensions.Options;

namespace DSmm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly AppConfig _config;

        public VersionController(IOptions<AppConfig> options)
        {
            _config = options.Value;
        }
        // ../api/version
        [HttpGet]
        public string Version()
        {
            return _config.Version;
        }
    }
}