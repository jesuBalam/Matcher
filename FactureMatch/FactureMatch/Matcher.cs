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
            tableAmounts.Columns.Add("Monto", typeof(decimal));
            tableAmounts.Columns.Add("Producto", typeof(string));
            tableAmounts.Columns.Add("ID", typeof(string));

            tableAmounts.Rows.Add(40, "Magna", 1);
            tableAmounts.Rows.Add(30, "Magna", 2);
            tableAmounts.Rows.Add(45, "Diesel", 3);
            tableAmounts.Rows.Add(44, "Magna", 4);
            tableAmounts.Rows.Add(42, "Diesel", 5);
            tableAmounts.Rows.Add(40, "Diesel", 6);
            tableAmounts.Rows.Add(22, "Diesel", 7);
            tableAmounts.Rows.Add(20, "Diesel", 8);
            tableAmounts.Rows.Add(62, "Diesel", 9);


            tableTargetAmount.Columns.Add("Monto", typeof(decimal));
            tableTargetAmount.Columns.Add("Producto", typeof(string));
            tableTargetAmount.Columns.Add("MetodoPago", typeof(string));

            tableTargetAmount.Rows.Add(70, "Magna", "Efectivo");
            tableTargetAmount.Rows.Add(82, "Magna", "Tarjeta");
            tableTargetAmount.Rows.Add(106, "Magna", "Transferencia");
            tableTargetAmount.Rows.Add(70, "Magna", "Transferencia Oxxo");
            //tableTargetAmount.Rows.Add(70, "Magna", "Transferencia Cell");
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

            return resultTable;

            //foreach (var res in responseList)
            //{
            //    Console.WriteLine(res.id + " | " + res.price + " | " + res.product + " | " + res.paymentMethod);
            //}

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
