using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class Transaction
    {
        //public Transaction(Guid buyerId)
        //{
        //    Id = Guid.NewGuid();
        //    BuyerId = buyerId;
        //}
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; } //Solo utilizamos el Id del comprador, no cargamos mucha información.
        public IEnumerable<FinanceCourse> BoughtCourses { get; set; } //Guardamos los cursos simpli.
        public double TotalValue { get; set; } 
        public DateTimeOffset TransactionTime { get; set; }

        public double TotalPrice()
        {
            double Price = 0;

            foreach (var course in BoughtCourses)
            {
                Price += course.Price;
            }
            Price = Price* 1.21;
            var TotalValue = Price;
            return TotalValue;
        }
    }
}
