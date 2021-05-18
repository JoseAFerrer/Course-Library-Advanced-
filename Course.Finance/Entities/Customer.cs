using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class Customer //Podría ser un resumen de la información contenida en un User.
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<FinanceCourse> CoursesBought { get; set; }
        // public IEnumerable<Transaction> PastTransactions { get; set; } //Esto no es necesario, se puede conseguir mediante queries.
        //Implementar dichas queries cuando se pueda.
        public DateTimeOffset LastActiveOn { get; set; }
    }

}
