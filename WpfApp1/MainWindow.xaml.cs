﻿using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Text;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string _fileName = null;
        private Read1c _ptrRead1c = null;
        private enum argState
        {
            None,
            Ok,
            Fail,
        }
        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnClickOpenFile(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("OpenFile button is clicked");
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Текстовый файл|*.txt";

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                this._fileName = openFileDialog.FileName;
                Debug.WriteLine($"{this._fileName}");
                if (!ReadFile(this._fileName))
                {
                    var res = MessageBox.Show("Ошибка чтения файла.\nОткрыть в блокноте?",
                        Read1c.State.ToString(),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);
                    if (res == MessageBoxResult.Yes)
                        Process.Start("notepad.exe", this._fileName);
                }
                else this.ApplyReader();
            }
            else
            {
                Debug.WriteLine("no result");
            }
        }

        private void OnClickSaveAs(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SaveAs button is clicked");

            Read1c read1C = this._ptrRead1c;
            if (read1C == null) return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            // Default file name
            dlg.FileName = read1C.PayerName.Replace("\"", string.Empty) + " " +
                DateTime.Now.ToString().Replace(":", "-") +
                ".txt";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Текстовый файл|*.txt";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                SaveToFile(filename);
            }
        }

        // removes all characters except digits = {0, 1, 2 - 9}
        private void OnLostFocusTextBoxCleanDigits(object sender, RoutedEventArgs e)
        {
            TextBox tb = e.Source as TextBox;
            string user_text = tb.Text;
            if (user_text == null) return;
            tb.Text = Regex.Replace(user_text, @"[^0-9]", "");
        }


        private void SaveToFile(string filename)
        {
            bool isNewDate = CheckBoxDate.IsChecked ?? false;
            bool isNewNumbers = CheckBoxNumbers.IsChecked ?? false;
            bool isNewRequisites = CheckBoxRequisites.IsChecked ?? false;
            string newDate = String.Empty;
            int newNumerationNumber = 0;
            string newAccount = String.Empty;
            string newBankName = String.Empty;
            string newBankCity = String.Empty;
            string newBankKS = String.Empty;
            string newBankBik = String.Empty;

            // read all needed data
            if (isNewDate)
            {
                DateTime? selectedDate = NewDatePicker.SelectedDate;
                if (selectedDate == null)
                {
                    MessageBox.Show("Выбрано изменение даты, но значение даты не установлено!", "Выберите дату",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                newDate = selectedDate?.ToString("dd.MM.yyyy");
            }
            if (isNewNumbers)
            {
                String newNumerationString = NewNumerationFrom.Text;
                if (newNumerationString.Length == 0)
                {
                    MessageBox.Show("Выбрано изменение нумерации, но не задано новое начальное значение!", "Выберите номер",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                try
                {
                    newNumerationNumber = Int32.Parse(newNumerationString);
                }
                catch (FormatException)
                {
                    Debug.WriteLine("{0}: Bad Format", newNumerationString);
                    MessageBox.Show("Неправильный формат числа", "Ошибка номера",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                catch (OverflowException)
                {
                    Debug.WriteLine("{0}: Overflow", newNumerationString);
                    MessageBox.Show("Слишком большое или слишком маленькое число", "Ошибка номера",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

            }
            if (isNewRequisites)
            {
                newAccount = PayerAccount.Text;
                newBankName = PayerBankName.Text;
                newBankCity = PayerBankCity.Text;
                newBankKS = PayerBankKS.Text;
                newBankBik = PayerBankBik.Text;

                if (newAccount.Length != 20 || !newAccount.All(char.IsDigit))
                {
                    MessageBox.Show("Номер расчетного счёта должен состоять из 20 цифр", "Ошибка номера счёта",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (newBankName.Length == 0)
                {
                    MessageBox.Show("Наименование банка не может быть пустым", "Ошибка наименования",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (newBankCity.Length == 0)
                {
                    MessageBox.Show("Наименование города банка не может быть пустым", "Ошибка города банка",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (newBankKS.Length != 20 || !newBankKS.All(char.IsDigit))
                {
                    MessageBox.Show("Номер корреспондентского счёта должен состоять из 20 цифр", "Ошибка номера корр. счёта",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (newBankBik.Length != 9 || !newBankBik.All(char.IsDigit))
                {
                    MessageBox.Show("Номер БИК банка должен состоять из 9 цифр", "Ошибка БИК",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }

            var output_list = this._ptrRead1c.ApplyChanges(isNewDate, isNewNumbers, isNewRequisites, newDate,
                newNumerationNumber, newAccount, newBankName, newBankCity, newBankKS, newBankBik);

            File.WriteAllLines(filename, output_list, Encoding.GetEncoding("windows-1251"));
            return;
        }

        private void ReadArguments(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            Thread.Sleep(500);
            switch (get_first_arg())
            {
                case argState.None:
                    Debug.WriteLine("Нет аргументов");
                    break;
                case argState.Fail:
                    MessageBox.Show("Ошибка в передаваемом аргументе: " + Environment.GetCommandLineArgs()[1]);
                    break;
                case argState.Ok:
                    Debug.WriteLine("Корректный аргумент");
                    if (!this.ReadFile(this._fileName))
                    {
                        Process.Start("notepad.exe", this._fileName);
                        // this.Close();
                    }
                    else this.ApplyReader();
                    break;
            }
            this.Cursor = null;
        }

        private argState get_first_arg()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                Debug.WriteLine($"Found an argument {args[1]}");
                System.IO.FileInfo fi = null;
                try
                {
                    fi = new System.IO.FileInfo(args[1]);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine("Argument exception");
                    return argState.Fail;
                }
                catch (System.IO.PathTooLongException)
                {
                    Debug.WriteLine("PathTooLongException exception");
                    return argState.Fail;
                }
                catch (NotSupportedException)
                {
                    Debug.WriteLine("NotSupportedException exception");
                    return argState.Fail;
                }
                if (ReferenceEquals(fi, null))
                {
                    Debug.WriteLine("File name is not valid");
                    return argState.Fail;
                }
                else
                {
                    this._fileName = fi.FullName;
                    Debug.WriteLine($"Determined a correct file name: {this._fileName}");
                    return argState.Ok;
                }
            }
            else
            {
                return argState.None;
            }
        }

        private bool ReadFile(string fileName)
        {
            Debug.WriteLine($"Reading file {fileName}");
            Read1c read1c = Read1c.FromFile(fileName);

            if (read1c == null)
            {
                Debug.WriteLine("Ошибка чтения файла: " + Read1c.State.ToString());
                return false;
            }

            // save the Reader1c instance for updating main window and creating output file
            this._ptrRead1c = read1c;

            return true;
        }

        private void ApplyReader()
        {
            Read1c read1c = this._ptrRead1c;
            if (read1c == null) return;

            TotalSum.Text = "Общая сумма: " + read1c.TotalSum;
            TotalCount.Text = "Количество платежей: " + read1c.TotalCount;
            PayerName.Text = read1c.PayerName;
            PayerAccount.Text = read1c.PayerAccount;
            PayerBankName.Text = read1c.PayerBankName;
            PayerBankCity.Text = read1c.PayerBankCity;
            PayerBankKS.Text = read1c.PayerBankKS;
            PayerBankBik.Text = read1c.PayerBankBik;

            MainList.ItemsSource = read1c.PaymentsList;

            CheckBoxDate.IsChecked = false;
            CheckBoxNumbers.IsChecked = false;
            CheckBoxRequisites.IsChecked = false;

            NewDatePicker.SelectedDate = DateTime.Now;
            try
            {
                NewNumerationFrom.Text = read1c.PaymentsList[0].Number;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                NewNumerationFrom.Text = "1";
            }
            this.Title = "1c_to_kl Редактор [ " + this._fileName + " ]";
        }

    }
}
