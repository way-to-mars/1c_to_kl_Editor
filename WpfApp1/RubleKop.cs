﻿using System;
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
            string[] str_list;
            int rub;
            int kop;
            try
            {
                str_list = str.Split(_input_delimiter);
                rub = Int32.Parse(str_list[0]);
                if (str_list.Length > 1)
                    switch (str_list[1].Length)
                    {
                        case 0: kop = 0; break;                                                 // "1230." = 1230-00
                        case 1: kop = 10 * Int32.Parse(str_list[1]); break;                     // "1230.5" = 1230-50
                        case 2: kop = Int32.Parse(str_list[1]); break;                          // "1230.57" = 1230-57
                        default: kop = Int32.Parse(String.Concat(str_list[1].Take(2))); break;  // "1230.57489" = 1230-57 - truncate
                    }
                else kop = 0;
            }
            catch (Exception) { return RubleKop.ZERO; }
            return new RubleKop(rub, kop);
        }

        public static RubleKop operator +(RubleKop a, RubleKop b)
        => new RubleKop(a._value + b._value);
    }
}
