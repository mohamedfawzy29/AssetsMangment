using AssetsMangment.Data;
using AssetsMangment.DTOs;
using AssetsMangment.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetsMangment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        public AssetsController(ApplicationDbContext context)
        {
            This.context = context;
        }
    }
}