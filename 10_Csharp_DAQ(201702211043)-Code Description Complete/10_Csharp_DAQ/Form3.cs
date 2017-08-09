
#define MYTEST
//#define RUN1DEBUG
//#define RUN2DEBUG
//#define TICK1DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpalKelly.FrontPanel;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System.Diagnostics;
using System.Runtime.InteropServices;



namespace _10_Csharp_DAQ
{
    public partial class Form3 : Form
    {
        //Information (정보출력용 변수)
        double totalcnt;
        double past_totalcnt;
        double past_cntpersec;

        //System (시스템용 변수)
        Queue<byte[][]> queue = new Queue<byte[][]>();
        Thread t1;
        Thread t2;
        Stopwatch systimer = new Stopwatch();
        Stopwatch ticktimer = new Stopwatch();
        Webcam_State state;
        int file_num;

        //디버깅용 변수
        Stopwatch sw_Run1 = new Stopwatch();
        Stopwatch sw_Run2 = new Stopwatch();
        Stopwatch sw_Tick1 = new Stopwatch();
        Stopwatch sw_Tick_interval = new Stopwatch();

        //영상처리용 변수
        static int width = 640, height = 480;
        Bitmap bmp = new Bitmap(width, height);
        int frame_num = 0;
        double[,] PDF_LUT = new double[height, width]; //2차원
        double[] LUT1D = new double[height * width]; //1차원
        double[] PDFLUT = new double[height * width]; //1차원
        double[] PDFLUT0 = new double[height * width]; //1차원

        //OpenCV 용 변수
        IplImage img = new IplImage(width, height, BitDepth.U8, 3);
        IplImage gammaSrc = new IplImage(width, height, BitDepth.U8, 3);
        Mat mat;
        CvFont font;
        CvCapture capture;
        CvVideoWriter writer;

        //OpalKelly용 변수
        okCFrontPanel dev;

        /*
         함수 목록
         1. Form3(okCFrontPanel m_dev, int gamma_num)                                           : 초기화 함수(진입점), 변수 초기화, 타이머 시작, DAQ 스레드 시작
         2. initCamera()                                                                        : 웹캠 초기화
         3. StartTimer()                                                                        : 웹캠 타이머 시작
         4. timer1_Tick(object sender, EventArgs e)                                             : 웹캠 & 감마카메라 영상처리 및 출력
         5. Run2()                                                                              : LINQ에서 출력하여 (X,Y,SUM) 디코딩
         6. Run1()                                                                              : (X,Y,SUM) 데이터획득 (DAQ) 무한루프 실행, LINQ에 배열 저장
         7. Form3_FormClosing(object sender, FormClosingEventArgs e)                            : Form3 종료될 때 실행, 관련 메모리 해제
         8. NormImage(double[] OUTPUT1D, double[] INPUT1D, double min, double max, int length)  : 픽셀 정규화 함수

        */

        public Form3(okCFrontPanel m_dev, int gamma_num) //form3 초기화 및 진입점 (form3 : 감마카메라 영상출력창)
        {
            file_num = gamma_num;//Form1에서 Form3를 몇번 호출하는지 기록
            InitializeComponent();  //Form3 초기화
            
            this.dev = m_dev;//통신용 API 객체 주소 가져옴

            font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7);
            
            //영상용 메모리 초기화
            Array.Clear(PDF_LUT, 0, PDF_LUT.Length); //2차원
            Array.Clear(LUT1D, 0, LUT1D.Length); //1차원

            mat = new Mat(gammaSrc); //OpenCV용 Mat type 메모리 영역
            StartTimer(); //타이머 시작 (DAQ 함수 실행)

            t1 = new Thread(new ThreadStart(Run1)); // Run1 
            t1.Start();

            t2 = new Thread(new ThreadStart(Run2));
            t2.Start();
        }

