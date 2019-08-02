using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace osil
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        const int size_sample = 200;
        private void toolStripComboBox1_DropDown(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                toolStripComboBox1.Items.Clear();
                string[] opens = System.IO.Ports.SerialPort.GetPortNames();
                try
                {
                    if(opens.Length == 0)
                    {
                        //toolStripComboBox1.Items.Add("No Serial Port!");
                    }
                    else
                    {
                        foreach (string k in opens)
                        {
                            toolStripComboBox1.Items.Add(k);
                        }
                    }
                    
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message, "Error Opening!");
                }
            }
            else
            {
                serialPort1.Close();
            }
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
            

        }
        int x = 0;
        int cnt = 0;
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //doing(0);
            
            if (serialPort1.BytesToRead >= size_sample*4)
            {
                solve(cnt);
                cnt++;
                cnt %= 10;
                x = 0;
            }
        }
        int abx = 0;
        int axis = 200;
        void solve(int x)
        {

            byte[] k0 = new byte[size_sample * 2];
            int[] k1 = new int[size_sample];
            int[] k2 = new int[size_sample];
            int max=0;

            serialPort1.NewLine = "AA";
            string delta = serialPort1.ReadLine();
            
            for (int i = 0; i < size_sample; i++)
            {
                
                try
                {
                    string pp = delta.Substring(i * 4, 4);
                    k1[i] = Convert.ToInt32(pp);
                }
                catch (Exception)
                {
                    serialPort1.DiscardInBuffer();
                    break;
                    //throw;
                }
                
            }
            Array.Copy(k1, k2,200);
            Array.Sort(k2);

            {
                chart1.Invoke(
                    new MethodInvoker(delegate
                    {
                        chart1.Series[x].Points.SuspendUpdates();
                        chart1.Series[Math.Abs(x-9)].Points.Clear();
                        chart1.Series[x].Points.ResumeUpdates();

                        for (int i = 0; i < axis; i++)
                        {
                            if (k1[i] <= k2[190] && k1[i] >= k2[10])
                            {
                                chart1.Series[x].Points.AddXY(i, k1[i] & (0xffff - (0xffff >> (16 - abx))));
                                max += k1[i];
                            }
                                    
                                    
                        }
                        max /= 160;
                        chart1.Series[x].Name = x + ": " + (max);
                    }
                    ));
            }
        }
        int errors=0;
        private void serialPort1_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            errors++;
            this.Invoke(new MethodInvoker(delegate {this.Text ="errors: " + errors.ToString();}));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.ReceivedBytesThreshold = size_sample * 4 +2;

        }
        private void toolStripComboBox1_DropDownClosed(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                toolStripComboBox1.Items.Clear();
                serialPort1.Close();
                serialPortToolStripMenuItem.Text = "SerialPort";
            }
            else
            {
                if (toolStripComboBox1.Items.Count > 0)
                {
                    serialPort1.PortName = (string)toolStripComboBox1.SelectedItem;
                    serialPort1.Open();
                    serialPortToolStripMenuItem.Text = "SerialPort: [" + serialPort1.PortName + "]";
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void toolStripComboBox3_Click(object sender, EventArgs e)
        {
        }

        private void toolStripComboBox3_DropDownClosed(object sender, EventArgs e)
        {
            abx = Convert.ToInt16(toolStripComboBox3.SelectedItem);
        }

        private void toolStripComboBox2_DropDownClosed(object sender, EventArgs e)
        {
            axis = Convert.ToInt16(toolStripComboBox2.SelectedItem);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        byte[] send = new byte[2];

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            send[0] = Convert.ToByte( numericUpDown1.Value / 10);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            send[1] = 0xf0;
            try
            {
                serialPort1.Write(send, 0, 1);
            }
            catch (Exception)
            {
                //throw;
            }
        }
    }
}
