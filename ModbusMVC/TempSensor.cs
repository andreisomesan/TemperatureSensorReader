using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusUI.Model
{
    class TempSensor
    {
        float TempFloat;
        

        public TempSensor() { }

        public void setTempFloat(float temp) {
            this.TempFloat = temp;
        }

        public float getTempFloat() {
            float result = TempFloat;
            return result;
        }

       

    }
}
