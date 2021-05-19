using Course.Finance.Entities;
using CourseLibrary.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Services
{
    public class FinanceRepository : IFinanceRepository
    {
        private readonly FinanceCourse MockCourse = new FinanceCourse
        {
            Title = "Potatoes",
            Id = Guid.NewGuid().ToString()
    };
        private readonly RavenDbCoursesRepository _ravenRepo;
        public Customer MockCustomer = new Customer
        {
            FirstName = "John",
            LastName = "McJonas",
            LastActiveOn = DateTimeOffset.UtcNow
        };

        public FinanceRepository(RavenDbCoursesRepository ravenRepo)
        {
            _ravenRepo = ravenRepo;
        }

        public IEnumerable<FinanceCourse> GetCourses()
        {
            var listedcourses = new List<FinanceCourse>() { MockCourse };

            //Something like...
            // var listedcourses =_ravenRepo.GetCourses(authorId);

            return listedcourses; //Aquí solo querríamos mostrar los cursos resumidos; ShortCourse
        }

        public Transaction BuyCourses(IEnumerable<Guid> buyingCoursesId, Guid buyerId)
        {
            var buyingCourses = new List<FinanceCourse>() { MockCourse };

            Transaction currentOp = new Transaction()
            {
                BuyerId = buyerId.ToString(),
                TransactionTime = DateTimeOffset.UtcNow,
                BoughtCourses = buyingCourses
            };

            //Algo que añada al historial del cliente el curso que ha comprado
            // y lo guarde en la base de datos (!)


            return currentOp;
        }

        public IEnumerable<FinanceCourse> CheckBoughtCourses(Customer customer)
        {//Es un poco innecesario pasar por aquí, ¿no?
         //Bueno, a no ser que tengas que acceder a la base de datos, entonces todo tiene sentido.
         //Pero habría que pasar el Id entonces, ¿no?
            return customer.CoursesBought;
        }

        public IEnumerable<Transaction> CheckTransactionsForCustomer(Customer customer) //Pedir por Id
        {
            //return customer.PastTransactions;
            throw new NotImplementedException();
        }

        public void DeleteEveryTransaction(Customer customer)
        {
            //customer.PastTransactions = null;
            throw new NotImplementedException();
        }
    }
}
