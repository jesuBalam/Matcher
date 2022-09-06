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
        static int count = 0;
        
        static decimal lastValidNumberDec;        


        private static List<Reference> referenceList = new List<Reference>();
        private static List<Target> targetList = new List<Target>();

        private static List<Result> responseList = new List<Result>();
        private static List<Result> responseListAux = new List<Result>();



        //Reuse 
        static Result resultEqual = new Result();
        static Result resultLess = new Result();
        static bool foundedIncomplete;
        static bool stopProces;

        public static void GenerateDummyTable()
        {
            tableAmounts.Columns.Add("ID", typeof(int));
            tableAmounts.Columns.Add("Monto", typeof(decimal));
            tableAmounts.Columns.Add("Producto", typeof(string));
            tableAmounts.Columns.Add("MetodoPago", typeof(string));



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
            //tableTargetAmount.Rows.Add(70, "Magna", "Transferencia Cell");



            //ISAAC EXAMPLE::
            /*
             * TABLE 1: (Precio, Producto, FormaPago)
                1, 10, Magna, Tarjeta
                2, 12, Magna, Tarjeta
                3, 5, Magna, Tarjeta
                4, 15, Magna, Transferencia
                5, 10, Magna, Transferencia
                6, 2, Magna, Transferencia
                7, 8, Magna, Transferencia
                8, 20, Magna, Efectivo
                9, 11, Magna, Efectivo
                10, 10, Diesel, Tarjeta
                11, 12, Diesel, Efectivo

                TABLE 2: (Id, Precio, Producto, FormaPago)
                1, 38, Magna, Tarjeta
                2, 35, Magna, Transferencia
                3, 20, Magna, Efectivo
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
                10, 10, Diesel, Tarjeta, 4
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
            for (int row = 0; row < 1800; row++)
            {
                Reference reference = new Reference();
                reference.id = referenceTable.Rows[row]["Id"].ToString();
                reference.price = Convert.ToDecimal(referenceTable.Rows[row]["Monto"]);
                reference.product = referenceTable.Rows[row]["Producto"].ToString();
                reference.paymentMethod = referenceTable.Rows[row]["FormPago"].ToString();
                referenceList.Add(reference);
            }

            for (int row = 0; row < targetTable.Rows.Count; row++)
            {
                Target target = new Target();
                target.id = targetTable.Rows[row]["Id"].ToString();
                target.paymentMethod = targetTable.Rows[row]["FormPago"].ToString();
                target.finalPrice = Convert.ToDecimal(targetTable.Rows[row]["Monto"]);
                target.product = targetTable.Rows[row]["Producto"].ToString();
                targetList.Add(target);
            }

            //Matcher
            for (int row = 0; row < targetList.Count; row++)
            {
                Console.WriteLine("TargetList : " + row + " from " + targetList.Count);
                if (referenceList.Count > 0)
                {
                    foundedIncomplete = false;
                    hasValid = false;
                    lastValidNumberDec = 0;
                    MatchRecursive(referenceList, targetList[row], new List<Reference>());
                    Target tar = new Target();
                    tar = targetList[row];
                    if (!hasValid)
                    {
                        foreach (var responseAux in responseListAux)
                        {
                            //Console.WriteLine("Responses Aux Less:: " + responseAux.id + " |  " + responseAux.price + " |  " + responseAux.product + " |  " + responseAux.price + " |  " + responseAux.idReference);
                            responseList.Add(responseAux);
                            var refeFound = referenceList.Find(element => element.id == responseAux.id);
                            referenceList.Remove(refeFound);
                            tar.finalPrice -= responseAux.price;


                        }
                        responseListAux.Clear();
                        lastValidNumberDec = 0;
                        hasValid = false;
                        foundedIncomplete = false;                        
                        MatchRecursive(referenceList, tar, new List<Reference>(), true);
                        if (!hasValid)
                        {
                            foreach (var responseAux in responseListAux)
                            {
                                responseList.Add(responseAux);
                                var refeFound = referenceList.Find(element => element.id == responseAux.id);
                                referenceList.Remove(refeFound);
                                tar.finalPrice -= responseAux.price;
                                //Console.WriteLine("Responses Aux Less:: " + responseAux.id + " |  " + responseAux.price + " |  " + responseAux.product + " |  " + responseAux.price + " |  " + responseAux.idReference);
                            }
                        }
                        //foreach (var refL in referenceList)
                        //{

                        //    Console.WriteLine("LEFT:: " + refL.id + " | " + refL.price + " | " + refL.product + " | " + refL.paymentMethod);
                        //}
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
            resultTable.Columns.Add("IdRef");

            for(int row=0; row<resultTable.Rows.Count; row++)
            {
                resultTable.Rows[row]["IdRef"] = "";
                if (row <= responseList.Count)
                {
                    var responseFound = responseList.Find(element => element.id == resultTable.Rows[row]["ID"].ToString());
                    if (responseFound != null)
                    {
                        resultTable.Rows[row]["IdRef"] = responseFound.idReference;
                    }
                }
            }

            foreach (var res in responseList)
            {
                Console.WriteLine(res.id + " | " + res.price + " | " + res.product + " | " + res.paymentMethod + " | " + res.idReference);
            }

            return resultTable;

            

        }


        private static void MatchRecursive(List<Reference> numbers, Target target, List<Reference> partial, bool nextToEfective = false)
        {            
            if (stopProces && count!= 0) 
            {
                numbers.Clear();
                target = null;
                partial.Clear();
                return;
            }
            decimal s = 0;
            foreach (Reference x in partial) 
            {                
                if (x.product.ToString().ToUpper() == target.product.ToString().ToUpper() && x.paymentMethod.ToString().ToUpper() == target.paymentMethod.ToString().ToUpper())
                {
                    s += x.price;
                }
                else if (nextToEfective)
                {
                    if(x.product.ToString().ToUpper() == target.product.ToString().ToUpper() && x.paymentMethod.ToString().ToUpper() == target.paymentMethod.ToString().ToUpper() || x.paymentMethod.ToString().ToUpper() == "EFECTIVO")
                    {
                        s += x.price;
                    }
                }
            }
            if(s!= target.finalPrice)
            {
                if (s < lastValidNumberDec)
                {
                    stopProces = true;
                    return;
                }
            }
            count++;
            //Check if match equals and remove from ref list
            if (s == target.finalPrice)
            {

                Result resultFounded = responseList.Find(element => element.idReference == target.id);
                if (!responseList.Contains(resultFounded))
                {
                    foreach (var refe in partial)
                    {
                        var refeFound = referenceList.Find(element => element.id == refe.id);                        
                        referenceList.Remove(refeFound);
                        Result resultEqual = new Result();
                        resultEqual.id = refe.id;
                        resultEqual.price = refe.price;
                        resultEqual.product = refe.product;
                        resultEqual.paymentMethod = refe.paymentMethod;
                        resultEqual.idReference = target.id;
                        responseList.Add(resultEqual);
                        numbers.Remove(refe);
                    }
                    hasValid = true;
                    return;
                }
                //TODO CHECK THIS!!!!!!
                if (nextToEfective)
                {
                    foreach (var refe in partial)
                    {
                        var refeFound = referenceList.Find(element => element.id == refe.id);
                        referenceList.Remove(refeFound);
                        Result resultEqual = new Result();
                        resultEqual.id = refe.id;
                        resultEqual.price = refe.price;
                        resultEqual.product = refe.product;
                        resultEqual.paymentMethod = refe.paymentMethod;
                        resultEqual.idReference = target.id;
                        responseList.Add(resultEqual);
                        numbers.Remove(refe);
                    }
                    hasValid = true;
                    return;
                }
                return;
                //for (int i = 0; i < partial.ToArray().Length; i++)
                //{
                //    Console.WriteLine("Eq sum" + partial.ToArray()[i].id + " | " + partial.ToArray()[i].price);
                //}

            }

            //Check if match less than ref and remove from ref list
            if (s < target.finalPrice)
            {
                if(s< lastValidNumberDec)
                {
                    return;
                }
                
                if (s > lastValidNumberDec)
                {                    
                    foundedIncomplete = true;
                    Console.WriteLine("Founded");
                    lastValidNumberDec = s;
                    responseListAux.Clear();
                    foreach (var refe in partial)
                    {
                        Result resultLess = new Result();
                        resultLess.id = refe.id;
                        resultLess.price = refe.price;
                        resultLess.product = refe.product;
                        resultLess.paymentMethod = refe.paymentMethod;
                        resultLess.idReference = target.id;
                        responseListAux.Add(resultLess);
                        numbers.Remove(refe);
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
                MatchRecursive(remaining, target, partialList, nextToEfective);
            }
        }
    }

    public class Reference
    {
        public string id;
        public decimal price;
        public string product;
        public string paymentMethod;
    }

    public class Result
    {
        public string id;
        public decimal price;
        public string product;
        public string paymentMethod;
        public string idReference;
    }

    public class Target
    {
        public string id;
        public string paymentMethod;
        public decimal finalPrice;
        public string product;
    }
}
