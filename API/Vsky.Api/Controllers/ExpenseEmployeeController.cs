using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using Vsky.Api.ApiErrors;
using Vsky.Data;


namespace Vsky.Api.Controllers
{

    [Route("expense-employee")]
    public class ExpenseEmployeeController : BaseController
    {
        #region Fields

        private readonly IMapper _mapper; //This is a dependency injection for an object that helps with converting data between different formats, like mapping a database model to a view model       
        private readonly ApplicationDbContext _db; //this is database context used to interact with db - saving / retriving cus data

        #endregion

        #region Ctor

        public ExpenseEmployeeController(IMapper mapper, ApplicationDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }

        #endregion        

        #region Employee

        ////[HttpGet("GetEmployeesAsSelectItemList")]
        //[HttpGet("/activedropdown/list")]
        //public IActionResult GetEmployeesAsSelectItemList()
        //{
        //    try
        //    {
        //        var entity = _expenseEmployeeService.GetEmployeeSelectItemList();
        //        if (entity == null)
        //        {
        //            return BadRequest(new BadRequestError("No employee found"));
        //        }
        //        return Ok(entity);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}
        #endregion
    }

}
