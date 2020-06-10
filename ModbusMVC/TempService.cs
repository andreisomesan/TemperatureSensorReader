using ModbusUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModbusUI.Service {

    enum FUNCTION_CODE
    {
        READ_HOLDING_REGISTERS = 0x03,
        READ_INPUT_REGISTERS = 0x04
    }

    class TempService
    {

        SerialPort sp;
        byte[] ModbusTxBuffer = new byte[255];
        byte[] ModbusRxBuffer = new byte[255];
        byte TxIndex;
        byte RxIndex;
        byte RxBufferLength;
        byte ModuleAddress = 0x40;
        float TempFloat;
        ushort TempInt;
        object sender;
        SerialDataReceivedEventArgs e;
        TextBox tbRegisterCount;


        public TempService(ComboBox cbSerialPorts, object sender, SerialDataReceivedEventArgs e, TextBox tbRegisterCount) {
            cbSerialPorts.Items.AddRange(SerialPort.GetPortNames());
            if (cbSerialPorts.Items.Count != 0)
            {
                cbSerialPorts.SelectedIndex = 0;
            }

            this.sender = sender;
            this.e = e;
            this.tbRegisterCount = tbRegisterCount;

            sp = new SerialPort();
            sp.BaudRate = 9600;
            sp.DataBits = 8;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.None;
            sp.DataReceived += Sp_DataReceived;
        }

        public void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int NoOfBytes;
            int ByteIndex;
                        
            NoOfBytes = sp.BytesToRead;
            for (ByteIndex = 0; ByteIndex < NoOfBytes; ByteIndex++)
            {
                ModbusRxBuffer[RxIndex] = (byte)sp.ReadByte();

                if (RxIndex == 2)
                {
                    RxBufferLength = (byte)(3 + ModbusRxBuffer[RxIndex] + 2);
                }

                if (RxIndex == (RxBufferLength - 1))
                {
                    LogArray(ModbusRxBuffer, RxBufferLength, 1);
                    
                   Modbus_HandleFrame(this.tbRegisterCount);
                }

                RxIndex++;
            }

            Console.WriteLine("Done");
        }

       

        public void Modbus_ReadInputRegisters(ushort StartingAddress, ushort Quantity)
        {
            ushort checksum;
            TxIndex = 0;
            RxIndex = 0;
            RxBufferLength = 0;

            ModbusTxBuffer[TxIndex++] = ModuleAddress;
            ModbusTxBuffer[TxIndex++] = (byte)FUNCTION_CODE.READ_INPUT_REGISTERS;
            ModbusTxBuffer[TxIndex++] = (byte)(StartingAddress >> 8);
            ModbusTxBuffer[TxIndex++] = (byte)(StartingAddress & 0xFF);
            ModbusTxBuffer[TxIndex++] = (byte)(Quantity >> 8);
            ModbusTxBuffer[TxIndex++] = (byte)(Quantity & 0xFF);

            checksum = GetCRC(ModbusTxBuffer, TxIndex);

            ModbusTxBuffer[TxIndex++] = (byte)(checksum & 0xFF); 
            ModbusTxBuffer[TxIndex++] = (byte)(checksum >> 8);

            LogArray(ModbusTxBuffer, TxIndex, 0);
            sp.Write(ModbusTxBuffer, 0, TxIndex);
        }

        public void Modbus_ReadHoldingRegisters(ushort StartingAddress, ushort Quantity)
        {
            ushort checksum;
            TxIndex = 0;
            RxIndex = 0;
            RxBufferLength = 0;

            ModbusTxBuffer[TxIndex++] = ModuleAddress;
            ModbusTxBuffer[TxIndex++] = (byte)FUNCTION_CODE.READ_HOLDING_REGISTERS;
            ModbusTxBuffer[TxIndex++] = (byte)(StartingAddress >> 8);
            ModbusTxBuffer[TxIndex++] = (byte)(StartingAddress & 0xFF);
            ModbusTxBuffer[TxIndex++] = (byte)(Quantity >> 8);
            ModbusTxBuffer[TxIndex++] = (byte)(Quantity & 0xFF);

            checksum = GetCRC(ModbusTxBuffer, TxIndex);

            ModbusTxBuffer[TxIndex++] = (byte)(checksum & 0xFF);
            ModbusTxBuffer[TxIndex++] = (byte)(checksum >> 8);


            LogArray(ModbusTxBuffer, TxIndex, 0);
            sp.Write(ModbusTxBuffer, 0, TxIndex);

        }

        public ushort GetCRC(byte[] message, byte length)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < length; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }

            return CRCFull;
        }

        public void LogArray(byte[] buffer, byte size, byte dir)
        {
            byte index;

            if (dir == 0)
            {
                //Console.Write("-> ");
            }
            else
            {
                //Console.Write("<- ");
            }

            for (index = 0; index < size; index++)
            {
               // Console.Write(buffer[index].ToString("X2") + " ");
            }

            //Console.WriteLine();
        }

        public void Modbus_HandleFrame(TextBox tb)
        {
            RxIndex = 0;
            RxBufferLength = 0;
             List<TempSensor> tempArray = new List<TempSensor>();
            //float[] list = new float[4];
            for (ushort i = 0; i < ushort.Parse(tb.Text); i++)
            {
                TempInt = (ushort)((ModbusRxBuffer[2 * i + 3] << 8) + ModbusRxBuffer[2 * i + 4]);
                TempFloat = (float)TempInt / 100;
                //Console.WriteLine(TempFloat.ToString());

                TempSensor temps = new TempSensor();
                temps.setTempFloat(TempFloat);
                tempArray.Add(temps);
                // list[i] = TempFloat;
            }
            string result = "";
            for (int i = 0; i < tempArray.Count(); i++)
            {
                result += "T" + i.ToString() + " " + tempArray.ElementAt(i).getTempFloat().ToString() + " // ";
            }
            File.WriteAllText("Temperaturi.txt", result);
            //Console.WriteLine(result);
            

            
        }

        public void HandleStuff(ComboBox cbSerialPorts, Button bOpenPort) {
            if (bOpenPort.Text == "Open")
            {
                bOpenPort.Text = "Close";

                if (cbSerialPorts.SelectedIndex != -1)
                {
                    sp.PortName = cbSerialPorts.Items[cbSerialPorts.SelectedIndex].ToString();
                    sp.Open();
                }
            }
            else
            {
                bOpenPort.Text = "Open";
                sp.Close();
            }
        }

    }
}
