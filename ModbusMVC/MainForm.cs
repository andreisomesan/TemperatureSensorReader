using ModbusUI.Model;
using ModbusUI.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModbusUI
{
    

    public partial class MainForm : Form
    {

        TempService service;
        readonly object sender;
        SerialDataReceivedEventArgs e;

        public MainForm()
        {
            InitializeComponent();
            this.service = new TempService(cbSerialPorts, this.sender, this.e, tbRegisterCount);
        }

        private void bOpenPort_Click(object sender, EventArgs e)
        {
            service.HandleStuff(cbSerialPorts, bOpenPort);
        }

        private void bReadInputRegisters_Click(object sender, EventArgs e)
        {
            service.Modbus_ReadInputRegisters(ushort.Parse(tbStartAddress.Text), ushort.Parse(tbRegisterCount.Text));
        }
       
        private void bReadHoldingRegisters_Click(object sender, EventArgs e)
        {
            service.Modbus_ReadHoldingRegisters(ushort.Parse(tbStartAddress.Text), ushort.Parse(tbRegisterCount.Text));
            service.Modbus_HandleFrame(tbRegisterCount);
            tbLog.Text = File.ReadAllText("Temperaturi.txt");

        }

        private void bClear_Click(object sender, EventArgs e)
        {
                       
            cbSerialPorts.SelectedIndex = -1;
            ClearAllText(this);
            
        }
        void ClearAllText(Control con)
        {
            foreach (Control c in con.Controls)
            {
                if (c is TextBox)
                    ((TextBox)c).Clear();
                else
                    ClearAllText(c);
            }
        }
        
        private void tbLog_TextChanged(object sender, EventArgs e)
        {
            
        }
        
    }
}
