using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendPlayground.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Trainee.Catalogue.Abstraction;
using BackendPlayground.ViewModels.TestViewModels;
using Trainee.Business.DAL.Repositories;
using Trainee.Business.Business;
using Trainee.Business.Business.Wrappers;
using Trainee.Business.Business.Enums;
using Newtonsoft.Json;

namespace BackendPlayground.Controllers
{
    public class TestController : Controller
    {
        IHostingEnvironment _env;
        ICategoryRepository _rep;
        BusinessService _serv;
        public TestController(IHostingEnvironment env, ICategoryRepository categoryRep,BusinessService serv)
        {
            _serv = serv;
            _env = env;
            _rep = categoryRep;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FileUp()
        {
            var model = new FileUp();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> FileUp(FileUp model)
        {
            if (ModelState.IsValid)
            {
                using (var stream = new FileStream(_env.WebRootPath + "/" + model.File.FileName, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

            }
            return View(model);

        }
        [HttpGet]
        public IActionResult CheckBoxTest()
        {
            return View();
        }
        //[HttpGet("test/product/{name}")]
        //public IActionResult
        
        [HttpGet]
        public IActionResult CategoryTest()
        {var repo = new ProductRatingRepository("Server=DEVSQL_STAZ\\DEV_STAZ;Database=group2;Trusted_Connection=True;");
            var rand = new Random();
            List<int> idList = new List<int>();
            for (int i = 0; i < 500; i++)
            {
                idList.Add(rand.Next(1, 999));
            }
            var test = repo.GetRatings().Where(r => idList.Contains(r.ProductId));
            return View();
            
            
        }
        [HttpGet("/Test/ProductTest/{id}")]
        public IActionResult ProductTest(int id)
        {
           // QueryParametersWrapper filter = new QueryParametersWrapper() {SortingType=SortType.Asc,SortingParameter=SortingParameter.Name,PageSize=5,CategoryId=5,PageNum=1 };
            var res = _serv.GetProduct(id).data;
            
            return View();

        }


    }
}