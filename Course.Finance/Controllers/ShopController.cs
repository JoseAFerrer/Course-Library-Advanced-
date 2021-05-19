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
    [Route("api/shop")] //Lo óptimo sería que cada controlador solo se encargara de un "tipo" de tarea o una entidad y sus satélites.
    // No hace falta cambiarlo pero sería lo suyo que cada uno se centrara en un grupo de cosas. TransactionController, FinanceCourse.
    // El controlador sirve recursos para _esa_ entidad (y posiblemente sus satélites).
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
                .OfType<Transaction>().ToListAsync(); 

            return Ok(pastTransactions);
        }

        [HttpPost(Name ="BuyCourses")]
        public async Task<ActionResult<Transaction>> BuyCourses(IEnumerable<Guid> coursesIds, Guid buyerId)
        {
            using var session = _documentStore.OpenAsyncSession();

            var buyingCourses = await session
                .Query<FinanceCourse>()
                .OfType<FinanceCourse>()
                .Where(x => coursesIds.Contains(x.Id) ) //Sácame un curso solo si esta lista de ids contiene su id. Does it work?
                .ToListAsync();

            //Obtener los cursos sencillitos de la base de datos. No parece muy óptimo pero no se me ocurre otra manera de momento.
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                BuyerId = buyerId,
                BoughtCourses = buyingCourses,
                TransactionTime = DateTimeOffset.UtcNow
            };
            transaction.TotalValue = transaction.TotalPrice(); //Should this method also be async?

            await session.StoreAsync(transaction); //No hay return type?

            return Ok();
        }

    }
}
