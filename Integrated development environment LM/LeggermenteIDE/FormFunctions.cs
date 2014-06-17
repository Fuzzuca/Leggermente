using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace LeggermenteIDE
{
    class FormFunctions
    {

        public static bool Salva(string[] lines, string FileName, bool flag_saved)
        {
            if (lines == null)              //controllo il contenuto
                return false;
            if (flag_saved)                 //se già precendentemente salvato salvo nel FileName designato.
            { return ScriviSuFile(FileName, lines); }
            else                            // altrimenti chiedo dove salvare
            {
                SaveFileDialog saveFD = new SaveFileDialog();
                if (saveFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    bool ret;
                    ret = ScriviSuFile(saveFD.FileName, lines);
                    if (ret)
                    {
                        FileName = saveFD.FileName; //designo il FileName
                    }
                    return ret;
                }
                MessageBox.Show("Salvataggio Annullato");
                flag_saved = true;          //imposto come Salvato
                return false;
            }
        }

        public static bool ScriviSuFile(string FileName, string[] lines)
        {
            try
            {
                File.WriteAllLines(FileName, lines);        //provo a scrivere
            }
            // tutti le gestioni delle eccezioni più probabili.
            #region Catches
            //se non esiste la cartella (collegamenti a chiavette rimosse etc.)
            catch (DirectoryNotFoundException)
            {
                bool flag_saved = false;
                if (MessageBox.Show("La Cartella Desisderata Non Esiste.\nVuoi Salvare altrove?", "Exception Found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Salva(lines, FileName, flag_saved);
                return false;
            }
            //se l'accesso è negato
            catch (System.UnauthorizedAccessException) { MessageBox.Show("Non si Dispongono i Diritti per Scrivere in Questa Cartella"); return false; }
            catch (System.Security.SecurityException) { MessageBox.Show("Non si Dispongono i Diritti per Scrivere in Questa Cartella"); return false; }
            //se si fallisce la scrittura
            catch (IOException) { MessageBox.Show("Errore in Fase di Scrittura"); return false; }
            //se il FileName non rispetta gli standard necessari
            catch (NotSupportedException) { MessageBox.Show("Non Supportato"); return false; }
            #endregion
            return true;
        }

        public static void ControlloInChiusura(bool flag_modified, RichTextBoxEx RTBText, string FileName, bool flag_saved)
        {
            if (flag_modified)
            {
                System.Windows.Forms.DialogResult mboxres;
                mboxres = MessageBox.Show("Vuoi Salvare?", "File non Salvato", MessageBoxButtons.YesNoCancel);
                if (mboxres == System.Windows.Forms.DialogResult.Yes)
                {
                    Salva(RTBText.Lines,FileName, flag_saved);
                }
                else if (mboxres == System.Windows.Forms.DialogResult.Cancel)
                {
                    throw new OperationCanceledException("Operazione annullata");
                }
            }
        }



        public static string GetPathByFileName(string FileName)
        {
            if (FileName != "" && FileName != null)
            {
                string[] _path = FileName.Split('\\');
                string nPath = "";
                for (int i = 0; i < _path.Length - 1; i++)
                {
                    nPath += _path[i];
                    nPath += '\\';
                }
                return nPath;
            }
            else return null;
        }

        public static void PopulateTreeView(TreeView TWfiles, string FileName)
        {
            if (TWfiles.Nodes.Count > 0)
                TWfiles.Nodes.RemoveAt(0);
            TreeNode rootNode;
            string nPath = GetPathByFileName(FileName);
            if (nPath != null)
            {
                DirectoryInfo info = new DirectoryInfo(nPath);
                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    GetDirectories(info.GetDirectories(), rootNode);
                    TWfiles.Nodes.Add(rootNode);
                }
            }
            else
            {
                TWfiles.Nodes.Add("Salva il file");
            }
        }

        public static void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            bool catched = false;
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                try
                {
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0)
                    {
                        GetDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Nodes.Add(aNode);
                }
                catch (UnauthorizedAccessException)
                {
                    if (!catched)
                        MessageBox.Show("Non tutte le cartelle sono accessibili");
                    catched = true;
                }
            }
        }

    }
}
