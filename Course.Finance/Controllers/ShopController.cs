using Course.Finance.Entities;
using Course.Finance.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Controllers
{
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        private readonly FinanceRepository _financeRepo;

        public ShopController(FinanceRepository financeRepo)
        {
            _financeRepo = financeRepo;
        }

        //////////////////////////////////////////
        [HttpGet(Name = "GetBuyableCourses")]
        public ActionResult<IEnumerable<FinanceCourse>> GetBuyableCourses()
        {
            var coursesFromDb = _financeRepo.GetCourses();

            return Ok(coursesFromDb);
        }

        [HttpGet(Name = "GetTransactions")]
        public ActionResult<IEnumerable<FinanceCourse>> GetTransactionHistory(Customer customer)
        {
            var pastTransactions = _financeRepo.CheckBoughtCourses(customer);

            return Ok(pastTransactions);
        }

        [HttpPost(Name ="BuyCourses")]
        public ActionResult<Transaction> BuyCourses(IEnumerable<Guid> coursesIds, string customerName)
        {
            var currentOp = _financeRepo.BuyCourses(coursesIds, customerName);

            return Ok(currentOp);
        }

    }
}