        private bool initCamera()
        {
            //OpenCV
            try
            {
                capture = CvCapture.FromCamera(CaptureDevice.DShow, 0); //카메라에서 프레임 캡쳐 시작
                capture.SetCaptureProperty(CaptureProperty.FrameWidth, width);//카메라로부터 받아오는 프레임의 가로 크기 지정
                capture.SetCaptureProperty(CaptureProperty.FrameHeight, height);
                string str = string.Format("gamma{0}.avi", file_num);
                writer = new CvVideoWriter(str, FourCC.MJPG, timer1.Interval, new CvSize(capture.FrameWidth, capture.FrameHeight));
                state = Webcam_State.running;
                return true;
            }
            catch
            {
                state = Webcam_State.waiting;
                string str = string.Format("gamma{0}.avi", file_num);
                writer = new CvVideoWriter(str, FourCC.MJPG, timer1.Interval, new CvSize(width, height));

                //MessageBox.Show("캠 인식불가");
                return false;
            }
        }
        private void StartTimer()
        {
            timer1.Interval = 50; //10:초당 100프레임 설정//100:초당 10프레임 설정//180:초당 5프레임 설정
            timer1.Enabled = true; //타이머 시작
            systimer.Start();
            initCamera();
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {

            
#if (TICK1DEBUG)
            sw_Tick1.Start();
            sw_Tick_interval.Stop();
            TimeSpan interval = sw_Tick_interval.Elapsed;
            sw_Tick_interval.Reset();
            sw_Tick_interval.Start();
#endif
            double scale = 10000; //감마영상 밝기 스케일
            if (state == Webcam_State.running)
            {
                lock(capture)
                {
                    img = capture.QueryFrame();
                }
            }
            else if (state == Webcam_State.waiting)
            {
                Cv.SetZero(img); 
            }
            //(Example) imgSrc = capture.QueryFrame();
            ///////////////////////////////////////////////////GPU 함수 추가///////////////////////////////////
            lock(LUT1D)
            {
                double max_val = LUT1D.Max();
                double min_val = LUT1D.Min();
                //exportCppFunctionNorm(PDFLUT, LUT1D, min_val, max_val, LUT1D.Length);
                //exportCppFunctionNormConv(PDFLUT, LUT1D, 640, 480, 21, min_val, max_val);
                //NormImage(PDFLUT, LUT1D, min_val, max_val, LUT1D.Length);
            }
            /////////////////////////////////////////////////블러링/////////////////////////////
            //exportCppFunctionConv(PDFLUT, PDFLUT, 640, 480, 11);
            /////////////////////////////////////////////////감마카메라 영상 정규화/////////////////////////////

            MatOfByte3 mat3 = new MatOfByte3(mat);//MatOfFloat //MatOfDouble3// cv::Mat_<cv::Vec3b>
            var indexer = mat3.GetIndexer();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    byte scaled_normed_pix = (byte)Math.Min(255, /*scale */ LUT1D[index]);
                    Vec3b color;
                    color.Item0 = scaled_normed_pix;
                    color.Item1 = scaled_normed_pix;
                    color.Item2 = scaled_normed_pix;
                    ///////////////////////////////////////////Mat입력///////////////////////////
                    indexer[y, x] = color;
                }
            }
            /////////////////////////////////////////////////블러링/////////////////////////////
            //double min1;// = new double();
            //double max1;// = new double();
            //Cv2.MinMaxLoc(mat, out min1, out max1);
            //gammaSrc.Smooth(gammaSrc, SmoothType.Blur, 3, 0, 2, 0);
            
            
            //Cv2.MinMaxLoc(mat, out min1, out max1);
            //Cv.Normalize(gammaSrc, gammaSrc, 0, 255, NormType.MinMax);
            /////////////////////////////////////////////////컬러변환/////////////////////////////
            Cv2.ApplyColorMap(mat, mat, ColorMapMode.Jet);
            /////////////////////////////////////////////////영상 융합/////////////////////////////
            //Cv.Add(img, gammaSrc, img);
            Cv.AddWeighted(img, 0.5, gammaSrc, 0.9, 0.0, img);
            /////////////////////////////////////////////////영상 출력/////////////////////////////
            frame_num++;//프레임 순번
            string str = string.Format("{0}[frame]", frame_num);
            lock(img)
            {
                img.PutText(str, new CvPoint(10, 20), font, new CvColor(0, 255, 100));
                pictureBoxIpl1.ImageIpl = img;
                writer.WriteFrame(img);
            }
#if (TICK1DEBUG)
            int qCnt = queue.Count();
            sw_Tick1.Stop();
            TimeSpan TickInterval = new TimeSpan(0, 0, 0, 0, timer1.Interval);
            TimeSpan GAP = TickInterval - interval;
            Console.WriteLine("Tick1={0} and Tick_interval={1} and GAP={2} and qCnt={3}\n", sw_Tick1.Elapsed, interval, GAP, qCnt);
            sw_Tick1.Reset();
#endif
            
