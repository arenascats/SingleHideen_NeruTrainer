using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Drawing;
namespace NeruTrainer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const int Liner = 0, Ramp = 1, Threshold = 2;
        const int Sigmoid = 3, DoubleSigmoid = 4;
        private int CurrentTrigFunction = Sigmoid;
        private int RetrainTime = 1;
        public int BeenInit = 1;
        public int SampleNum; //样本数量计数器
        public double[,] InputData = new double[1000, 15];//最大支持1000组训练数据，每组15个
        public double[,] OutputData = new double[1000, 15];
        public double[,] FactOutputData = new double[1000, 15];
        public double[,] HideW = new double[15, 15];//各个输入节点到隐含层的权重，比如HideW[3,4]就是输入3到隐藏4的权重
        public double[,] OutW = new double[15, 15];//各个隐含节点到输出层
        public double Rate_InputToHideW = 0.05;//权重变化率
        public double Rate_HideToOutW = 0.05;//权重变化率
        public double Rate_ThresholdIH = 0.9;
        public double Rate_ThresholdHO = 0.9;

        public double[] IHThreshold = new double[50];
        public double[] HOThreshold = new double[50];

        private int InputNodeNum, OutputNodeNum, HideNodeNum; //隐藏层节点数
        private double[] TrainInputValue = new double[15];
        private double[] TrainOutputValue = new double[15];
        private double[] CurrentHideInputValue = new double[15];
        private double[] CurrentHideOutputValue = new double[15];
        private double[] CurrentOutInputValue = new double[15];
        private double[] CurrentOutOutputValue = new double[15];
        private double[] OutputOffset = new double[15];
        private double[] HideOffset = new double[15];
        double e;//误差
        private double Error;
        private double ErrorC = 1;

        public MainWindow()
        {
            InitializeComponent();
            InitCbBox();
        }
        public void Init()//初始化，从UI读入数值以对变量进行赋值
        {

            InputNodeNum = Convert.ToInt16(tbInputNodeNum.Text);
            OutputNodeNum = Convert.ToInt16(tbOutputNodeNum.Text);
            HideNodeNum = Convert.ToInt16(tbHideNodeNum.Text);
            Rate_HideToOutW = Convert.ToDouble(tbRateHide.Text);
            Rate_InputToHideW = Convert.ToDouble(tbRateOut.Text);
            CurrentTrigFunction = cbTrigFunction.SelectedIndex;//设置函数
            RetrainTime = Convert.ToInt16(tbReTime.Text);


        }
        public void InitCbBox()
        {
            cbTrigFunction.Items.Add("Liner");
            cbTrigFunction.Items.Add("Ramp");
            cbTrigFunction.Items.Add("Threshold");
            cbTrigFunction.Items.Add("Sigmoid");
            cbTrigFunction.Items.Add("DoubleSigmoid");
            cbTrigFunction.SelectedIndex = 3;
        }
        public void InitHideW(double HideW, double per)//初始化输入层到隐藏层的权重w，使用方式为InitHideW(HideW[SampleNum])
        {
            System.Random rand = new Random();
            int i = rand.Next();
            HideW = ((2.0 * i) / per) - 1;
        }
        public void InitOutW(double OutW, double per)//初始化输入层到隐藏层的权重w，使用方式例InitOutW(HideW[SampleNum] , 4)
        {
            System.Random rand = new Random();
            int j = rand.Next();
            OutW = (2.0 * j / per) - 1;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string FileName = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "文本文件(*.txt)|*.txt|CSV表格(*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == true)
            {
                try
                {
                    FileName = openFileDialog1.FileName;
                    tbReadPath.Text = FileName;
                    ReadFileAndGiveDataSerial(FileName, Convert.ToInt16(tbInputNodeNum.Text), Convert.ToInt16(tbOutputNodeNum.Text));
                }
                catch
                {
                    MessageBox.Show("在读取文件时发生错误");
                }
            }
        }

        private void btStartTrain_Click(object sender, RoutedEventArgs e)
        {
            if (BeenInit == 1)
            {
                Init();
                RandInitWMode2();
                BeenInit = 0;
            }

            for (int time = 0; time < Convert.ToInt32( tbReTime.Text); time++)
                TrainRun();
            DataDisplay();

            btDisplayW.IsEnabled = true;
            btSaveTrain.IsEnabled = true;
        }

        public void ReadFileAndGiveDataSerial(string Filepath, int innode, int outnode)//读取文件中的训练数据
        {
            StreamReader sr = new StreamReader(Filepath, Encoding.Default);
            String line;
            Regex reg = new Regex(@"^\d+\.\d+$");
            string[] cut = new string[20];
            int SampleNumCount = 0;
            try
            {

                while ((line = sr.ReadLine()) != null)
                {
                    cut = Regex.Split(line, ",", RegexOptions.IgnoreCase);//按照逗号切割字符串并保存
                    for (int i = 0; i < innode; i++)
                    {
                       // if (reg.IsMatch(cut[i]))
                            InputData[SampleNumCount, i] = Convert.ToDouble(cut[i]);

                    }
                    for (int j = 0; j < outnode; j++)
                    {
                      //  if (reg.IsMatch(cut[j]))
                            OutputData[SampleNumCount, j] = Convert.ToDouble(cut[innode + j]);
                    }
                    SampleNumCount++;
                }
            }
            catch
            {
                MessageBox.Show("文件读取分割过程失败", "检查文件是否有字符串，是否有空格");
            }
            SampleNum = SampleNumCount;
            Console.WriteLine("SampleNum = {0}", SampleNum);
        }
        public void InitCurrentTrainValue(int SampleNum)
        {
            for (int i = 0; i < InputNodeNum; i++)
                TrainInputValue[i] = InputData[SampleNum, i];
            for (int j = 0; j < OutputNodeNum; j++)
                TrainOutputValue[j] = OutputData[SampleNum, j];
            return;

        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            WDisplay();
        }

        private void btTest_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Random ran = new Random();
            int RAND_MAX = ran.Next();
            for (int j = 0; j < InputNodeNum; j++)//随机初始化输入层-隐藏层权重
            {
                for (int i = 0; i < HideNodeNum; i++)
                    InitHideW(HideW[j, i], RAND_MAX);
            }
            for (int j = 0; j < HideNodeNum; j++)//初始化隐藏层-输出层权重
            {
                for (int i = 0; i < OutputNodeNum; i++)
                    InitOutW(OutW[j, i], RAND_MAX);
            }
        }

        public void HideNodeDataInput()
        {
            TrigFunction MyFunc = new TrigFunction();
            for (int j = 0; j < HideNodeNum; j++)//输入层到隐含层
            {
                CurrentHideInputValue[j] = 0.0;
                for (int i = 0; i < InputNodeNum; i++)
                    CurrentHideInputValue[j] = CurrentHideInputValue[j] + HideW[i, j] * TrainInputValue[i];

                if (CurrentTrigFunction == Liner)//激活函数为线性函数
                {
                    CurrentHideOutputValue[j] = MyFunc.Liner(HOThreshold[j], CurrentHideInputValue[j], 0);
                }
                else if (CurrentTrigFunction == Sigmoid)
                {
                    CurrentHideOutputValue[j] = MyFunc.Sigmoid(CurrentHideInputValue[j], IHThreshold[j]);/////////
                  //  CurrentHideOutputValue[j] = MyFunc.Sigmoidnob(CurrentHideInputValue[j]);
                }
                else if (CurrentTrigFunction == DoubleSigmoid)//激活函数为双S型函数
                {
                    CurrentHideOutputValue[j] = MyFunc.DoubleSigmoid(CurrentHideInputValue[j], IHThreshold[j]);
                }
            }
            for (int k = 0; k < OutputNodeNum; k++)//隐含层到输出层
            {
                CurrentOutInputValue[k] = 0.0;
                for (int m = 0; m < HideNodeNum; m++)
                    CurrentOutInputValue[k] = CurrentOutInputValue[k] + OutW[m, k] * CurrentHideInputValue[m];

                if (CurrentTrigFunction == Liner)//激活函数为线性函数
                {

                    CurrentOutOutputValue[k] = MyFunc.Liner(HOThreshold[k], CurrentOutInputValue[k], 0);
                }
                else if (CurrentTrigFunction == Sigmoid)//--------------------激活函数为S型函数
                {
                   CurrentOutOutputValue[k] = MyFunc.Sigmoid(CurrentOutInputValue[k], HOThreshold[k]);//这里有问题
                  //  CurrentOutOutputValue[k] = MyFunc.Sigmoidnob(CurrentOutInputValue[k]);
                }
                else if (CurrentTrigFunction == DoubleSigmoid)//激活函数为双S型函数
                {

                    CurrentOutOutputValue[k] = MyFunc.DoubleSigmoid(CurrentOutInputValue[k], HOThreshold[k]);
                }

            }



        }
        void CountOffset(int Sample)//对隐藏层和输出层的误差进行计算
        {
            e = 0;

            for (int k = 0; k < OutputNodeNum; k++)
            {
                OutputOffset[k] = (OutputData[Sample, k] - CurrentOutOutputValue[k]) * CurrentOutOutputValue[k] * (1 - CurrentOutOutputValue[k]); //希望输出与实际输出的偏差  
                for (int j = 0; j < HideNodeNum; j++)
                    OutW[j, k] += Rate_HideToOutW * OutputOffset[k] * CurrentHideOutputValue[j];  //下一次的隐含层和输出层之间的新连接权  
            }

            for (int j = 0; j < HideNodeNum; j++)
            {
                HideOffset[j] = 0.0;
                for (int k = 0; k < OutputNodeNum; k++)
                    HideOffset[j] = HideOffset[j] + OutputOffset[k] * OutW[j, k];
                HideOffset[j] = HideOffset[j] * CurrentHideOutputValue[j] * (1 - CurrentHideOutputValue[j]); //隐含层的校正误差  

                for (int i = 0; i < InputNodeNum; i++)
                    HideW[i, j] += Rate_HideToOutW * HideOffset[j] * InputData[Sample, i]; //下一次的输入层和隐含层之间的新连接权  
            }




            for (int k = 0; k < OutputNodeNum; k++)
                HOThreshold[k] = HOThreshold[k] + Rate_ThresholdHO * OutputOffset[k]; //下一次的隐含层和输出层之间的新阈值  
            for (int j = 0; j < HideNodeNum; j++)
                IHThreshold[j] = IHThreshold[j] + Rate_ThresholdIH * HideOffset[j]; //下一次的输入层和隐含层之间的新阈值


            for (int k = 0; k < OutputNodeNum; k++)
            {
                e += Math.Abs(OutputData[Sample, k] - CurrentOutOutputValue[k]) * Math.Abs(OutputData[Sample, k] - CurrentOutOutputValue[k]);
            }
            Error = e / 2.0;
            ErrorC = Error;
            //  if (Sample % 50 == 0)   tbInformation.Text += "\n输出数据:" + CurrentOutOutputValue[k];
            //if(Convert.ToInt32( tbReTime.Text) <2)//DEBUG使用，用于显示输出数据
            //if (Sample % 50 == 0 || Sample == 149 )
            //{
            //    for (int i = 0; i < OutputNodeNum; i++)
            //    {
            //        tbInformation.Text += "\n";
            //        tbInformation.Text += "NODEOUT:" + i + ": " + CurrentOutOutputValue[i];
            //    }
            //    tbInformation.Text += "\n";

            //}
        }

        private void btDisNetwork_Click(object sender, RoutedEventArgs e)
        {
            //     int ImageDrawRatio = 3;
            int InNodeNum = Convert.ToInt16(tbInputNodeNum.Text);
            int HideNodeNum = Convert.ToInt16(tbHideNodeNum.Text);
            int OutNodeNum = Convert.ToInt16(tbOutputNodeNum.Text);
            ImgBuild(InNodeNum, HideNodeNum, OutNodeNum);


        }
        void ImgBuild(int InNode, int HideNode, int OutNode)//////绘制用以表示当前网络结构的图片
        {
            NetWorkImg IMG = new NetWorkImg();
            int Width = (int)IMG.image.Width;
            int High = (int)IMG.image.Height;
            int RoundWidth = 18;
            int Ratio = 6;
            Bitmap Bmp = new Bitmap(Width, High);//实例化位图图片类
            Graphics g = Graphics.FromImage(Bmp);//实例化图形绘制类
            int R, G, B;
            R = 11;
            G = 230;
            B = 79;
            System.Drawing.Color color = System.Drawing.Color.FromArgb(R, G, B);//颜色
            R = 50;
            G = 200;
            B = 79;
            System.Drawing.Color color2 = System.Drawing.Color.FromArgb(R, G, B);//颜色
            System.Drawing.Pen myPen = new System.Drawing.Pen(color);//笔
            System.Drawing.Pen myPen2 = new System.Drawing.Pen(color2);//笔
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(color);//刷子
            System.Drawing.SolidBrush WhiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);//刷子2
            PointF[] I = new PointF[InNode];
            PointF[] H = new PointF[HideNode];
            PointF[] O = new PointF[OutNode];
            for (int One = 0; One < InNode; One++)//绘制输入节点图示
            {

                if (InNode % 2 != 0)
                {
                    g.FillEllipse(myBrush, 50, High / 2 - RoundWidth + Ratio * One * (RoundWidth / 2), RoundWidth, RoundWidth);

                }
                else
                {
                    g.FillEllipse(myBrush, 50, RoundWidth + Ratio * One * (RoundWidth / 2), RoundWidth, RoundWidth);
                    I[One].X = 50 + RoundWidth / 2;
                    I[One].Y = RoundWidth + Ratio * One * (RoundWidth / 2) + RoundWidth / 2;
                }
            }
            for (int Two = 0; Two < HideNode; Two++)//绘制隐藏节点图示
            {
                if (HideNode % 2 != 0)
                {
                    g.FillEllipse(myBrush, 50 * Ratio, High / 2 - RoundWidth + Ratio * Two * (RoundWidth / 2), RoundWidth, RoundWidth);
                }
                else
                {
                    g.FillEllipse(myBrush, 50 * Ratio, RoundWidth + Ratio * Two * (RoundWidth / 2), RoundWidth, RoundWidth);
                    H[Two].X = 50 * Ratio + RoundWidth / 2;//记录点的位置
                    H[Two].Y = RoundWidth + Ratio * Two * (RoundWidth / 2) + RoundWidth / 2;

                }
            }
            for (int Thr = 0; Thr < OutNode; Thr++)//绘制输出节点图示
            {
                if (OutNode % 2 != 0)
                {
                    g.FillEllipse(myBrush, 50 * Ratio * (float)2.0, RoundWidth + Ratio * Thr * (RoundWidth / 2) + Width / 2, RoundWidth, RoundWidth);
                    O[Thr].X = 50 * Ratio * (float)2.0 + RoundWidth / 2;
                    O[Thr].Y = RoundWidth + Ratio * Thr * (RoundWidth / 2) + RoundWidth / 2 + Width / 2;

                }
                else
                {
                    g.FillEllipse(myBrush, 50 * Ratio * (float)2.0, RoundWidth + Ratio * Thr * (RoundWidth / 2), RoundWidth, RoundWidth);
                    O[Thr].X = 50 * Ratio * (float)2.0 + RoundWidth / 2;
                    O[Thr].Y = RoundWidth + Ratio * Thr * (RoundWidth / 2) + RoundWidth / 2;

                }
            }
            for (int One = 0; One < InNode; One++)//绘制节点连线
            {
                for (int Two = 0; Two < HideNode; Two++)
                {
                    g.DrawLine(myPen2, I[One], H[Two]);
                }
            }
            for (int Two = 0; Two < HideNode; Two++)//绘制节点连线2
            {
                for (int Thr = 0; Thr < OutNode; Thr++)
                {
                    g.DrawLine(myPen2, H[Two], O[Thr]);
                }
            }

            Bmp.SetResolution(72, 72);
            DateTime now = DateTime.Now;//时间方式来命名图片
            string CurrentFilename = "NW" +now.Month+now.Day+ now.Hour + now.Minute + now.Second + ".nwdat";

            Bmp.Save("NetworkImg" + CurrentFilename + ".jpg");
            String str = AppDomain.CurrentDomain.BaseDirectory;
            BitmapImage image = new BitmapImage(new Uri(str + @"\NetworkImg" + CurrentFilename + ".jpg", UriKind.RelativeOrAbsolute));

            IMG.image.Source = image;
            IMG.Show();
        }

        private void btSaveTrain_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;//时间方式来命名
            string CurrentFilename = "NW" + now.Month + now.Day + now.Hour + now.Minute + now.Second + ".nwdat";
            FileStream fs = new FileStream(CurrentFilename, FileMode.Create);
            tbSavefileName.Text = CurrentFilename;
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("Train Filename:" + tbReadPath.Text);
            sw.WriteLine("Input node num:" + tbInputNodeNum.Text);
            sw.WriteLine("Hide node num:" + tbHideNodeNum.Text);
            sw.WriteLine("Output node num:" + tbOutputNodeNum.Text);

            sw.WriteLine("TIH");
            for (int j = 0; j < HideNodeNum; j++)
            {
                sw.WriteLine("TIH "+j + "=" + IHThreshold[j]); //下一次的输入层和隐含层之间的新阈值
            }

            sw.WriteLine("THO");
            for (int k = 0; k < OutputNodeNum; k++)
            {
                sw.WriteLine("THO " + k + "=" + HOThreshold[k]);  //下一次的隐含层和输出层之间的新阈值
            }
            sw.WriteLine("I-H");
            for (int i = 0; i < InputNodeNum; i++)
            {
                for (int j = 0; j < HideNodeNum; j++)
                {
                    sw.WriteLine("W" + i + "-" + j + "=" + HideW[j, i]);
                    // sw.WriteLine("输入层" + i + "与隐藏层" + j + "的权W" + i + "-" + j + "= " + HideW[j, i]);
                }
            }
            sw.WriteLine("H-O");
            for (int i = 0; i < HideNodeNum; i++)
            {
                for (int j = 0; j < OutputNodeNum; j++)
                {
                    sw.WriteLine("W" + i + "-" + j + "= " + OutW[j, i]);
                }
            }
            sw.Close();
            tbInformation.Text += '\n' + "保存完成，文件名为:" + CurrentFilename;
        }

        private void btTestData_Click(object sender, RoutedEventArgs e)//数据手工验证部分
        {
            TrigFunction MyFunc = new TrigFunction();
            String line;
            double[] ManInputData = new double[10];
            Regex reg = new Regex(@"^\d+\.\d+$");
            string[] cut = new string[20];
           
                line = tbManInput.Text;
                if (line.Length > 0)
                {
                    cut = Regex.Split(line, ",", RegexOptions.IgnoreCase);//按照逗号切割字符串并保存
                    for (int i = 0; i < InputNodeNum; i++)
                    {
                        if (reg.IsMatch(cut[i]))
                            ManInputData[i] = Convert.ToDouble(cut[i]);
                    }
                }
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
            for (int j = 0; j < HideNodeNum; j++)//输入层到隐含层
            {
                CurrentHideInputValue[j] = 0.0;
                for (int i = 0; i < InputNodeNum; i++)
                    CurrentHideInputValue[j] = CurrentHideInputValue[j] + HideW[i, j] * ManInputData[i];

                if (CurrentTrigFunction == Liner)//激活函数为线性函数
                {
                    CurrentHideOutputValue[j] = MyFunc.Liner(HOThreshold[j], CurrentHideInputValue[j], 0);
                }
                else if (CurrentTrigFunction == Sigmoid)
                {
                    CurrentHideOutputValue[j] = MyFunc.Sigmoid(CurrentHideInputValue[j], IHThreshold[j]);
                   // CurrentHideOutputValue[j] = MyFunc.Sigmoidnob(CurrentHideInputValue[j]);
                }
                else if (CurrentTrigFunction == DoubleSigmoid)//激活函数为双S型函数
                {
                    CurrentHideOutputValue[j] = MyFunc.DoubleSigmoid(CurrentHideInputValue[j], IHThreshold[j]);
                }
            }
            for (int k = 0; k < OutputNodeNum; k++)//隐含层到输出层
            {
                CurrentOutInputValue[k] = 0.0;
                for (int m = 0; m < HideNodeNum; m++)
                    CurrentOutInputValue[k] = CurrentOutInputValue[k] + OutW[m, k] * CurrentHideInputValue[m];///////////////

                if (CurrentTrigFunction == Liner)//激活函数为线性函数
                {

                    CurrentOutOutputValue[k] = MyFunc.Liner(HOThreshold[k], CurrentOutInputValue[k], 0);
                }
                else if (CurrentTrigFunction == Sigmoid)//--------------------激活函数为S型函数
                {
                  CurrentOutOutputValue[k] = MyFunc.Sigmoid(CurrentOutInputValue[k], HOThreshold[k]);//这里有问题
                 //   CurrentOutOutputValue[k] = MyFunc.Sigmoidnob(CurrentOutInputValue[k]);
                }
                else if (CurrentTrigFunction == DoubleSigmoid)//激活函数为双S型函数
                {

                    CurrentOutOutputValue[k] = MyFunc.DoubleSigmoid(CurrentOutInputValue[k], HOThreshold[k]);
                }

            }
           ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            tbInformation.Text +="\n"+ "运算结果：";
            for (int outer = 0; outer < OutputNodeNum; outer++)
            {
                tbInformation.Text += "\n" + " 输出节点" + outer + ": " + CurrentOutOutputValue[outer];
            }
        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            if (BeenInit == 1)
            {
                Init();
                RandInitWMode2();
                BeenInit = 0;
            }

            for (int time = 0; ErrorC > Convert.ToDouble(tbLimitValue.Text); time++)
            {
                TrainRun();
                if(time > Convert.ToInt64(tbMaxTime.Text))
                {
                    tbInformation.Text += "\n\n错误，无法收敛到指定值\n";
                    break;
                }
            }
            DataDisplay();

            btDisplayW.IsEnabled = true;
            btSaveTrain.IsEnabled = true;
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            tbInformation.Text = "";
        }

        public double GetRandomNumber(double minimum, double maximum, int Len)   //Len小数点保留位数
        {
            int iSeed = 10;
            Random ro = new Random(iSeed);
            long tick = DateTime.Now.Ticks;
            Random random = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            return Math.Round(random.NextDouble() * (maximum - minimum) + minimum, Len);
        }

        void RandInitWMode2()//一种符合二项分布的赋予权重初值的方法 http://blog.csdn.net/xbinworld/article/details/50603552
        {
            Random ro = new Random();
            double iResult, iDownH, iUpH, iDownO, iUpO;
            iDownH = -(1 / Math.Sqrt(InputNodeNum));
            iUpH = 1 / Math.Sqrt(InputNodeNum);
            iDownO = -(1 / Math.Sqrt(HideNodeNum));
            iUpO = 1 / Math.Sqrt(HideNodeNum);
            for (int j = 0; j < InputNodeNum; j++)//随机初始化输入层-隐藏层权重
            {
                for (int i = 0; i < HideNodeNum; i++)
                {
                    iResult = GetRandomNumber(iDownH, iUpH, 8);
                    HideW[j, i] = iResult;
                }
            }
            for (int j = 0; j < HideNodeNum; j++)//初始化隐藏层-输出层权重
            {
                for (int i = 0; i < OutputNodeNum; i++)
                {
                    iResult = GetRandomNumber(iDownO, iUpO, 8);
                    OutW[j, i] = iResult;
                }
            }

            for (int i = 0; i < HideNodeNum; i++)
            //隐含层阀值
            {
                iResult = GetRandomNumber(iDownO, iUpO, 8);
                IHThreshold[i] = iResult;
            }
            for (int i = 0; i < OutputNodeNum; i++)
            //   输出层阀值
            {
                iResult = GetRandomNumber(iDownO, iUpO, 8);
                HOThreshold[i] = iResult;
            }


        }
        void TrainRun()
        {

            for (int CurrentSample = 0; CurrentSample < SampleNum; CurrentSample++)
            {
                InitCurrentTrainValue(CurrentSample);//向数据缓冲中输入数据
                HideNodeDataInput();//向隐藏层输入数据，向输出层输出数据
                CountOffset(CurrentSample);//计算当前的误差值并修正权重各个节点的权重
            }


        }
        void DataDisplay()
        {
            tbInformation.Text += "----------------训练已经完成--------------" + '\n';
            tbInformation.Text += "当前错误率：" + Error + '\n';
            tbInformation.Text += " ";

        }
        void WDisplay()
        {
            tbInformation.Text += "----------------当前权值显示--------------" + '\n';
            for (int i = 0; i < InputNodeNum; i++)
            {
                for (int j = 0; j < HideNodeNum; j++)
                {
                    tbInformation.Text += "输入层" + i + "与隐藏层" + j + "的权W" + i + "-" + j + "= " + HideW[j, i] + '\n';
                }
            }
            for (int i = 0; i < HideNodeNum; i++)
            {
                for (int j = 0; j < OutputNodeNum; j++)
                {
                    tbInformation.Text += "隐藏层" + i + "与输出层" + j + "的权W" + i + "-" + j + "= " + OutW[j, i] + '\n';
                }
            }

            tbInformation.Text += " ";
        }
    }
}
