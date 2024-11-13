using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricEngine
{
    public class ResultObject
    {
        public bool state;
        public string message;
        public float threshold;
        public float distance;

        public ResultObject() {
            state = false;
            message = "";
            threshold = 0;
            distance = 0;
        }

        public ResultObject(bool state, string message, float threshold, float distance)
        {
            this.state = state;
            this.message = message;
            this.threshold = threshold;
            this.distance = distance;
        }

        public ResultObject(bool state, string message)
        {
            this.state = state;
            this.message = message;
            this.threshold = 0;
            this.distance = 0;
        }

    }
}