            ((Form1)Owner).TextBoxValue1 = systimer.Elapsed.ToString();
            ((Form1)Owner).TextBoxValue2 = totalcnt.ToString();
            double cntpersec = (totalcnt - past_totalcnt) / ticktimer.Elapsed.TotalSeconds;
            if (cntpersec == 0) cntpersec = past_cntpersec;
            past_cntpersec = cntpersec;
            past_totalcnt = totalcnt;
            ((Form1)Owner).TextBoxValue3 = cntpersec.ToString(); //ticktimer.Elapsed.TotalSeconds.ToString();//
            ticktimer.Restart();
            //text = "abcd";
            //mat3.Dispose();
            //mat.Dispose();
        }
#if (MYTEST)
        void Run2()
        {
            int bufsize = 1024;
            while (true)
            {
                if (queue.Count != 0)
                {
#if (RUN2DEBUG)
                    sw_Run2.Start();
#endif
                    byte[][] buffer;
                    int qCnt;
                    lock (queue)
                    {
                        buffer = queue.Dequeue();
                        qCnt = queue.Count();
                    }
                    
                    
                    for (int i = 0; i < bufsize * sizeof(int); i += sizeof(int))
                    {
                        //#1
                        //int temp0 = (buffer[0][3 + i] << 24) | (buffer[0][2 + i] << 16) | (buffer[0][1 + i] << 8) | (buffer[0][0 + i]);
                        //int temp0 = (buffer[0][3 + i] << 8) | (buffer[0][2 + i]);
                        //double value0 = (double)(temp0 >> 6);
                        //double fract0 = (double)(temp0 & 0x000003F);
                        //double floatvalue0 = value0 + fract0 / Math.Pow(2, 19);
                        //int temp1 = (buffer[1][3 + i] << 24) | (buffer[1][2 + i] << 16) | (buffer[1][1 + i] << 8) | (buffer[1][0 + i]);
                        //int temp1 = (buffer[1][1 + i] << 8) | (buffer[1][0 + i]);
                        //double value1 = (double)(temp1 >> 19);
                        //double fract1 = (double)(temp1 & 0x000003F);
                        //double floatvalue1 = value1 + fract1 / Math.Pow(2, 19);
                        //int temp2 = (buffer[2][3 + i] << 24) | (buffer[2][2 + i] << 16) | (buffer[2][1 + i] << 8) | (buffer[2][0 + i]);
                        //int temp2 = (buffer[2][3 + i] << 8) | (buffer[2][2 + i]);
                        //double value2 = (double)(temp2 >> 19);
                        //double fract2 = (double)(temp2 & 0x0007FFFF);
                        //double floatvalue2 = value2 + fract2 / Math.Pow(2, 19);
                        //int temp3 = (buffer[3][3 + i] << 24) | (buffer[3][2 + i] << 16) | (buffer[3][1 + i] << 8) | (buffer[3][0 + i]);
                        //int temp3 = (buffer[3][1 + i] << 8) | (buffer[3][0 + i]);
                        //double value3 = (double)(temp3 >> 19);
                        //double fract3 = (double)(temp3 & 0x0007FFFF);
                        //double floatvalue3 = value1 + fract3 / Math.Pow(2, 19);

                        //#2
                        //int temp2H = (buffer[2][3 + i] << 8) | (buffer[2][2 + i]);
                        //double value2H = (double)(temp2H >> 6);
                        //double fract2H = (double)(temp2H & 0x0000003F);
                        //double floatvalue2H = value2H + fract2H / Math.Pow(2, 14);

                        //int temp2L = (buffer[2][1 + i] << 8) | (buffer[2][0 + i]);
                        //double value2L = (double)(temp2L >> 6);
                        //double fract2L = (double)(temp2L & 0x0000003F);
                        //double floatvalue2L = value2L + fract2L / Math.Pow(2, 14);

                        //double x = i / 4;
                        //double y0 = floatvalue2H;//(floatvalue2H + 3) * 80;
                        //double y1 = floatvalue2L;//(floatvalue2L + 3) * 80;
                        //int temp0 = (freebuffer0[3 + i] << 24) | (freebuffer0[2 + i] << 16) | (freebuffer0[1 + i] << 8) | (freebuffer0[0 + i]);
                        ////int temp0 = (freebuffer0[3 + i] << 8) | (freebuffer0[2 + i]);
                        //int value0 = (int)((temp0 & 0xFF800000) >> 23);
                        //int value1 = (int)((temp0 & 0x007FC000) >> 14);
                        //int value2 = (int)((temp0 & 0x00003FFF));

                        //#3
                        int temp0 = (buffer[0][3 + i] << 24) | (buffer[0][2 + i] << 16) | (buffer[0][1 + i] << 8) | (buffer[0][0 + i]);
                        //int temp0 = (freebuffer0[3 + i] << 8) | (freebuffer0[2 + i]);
                        int value0 = (int)((temp0 & 0xFF800000) >> 23);
                        int value1 = (int)((temp0 & 0x007FC000) >> 14);
                        int value2 = (int)((temp0 & 0x00003FFF));

                        //디버깅용
                        double y0 = Math.Min(639, value0);
                        double y1 = Math.Min(479, value1);
                        if ( (y0 != 0) && (y1 != 0) )
                        {
                            lock (LUT1D)
                            {
                                LUT1D[width * (int)y1 + (int)y0]++;
                                totalcnt++;
                            }
                        }
                    }
#if (RUN2DEBUG)
                    sw_Run2.Stop();
                    Console.WriteLine("Run2={0} and queue.Count={1}\n", sw_Run2.Elapsed, qCnt);
                    sw_Run2.Reset();
#endif
                }
            }
        }
