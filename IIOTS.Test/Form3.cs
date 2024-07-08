using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IIOTS.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using IIOTS.Driver;
using IIOTS.Communication;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using IIOTS.Models;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;

namespace IIOTS.Test
{

    public partial class Form3 : Form
    {

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string text = string.Empty;
            bool jin = false;
            foreach (var item in textBox1.Text.Replace('\'', ' '))
            {
                if (item == '\"')
                {
                    jin = !jin;
                }
                if (!(jin && item == '\n'))
                {
                    text += item;
                }
            }
            var rows = text.Split("\n");
            List<string> rowslist = [];
            foreach (var row in rows)
            {
                if (!string.IsNullOrEmpty(row))
                {
                    var column = row.Split("\t");
                    List<string> strings = ["NULL", "0"];
                    if (column.Length == 21)
                    {
                        var dataType = column[1]
                            .Replace("Boolean", "Boole")
                            .Replace("Uint32", "Uint")
                            .Replace("Int32", "Int")
                            .Replace("Uint16", "UShort")
                            .Replace("Int16", "Short");
                        strings.Add($"'{dataType}'");
                        strings.Add($"'{column[0].Replace("WARN", "Alarm/WARN")}'");
                        strings.Add($"'{column[2]}'");
                        strings.Add("0");
                        strings.Add("1");
                        strings.Add($"'{column[3].Replace("ReadWrite", "RW").Replace("ReadOnly", "OR")}'");
                        strings.Add("''");
                        strings.Add($"'{column[20].Replace("\r", "")}'");
                        strings.Add("'ABCD'");
                        strings.Add("'ASCII'");
                        strings.Add(textBox3.Text);
                        strings.Add($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.000'");
                        strings.Add($"'{comboBox1.Text}'");
                    }
                    else
                    {
                        var dataType = column[1].Replace("Boolean", "Boole");
                        if (dataType.Equals("Uint32") || dataType.Equals("Int32") || dataType.Equals("Int16") || dataType.Equals("Uint16"))
                        {
                            if (column[5] == "1")
                            {
                                dataType = "UShort";
                            }
                            else
                            {
                                dataType = "Uint";
                            }

                        }
                        strings.Add($"'{dataType}'");
                        strings.Add($"'{column[0].Replace("WARN", "Alarm/WARN")}'");
                        string? addressType = TagAddressMatches().Matches(column[2]).FirstOrDefault()?.Value;
                        var Address = addressType + "." + column[3];
                        if (column[20].Contains(")&1"))
                        {
                            Address += ".0";
                        }
                        else if (column[20].Contains(">>"))
                        {
                            Address += "." + column[20].Split(">>")[1].Split("&")[0];
                        }
                        else if (column[20].Contains("<<"))
                        {
                            Address += "." + column[20].Split("<<")[1].Split("&")[0];
                        }
                        strings.Add($"'{Address}'");
                        strings.Add(column[5]);
                        strings.Add("1");
                        strings.Add($"'{column[6].Replace("ReadWrite", "RW").Replace("ReadOnly", "OR")}'");
                        strings.Add("''");
                        strings.Add($"'{column[23].Replace("\r", "")}'");
                        strings.Add("'ABCD'");
                        strings.Add("'ASCII'");
                        strings.Add(textBox3.Text);
                        strings.Add($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.000'");
                        strings.Add($"'{comboBox1.Text}'");
                    }
                    rowslist.Add($"({string.Join(",", strings.ToArray())})");
                }
            }
            textBox2.Text = $"INSERT INTO `eapdb`.`TagConfig` (`ID`, `StationNumber`, `DataType`, `TagName`, `Address`, `DataLength`, `Magnification`, `ClientAccess`, `EngUnits`, `Description`, `Sort`, `Coding`, `GID`, `CreationDate`, `UpdateMode`) VALUES {string.Join(",", rowslist.ToArray())}";
        }
        [GeneratedRegex("^[a-zA-Z0-9]+")]
        private static partial Regex TagAddressMatches();

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
