using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Word = Microsoft.Office.Interop.Word;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string FullName { get; set; }
        private readonly char[] _forbiddenChars = { '%', '&' };

        private bool ContainsForbiddenChars(string input)
        {
            return input.Any(c => _forbiddenChars.Contains(c));
        }

        private void GetFullName_Click(object sender, RoutedEventArgs e)
        {
            string URL = "http://localhost:4444/TransferSimulator/fullName";
            var request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";

            request.Proxy.Credentials = new NetworkCredential("student", "student");

            var response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string text = reader.ReadToEnd();
            var JsonText = JsonConvert.DeserializeObject<FullNameSerializator>(text);

            FullName = JsonText.value;
            TextBoxFullName.Text = FullName;
        }

        public void AddToWordTable(string[] rowData)
        {
            string filePath = @"F:\4 курс\АПИ модуль4 демоэкзамен\Прил_КОД 09.02.07-5-2025/ТестКейс.docx";

            Word.Application wordApp = new Word.Application();
            Word.Document doc = null;

            try
            {
                doc = wordApp.Documents.Open(filePath);
                wordApp.Visible = false;

                Word.Table table = doc.Tables[1];

                Word.Row row = table.Rows.Add();

                for(int i=0; i<rowData.Length; i++)
                {
                    row.Cells[i + 1].Range.Text = rowData[i];
                }

                doc.Save();
            }
            catch
            {
                MessageBox.Show("Ошибка");
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close(Word.WdSaveOptions.wdSaveChanges);
                }
                wordApp.Quit(Word.WdSaveOptions.wdSaveChanges);
            }
        }

        private void SendTestResult_Click(object sender, RoutedEventArgs e)
        {
            if(FullName == null)
            {
                MessageBox.Show("Данные не были получены");
                return;
            }

            bool isValidFullName = ContainsForbiddenChars(FullName);
            if (isValidFullName)
            {
                TextBoxResult.Text = "ФИО содержит запрещенные символы";
            }
            else
            {
                TextBoxResult.Text = "ФИО валидно";
            }

            string[] rowData = { "Столбец действие", FullName, !isValidFullName ? "Валидно" : "ФИО содержит запрещенные символы" };

            AddToWordTable(rowData);

            MessageBox.Show("Информация была добавлена в файлы!");
        }


    }
}
