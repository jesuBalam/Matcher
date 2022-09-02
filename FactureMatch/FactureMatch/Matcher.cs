using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactureMatch
{
    public static class Matcher
    {
        public static DataTable tableAmounts = new DataTable();
        public static DataTable tableTargetAmount = new DataTable();
        private static DataTable tableResult;
        static bool isRunning;
        static bool hasValid;
        
        static decimal lastValidNumberDec;        


        private static List<Reference> referenceList = new List<Reference>();
        private static List<Target> targetList = new List<Target>();

        private static List<Result> responseList = new List<Result>();
        private static List<Result> responseListAux = new List<Result>();


        public static void GenerateDummyTable()
        {
            tableAmounts.Columns.Add("ID", typeof(string));
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


            tableTargetAmount.Columns.Add("ID", typeof(string));
            tableTargetAmount.Columns.Add("Monto", typeof(decimal));
            tableTargetAmount.Columns.Add("Producto", typeof(string));
            tableTargetAmount.Columns.Add("MetodoPago", typeof(string));

            tableTargetAmount.Rows.Add(1, 38, "Magna", "Tarjeta");
            tableTargetAmount.Rows.Add(2, 35, "Magna", "Transferencia");
            tableTargetAmount.Rows.Add(3, 20, "Magna", "Efectivo");
            tableTargetAmount.Rows.Add(4, 22, "Diesel", "Efectivo");
            //tableTargetAmount.Rows.Add(70, "Magna", "Transferencia Cell");



            //  CARLOS EXAMPLE::
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
                9, 11, Magna, Efectivo 1 38
                10, 10, Diesel, Tarjeta  4 22
                11, 12, Diesel, Efectivo 4 12

                TABLE 2: (Id, Precio, Producto, FormaPago)
                1, 38, Magna, Tarjeta cumplio
                2, 35, Magna, Transferencia cumplio
                3, 20, Magna, Efectivo cumplio
                4, 22, Diesel, Efectivo

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

        public static void ProcessDummyTables()
        {
            MatchFunction(tableAmounts, tableTargetAmount);
        }

        public static DataTable MatchFunction(DataTable referenceTable, DataTable targetTable)
        {

            //Create Lists from data table
            for (int row = 0; row < referenceTable.Rows.Count; row++)
            {
                Reference reference = new Reference();
                reference.id = referenceTable.Rows[row]["ID"].ToString();
                reference.price = Convert.ToDecimal(referenceTable.Rows[row]["Monto"]);
                reference.product = referenceTable.Rows[row]["Producto"].ToString();
                referenceList.Add(reference);
            }

            for (int row = 0; row < targetTable.Rows.Count; row++)
            {
                Target target = new Target();
                target.paymentMethod = targetTable.Rows[row]["MetodoPago"].ToString();
                target.finalPrice = Convert.ToDecimal(targetTable.Rows[row]["Monto"]);
                target.product = targetTable.Rows[row]["Producto"].ToString();
                targetList.Add(target);
            }

            //Matcher
            for (int row = 0; row < targetList.Count; row++)
            {
                if (referenceList.Count > 0)
                {
                    hasValid = false;
                    lastValidNumberDec = 0;
                    MatchRecursive(referenceList, targetList[row], new List<Reference>());

                    if (!hasValid)
                    {
                        foreach (var responseAux in responseListAux)
                        {
                            responseList.Add(responseAux);
                            var refeFound = referenceList.Find(element => element.id == responseAux.id);
                            referenceList.Remove(refeFound);
                        }
                        responseListAux.Clear();
                    }

                    //foreach (var res in referenceList)
                    //{
                    //    Console.WriteLine(res.id + " | " + res.price + " | " + res.product);
                    //}
                }

            }

            //Debug result

            DataTable resultTable = new DataTable();
            resultTable = referenceTable;
            resultTable.Columns.Add("MetodoPago");

            for(int row=0; row<resultTable.Rows.Count; row++)
            {
                resultTable.Rows[row]["MetodoPago"] = "";
                if (row <= responseList.Count)
                {
                    var responseFound = responseList.Find(element => element.id == resultTable.Rows[row]["ID"].ToString());
                    if (responseFound != null)
                    {
                        resultTable.Rows[row]["MetodoPago"] = responseFound.paymentMethod;
                    }
                }
            }

            foreach (var res in responseList)
            {
                Console.WriteLine(res.id + " | " + res.price + " | " + res.product + " | " + res.paymentMethod + " | " + res.id);
            }

            return resultTable;

            

        }


        private static void MatchRecursive(List<Reference> numbers, Target target, List<Reference> partial)
        {
            decimal s = 0;
            foreach (Reference x in partial) s += x.price;

            //Check if match equals and remove from ref list
            if (s == target.finalPrice)
            {

                Result resultFounded = responseList.Find(element => element.paymentMethod == target.paymentMethod);
                if (!responseList.Contains(resultFounded))
                {
                    foreach (var refe in partial)
                    {
                        var refeFound = referenceList.Find(element => element.id == refe.id);
                        referenceList.Remove(refeFound);
                        Result result = new Result();
                        result.id = refe.id;
                        result.price = refe.price;
                        result.product = refe.product;
                        result.paymentMethod = target.paymentMethod;
                        responseList.Add(result);
                    }

                    //for (int i = 0; i < partial.ToArray().Length; i++)
                    //{
                    //    Console.WriteLine("Eq sum" + partial.ToArray()[i].id + " | " + partial.ToArray()[i].price);
                    //}
                    hasValid = true;
                    return;
                }

            }

            //Check if match less than ref and remove from ref list
            if (s < target.finalPrice)
            {

                if (s > lastValidNumberDec)
                {
                    //Result resultFounded = responseListAux.Find(element => element.paymentMethod == target.paymentMethod);
                    //if (!responseListAux.Contains(resultFounded))
                    //{

                    //}                    

                    lastValidNumberDec = s;
                    responseListAux.Clear();
                    foreach (var refe in partial)
                    {
                        Result res = new Result();
                        res.id = refe.id;
                        res.price = refe.price;
                        res.product = refe.product;
                        res.paymentMethod = target.paymentMethod;
                        responseListAux.Add(res);
                    }
                }
            }


            //Check if match greater than ref and return
            //TODO: Need to check if may include this option. Can cause conflicts whith actual algorithm
            if (s > target.finalPrice && !hasValid)
            {
                hasValid = false;
                return;
            }

            for (int i = 0; i < numbers.Count; i++)
            {
                List<Reference> remaining = new List<Reference>();
                Reference n = numbers[i];
                for (int j = i + 1; j < numbers.Count; j++) remaining.Add(numbers[j]);

                List<Reference> partialList = new List<Reference>(partial);
                partialList.Add(n);
                MatchRecursive(remaining, target, partialList);
            }
        }
    }

    public class Reference
    {
        public string id;
        public decimal price;
        public string product;
    }

    public class Result
    {
        public string id;
        public decimal price;
        public string product;
        public string paymentMethod;
    }

    public class Target
    {
        public string paymentMethod;
        public decimal finalPrice;
        public string product;
    }
}
