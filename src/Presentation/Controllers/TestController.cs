using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
 
 [ApiController]
 [Route("api/test")]
 public class TestController : ControllerBase
 {
     private readonly IMediator _mediator;
 
     public TestController(IMediator mediator)
     {
         _mediator = mediator;
     }
 
     [HttpGet("{id}")]
     public async Task<IActionResult> GetTest(int id)
     {
         var result = await _mediator.Send(new GetTestQuery(id));
         return Ok(result);
     }
 }