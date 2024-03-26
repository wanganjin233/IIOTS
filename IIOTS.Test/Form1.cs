using IIOTS.Driver;
using IIOTS.Models; 

namespace IIOTS.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        BaseDriver? mC3EEthernet;
        Dictionary<int, BaseDriver> mC3Drivers = new Dictionary<int, BaseDriver>();
        private void ConnButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(Port.Text, out int _Port))
            {
                try
                {
                    if (!mC3Drivers.ContainsKey(DriveChoose.SelectedIndex))
                    {
                        switch (DriveChoose.SelectedIndex)
                        {
                            case 1:
                                mC3EEthernet = new MC3E($"{IPAddress.Text}:{_Port}");
                                break;
                            case 0:
                                mC3EEthernet = new Fins($"{IPAddress.Text}:{_Port}");
                                break;
                            case 2:
                                mC3EEthernet = new ModbusRtu($"{IPAddress.Text},9600,8,нч,1");
                                break;
                            case 3:
                                mC3EEthernet = new ModbusRtu($"{IPAddress.Text}:{_Port}");
                                break;
                            case 4:
                                mC3EEthernet = new ModbusTcp($"{IPAddress.Text}:{_Port}");
                                break;
                        }
                        if (mC3EEthernet != null)
                        {
                            mC3Drivers.Add(DriveChoose.SelectedIndex, mC3EEthernet);
                        }
                    }
                    else
                    {
                        mC3EEthernet = mC3Drivers[DriveChoose.SelectedIndex];
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }

            }

        }

        private void BoolRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadBoole(Address.Text, length));
            }

        }

        public void Read<T>(OperateResult<T[]>? ushorts)
        {
            DateTime beforDT = DateTime.Now;
            if ((ushorts?.IsSuccess ?? false) && ushorts.Content != null)
            {
                string showText = $"[{ushorts.TimeSpan:T}][{Address.Text}][";
                foreach (var item in ushorts.Content)
                {
                    showText += $"{item},";
                }
                ReadResult.Text += showText.Remove(showText.Length - 1) + "]\r\n";
            }
            else
            {
                MessageBox.Show(ushorts?.Message);
            }
            DateTime afterDT = DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            Ping.Text = $"{ts.TotalMilliseconds}ms";
        }

        private void ShortRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadShort(Address.Text, length));
            }
        }

        private void UShortRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                //Read(mC3EEthernet?.ReadUshort(Address.Text, length));
            }
        }

        private void IntRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadInt(Address.Text, length));
            }
        }

        private void UintRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadUint(Address.Text, length));
            }
        }

        private void FloatRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadFloat(Address.Text, length));
            }

        }

        private void doubleRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {
                // Read(mC3EEthernet?.ReadDouble(Address.Text, length));
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            int? removeKey = mC3Drivers.FirstOrDefault(p => p.Value == mC3EEthernet).Key;
            if (removeKey != null)
            {
                mC3Drivers.Remove((int)removeKey);
            }
            //mC3EEthernet?.Dispose();
        }

        private void LongRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {

            }
        }

        private void ULongRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length))
            {

            }
        }

        private void StringRead_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(Length.Text, out ushort length) && ushort.TryParse(StringLength.Text, out ushort strlength))
            {

            }
        }

        private void w(object obj)
        {
            DateTime beforDT = System.DateTime.Now;
            // var Result = mC3EEthernet?.WriteObject(Waddress.Text, obj);
            // if (Result != null && !Result.IsSuccess)
            // {
            //     MessageBox.Show(Result.Message);
            // }
            // DateTime afterDT = System.DateTime.Now;
            // TimeSpan ts = afterDT.Subtract(beforDT);
            // Ping.Text = $"{ts.TotalMilliseconds}ms";
        }
        private void shortW_Click(object sender, EventArgs e)
        {
            if (short.TryParse(WValue.Text, out short obj))
            {
                w(obj);
            }
        }

        private void UshortW_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(WValue.Text, out ushort obj))
            {
                w(obj);
            }
        }

        private void intW_Click(object sender, EventArgs e)
        {
            if (int.TryParse(WValue.Text, out int obj))
            {
                w(obj);
            }
        }

        private void uintW_Click(object sender, EventArgs e)
        {
            if (uint.TryParse(WValue.Text, out uint obj))
            {
                w(obj);
            }
        }

        private void longW_Click(object sender, EventArgs e)
        {
            if (long.TryParse(WValue.Text, out long obj))
            {
                w(obj);
            }
        }
        private void UlongW_Click(object sender, EventArgs e)
        {
            if (ulong.TryParse(WValue.Text, out ulong obj))
            {
                w(obj);
            }
        }

        private void floatW_Click(object sender, EventArgs e)
        {
            if (float.TryParse(WValue.Text, out float obj))
            {
                w(obj);
            }
        }

        private void doubleW_Click(object sender, EventArgs e)
        {
            if (double.TryParse(WValue.Text, out double obj))
            {
                w(obj);
            }
        }

        private void stringW_Click(object sender, EventArgs e)
        {
            // var Result = mC3EEthernet?.WriteObject(Waddress.Text, WValue.Text, Encoding.ASCII);
            // if (Result != null && !Result.IsSuccess)
            // {
            //     MessageBox.Show(Result.Message);
            // }

        }

        private void DriveChoose_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mC3Drivers.ContainsKey(DriveChoose.SelectedIndex))
            {
                mC3EEthernet = mC3Drivers[DriveChoose.SelectedIndex];
            }
            else
            {
                mC3EEthernet = null;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    // mC3EEthernet?.ReadShort("D4700", 100);
                }
            });
        }

        private void BoolW_Click(object sender, EventArgs e)
        {
            if (bool.TryParse(WValue.Text, out bool obj))
            {
                w(obj);
            }
        }
    }
}