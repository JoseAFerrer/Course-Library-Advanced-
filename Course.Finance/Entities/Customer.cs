using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<FinanceCourse> CoursesBought { get; set; }
        public IEnumerable<Transaction> PastTransactions { get; set; }
        public DateTimeOffset LastActiveOn { get; set; }
    }

}
