using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SSISISSUCK;

namespace FileLoaderClient
{
    public partial class SQLConnectionForm : Form
    {
        public PipeLineContext Context { get; set; }
        List<string> DatabaseList = new List<string>();
        SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();


        public SQLConnectionForm()
        {
            InitializeComponent();
            ddlDatabaseName.AllowDrop = false;
            ddlDatabaseName.Click += OnSelectingDatabaseName;
            ddlDatabaseName.SelectionChangeCommitted += OnDatabaseSelectionMade;
            btnConfirm.Click += OnConfirmationButtonClicked;
        }

        private void OnSelectingDatabaseName(object sender, EventArgs e)
        {

            if (DatabaseList.Count == 0)
            {
                sb.ConnectTimeout = 10;
                sb.DataSource = txtServerName.Text;
                sb.IntegratedSecurity = true;
                using (SqlConnection con = new SqlConnection(sb.ConnectionString))
                {
                    con.Open();
                    using (SqlCommand Command = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        SqlDataReader reader = Command.ExecuteReader();
                        while (reader.Read())
                        {
                            DatabaseList.Add(reader.GetString(0));
                        }
                    }
                    con.Close();
                }

                //filling the values
                ddlDatabaseName.BeginUpdate();
                ddlDatabaseName.DataSource = DatabaseList;
                ddlDatabaseName.EndUpdate(); 
            }
        }

        private void OnDatabaseSelectionMade(object sender, EventArgs e)
        {
            sb.InitialCatalog = (string)ddlDatabaseName.SelectedValue;
        }

        private void OnConfirmationButtonClicked(object sender, EventArgs e)
        {
            string ConnectionString = sb.ConnectionString;
            Context.ConnectionString = ConnectionString;
            Close();
        }
    }
}
