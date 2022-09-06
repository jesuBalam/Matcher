using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace FactureMatch
{
    public static class ReaderQuery
    {
        public static void FillTables()
        {
            string query = "SELECT *  FROM [Saigsa].[dbo].[Despachos] order by FormPago  DESC, Monto DESC";
            string queryTarget = "SELECT *  FROM [Saigsa].[dbo].[Objetivo] order by FormPago DESC";
            Console.WriteLine("Executing querys");
            //Integrated Security=SSPI;
            SqlConnection connection = new SqlConnection(string.Format("Data Source={0};database={1};Integrated Security=SSPI; User ID={2};Password={3}", ConfigurationManager.AppSettings["ServerDatabase"], ConfigurationManager.AppSettings["Database"], ConfigurationManager.AppSettings["User"], ConfigurationManager.AppSettings["Pass"]));
            connection.Open();
            using (var command = new SqlCommand(query, connection))
            {
                var adapter = new SqlDataAdapter(command);
                var dataset = new DataSet();
                adapter.Fill(dataset);
                Matcher.tableAmounts = dataset.Tables[0];
                Console.WriteLine("Completed query");
            }
            using (var command = new SqlCommand(queryTarget, connection))
            {
                var adapter = new SqlDataAdapter(command);
                var dataset = new DataSet();
                adapter.Fill(dataset);
                Matcher.tableTargetAmount = dataset.Tables[0];
                Console.WriteLine("Completed query");
            }
            connection.Close();
        }

        public static void WriteExcelFileEPPLUS(string path, DataTable table)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(path))
            {
                OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SheetFinal18");
                worksheet.Cells["A1"].LoadFromDataTable(table, true);
                //worksheet.Column(1).Style.Numberformat.Format = "@"; Texto
                //worksheet.Column(1).Style.Numberformat.Format = "yy-MM-dd HH:mm:ss"; Fecha
                //worksheet.Column(1).Style.Numberformat.Format = "0"; Numero
                //worksheet.Column(1).Style.Numberformat.Format = "0.00"; Decimal
                //worksheet.Column(1).Style.Font.Size = 20; Estilos

                package.Save();
            }
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static DataTable ToDataTable2<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }

}
