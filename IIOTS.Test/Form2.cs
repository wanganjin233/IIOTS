using System.Text; 
using System.Net.Sockets;
using IIOTS.Models;
using IIOTS.Driver;
using IIOTS.Util;
using System.Diagnostics;
using System.Collections.Generic;

namespace IIOTS.Test
{
    public partial class Form2 : Form
    {
        private void InitListView(ListView listView, ImageList imageList)
        {




            listView.SmallImageList = imageList;
            ColumnHeader columnHeader1 = new ColumnHeader() { Name = "dateTime", Text = "日志时间", Width = 150 };
            ColumnHeader columnHeader2 = new ColumnHeader() { Name = "infoString", Text = "日志信息", Width = 1000 };
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });

            listView.HeaderStyle = ColumnHeaderStyle.None;
            listView.View = View.Details;
            listView.HideSelection = false;
            listView.SmallImageList = imageList;
        }

        private void Addlog(int imageIndex, string info)
        {
            Addlog(listView1, imageList1, imageIndex, info, 1000);
        }

        private void Addlog(ListView listView, ImageList imageList, int imageIndex, string info, int maxDisplayItems)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(new Action(() =>
                {
                    if (listView.Items.Count > maxDisplayItems)
                    {
                        listView.Items.RemoveAt(maxDisplayItems);
                    }

                    ListViewItem lstItem = new ListViewItem(" " + DateTime.Now.ToString(), imageIndex);
                    lstItem.SubItems.Add(info);
                    listView.Items.Insert(0, lstItem);
                }));
            }
            else
            {
                if (listView.Items.Count > maxDisplayItems)
                {
                    listView.Items.RemoveAt(maxDisplayItems);
                }

                ListViewItem lstItem = new ListViewItem(" " + DateTime.Now.ToString(), imageIndex);
                lstItem.SubItems.Add(info);
                listView.Items.Insert(0, lstItem);
            }
        }


        string mesIP = string.Empty;

        public Form2()
        {
            InitializeComponent();

        }
        public class MyClass
        {
            public string? Ass { get; set; }
        }
        public class MyClass1
        {
            public string? Ass { get; set; }
        }
        public void RunSoftware(string strName, string strPath)
        {
            try
            {
                Process[] localByName = Process.GetProcessesByName(strName);    //因为可以同时启动多个CDGRegedit.exe  
                if (localByName.Length >= 1)
                {
                    return;
                }
                ProcessStartInfo info = null;
                info = new ProcessStartInfo();
                info.FileName = strName;// + ".exe.lnk";这是快捷方式
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.Arguments = "";
                info.WorkingDirectory = strPath;
                //启动外部程序
                Process process = Process.Start(info);
                process.WaitForInputIdle();

            }
            catch
            {
                MessageBox.Show("打开" + strName + "失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
         

        private void Form2_Load(object sender, EventArgs e)
        {  
            var config = File.ReadAllText(Application.StartupPath + "MC3E.json");
            if (config.TryToObject(out EquConfig? _EQUValue))
            {
                string? ConnectionString = _EQUValue?.ConnectionString;
                if (ConnectionString != null)
                { 
                    modbusRtu = new FXSerialOverTcp(ConnectionString); 
                    modbusRtu.AddTags(_EQUValue.Tags.ToJson().ToObject<List<Tag>>());
                    this.dataGridView1.AutoGenerateColumns = false;
                    dataGridView1.DataSource = modbusRtu.AllTags.OrderBy(p => p.Address).ToList();
                    Tag? TotalQTY = modbusRtu?.AllTags.Find(p => p.TagName == "TotalQTY");
                    if (TotalQTY != null)
                    {
                        TotalQTY.ValueChangeEvent += UpdateData;
                    }
                    modbusRtu?.AllTags.ForEach(p =>
                    {
                        p.ValueChangeEvent += new Tag.ValueChangeDelegate((Tag tag) =>
                        {
                            Addlog(0, tag.Description + "       " + tag.Value);
                        });
                    });
                    AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
                    {
                        modbusRtu.Dispose();
                    };
                    modbusRtu?.Start();
                }
            }
        } 

        private void Ad_ReceiveEvent(Socket client, byte[] bytes)
        {
            Addlog(0, "     " + bytes.To0XString());
            return;
            if (bytes.Length > 2)
            {
                try
                {
                    string data = Encoding.ASCII.GetString(bytes);
                    Addlog(0, "     " + data);
                    var response = HttpRequest.PostAsyncJson($"{mesIP}/api/Packaging/GetInnerBoxInfoByNoFromEB", @$"{{ ""InnerPackageNo"":""{data}"" }}").Result;
                    var Jresponse = response.ToJObject();
                    if ((bool?)Jresponse["Success"] ?? false)
                    {
                        var result = Jresponse["Result"]?.ToString().ToJObject();
                        if (result != null)
                        {
                            if (result["ResponseResult"]?["ResultCode"]?.ToString() == "0")
                            {
                                Addlog(1, result["ResponseResult"]?["ResultMsg"]?.ToString() ?? string.Empty);
                            }
                            else
                            {
                                var ceBagWeightDown = (double?)result["Data"]?["CeBagWeightDown"];
                                var ceBagWeightUp = (double?)result["Data"]?["CeBagWeightUp"];
                                var standardWeight = (double?)result["Data"]?["StandardWeight"];
                                var pnSubstr = result["Data"]?["PnSubstr"]?.ToString();
                                InnerPackageNo = result["Data"]?["InnerPackageNo"]?.ToString() ?? string.Empty;
                                Tag? CeBagWeightDown = modbusRtu?.AllTags.Find(p => p.TagName == "CeBagWeightDown");
                                Tag? CeBagWeightUp = modbusRtu?.AllTags.Find(p => p.TagName == "CeBagWeightUp");
                                Tag? StandardWeight = modbusRtu?.AllTags.Find(p => p.TagName == "StandardWeight");
                                Tag? PnSubstr = modbusRtu?.AllTags.Find(p => p.TagName == "PnSubstr");
                                if (CeBagWeightDown != null
                                    && CeBagWeightUp != null
                                    && StandardWeight != null
                                    && PnSubstr != null)
                                {
                                    CeBagWeightDown.Value = ceBagWeightDown;
                                    CeBagWeightUp.Value = ceBagWeightUp;
                                    StandardWeight.Value = standardWeight;
                                    PnSubstr.Value = pnSubstr;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Addlog(1, e.Message);
                }
            }
            else
            {
                Addlog(0, "     接收到心跳");
            }
        }
        string InnerPackageNo = string.Empty;
        BaseDriver? modbusRtu = null;
        private void UpdateData(Tag tag)
        {
            try
            {
                Tag? RealTimeWeight = modbusRtu?.AllTags.Find(p => p.TagName == "RealTimeWeight");
                int? TotalQTY = (int?)tag.Value;
                int? OldTotalQTY = (int?)tag.OldValue;
                if (RealTimeWeight != null && TotalQTY != 0 && TotalQTY - OldTotalQTY == 1 && InnerPackageNo != string.Empty)
                {
                    var response = HttpRequest.PostAsyncJson($"{mesIP}/api/Packaging/InnerWeighConfirmFromEB", @$"{{ ""InnerPackageNo"":""{InnerPackageNo}"",""ActualWeight"":""{RealTimeWeight.Value}""  }}").Result;
                    Addlog(0, tag.Description + "       " + response);
                    InnerPackageNo = string.Empty;
                }
            }
            catch (Exception e)
            {
                Addlog(1, e.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataGridView1.Refresh();
        }
    }
}
