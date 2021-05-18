using Course.Finance.Entities;
using Course.Finance.Services;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
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
        private readonly IDocumentStore _documentStore;

        public ShopController(FinanceRepository financeRepo, IDocumentStore documentStore)
        {
            _financeRepo = financeRepo;
            _documentStore = documentStore;
        }

        //////////////////////////////////////////
        [HttpGet(Name = "BuyableCourses")]
        public async Task<ActionResult<IEnumerable<FinanceCourse>>> GetBuyableCourses()
        {
            // var coursesFromDb = _financeRepo.GetCourses();
            using var session = _documentStore.OpenAsyncSession();

            var allCourses = await session.Query<FinanceCourse>().OfType<FinanceCourse>().ToListAsync(); 
            //El OfType es por si antes has utilizado un índice, al especificar la Query, que es lo más normal (porque lo hace todo mucho más eficiente).

            return Ok(allCourses);
        }

        [HttpGet(Name = "Transactions/{Id}")] //Utilizamos simplemente el nombre del recurso, la acción se intuye por el verbo Rest.
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionHistory(Guid Id) //Pasar el Id
        {
            // var pastTransactions = _financeRepo.CheckTransactionsForCustomer(Id);
            using var session = _documentStore.OpenAsyncSession();

            var pastTransactions = await session.Query<Transaction>()
                .Where(x => x.BuyerId == Id)
                .OfType<Transaction>().ToListAsync(); //El OfType es por si antes has utilizado un índice, al especificar la Query

            return Ok(pastTransactions);
        }

        [HttpPost(Name ="BuyCourses")]
        public ActionResult<Transaction> BuyCourses(IEnumerable<Guid> coursesIds, Guid buyerId)
        {
            var currentOp = _financeRepo.BuyCourses(coursesIds, buyerId);

            return Ok(currentOp);
        }

    }
}
