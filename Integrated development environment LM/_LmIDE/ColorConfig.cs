using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace LmIDE
{
    class ColorConfig
    {
        public string Value;
        Color color;
        private int rexop = -1;


        public int RegexOption
        {
            get { return rexop; }
        }

        public ColorConfig(string value, int red, int green, int blue)
        {
            Value = value;
            color = Color.FromArgb(red, green, blue);
        }

        public ColorConfig(string value, int red, int green, int blue, int RegexOption)
        {
            Value = value;
            color = Color.FromArgb(red, green, blue);
            rexop = RegexOption;
        }

        public static List<ColorConfig> Leggifile(string path)
        {
            string[] PreConfig;
            List<ColorConfig> ret;
            if (!File.Exists(path))
            {
                File.Create(path);
                return null;
            }
            else
            {
                PreConfig = File.ReadAllLines(path);
                ret = new List<ColorConfig>();
                string[] values;

                for (int i = 0; i < PreConfig.Length; i++)
                    if (PreConfig[i] != "")
                        if (!(PreConfig[i][0] == '#' || char.IsWhiteSpace(PreConfig[i], 0)))
                        {
                            bool isrex = false;
                            if (PreConfig[i][0] == '^')
                            {
                                isrex = true;
                                PreConfig[i] = PreConfig[i].Remove(0, 1);
                            }
                            values = PreConfig[i].Split('=', ';');
                            int red, green, blue, Rexop;
                            string value;
                            value = values[0].Trim();
                            if (int.TryParse(values[1].Trim(), out red) &&
                                int.TryParse(values[2].Trim(), out green) &&
                                int.TryParse(values[3].Trim(), out blue))
                                if (isrex)
                                {
                                    if (int.TryParse(values[4].Trim(), out Rexop))
                                        ret.Add(new ColorConfig(value, red, green, blue, Rexop));
                                }
                                else
                                    ret.Add(new ColorConfig(value, red, green, blue));

                            isrex = false;
                        }
            }
            return ret;
        }

        public static void ColoraTesto(RichTextBoxEx rtb, List<ColorConfig> color)
        {
            rtb.SuspendLayout();
            int selIndx = rtb.SelectionStart;                                   //salvo le coordinate di un eventuale selezione
            int selLeght = rtb.SelectionLength;
            Color col = rtb.ForeColor;                                          //salvo il colore iniziale del font
            string rex = @"[\w]+[.\s]($)?";
            MatchCollection Collections = Regex.Matches(rtb.Text, rex);         //prendo tutte le parole
            foreach (ColorConfig i in color)
            {
                if (i.RegexOption != -1)
                {
                    MatchCollection rexres = Regex.Matches(rtb.Text, i.Value, (RegexOptions)i.RegexOption);
                    foreach (Match n in rexres)
                    {
                        rtb.Select(n.Index, n.Length);                              // seleziono il match
                        rtb.SelectionColor = i.color;                                // coloro
                    }
                }
                else
                {
                    foreach (Match m in Collections)
                    {
                        if (i.Value.ToLower() == m.ToString().Trim().ToLower())     //guardo se la parola corrisponde
                        {
                            rtb.Select(m.Index, m.Length);                          //seleziono la parola
                            rtb.SelectionColor = i.color;                           //coloro
                        }
                    }
                }
            }
            rtb.Select(selIndx, 0);                                     //riporto la selezione allo stato iniziale
            rtb.SelectionStart = selIndx;
            rtb.SelectionLength = selLeght;
            rtb.SelectionColor = col;
            rtb.ResumeLayout(true);
        }
    }
}
