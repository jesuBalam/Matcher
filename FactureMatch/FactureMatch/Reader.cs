﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FactureMatch
{
    class Reader
    {
        public static void FillTables(string path)
        {
            string query = "SELECT *  FROM [Saigsa].[dbo].[Despachos]";
            string queryTarget = "SELECT *  FROM [Saigsa].[dbo].[Objetivo]";
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
    }
}
