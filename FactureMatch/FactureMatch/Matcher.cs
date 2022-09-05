using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FactureMatch
{
    public class Matcher
    {
        public static DataTable tableAmounts = new DataTable();
        public static DataTable tableTargetAmount = new DataTable();

        private static List<Reference> referenceList = new List<Reference>();
        private static List<Target> targetList = new List<Target>();

        private static List<Result> responseList = new List<Result>();


        public void GenerateDummyTable()
        {
            tableAmounts.Columns.Add("ID", typeof(int));
            tableAmounts.Columns.Add("Monto", typeof(decimal));
            tableAmounts.Columns.Add("Producto", typeof(string));
            tableAmounts.Columns.Add("FormaPago", typeof(string));



            tableAmounts.Rows.Add(1, 10, "Magna", "Tarjeta");
            tableAmounts.Rows.Add(2, 12, "Magna", "Tarjeta");
            tableAmounts.Rows.Add(3, 5, "Magna", "Tarjeta");
            tableAmounts.Rows.Add(4, 15, "Magna", "Transferencia");
            tableAmounts.Rows.Add(5, 10, "Magna", "Transferencia");
            tableAmounts.Rows.Add(6, 2, "Magna", "Transferencia");
            tableAmounts.Rows.Add(7, 8, "Magna", "Transferencia");
            tableAmounts.Rows.Add(8, 20, "Magna", "Efectivo");
            tableAmounts.Rows.Add(9, 11, "Magna", "Efectivo");
            tableAmounts.Rows.Add(10, 10, "Diesel", "Tareta");
            tableAmounts.Rows.Add(11, 12, "Diesel", "Efectivo");


            tableTargetAmount.Columns.Add("ID", typeof(int));
            tableTargetAmount.Columns.Add("Monto", typeof(decimal));
            tableTargetAmount.Columns.Add("Producto", typeof(string));
            tableTargetAmount.Columns.Add("MetodoPago", typeof(string));

            tableTargetAmount.Rows.Add(1, 38, "Magna", "Tarjeta");
            tableTargetAmount.Rows.Add(2, 35, "Magna", "Transferencia");
            tableTargetAmount.Rows.Add(3, 20, "Magna", "Efectivo");
            tableTargetAmount.Rows.Add(4, 22, "Diesel", "Efectivo");

            //  ISAAC EXAMPLE::
            /*
             * TABLE 1: (Precio, Producto, FormaPago)
                1, 10, Magna, Tarjeta 1
                2, 12, Magna, Tarjeta 1 22
                3, 5, Magna, Tarjeta  1 27
                4, 15, Magna, Transferencia 2
                5, 10, Magna, Transferencia 2
                6, 2, Magna, Transferencia 2
                7, 8, Magna, Transferencia 2 35
                8, 20, Magna, Efectivo 3 20
                9, 12, Magna, Efectivo 1 38
                10, 10, Diesel, Tarjeta  4 22
                11, 12, Diesel, Efectivo 4 12

                TABLE 2: (Id, Precio, Producto, FormaPago)
                1, 38, Magna, Tarjeta cumplio 11
                2, 35, Magna, Transferencia cumplio
                3, 20, Magna, Efectivo cumplio
                4, 22, Diesel, Efectivo 10

                TABLE 3: Resultado (Clon primera tabla + Id segunda tabla) OUTPUT::: 
                1, 10, Magna, Tarjeta, 1
                2, 12, Magna, Tarjeta, 1
                3, 5, Magna, Tarjeta, 1
                4, 15, Magna, Transferencia, 2
                5, 10, Magna, Transferencia, 2
                6, 2, Magna, Transferencia, 2
                7, 8, Magna, Transferencia, 2
                8, 20, Magna, Efectivo, 3
                9, 11, Magna, Efectivo, 1
                10, 10, Diesel, Tarjeta,  4
                11, 12, Diesel, Efectivo, 4

                Tomar en cuenta el monto objetivo y que coincidan los productos
                (Magna con magna y diesel con diesel en este ejemplo)
             */
        }

        public void ProcessDummyTables()
        {
            MatchFunction(tableAmounts, tableTargetAmount);
        }

        public void MatchFunction(DataTable referenceTable, DataTable targetTable)
        {

            //Create Lists from data table
            for (int row = 0; row < referenceTable.Rows.Count; row++)
            {
                Reference reference = new Reference();
                reference.id = (int)referenceTable.Rows[row]["ID"];
                reference.price = Convert.ToDecimal(referenceTable.Rows[row]["Monto"]);
                reference.product = referenceTable.Rows[row]["Producto"].ToString();
                reference.PaymentMethod = referenceTable.Rows[row]["FormaPago"].ToString();
                referenceList.Add(reference);
            }

            for (int row = 0; row < targetTable.Rows.Count; row++)
            {
                Target target = new Target();
                target.Id = (int)targetTable.Rows[row]["ID"];
                target.paymentMethod = targetTable.Rows[row]["MetodoPago"].ToString();
                target.finalPrice = Convert.ToDecimal(targetTable.Rows[row]["Monto"]);
                target.product = targetTable.Rows[row]["Producto"].ToString();
                targetList.Add(target);
            }



            foreach (var item in targetList.ToList())
            {
                foreach (var r in referenceList.Where(x => x.product.Equals(item.product) && x.PaymentMethod.Equals(item.paymentMethod)).ToList())
                {
                    AddReferenceList(item, r);
                    if (item.finalPrice == 0)
                    {
                        targetList.Remove(item);
                        break;
                    }
                }

            }

            while (referenceList.Count() > 0)
            {
                var reference = referenceList.FirstOrDefault();
                var target = targetList.FirstOrDefault(x => x.finalPrice >= reference.price && x.product.Equals(reference.product));
                if (target != null)
                {
                    AddReferenceList(target, reference);
                    if (target.finalPrice == 0)
                    {
                        targetList.Remove(target);
                    }
                }
                else
                {
                    AddReferenceList(new Target(), reference);
                }

            }

            foreach (var res in responseList.OrderBy(x => x.id))
            {
                Console.WriteLine(res.id + " | " + res.price + " | " + res.product + " | " + res.paymentMethod + " | " + res.IdTarget);
            }
        }

        private static void AddReferenceList(Target item, Reference r)
        {
            item.finalPrice -= r.price;

            var result = new Result()
            {
                id = r.id,
                price = Convert.ToDecimal(r.price),
                product = r.product,
                paymentMethod = r.PaymentMethod,
                IdTarget = item.Id
            };
            responseList.Add(result);
            referenceList.Remove(r);
        }
    }

    public class Reference
    {
        public int id;
        public decimal price;
        public string product;
        public string PaymentMethod;
    }

    public class Result
    {
        public int id;
        public decimal price;
        public string product;
        public string paymentMethod;
        public int? IdTarget;
    }

    public class Target
    {
        public int? Id;
        public string paymentMethod;
        public decimal finalPrice;
        public string product;
    }
}
