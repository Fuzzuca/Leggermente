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
            FormFunctions.PopulateTreeView(TWfiles,FileName);
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

        #endregion


        #region Gestione Eventi Form

        #region Gestione Menù

        #region Menù File

        private void nuovoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormFunctions.ControlloInChiusura(flag_modified,RTBText,FileName,flag_saved);
            }
            catch (OperationCanceledException) { return; };
            flag_modified = false;
            flag_saved = false;
            RTBText.Text = "|Scrivi il tuo codice qui|";
        }

        private void salvaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormFunctions.Salva(RTBText.Lines,FileName,flag_saved);
        }

        private void salvaconnomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_saved = false;
            FormFunctions.Salva(RTBText.Lines, FileName, flag_saved);
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
                    FormFunctions.ControlloInChiusura(flag_modified, RTBText, FileName, flag_saved);
                    RTBText.Lines = File.ReadAllLines(openFD.FileName);
                    FileName = openFD.FileName;
                    FormFunctions.PopulateTreeView(TWfiles,FileName);
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
            RTBText.Undo();
            ripristinaToolStripMenuItem.Enabled = true;
            if (!RTBText.CanUndo)
                annullaToolStripMenuItem.Enabled = false;
        }

        private void ripristinaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RTBText.Redo();
            if (!RTBText.CanRedo)
                ripristinaToolStripMenuItem.Enabled = false;
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
                string[] load = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\Leggermente\FormConfig.lmc");
                load[6] = "FontFamily="+fontDialog.Font.Name+";"+fontDialog.Font.Size.ToString();
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
            if (flag_saved == false || flag_modified == true)
                FormFunctions.Salva(RTBText.Lines, FileName, flag_saved);
            LogManager errori = new LogManager();
            Translator traduttore = new Translator(errori);
            string[] pacccccccchetttttttu;
            string rex = "aggiungi";
            MatchCollection matches = Regex.Matches(RTBText.Text, rex);
            int j = 0;
            pacccccccchetttttttu = new string[matches.Count+1];
            foreach (Match i in matches)
            {
                int index = i.Index+i.Length;
                string line = RTBText.Lines[RTBText.GetLineFromCharIndex(index)];
                RTBText.Select(index, line.Length - i.Length);
                pacccccccchetttttttu[j] = "./" + RTBText.SelectedText.Trim() + ".lmp";
                j++;
            }
            pacccccccchetttttttu[j] = "./LEGGERMENTE.lmp";

            ResultCode risultato = traduttore.Translate(CodeType.Program, RTBText.Text, pacccccccchetttttttu, "./result.exe");
            if (!errori.WithOutError) RTBLog.Lines = errori.LogList;
            else MessageBox.Show(risultato.ToString());
        }


        private void pacchettoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogManager errori = new LogManager();
            Translator traduttore = new Translator(errori);
            string[] pacccccccchetttttttu;
            string rex = "aggiungi";
            MatchCollection matches = Regex.Matches(RTBText.Text, rex);
            int j = 0;
            pacccccccchetttttttu = new string[matches.Count + 1];
            foreach (Match i in matches)
            {
                int index = i.Index + i.Length;
                string line = RTBText.Lines[RTBText.GetLineFromCharIndex(index)];
                RTBText.Select(index, line.Length - i.Length);
                pacccccccchetttttttu[j] = "./" + RTBText.SelectedText.Trim() + ".lmp";
                j++;
            }
            pacccccccchetttttttu[j] = "./LEGGERMENTE.lmp";

            ResultCode risultato = traduttore.Translate(CodeType.Package, RTBText.Text, pacccccccchetttttttu, "./result.lmp");
            if (!errori.WithOutError) RTBLog.Lines = errori.LogList;
            else MessageBox.Show(risultato.ToString());
        }

        #endregion

        #endregion

        #endregion

        #region Eventi

        private void FormBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                FormFunctions.ControlloInChiusura(flag_modified, RTBText, FileName, flag_saved);
            }
            catch (OperationCanceledException) { e.Cancel = true; }
        }//chiusura form

        private void RTBText_TextChanged(object sender, EventArgs e)
        {
            RefreshControl.SuspendDrawing(MainSplit);
            ColorConfig.ColoraTesto(RTBText,color);


            if (RTBText.TextLength == 0)
                RTBText.ForeColor = Color.White;


            //controllo del focus per gestione ottimale undo / redo
            char last = (RTBText.Text.Length > 2)?RTBText.Text[RTBText.Text.Length - 1]:' '; 
            char _last = (RTBText.Text.Length > 2)?RTBText.Text[RTBText.Text.Length - 2]:' ';
            char enter = '\n';

            if ((last == ' ' || last == enter) && last != _last) 
            {
                this.Focus();
                RTBText.Focus();
            }
            if (!annullaToolStripMenuItem.Enabled) //attivo il bottone annulla
                annullaToolStripMenuItem.Enabled = true;


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
                RTBText.SelectedText =  "   ";
                RTBText.HideSelection = true;
                e.KeyChar = ' ';
            }
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                RefreshControl.SuspendDrawing(MainSplit);
                string line = RTBText.Lines[RTBText.GetLineFromCharIndex(RTBText.SelectionStart)-1];
                int lineln = line.Length;
                line = line.Trim(' ');
                int _lineln = line.Length;
                int tabs = (lineln - _lineln)/4;
                for (int i = 0; i < tabs; i++)
                {
                    RTBText.SelectedText = "    ";
                    RTBText.HideSelection = true;
                }
                RefreshControl.ResumeDrawing(MainSplit);
            }
        }

        void treeView1_NodeMouseClick(object sender,TreeNodeMouseClickEventArgs e)
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
