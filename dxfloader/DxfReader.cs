using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace dxfloader
{
    public class DxfNode
    {
        public DxfNode(int typeCode, string original, object value)
        {
            _typeCode = typeCode;
            _original = original;
            _value = value;
        }

        public int TypeCode
        {
            get { return _typeCode; }
        }

        public string Original
        {
            get { return _original; }
        }

        public object Value
        {
            get { return _value; }
        }

        private int _typeCode;
        private string _original;
        private object _value;
    }

    public class DxfReader
    {
        public DxfReader(TextReader stm)
        {
            _stm = stm;
            _eof = false;
        }

        public DxfNode Read()
        {
            int nodeType = int.Parse(_stm.ReadLine());
            string original = _stm.ReadLine();
            if (nodeType == 0)
            {
                if (original == "EOF")
                {
                    _eof = true;
                }
                return new DxfNode(nodeType, original, original);
            }
            else if (nodeType < 10)
            {
                // 1..9
                // строки
                return new DxfNode(nodeType, original, original);
            }
            else if (nodeType < 60)
            {
                // 10..39
                return new DxfNode(nodeType, original, double.Parse(original, CultureInfo.InvariantCulture));
            }
            else if (nodeType < 80)
            {
                return new DxfNode(nodeType, original, short.Parse(original));
            }
            else if (nodeType < 90)
            {
                return null;
            }
            else if (nodeType < 100)
            {
                return new DxfNode(nodeType, original, int.Parse(original));
            }
            else if (nodeType >= 100 && nodeType < 110)
            {
                return new DxfNode(nodeType, original, original);
            }
            else if (nodeType >= 110 && nodeType < 150)
            {
                return new DxfNode(nodeType, original, double.Parse(original, CultureInfo.InvariantCulture));
            }
            else if (nodeType >= 170 && nodeType < 180)
            {
                return new DxfNode(nodeType, original, short.Parse(original));
            }
            else if (nodeType >= 270 && nodeType < 290)
            {
                return new DxfNode(nodeType, original, short.Parse(original));
            }
            else if (nodeType >= 290 && nodeType < 300)
            {
                int b = int.Parse(original);
                return new DxfNode(nodeType, original, b == 1);
            }
            else if (nodeType >= 330 && nodeType < 370)
            {
                return new DxfNode(nodeType, original, short.Parse(original, NumberStyles.HexNumber));
            }
            else if (nodeType >= 370 && nodeType < 390)
            {
                return new DxfNode(nodeType, original, short.Parse(original));
            }
            else if (nodeType >= 390 && nodeType < 400)
            {
                return new DxfNode(nodeType, original, int.Parse(original, NumberStyles.HexNumber));
            }
            else
            {
                return new DxfNode(nodeType, original, original);
            }
        }

        public bool IsEof
        {
            get { return _eof; }
        }

        private TextReader _stm;
        private bool _eof;
    }
}
