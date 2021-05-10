using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Customer Buyer { get; set; }
        public IEnumerable<FinanceCourse> BoughtCourses { get; set; }
        public double TotalValue = 0.00;
        public DateTimeOffset TransactionTime { get; set; }
    }
}
