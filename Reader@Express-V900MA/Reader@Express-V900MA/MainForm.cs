using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Xml;
using MySql.Data.MySqlClient;
namespace Reader_Express_V900MA
{
    public partial class MainForm : Form
    {
        delegate void SetTextCallback(string text); // Khai bao delegate SetTextCallBack voi tham so string
        public int size;
        SerialPort sPort = new SerialPort();
        String inputData = String.Empty;
        public MainForm()
        {
            InitializeComponent();
            tsFunctions.Visible = true;
            tsMainControls.Visible = true;

            String[] port = SerialPort.GetPortNames();
            tsCbbPort.Items.AddRange(port);
            sPort.ReadTimeout = 1000;
            sPort.DataReceived += new SerialDataReceivedEventHandler(DataReceive);

        }
        #region MySql
        static String connString = "Server=localhost;Database=rfidreader;Port=3306;User ID=root;Password=";
        MySqlConnection conn = new MySqlConnection(connString);
        //Hàm Open kết nối tới cơ sở dũ liệu trên localhost
        MySqlCommand cmd;
        private void OpenConnection()
        {
            try
            {
                conn.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        // Đóng kết nối lại
        private bool CloseConnection()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void Delete()
        {
            string query = "DELETE FROM rfidreader.rfid";
            cmd = new MySqlCommand(query, conn);
            MySqlDataReader MyReader;
            try
            {
                OpenConnection();
                MyReader = cmd.ExecuteReader();
                MessageBox.Show("Delete all Database", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                CloseConnection();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                CloseConnection();
            }
        }
        #endregion
        private void tsModelType_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem nItem = sender as ToolStripMenuItem;
            tsModelType.Tag = nItem.Tag;
            tsModelType.Text = nItem.Text;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tsModelType.Tag = tsModelIDRO900MA.Tag;
            tsModelType.Text = tsModelIDRO900MA.Text;
            tsCbbPort.SelectedIndex = 0;
            tsCbbBauds.SelectedIndex = 10;
            tsCbbDataSize.SelectedIndex = 2;
            tsCbbParity.SelectedIndex = 0;
            tsCbbStopBit.SelectedIndex = 0;
            

        }

        private void btTypeTag_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem nItem = sender as ToolStripMenuItem;
            tsCbbTypeTag.Tag = nItem.Tag;
            tsCbbTypeTag.Text = nItem.Text;
        }
        private void tsCbbPort_Click(object sender, EventArgs e)
        {
            if (sPort.IsOpen)
            {
                sPort.Close();
            }
            sPort.PortName = tsCbbPort.SelectedItem.ToString();
        }

        private void tsBauds_Click(object sender, EventArgs e)
        {
            if (sPort.IsOpen)
            {
                sPort.Close();
            }
            sPort.BaudRate = Convert.ToInt32(tsCbbBauds.Text);

        }

        private void tsCbbDataSize_Click(object sender, EventArgs e)
        {
            if (sPort.IsOpen)
            {
                sPort.Close();
            }
            sPort.DataBits = Convert.ToInt32(tsCbbDataSize.Text);
        }

        private void tsCbbParity_Click(object sender, EventArgs e)
        {
            String parity = tsCbbParity.SelectedItem.ToString();
            if (sPort.IsOpen)
            {
                sPort.Close();
            }
            switch (parity)
            {
                case "None":
                    {
                        sPort.Parity = Parity.None;
                    }break;
                case "Even":
                    {
                        sPort.Parity = Parity.Even;
                    }break;
                case "Odd":
                    {
                        sPort.Parity = Parity.Odd;
                    } break;
                case "Mark":
                    {
                        sPort.Parity = Parity.Mark;
                    } break;
                case "Space":
                    {
                        sPort.Parity = Parity.Space;
                    }break;
            }
        }
        private void tsCbbStopBit_Click(object sender, EventArgs e)
        {
            string stopBit = tsCbbStopBit.SelectedItem.ToString();
            if (sPort.IsOpen)
            {
                sPort.Close();
            }
            switch (stopBit)
            {
                case "1":
                    {
                        sPort.StopBits = StopBits.One;
                    }break;
                case "1.5":
                    {
                        sPort.StopBits = StopBits.OnePointFive;
                    } break;
                case "2":
                    {
                        sPort.StopBits = StopBits.Two;
                    } break;

            }
        }
        
        private void tsBtConnect_Click(object sender, EventArgs e)
        {
            try
            {
                sPort.Open();
                lbStatus.Text = "Connected to Port: " + tsCbbPort.SelectedItem.ToString();
                OpenConnection();
                tsBtDisconnect.Enabled = true;
                tsBtConnect.Enabled = false;
                tsCbbPort.Enabled = false;
                tsCbbBauds.Enabled = false;
                tsCbbDataSize.Enabled = false;
                tsCbbParity.Enabled = false;
                tsCbbStopBit.Enabled = false;
                tsCbbHandshake.Enabled = false;
                tsLbAddress.Enabled = false;
                tsLbBaud.Enabled = false;
                tsLbDatasize.Enabled = false;
                tsLbParity.Enabled = false;
                tsLbHandShake.Enabled = false;
                tsLbStopBit.Enabled = false;
                inputData = sPort.ReadExisting().ToString();
                txbDisplay.Text = inputData;
               // String command = ">s\r";
              //  sPort.Write(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Connect to Port: " + tsCbbPort.SelectedItem.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
               MessageBox.Show(ex.ToString());          
            }
        }
        private void DataReceive(object obj, SerialDataReceivedEventArgs e)
        {
            inputData = sPort.ReadExisting().ToString();
           
            if (inputData != String.Empty)
            {         
                SetText(inputData);
            }       
        }

        private void SetText(string text)
        {
            size = inputData.Length;
            if (this.txbDisplay.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txbDisplay.Text += text;
                string tag = this.txbDisplay.SelectedText;
                tsRead1.Text = text;
            }                  
        }
        private void tsCbbStop_Click(object sender, EventArgs e)
        {
            string command = ">a3\r\n";
            if (sPort.IsOpen)
            {
                sPort.Write(command);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (sPort.IsOpen)
            {
                if (txSend.Text == "") MessageBox.Show("Please, Input Data", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    sPort.Write(txSend.Text);
                    MessageBox.Show("OK");
                }
            }
            else  MessageBox.Show("Port not Open!!!!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txSend.Clear();
        }

        private void tsBtDisconnect_Click(object sender, EventArgs e)
        {
            sPort.Close();
            tsBtConnect.Enabled = true;
            tsBtDisconnect.Enabled = false;
            lbStatus.Text = "Disconnect";

            tsBtConnect.Enabled = true;
            tsCbbPort.Enabled = true;
            tsCbbBauds.Enabled = true;
            tsCbbDataSize.Enabled = true;
            tsCbbParity.Enabled = true;
            tsCbbStopBit.Enabled = true;
            tsCbbHandshake.Enabled = true;

            tsLbAddress.Enabled = true;
            tsLbBaud.Enabled = true;
            tsLbDatasize.Enabled = true;
            tsLbParity.Enabled = true;
            tsLbHandShake.Enabled = true;
            tsLbStopBit.Enabled = true;
        }

        private void tsCbbInventoryMulti_Click(object sender, EventArgs e)
        {
            string command = ">ad\r";
            if (sPort.IsOpen)
            {
                sPort.Write(command);
            }
            tsCbbInventoryMulti.Enabled = false;
            tsCbbStop.Enabled = true;

        }

        private void tsBtnClear_Click(object sender, EventArgs e)
        {
            txbDisplay.Clear();
            sPort.DiscardInBuffer();
        }

        private void tsBtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsCbbClearDatabase_Click(object sender, EventArgs e)
        {

        }

        private void tsBtnDatabase_Click(object sender, EventArgs e)
        {

        }
 
    }

}