#else
        void Run2()
        {
            while (true)
            {
                if (queue.Count != 0)
                {
                    byte[][] buffer;
                    //lock (queue)
                    {
                        buffer = queue.Dequeue();
                    }
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        //buffer의 4byte 단위로 끊어서 x, y 입력해야하지만 지금은 실험//////////////////////////////////////////////////////////////////테스트
                        //LUT[buffer[0][i], buffer[1][i]]++;
                        int y = buffer[0][i] + frame_num;
                        int x = buffer[1][i] + frame_num;
                        y = Math.Min(height, y);
                        x = Math.Min(width, x);
                        LUT1D[height * y + x]++;
                    }
                }
            }
        }
#endif
        void Run1()
        {
            while (true)
            {
#if (RUN1DEBUG)
                sw_Run1.Start();
#endif
                int bufsize = 1024;
                byte[][] freebuffer = new byte[4][];
                freebuffer[0] = new byte[bufsize * sizeof(int)];
                freebuffer[1] = new byte[bufsize * sizeof(int)];
                freebuffer[2] = new byte[bufsize * sizeof(int)];
                freebuffer[3] = new byte[bufsize * sizeof(int)];

                okCFrontPanel.ErrorCode error_mess = new okCFrontPanel.ErrorCode();
                lock(dev)
                {
                    error_mess = (okCFrontPanel.ErrorCode)
                        dev.ReadFromBlockPipeOut
                        (0xA0, bufsize, bufsize * sizeof(int), freebuffer[0]); //데이터 획득
                }
                int qCnt;
                lock(queue)
                {
                    queue.Enqueue(freebuffer);
                    qCnt = queue.Count;
                }
#if (RUN1DEBUG)
                sw_Run1.Stop();
                Console.WriteLine("Run1={0} and queue.Count={1}\n", sw_Run1.Elapsed, qCnt);
                sw_Run1.Reset();
#endif
            }
        }
        
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((Form1)this.Owner).FPGAState = FPGA_State.ready;
            ((Form1)this.Owner).Button4Text = "Gamma On";
            ((Form1)this.Owner).Button6Enabled = true;
            //system timer
            systimer.Stop();
            //Writer
            if (writer != null) writer.Dispose();
            //Image
            if (timer1 != null) timer1.Enabled = false;
            //if (timer2 != null) timer2.Enabled = false;
            if (bmp != null) bmp.Dispose();
            try
            {
                t1.Abort();//Run 스레드 강제 종료
                t2.Abort();
            }
            catch
            {
                t1.Suspend();
                t2.Suspend();
                MessageBox.Show("강제종료합니다.");
                t1.Abort();//Run 스레드 강제 종료
                t2.Abort();
            }
        }
        
        //[DllImport("10_CUDA.dll", CallingConvention = CallingConvention.Cdecl)]
        //unsafe public static extern void exportCppFunctionNorm(double[] dst, double[] src, double min, double max, int length);
        
        //[DllImport("10_CUDA.dll", CallingConvention = CallingConvention.Cdecl)]
        //unsafe public static extern void exportCppFunctionNormConv(double[] dst, double[] src, int width, int height, int fwidth, double min, double max);
        
        
        int NormImage(double[] OUTPUT1D, double[] INPUT1D, double min, double max, int length)
        {
            // P[tid] = (M[tid] - min) / (max - min) * 255;

            for(int i = 0; i < length; i++)
            {
                OUTPUT1D[i] = (INPUT1D[i] - min) / (max - min) * 255;
                //OUTPUT1D[i] = INPUT1D[i];
            }
            return 0;
        }
    }
}
