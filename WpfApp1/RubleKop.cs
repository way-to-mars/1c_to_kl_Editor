using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WpfApp1
{
    internal class RubleKop
    {
        private long _value;
        private static char _input_delimiter = '.';
        private static char _output_delimiter = '-';
        private static char _group_delimiter = ' ';

        public int Rubles
        {
            get
            {
                return (int)(this._value / 100);
            }
        }

        public int Kopeyki
        {
            get
            {
                return (int)(this._value % 100);
            }
        }


        public RubleKop(int rubles, int kopeyki)
        {
            this._value = (long)rubles * 100 + (long)kopeyki;
        }

        private RubleKop(long value)
        {
            _value = value;
        }

        public override string ToString()
        {
            var culture = new CultureInfo("ru-RU")
            {
                NumberFormat =
                {
                    NumberGroupSeparator = _group_delimiter.ToString(),
                },
            };

            return this.Rubles.ToString("#,#", culture) + _output_delimiter.ToString() + this.Kopeyki.ToString("00");
        }

        public static RubleKop FromString(string str)
        {
            string[] str_list = str.Split(_input_delimiter);
            int rub = 0;
            int kop = 0;
            try
            {
                rub = Int32.Parse(str_list[0]);
                if (str_list.Length > 1)
                {
                    kop = Int32.Parse(str_list[1]);
                }
            }
            catch (FormatException) { return new RubleKop(0, 0); }
            return new RubleKop(rub, kop);
        }

        public static RubleKop operator +(RubleKop a, RubleKop b)
        => new RubleKop(a._value + b._value);
    }
}
