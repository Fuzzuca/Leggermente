using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using LmIDE;
using System.Drawing.Printing;


namespace LmIDE
{
    public partial class FormBase : Form
    {

        #region Variabili Private
        private double size;
        private ToolStripMenuItem prev;
        private bool flag_zoom = false;
        private bool flag_modified = false;
        private bool flag_saved = false;
        private int FileExplorerSize;
        private int ErrorConsoleSize;
        private string path;
        List<ColorConfig> color;

        #endregion

        #region Metodi

        private bool Salva(string[] lines)
        {
            if (lines == null)              //controllo il contenuto
                return false;
            if (flag_saved)                 //se già precendentemente salvato salvo nel path designato.
            { return ScriviSuFile(path, lines); }
            else                            // altrimenti chiedo dove salvare
            {
                if (saveFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    bool ret;
                    ret = ScriviSuFile(saveFD.FileName, lines);
                    if (ret)
                    {
                        path = saveFD.FileName; //designo il path
                    }
                    return ret;
                }
                MessageBox.Show("Salvataggio Annullato");
                flag_saved = true;          //imposto come Salvato
                return false;
            }
        }

        private bool ScriviSuFile(string path, string[] lines)
        {
            try
            {
                File.WriteAllLines(path, lines);        //provo a scrivere
            }
            // tutti le gestioni delle eccezioni più probabili.
            #region Catches
            //se non esiste la cartella (collegamenti a chiavette rimosse etc.)
            catch (DirectoryNotFoundException)
            {
                flag_saved = false;
                if (MessageBox.Show("La Cartella Desisderata Non Esiste.\nVuoi Salvare altrove?", "Exception Found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Salva(lines);
                return false;
            }
            //se l'accesso è negato
            catch (System.UnauthorizedAccessException) { MessageBox.Show("Non si Dispongono i Diritti per Scrivere in Questa Cartella"); return false; }
            catch (System.Security.SecurityException) { MessageBox.Show("Non si Dispongono i Diritti per Scrivere in Questa Cartella"); return false; }
            //se si fallisce la scrittura
            catch (IOException) { MessageBox.Show("Errore in Fase di Scrittura"); return false; }
            //se il path non rispetta gli standard necessari
            catch (NotSupportedException) { MessageBox.Show("Non Supportato"); return false; }
            #endregion
            MessageBox.Show("Salvataggio Riuscito");
            return true;
        }

        private void ControlloInChiusura()
        {
            if (flag_modified)
            {
                System.Windows.Forms.DialogResult mboxres;
                mboxres = MessageBox.Show("Vuoi Salvare?", "File non Salvato", MessageBoxButtons.YesNoCancel);
                if (mboxres == System.Windows.Forms.DialogResult.Yes)
                {
                    Salva(RTBText.Lines);
                }
                else if (mboxres == System.Windows.Forms.DialogResult.Cancel)
                {
                    throw new OperationCanceledException("Operazione annullata");
                }
            }
        }

        #endregion

        #region Gestione Eventi Form

        #region Gestione Menù

        #region Menù File

        private void nuovoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ControlloInChiusura();
            }
            catch (OperationCanceledException) { return; };
            flag_modified = false;
            flag_saved = false;
            RTBText.Text = "|Scrivi il tuo codice qui|";
        }

        private void salvaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Salva(RTBText.Lines);
        }

        private void salvaconnomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_saved = false;
            Salva(RTBText.Lines);
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
                    ControlloInChiusura();
                    RTBText.Lines = File.ReadAllLines(openFD.FileName);
                    path = openFD.FileName;
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
            RTBText.Cut();
        }

        private void copiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RTBText.Copy();
        }

        private void incollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RTBText.Paste();
        }

        #endregion

        #region Menù Strumenti

        #region Personalizza

        private void coloreSfondoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
                RTBText.BackColor = colorDialog.Color;
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

        #endregion

        #endregion

        #endregion

        private void DocumentToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            StringReader reader = new StringReader(RTBText.Text);
            float LinesPerPage = 0;
            float YPosition = 0;
            int Count = 0;
            float LeftMargin = e.MarginBounds.Left;
            float TopMargin = e.MarginBounds.Top;
            string Line = null;
            Font PrintFont = this.RTBText.Font;
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

        private void FormBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ControlloInChiusura();
            }
            catch (OperationCanceledException) { e.Cancel = true; }
        }

        private void RTBText_TextChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();
            ColorConfig.ColoraTesto(RTBText,color);

            if (RTBText.TextLength == 0)
            {
                RTBText.ForeColor = Color.White;
            }
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
                flag_modified = true;
            this.ResumeLayout(true);
        }

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

        public FormBase()
        {
            InitializeComponent();
            size = RTBText.Font.Size;
            string pos = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Leggermente\ColorsConfig.lmc";
            color = ColorConfig.Leggifile(pos);
            
        }

        private void clock_Tick(object sender, EventArgs e)
        {

        }

        #endregion

        private void tsbCompila_Click(object sender, EventArgs e)
        {

        }

    }
}
