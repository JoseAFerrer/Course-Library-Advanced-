using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Course.Finance.Entities
{
    public class Transaction
    {
        public string Id { get; set; }
        public string BuyerId { get; set; } //Solo utilizamos el Id del comprador, no cargamos mucha información.
        public IEnumerable<FinanceCourse> BoughtCourses { get; set; } //Guardamos los cursos simplificados.
        
        [JsonIgnore] //Lo que esto hace es ignorar este campo al persistir en la base de datos.
        public double TotalValue { get
            {
                double Price = 0;
                foreach (var course in BoughtCourses)
                {
                    Price += course.Price;
                }
                return Price * 1.21;
            } }  //Propiedad calculable.
        public DateTimeOffset TransactionTime { get; set; }
    }
}
