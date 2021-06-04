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
        } //Todo: crear una nueva base de datos exclusiva para Finance.

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

        [HttpGet(Name = "Transactions/{Id}")] //Utilizamos simplemente el nombre del recurso, la acción se intuye por el verbo http.
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionHistory(Guid guidId) //Pasar el Id
        {
            string Id = guidId.ToString();

            using var session = _documentStore.OpenAsyncSession();

            var pastTransactions = await session.Query<Transaction>()
                .Where(x => x.BuyerId == Id)
                .OfType<Transaction>().ToListAsync(); 

            return Ok(pastTransactions);
        }

        [HttpPost(Name ="BuyCourses")]
        public async Task<ActionResult<Transaction>> BuyCourses(IEnumerable<Guid> guidCoursesIds, Guid guidBuyerId)
        {
            var coursesIds = new List<string>();
            foreach (var id in guidCoursesIds) //No estoy seguro de que esto sea lo más eficiente, pero funcionar debería funcionar.
            {//¿Y si la entrada del método fuera una string? ¿O eso es problemático porque dejaría entrar cosas que tal vez no son Guids?
                coursesIds.Add(id.ToString());
            }
            var buyerId = guidBuyerId.ToString();

            using var session = _documentStore.OpenAsyncSession();

            var buyingCourses = await session
                .Query<FinanceCourse>()
                .OfType<FinanceCourse>()
                .Where(x => coursesIds.Contains(x.Id)) //Sácame un curso solo si esta lista de ids contiene su id. Does it work?
                .ToListAsync();

            //Obtener los cursos sencillitos de la base de datos
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                BuyerId = buyerId,
                BoughtCourses = buyingCourses,
                TransactionTime = DateTimeOffset.UtcNow
            };

            await session.StoreAsync(transaction); //No hay return type?
            await session.SaveChangesAsync();

            return Ok(transaction);
        }

    }
}
