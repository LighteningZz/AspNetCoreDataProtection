using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreDataProtection.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/Todo")]
    public class ApiTodoController : Controller
    {
        public class TodoForm
        {
            [Required]
            [StringLength(70)]
            public string FirstName { get; set; }
            [Required]
            [StringLength(70)]
            public string LastName { get; set; }
            [Range(0, 999)]
            public int Age { get; set; }
        }

        [HttpPost("Example")]
        public IActionResult Example(TodoForm form)
        {

            if (!ModelState.IsValid)
            {
                StatusCode(400);
                return Json(new { Error = "Model not valid." });
            }

            return Json(new { Success = "Ok" });
        }


    }
}