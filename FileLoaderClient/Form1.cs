using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SSISISSUCK;

namespace FileLoaderClient
{
    public partial class FileLoaderForm : Form
    {
        private PipeLineContext Context = new PipeLineContext();
        private SQLConnectionForm ConnectionForm = new SQLConnectionForm();
        private int NumberOfWriterThreads = 3;
        private int NumberOfFinishedThreads = 0;
        public event Action LoadStageOneFinished;
        public FileLoaderForm()
        {
            InitializeComponent();
            this.Load += LoadUIComponents;
            OnLoad(null);
            ContextViewer.NodeMouseDoubleClick += UserRequestsContextChange;
            ConnectionForm.FormClosing += LoadUIComponents; //redraw tree when that form is closed
            btnStartTransfer.Click += OnStartButtonClicked;
        }

        private void LoadUIComponents(object sender, EventArgs e)
        {
            #region TreeView

            ContextViewer.BeginUpdate();
            ContextViewer.Nodes.Clear();
            ContextViewer.Nodes.Add(new TreeNode("Pipeline Context"));
            foreach (PropertyInfo property in Context.GetType().GetProperties())
            {
                string value;
                if (!(property.Name.Equals("ColumnNames") || property.Name.Equals("DataTypes")))
                {
                    
                    if ((value = property.GetValue(Context).ToString()).Equals("\t"))
                    {
                        ContextViewer.Nodes[0].Nodes.Add(property.Name, property.Name + ": " + "{TAB}");
                    }
                    else
                    {
                        ContextViewer.Nodes[0].Nodes.Add(property.Name, property.Name + ": " + value);
                    }
                }
            }
            ContextViewer.EndUpdate();
            ContextViewer.ExpandAll(); 

            #endregion
        }

        private void UserRequestsContextChange(object sender, TreeNodeMouseClickEventArgs e)
        {
            #region IfstatatementsforNodeSelection
            TreeNode SelectedNode = e.Node;
            if (SelectedNode.Name.Contains("PathToSourceFile"))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Context.PathToSourceFile = dialog.FileName;
                    OnLoad(null);
                }

            }
            else if (SelectedNode.Name.Equals("FieldDelimiter"))
            {
                try
                {
                    string response = Microsoft.VisualBasic.Interaction.InputBox("Enter delimiter", "delimiter setting");
                    if (response.Length == 1) { Context.FieldDelimiter = response[0]; }
                    else if (response.Equals(@"\t")) { Context.FieldDelimiter = '\t'; }
                    OnLoad(null);
                }
                catch (Exception)
                {
                    //how i make erro message?
                }
            }
            else if (SelectedNode.Name.Equals("ConnectionString"))
            {
                ConnectionForm.Context = this.Context;
                ConnectionForm.ShowDialog();
            }

            else if (SelectedNode.Name.Equals("DestinationTableName"))
            {
                string response = Microsoft.VisualBasic.Interaction.InputBox("Enter table name", "Table name chooser");
                if (response.Length > 0)
                {
                    Context.DestinationTableName = response;
                }
                OnLoad(null);
            }

            else if (SelectedNode.Name.Equals("IsSuggestingDataTypes"))
            {
                Context.IsSuggestingDataTypes ^= true;
                OnLoad(null);
            }

            else if (SelectedNode.Name.Equals("FirstRowContainsHeaders"))
            {
                Context.FirstRowContainsHeaders ^= true;
                OnLoad(null);
            }

            else if (SelectedNode.Name.Equals("LinesToScan"))
            {
                string response = Microsoft.VisualBasic.Interaction.InputBox("Enter amount of lines", "linescan selector");
                int lines = 0;
                if (int.TryParse(response, out lines))
                {
                    Context.LinesToScan = lines;
                    OnLoad(null);
                }
            }

            else if (SelectedNode.Name.Equals("IsAppendingDataToExistingTable"))
            {
                Context.IsAppendingDataToExistingTable ^= true;
                OnLoad(null);
            }
            #endregion
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            btnStartTransfer.Enabled = false;
            //do some validation on the context?
            StartFileTransfer();
            //celebrate
        }
#pragma warning disable S3241 // Methods should not return values that are never used
        private async Task StartFileTransfer()
#pragma warning restore S3241 // Methods should not return values that are never used
        {
            //build staging table
            if (!Context.IsAppendingDataToExistingTable)
            {
                DestinationTableCreator TableMaker = new DestinationTableCreator(Context);
                TableMaker.CreateTable(); 
            }

            //create buffer
            ConcurrentQueue<List<string>> Queu = new ConcurrentQueue<List<string>>();
            //create threads to transfer file
            SourceFileReader Reader = new SourceFileReader(Context, Queu);
            Inserter Writer = new Inserter(Context, Queu);
            Writer.done = false;
            Reader.ReadFinished += Writer.StopWriting;
            Writer.FinishedWriting += OnWriterFinishing;

            // start everything up and monitor for finish
            Reader.StartReading();
            for (int i = 0; i < NumberOfWriterThreads; i++)
            {
                Writer.CreateConcurrentWriter();
            }

            await Task.Run(() =>
            {
                while (NumberOfWriterThreads > NumberOfFinishedThreads)
                {
                    Task.Delay(1000).Wait();
                }
            });


            btnStartTransfer.Enabled = true;
            OnLoadStageOneFinished();
        }

        private void OnWriterFinishing()
        {
            NumberOfFinishedThreads++;
        }

        private void OnLoadStageOneFinished()
        {
            LoadStageOneFinished?.Invoke();
        }
    }
}
