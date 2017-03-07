using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NewParser
{

    class Program
    {
        private static string mainLink = "https://www.gumtree.com.au/s-property-for-rent/forrentby-ownr/c18364"; //start link for parsing
        private static string gumtreeBaselink = "https://www.gumtree.com.au"; //gumtree.com.au
        private static string dbConnectionString = "Data Source=NOTEBOOK;Initial Catalog=Offers;Integrated Security=True"; //string for database with Offers
        private SqlConnection dbConnection;
        private static string gumtreeLoginMail = "clfdynic5oep@mail.ru";
        private static string gumtreePassword = "megapass!";
        static void Main(string[] args)
        {
           
        }
    }
}
