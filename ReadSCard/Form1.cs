using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GS.SCard;

namespace ReadSCard
{

    public partial class Form1 : Form
    {

        WinSCard scard = new WinSCard();
        string str;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            str = System.IO.Directory.GetCurrentDirectory();
        }

        private void btnread_Click(object sender, EventArgs e)
        {
            try
            {

                scard.EstablishContext();
                scard.ListReaders();

                //chioce reader
                string readerName = scard.ReaderNames[int.Parse("0")];

                txtlog.Text = "Wait for card present...";

                scard.WaitForCardPresent(readerName);

                //建立 Smart Card 連線
                scard.Connect(readerName);

                Console.WriteLine("ATR: 0x" + scard.AtrString);

                byte[] cmdApdu = { 0x00, 0xA4, 0x04, 0x00, 0x10, 0xD1, 0x58, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00 };
                byte[] cmdApdu2 = { 0x00, 0xca, 0x11, 0x00, 0x02, 0x00, 0x00 };


                byte[] respApdu = new byte[2];
                byte[] respApdu2 = new byte[59];

                int respLength = respApdu.Length;
                int respLength2 = respApdu2.Length;

                //下達 Select Profile 檔的 APDU
                scard.Transmit(cmdApdu, cmdApdu.Length, respApdu, ref respLength);

                //下達讀取Profile指令
                scard.Transmit(cmdApdu2, cmdApdu2.Length, respApdu2, ref respLength2);

                txtlog.Text = "ok...";


                //健保卡ID
                textBox1.Text = Encoding.Default.GetString(respApdu2, 0, 12);
                //姓名
                textBox2.Text = Encoding.Default.GetString(respApdu2, 12, 6);
                //身份証字號
                textBox3.Text = Encoding.Default.GetString(respApdu2, 32, 10);
                //生日
                textBox4.Text = Encoding.Default.GetString(respApdu2, 43, 2) + "/" + Encoding.Default.GetString(respApdu2, 45, 2) + "/"
                    + Encoding.Default.GetString(respApdu2, 47, 2);
                //姓別
                textBox5.Text = Encoding.Default.GetString(respApdu2, 49, 1);
                //發卡日期
                textBox6.Text = Encoding.Default.GetString(respApdu2, 51, 2) + "/" + Encoding.Default.GetString(respApdu2, 53, 2) + "/"
                    + Encoding.Default.GetString(respApdu2, 55, 2);

            }
            catch (WinSCardException ex)
            {
                Console.WriteLine(ex.WinSCardFunctionName + " 0x" + ex.Status.ToString("X08") + " " + ex.Message);
            }
            catch (Exception ex)
            {
                txtlog.Text = "無卡片 或 卡片接觸有問題...";
            }
            finally
            {
                scard.Disconnect();
                scard.ReleaseContext();
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            txtlog.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveconfig();
            ProcessStartInfo Info2 = new ProcessStartInfo();
            Info2.FileName = "zebra.bat";//執行的檔案名稱
            Info2.WorkingDirectory = str + @"\printconfig\";//檔案所在的目錄
            Process.Start(Info2);
        }

        private void saveconfig()
        {
            StreamWriter sw = new StreamWriter(str + @"\printconfig\YK.TXT",false,Encoding.UTF8);

            sw.Write(textBox3.Text + "," + textBox2.Text + "," + textBox4.Text + "," + textBox5.Text + "," + textBox7.Text + "," + textBox8.Text+","+ DateTime.Now.ToString("yyyy/MM/dd") + "\r\n");

            sw.Close();
        }


    }
}
