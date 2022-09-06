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



            tableAmounts.Rows.Add(1, 20.97, "Magna", "Efectivo");
            tableAmounts.Rows.Add(2, 20.97, "Magna", "Efectivo");
            tableAmounts.Rows.Add(3, 20.97, "Magna", "Efectivo");
            tableAmounts.Rows.Add(4, 20.97, "Magna", "Efectivo");
            tableAmounts.Rows.Add(5, 21.37, "Magna", "Efectivo");       
            tableAmounts.Rows.Add(10, 22.60, "Diesel", "Tarjeta");
            tableAmounts.Rows.Add(12, 22.60, "Diesel", "Tarjeta");
            tableAmounts.Rows.Add(13, 22.60, "Diesel", "Efectivo");
            tableAmounts.Rows.Add(14, 22.62, "Diesel", "Efectivo");
            tableAmounts.Rows.Add(15, 22.67, "Diesel", "Efectivo");
            tableAmounts.Rows.Add(16, 22.67, "Diesel", "Efectivo");


            tableTargetAmount.Columns.Add("ID", typeof(int));
            tableTargetAmount.Columns.Add("Monto", typeof(decimal));
            tableTargetAmount.Columns.Add("Producto", typeof(string));
            tableTargetAmount.Columns.Add("MetodoPago", typeof(string));

            tableTargetAmount.Rows.Add(1, 105.25, "Magna", "Efectivo");
            tableTargetAmount.Rows.Add(2, 67.8, "Diesel", "Tarjeta");
            tableTargetAmount.Rows.Add(3, 67.96, "Diesel", "Efectivo");

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
                reference.id = (int)referenceTable.Rows[row]["Id"];
                reference.price = Convert.ToDecimal(referenceTable.Rows[row]["Monto"]);
                reference.product = referenceTable.Rows[row]["Producto"].ToString();
                reference.PaymentMethod = referenceTable.Rows[row]["FormPago"].ToString();
                referenceList.Add(reference);
            }

            for (int row = 0; row < targetTable.Rows.Count; row++)
            {
                Target target = new Target();
                target.Id = (int)targetTable.Rows[row]["Id"];
                target.paymentMethod = targetTable.Rows[row]["FormPago"].ToString();
                target.finalPrice = Convert.ToDecimal(targetTable.Rows[row]["Monto"]);
                target.product = targetTable.Rows[row]["Producto"].ToString();
                targetList.Add(target);
            }

            foreach(var a in referenceList)
            {
                var result = new Result()
                {
                    id = a.id,
                    price = a.price,
                    product = a.product,
                    paymentMethod = a.PaymentMethod,
                    IdTarget = null
                };                
                responseList.Add(result);
            }

            foreach (var item in targetList.ToList())
            {
                foreach (var r in referenceList.ToList())
                {                    

                    if (referenceList.Contains(r) && !r.used)
                    {
                        if(r.product.ToString().ToUpper().Trim() == item.product.ToString().ToUpper().Trim() && (r.PaymentMethod.ToString().ToUpper().Trim() == item.paymentMethod.ToString().ToUpper().Trim() || r.PaymentMethod.ToUpper().Trim() == "EFECTIVO".Trim()))
                        {
                            AddReferenceList(item, r);
                            if (item.finalPrice <= 0)
                            {
                                r.used = true;
                                referenceList.Remove(r);
                                //Console.WriteLine(targetList.Count());
                                targetList.Remove(item);
                                //break;
                            }
                        }                        
                    }
                    //Console.WriteLine(r.price);
                }
                //Console.WriteLine(item.Id);
            }
            //int count = 0;
            //while (targetList.Count() > 0)
            //{
            //    //Console.WriteLine("COunter" + referenceList.Count());
            //    var reference = referenceList.FirstOrDefault();
            //    var target = targetList.FirstOrDefault(x => x.finalPrice >= reference.price && x.product.ToString().ToUpper().Equals(reference.product.ToString().ToUpper()));
            //    if (target != null)
            //    {
            //        AddReferenceList(target, reference);
            //        if (target.finalPrice >= 0)
            //        {
            //            reference.used = true;
            //            targetList.Remove(target);
            //            referenceList.Remove(reference);                        
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if (count == 0)
            //            AddReferenceList(new Target(), reference);

            //        count++;
            //    }

            //}
            decimal total1 = 0;
            decimal total2 = 0;
            decimal total3 = 0;
            decimal total4 = 0;
            decimal total5 = 0;
            decimal total6 = 0;
            decimal total7 = 0;
            ReaderQuery.WriteExcelFileEPPLUS(@"C:/Users/enriq/Downloads/Hoja1/Final.xlsx", ReaderQuery.ToDataTable2(responseList));
            foreach (var res in responseList.OrderByDescending(x => x.price))
            {
                switch (res.IdTarget)
                {
                    case 1:
                        total1 += res.price;
                        break;
                    case 2:
                        total2 += res.price;
                        break;
                    case 3:
                        total3 += res.price;
                        break;
                    case 4:
                        total4 += res.price;
                        break;
                    case 5:
                        total5 += res.price;
                        break;
                    case 6:
                        total6 += res.price;
                        break;
                    case 7:
                        total7 += res.price;
                        break;
                }
                //Console.WriteLine(res.id + " | " + res.price + " | " + res.product + " | " + res.paymentMethod + " | " + res.IdTarget);
                
            }
            Console.WriteLine("Sum 1 = " + total1);
            Console.WriteLine("Sum 2 = " + total2);
            Console.WriteLine("Sum 3 = " + total3);
            Console.WriteLine("Sum 4 = " + total4);
            Console.WriteLine("Sum 5 = " + total5);
            Console.WriteLine("Sum 6 = " + total6);
            Console.WriteLine("Sum 7 = " + total7);
            //Console.WriteLine(targetList.Count());
        }

        private static void AddReferenceList(Target item, Reference r)
        {
            //if (!targetList.Contains(item)) return;
            if (item.finalPrice - r.price <= 0)
            {
                //targetList.Remove(item);
                return;
            }
            item.finalPrice -= r.price;

            //var result = new Result()
            //{
            //    id = r.id,
            //    price = r.price,
            //    product = r.product,
            //    paymentMethod = r.PaymentMethod,
            //    IdTarget = item.Id
            //};
            int indexR = responseList.FindIndex(element => element.id == r.id);
            r.used = true;
            responseList[indexR].IdTarget = item.Id;
            int index = referenceList.FindIndex(element => element.id == r.id);
            referenceList.RemoveAt(index);
        }
    }

    public class Reference
    {
        public int id { get; set; }
        public decimal price { get; set; }
        public string product { get; set; }
        public string PaymentMethod { get; set; }
        public bool used { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public decimal price { get; set; }
        public string product { get; set; }
        public string paymentMethod { get; set; }
        public int? IdTarget { get; set; }
    }

    public class Target
    {
        public int? Id;
        public string paymentMethod;
        public decimal finalPrice;
        public string product;
    }
}
