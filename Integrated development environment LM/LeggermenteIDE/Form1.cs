using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing.Printing;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using Leggermente.Translator;
using LeggermenteIDE;

namespace LeggermenteIDE
{
    public partial class FormBase : Form
    {
        public FormBase()
        {
            InitializeComponent();
            ApplicaConfigurazioni();
            size = RTBText.Font.Size;
            FormFunctions.PopulateTreeView(TWfiles, FileName);
        }

        #region Variabili Private
        private double size;
        private ToolStripMenuItem prev;
        private bool flag_zoom = false;
        private bool flag_modified = false;
        private bool flag_saved = false;
        private int FileExplorerSize;
        private int ErrorConsoleSize;
        private string FileName;
        List<ColorConfig> color;
        Stack<string> undoList = new Stack<string>();
        Stack<string> redoList = new Stack<string>();

        #endregion

        #region Gestione Eventi Form

            #region Gestione Menù

                #region Menù File

        private void nuovoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormFunctions.ControlloInChiusura(ref flag_modified, RTBText, ref FileName, ref flag_saved);
            }
            catch (OperationCanceledException) { return; };
            flag_modified = false;
            flag_saved = false;

            undoList.Clear();
            redoList.Clear();
            //RTBText.Text = "|Scrivi il tuo codice qui|";
        }

        private void salvaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FormFunctions.Salva(RTBText.Lines, ref FileName, ref flag_saved))
            {
                undoList.Clear();
                redoList.Clear();
            }
            FormFunctions.PopulateTreeView(TWfiles, FileName);
        }

        private void salvaconnomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_saved = false;
            salvaToolStripMenuItem_Click(sender, e);
        }

        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void apriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFD.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    FormFunctions.ControlloInChiusura(ref flag_modified, RTBText, ref FileName, ref flag_saved);
                    RTBText.Lines = File.ReadAllLines(openFD.FileName);
                    FileName = openFD.FileName;
                    FormFunctions.PopulateTreeView(TWfiles, FileName);
                }
                #region Catches
                catch (OperationCanceledException) { return; }
                catch (IOException)
                {
                    if (MessageBox.Show("Errore in Lettura\n\nRiprovare?", "Errore", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        apriToolStripMenuItem_Click(sender, e);
                    }
                    else
                        return;
                }
                catch (UnauthorizedAccessException) { MessageBox.Show("Accesso al file negato"); return; }
                catch (System.Security.SecurityException) { MessageBox.Show("accesso al file negato"); return; }
                #endregion
            }
            flag_saved = true;
            flag_modified = false;
            undoList.Clear();
            redoList.Clear();
        }

        private void stampaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument documentToPrint = new PrintDocument();
            printFD.Document = documentToPrint;

            if (printFD.ShowDialog() == DialogResult.OK)
            {
                StringReader reader = new StringReader(RTBText.Text);
                documentToPrint.PrintPage += new PrintPageEventHandler(DocumentToPrint_PrintPage);
                documentToPrint.Print();
            }
        }

        private void DocumentToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            StringReader reader = new StringReader(RTBText.Text);
            float LinesPerPage = 0;
            float YPosition = 0;
            int Count = 0;
            float LeftMargin = e.MarginBounds.Left;
            float TopMargin = e.MarginBounds.Top;
            string Line = null;
            Font PrintFont = RTBText.Font;
            SolidBrush PrintBrush = new SolidBrush(Color.Black);

            LinesPerPage = e.MarginBounds.Height / PrintFont.GetHeight(e.Graphics);

            while (Count < LinesPerPage && ((Line = reader.ReadLine()) != null))
            {
                YPosition = TopMargin + (Count * PrintFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(Line, PrintFont, PrintBrush, LeftMargin, YPosition, new StringFormat());
                Count++;
            }

            if (Line != null)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
            PrintBrush.Dispose();
        }

        #endregion

                #region Menù Visualizza

        private void fileExplorerToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (!fileExplorerToolStripMenuItem.Checked)
            {
                MainSplit.IsSplitterFixed = true;
                FileExplorerSize = MainSplit.SplitterDistance;
                MainSplit.SplitterDistance = 0;
            }
            else
            {
                MainSplit.IsSplitterFixed = false;
                MainSplit.SplitterDistance = FileExplorerSize;
            }
        }

        private void errorConsoleToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (!errorConsoleToolStripMenuItem.Checked)
            {
                splitError.IsSplitterFixed = true;
                ErrorConsoleSize = splitError.SplitterDistance;
                splitError.SplitterDistance = splitError.Height;
            }
            else
            {
                splitError.IsSplitterFixed = false;
                splitError.SplitterDistance = ErrorConsoleSize;
            }
        }

        #endregion

                #region Menù Modifica

        private void selezionatuttoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RTBText.SelectAll();
        }

        private void annullaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoList.Count > 0)
            {
                RTBText.Text = undoList.Pop();
                RTBText.ForeColor = Color.White;
                ColorConfig.ColoraTesto(RTBText, color);
                redoList.Push(RTBText.Text);
                ripristinaToolStripButton.Enabled = true;
                ripristinaToolStripMenuItem.Enabled = true;
            }
            else
            {
                annullaToolStripButton1.Enabled = false;
                annullaToolStripMenuItem.Enabled = false;
            }
        }

        private void ripristinaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoList.Count > 0)
            {
                RTBText.Text = redoList.Pop();
                RTBText.ForeColor = Color.White;
                ColorConfig.ColoraTesto(RTBText, color);
            }
            else
            {
                ripristinaToolStripMenuItem.Enabled = false;
                ripristinaToolStripButton.Enabled = false;
            }

        }

        private void tagliaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(RTBText.SelectedText);
            RTBText.SelectedText = "";
        }

        private void copiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(RTBText.SelectedText);
        }

        private void incollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RTBText.SelectedText = Clipboard.GetText();
        }

        #endregion

                #region Menù Strumenti

        #region Personalizza

        private void coloreSfondoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color c = colorDialog.Color;
                RTBText.BackColor = c;
                string[] load = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc");
                load[7] = "BgColor=" + c.R + ";" + c.G + ";" + c.B;
                File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc", load);
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                RTBText.Font = fontDialog.Font;
                string[] load = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc");
                load[6] = "FontFamily=" + fontDialog.Font.Name + ";" + fontDialog.Font.Size.ToString();
                File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc", load);
            }
        }

        private void coloreFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color c = colorDialog.Color;
                RTBText.ForeColor = c;
                string[] load = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc");
                load[8] = "ForeColor=" + c.R + ";" + c.G + ";" + c.B;
                File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\FormConfig.lmc", load);
            }
        }

        #endregion

        #region Compila

        private void tsbCompila_Click(object sender, EventArgs e)
        {

            string [] errori;
            if (FormFunctions.Salva(RTBText.Lines, ref FileName, ref flag_saved))
            {
                undoList.Clear();
                redoList.Clear();
            }
            FormFunctions.PopulateTreeView(TWfiles, FileName);
            if (flag_saved)
            {
                if (sender == tsbCompila)
                {
                    if (toolStripComboBox1.SelectedIndex == 0)
                    {
                        if (!FormFunctions.CompilaProgramma(RTBText, FileName, out errori))
                        {
                            RTBLog.Lines = errori;
                            errorConsoleToolStripMenuItem.Checked = true;
                        }
                    }
                    else
                    {
                        if (!FormFunctions.CompilaPacchetto(RTBText, FileName, out errori))
                        {
                            RTBLog.Lines = errori;
                            errorConsoleToolStripMenuItem.Checked = true;
                        }
                    }
                }
                else
                {
                    if (!FormFunctions.CompilaProgramma(RTBText, FileName, out errori))
                    {
                        RTBLog.Lines = errori;
                        errorConsoleToolStripMenuItem.Checked = true;
                    }
                }
            }
        }

        private void pacchettoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string[] errori;
            if (FormFunctions.Salva(RTBText.Lines, ref FileName, ref flag_saved))
            {
                undoList.Clear();
                redoList.Clear();
            }
            FormFunctions.PopulateTreeView(TWfiles, FileName);
            if (flag_saved)
            {
                if (FormFunctions.CompilaPacchetto(RTBText, FileName, out errori))
                {
                    RTBLog.Lines = errori;
                    errorConsoleToolStripMenuItem.Checked = true;
                }
            }
        }

        #endregion

        #endregion

                #region Menù ?

        private void informazionisuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutUs sup = new AboutUs();
            this.AddOwnedForm(sup);
            sup.ShowDialog();
        }

        private void documentazioneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Fuzzuca/Leggermente");
        }

        #endregion

        #endregion

            #region Eventi

        private void FormBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                FormFunctions.ControlloInChiusura(ref flag_modified, RTBText, ref FileName, ref flag_saved);
            }
            catch (OperationCanceledException) { e.Cancel = true; }
        }//chiusura form

        private void RTBText_TextChanged(object sender, EventArgs e)
        {
            RefreshControl.SuspendDrawing(MainSplit);
            ColorConfig.ColoraTesto(RTBText, color);


            if (RTBText.TextLength == 0)
                RTBText.ForeColor = Color.White;
            else 
            if (RTBText.Text[RTBText.TextLength-1] == ' ')
                undoList.Push(RTBText.Text.Trim());

            if (undoList.Count > 0)
            {
                annullaToolStripButton1.Enabled = true;
                annullaToolStripMenuItem.Enabled = true;
            }

            if (!flag_modified)
            {
                flag_modified = true;
                StatusLabel.Text = "Non Salvato";
            }
            RefreshControl.ResumeDrawing(MainSplit);
        }//Modifica Testo

        private void TSMI_Click(object sender, EventArgs e)
        {
            if (!flag_zoom) { flag_zoom = true; prev = TSMI100; } //se lo zoom non è stato ancora modificato imposto il precendente come 100%
            ToolStripMenuItem Clone;                    //creo un clone del sender (perchè gestito come oggetto non funziona)
            Clone = (ToolStripMenuItem)sender;
            string text = Clone.Text.TrimEnd('%');      //rimuovo il % dalla stringa del sender
            RTBText.Font = new Font(RTBText.Font.FontFamily, (int)size * Convert.ToInt32(text) / 100); //imposto la nuova dimensione del font
            Clone.Checked = true;                       //metto la spunta al sender
            prev.Checked = false;                       //la tolgo dal precedente
            prev = Clone;                               //imposto il precendente
            TSSLZoom.Text = "Zoom: " + Clone.Text;      //modifico la stringa
        }//gestione zoom

        private void RTBText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\t')
            {
                RTBText.SelectedText = "   ";
                RTBText.HideSelection = true;
                e.KeyChar = ' ';
            }
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                RefreshControl.SuspendDrawing(MainSplit);
                string line = RTBText.Lines[RTBText.GetLineFromCharIndex(RTBText.SelectionStart) - 1];
                int lineln = line.Length;
                line = line.Trim(' ');
                int _lineln = line.Length;
                int tabs = (lineln - _lineln) / 4;
                for (int i = 0; i < tabs; i++)
                {
                    RTBText.SelectedText = "    ";
                    RTBText.HideSelection = true;
                }
                RefreshControl.ResumeDrawing(MainSplit);
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"), 
                     new ListViewItem.ListViewSubItem(item, 
						dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"), 
                     new ListViewItem.ListViewSubItem(item, 
						file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        
        #endregion

        #endregion

    }
}
