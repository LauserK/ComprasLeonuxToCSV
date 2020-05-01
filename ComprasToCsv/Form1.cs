using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComprasToCsv
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            string fromDate = from.Value.ToString("yyyy-MM-dd");
            string toDate   = to.Value.ToString("yyyy-MM-dd");

            //var connectionString = @"Data Source=10.10.0.250\GRUPOPASEO,1433;Network Library=DBMSSOCN;Initial Catalog=GRUPOPASEO78LQNVEN;User ID=sa;Password=123;MultipleActiveResultSets=True";
            //var selectQuery = @"SELECT  `razon_social` AS PROVEEDOR,  `fecha_registro` AS `FECHA REGISTRO`,  `fecha` AS  `FECHA DOCUMENTO` ,  `documento` AS DOCUMENTO, total AS `MONTO FACTURA`,  `impuesto` AS  `MONTO IVA`,  `tasa_retencion_iva` AS `% RET IVA`, `retencion_iva` AS `IVA RETENIDO`,  `tasa_retencion_islr` AS `% RET ISLR`,  `retencion_islr` AS `ISLR RETENIDO`, base + ( impuesto - retencion_iva ) AS TOTAL FROM  `compras`  WHERE fecha >= '2019-01-01' AND fecha <= '2019-01-03'";
            var selectQuery = "SELECT razon_social, fecha_registro, fecha, documento, total as MONTO, retencion_iva, tasa_retencion_iva, retencion_islr, tasa_retencion_islr, base + exento + ( impuesto - retencion_iva ) AS TOTAL FROM compras WHERE fecha_registro >= '" + fromDate+ "' AND fecha_registro <= '" + toDate+"'";
            var server = "10.10.0.199";
            var database = "00000002";
            var username = "root";
            var password = "123";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try {
                var table = ReadTable(connectionString, selectQuery);

                //WriteToFile(table, @"C:\temp\outputfile.csv", false, ",");

                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in table.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field =>
                      string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                    sb.AppendLine(string.Join(",", fields));
                }
                var time = DateTime.Now;
                string formattedTime = time.ToString("yyyyMMddhhmmss");
                File.WriteAllText(@"C:\temp\compras-" + formattedTime + ".csv", sb.ToString());
                MessageBox.Show(@"ARCHIVO GUARDADO EN C:\temp\compras-" + formattedTime + ".csv", "MENSAJE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception _) {
                MessageBox.Show("ERROR AL GENERAR EL ARCHIVO", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            button1.Enabled = true;
        }

        public static DataTable ReadTable(string connectionString, string selectQuery)
        {
            var returnValue = new DataTable();

            var conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                var command = new MySqlCommand(selectQuery, conn);

                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(returnValue);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return returnValue;
        }        
    }
}
