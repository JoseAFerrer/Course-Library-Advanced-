using Course.Finance.Entities;
using System;
using System.Collections.Generic;

namespace Course.Finance.Services
{
    public interface IFinanceRepository
    {
        Transaction BuyCourses(IEnumerable<Guid> buyingCourses, Guid buyerId);
        IEnumerable<FinanceCourse> GetCourses();
        IEnumerable<FinanceCourse> CheckBoughtCourses(Customer customer);
        IEnumerable<Transaction> CheckTransactionsForCustomer(Customer customer);
        void DeleteEveryTransaction(Customer customer);

    }
}