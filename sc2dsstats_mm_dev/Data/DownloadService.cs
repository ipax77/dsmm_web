using dsmm_server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sc2dsstats_mm_dev.Data
{
    [ApiController]
    public class DownloadService : ControllerBase
    {
        private readonly ReportService _repserv;
        Regex regexItem = new Regex("^[a-zA-Z0-9._]*$");

        public DownloadService(ReportService repserv)
        {
            _repserv = repserv;
        }

        [HttpGet("download/{replayID}")]
        public IActionResult GetFile(string replayID)
        {
            if (replayID.Contains(".."))
                return (IActionResult)NotFound();
            if (!replayID.Any( ch => Char.IsLetterOrDigit( ch ) || ch == '_'))
                return (IActionResult)NotFound();
            if (replayID.Length > 100)
                return (IActionResult)NotFound();

            if (regexItem.IsMatch(replayID))
            {
                string myfile = "replays/" + replayID;
                return _repserv.GetFileAsStream(myfile) ?? (IActionResult)NotFound();
            } else
                return (IActionResult)NotFound();
        }
    }
}
