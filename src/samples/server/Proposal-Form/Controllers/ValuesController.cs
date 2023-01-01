using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Proposal.Form.Controllers
{
    public record ResponseModel(bool IsSuccess,string Message);
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Route("Trigger")]
        public async Task<IActionResult> Trigger()
        {
            return new JsonResult(new ResponseModel(true, "Signal triggered :)"));
        }

        [HttpPost]
        [Route("karaliste-sorgusu")]
        public async Task<JsonResult> KaralisteSorgusu([FromBody] object datas)
        {
            return new JsonResult(new ResponseModel(true,"Kara liste sorgusu yapıldı" ));
        }
        [HttpPost]
        [Route("KRM-entegrasyonu")]
        public async Task<JsonResult> KRMEntegrasyonu([FromBody] object datas)
        {
            return new JsonResult(new ResponseModel(true, "KRM Entegrasyonu"));
        }

        [HttpPost]
        [Route("karaagaci-sorgula")]
        //public async Task<JsonResult> KaraagaciSorgula([FromBody] object datas)
        public async Task<JsonResult> KaraagaciSorgula()
        {
            return new JsonResult(new ResponseModel(true, "Karaagaci Sorgula"));
        }

        [HttpGet]
        [Route("CreateProposal")]
        public async Task<JsonResult> CreateProposal()
        {
            return new JsonResult(new ResponseModel(true, "teklif oluşturuldu"));
        }

        [HttpPost]
        [Route("CustomerApproval")]
        public async Task<JsonResult> CustomerApproval([FromBody] object datas)
        {
            return new JsonResult(new ResponseModel(true, "teklif oluşturuldu"));
        }
        
    }
}
