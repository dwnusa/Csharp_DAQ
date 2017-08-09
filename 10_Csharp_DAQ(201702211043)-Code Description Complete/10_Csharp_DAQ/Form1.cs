using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpalKelly.FrontPanel;
using System.Diagnostics;

namespace _10_Csharp_DAQ //Form1 : 메인창 (주요기능선택창)
{
    public enum State { waiting, recording, paused } 
    public enum FPGA_State { disconnected, ready, running }
    public enum Webcam_State { waiting, running, terminated }

    public partial class Form1 : Form
    {
//변수초기화
        //System (시스템용 변수)
        int gamma_num = 0;
        int frame_num = 0;
        int file_num = 0;
        int fpga_trial = 0;

        State state; //시스템 상태 변수
        FPGA_State fpga_state; //FPGA 상태변수
        public FPGA_State FPGAState //서로 다른 Form끼리 변수 접근이 가능하게 하는 get/set 함수 (FPGA 상태변수)
        {
            get { return fpga_state;  }
            set { fpga_state = value; }
        }

        OpenFileDialog dlgOpen = new OpenFileDialog(); //bit파일 선택하는 대화창 객체선언

        static Form2 form2; //form2 : Waveform 출력창
        static Form3 form3; //form3 : 감마카메라 영상 출력창
        static Form4 form4; //form4 : 감마카메라 DAQ 컨트롤러창

        public string Button4Text //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (button4 Text표기 변수)
        {
            get { return button4.Text; }
            set { button4.Text = value; }
        }
        public bool Button6Enabled //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (button6 활성화 변수)
        {
            get { return button6.Enabled; }
            set { button6.Enabled = value; }
        }
        //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (textBox1 내용출력용 Text 변수) textBox1 : 총검출시간
        public string TextBoxValue1 
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (textBox2 내용출력용 Text 변수) textBox2 : 총검출량
        public string TextBoxValue2 
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
        //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (textBox3 내용출력용 Text 변수) textBox3 : 초당검출량
        public string TextBoxValue3 
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }
        //서로 다른 Form끼리 변수 수정이 가능하게 하는 get/set 함수  (bit파일명과 경로 저장용 string 변수)
        public string FILENAME
        {
            get { return filename; }
            set { filename = value; }
        }

        //OpenCV (영상처리용 변수)
        static int width = 640; //가로크기
        static int height = 480; //세로크기
        CvCapture capture; //프레임 캡쳐용 OpenCV 변수
        IplImage imgSrc = new IplImage(width, height, BitDepth.U8, 3); //영상 저장용 메모리할당 (BitDepth.U8 : 픽셀당 8비트)
        CvVideoWriter writer; //영상 저장용 Writer (영상을 캡쳐해서 동영상파일로 내보내는 변수)
        CvFont font; //영상에 글자쓰기 변수

        //FrontPanel (FPGA통신용 변수)
        string filename; //bit파일명과 경로 저장용 String 변수
        okCFrontPanel m_dev; //Opalkelly Frontpanel 통신용 API 변수


        /*
        //함수목록 : 객체(버튼, 글자출력박스, 영상출력박스, 그래픽출력박스) 동작을 정의하는 함수 나열
                             
        1. Form1()
            Form1 : 메인창(주요기능선택)입니다.
            Form1 창이 뜰 때 초기화하는 함수 InitializeComponent()를 수행하며, 
            Form1 실행시 가장 먼저 호출되는 진입점이 됩니다.
        2. OpenCvSharp_Load(object sender, EventArgs e)
            웹캠 연결상태를 확인(initCamera())하고 FPGA_State(FPGA 상태변수)를 초기화합니다.
            또한, 웹캠용 타이머를 시작(StartTimer())합니다. 
        3. initCamera()                                                : 웹캠 연결을 확인하고 초기화함
        4. button5_Click(object sender, EventArgs e)                   : Capture waveform
        5. button3_Click(object sender, EventArgs e)                   : FPGA Connection
        6. button4_Click(object sender, EventArgs e)                   : Gamma Camera display
        7. StartTimer()                                                : 웹캠용 타이머를 설정하고 시작
        8. timer1_Tick(object sender, EventArgs e)                     : 타이머 설정시간마다 웹캠 영상 출력
        9. button1_Click(object sender, EventArgs e)                   : Record Button Event (동영상파일로 저장)
        10.Form1_FormClosing(object sender, FormClosingEventArgs e)    : form1 종료될때 모든 변수의 메모리 해제
        11.button2_Click(object sender, EventArgs e)                   : form1 종료될때 모든 변수의 메모리 해제
        12.button6_Click(object sender, EventArgs e)                   : 세팅창 실행 (form4 : 컨트롤러창)
        13.initFPGA(Form owner, string bitfile)                        : FPGA 상태 확인과 초기화

        */
        //Form1 GUI 초기화
        public Form1() //Form1 진입점 *************************************디버깅 시작지점**************************************
        {
            InitializeComponent();//Form1 GUI 초기화
        }

        
        private void OpenCvSharp_Load(object sender, EventArgs e)
        {
            if(initCamera())
            {
                fpga_state = FPGA_State.disconnected;
                StartTimer();
            }
            else
            {
                //MessageBox.Show("캠과 연결에 문제발생");
            }
        }

        //폼 초기화
        private bool initCamera()
        {
            try
            {
                //System
                state = State.waiting;
                
                //OpenCV
                capture = CvCapture.FromCamera(CaptureDevice.DShow, 0); //카메라에서 프레임 캡쳐 시작
                capture.SetCaptureProperty(CaptureProperty.FrameWidth, width);//카메라로부터 받아오는 프레임의 가로 크기 지정
                capture.SetCaptureProperty(CaptureProperty.FrameHeight, height);
                //FrontPanel
                return true;
            }
            catch
            {
                button1.Text = "&Reset";
                MessageBox.Show("캠과 연결에 문제발생");
                return false;
            }
        }

        //Capture waveform
        private void button5_Click(object sender, EventArgs e)
        {
            //Chart Window Show
            if ((form2 != null))
                form2.Close();
            form2 = new Form2(null, this, m_dev, 0);
            form2.Show(this);
        }

        //FPGA Connection
        private void button3_Click(object sender, EventArgs e)
        {
            initFPGA(this, null);
            //m_dev.ResetFPGA();
        }

        //Gamma Camera display
        private void button4_Click(object sender, EventArgs e)
        {
            if (fpga_state == FPGA_State.ready)
            {
                if (form4 != null)
                    form4.Close();
                button6.Enabled = false;
                gamma_num++;
                button4.Text = "&Gamma Off";
                form3 = new Form3(m_dev, gamma_num);//, &fpga_state);//FPGA_State fpga_state
                form3.Show(this);
                fpga_state = FPGA_State.running;
            }
            else if (fpga_state == FPGA_State.disconnected)
            {
                MessageBox.Show("FPGA is not operating.");
                button4.Text = "&Gamma On";
                if (form3 != null)
                    form3.Close();
                button6.Enabled = true;
            }
            else
            {
                button4.Text = "&Gamma On";
                fpga_state = FPGA_State.ready;
                if (form3 != null)
                    form3.Close();
                button6.Enabled = true;
            }
        }

        //웹캠용 타이머 시작
        private void StartTimer()
        {
            timer1.Interval = 10; //초당 55프레임 설정
            timer1.Enabled = true; //타이머 시작
        }

        //타이머 (웹캠 출력)
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (imgSrc == null)
                {
                    int brk = 1;
                }
                lock (imgSrc)
                {
                    imgSrc = capture.QueryFrame();
                }

                switch (state)
                {
                    case State.recording://영상녹화
                        frame_num++;
                        string str = string.Format("{0}[frame]", frame_num);
                        imgSrc.PutText(str, new CvPoint(10, 20), font, new CvColor(0, 255, 100));
                        writer.WriteFrame(imgSrc);
                        break;
                    case State.paused://녹화중지
                        writer.Dispose();
                        state = State.waiting;
                        break;
                }
                pictureBoxIpl1.ImageIpl = imgSrc;//영상출력
                button1.Text = "&Record";
            }
            catch
            {
                //MessageBox.Show("웹캠 에러 발생");
                //OpenCV
                timer1.Stop();
                if (timer1 != null) timer1.Enabled = false;
                if (capture != null) capture.Dispose();
                if (writer != null) writer.Dispose();
                //FrontPanel
                if (m_dev != null) m_dev.Dispose();
                button1.Text = "&Reset";
                imgSrc = new IplImage(width, height, BitDepth.U8, 3);
                //initCamera();
            }
            
        }

        //Record Button Event
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text == "&Record")
                {
                    //FourCC.MJPG   file_num
                    file_num++;
                    frame_num = 0;
                    string str = string.Format("record{0}.avi", file_num);
                    writer = new CvVideoWriter(str, FourCC.MJPG, timer1.Interval, new CvSize(capture.FrameWidth, capture.FrameHeight));
                    font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7);
                    button1.Text = "&Pause";
                    richTextBox1.Text += "Video Recording : " + str + "\n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    state = State.recording;//녹화상태로 변경
                }
                else if (button1.Text == "&Pause")
                {
                    button1.Text = "&Record";
                    richTextBox1.Text += "Recording terminated : record" + file_num + ".avi\n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    state = State.paused;//녹화중지상태로 변경
                }
                else if(button1.Text == "&Reset")
                {
                    initCamera();
                    StartTimer();
                    button1.Text = "&Record";
                }
            }
            catch
            {
                MessageBox.Show("캠과 연결에 문제발생");
            }
        }

        //폼 닫힐때
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //OpenCV
            if (timer1 != null) timer1.Enabled = false;
            if (capture != null) capture.Dispose();
            if (writer != null) writer.Dispose();
            //FrontPanel
            if (m_dev != null) m_dev.Dispose();
        }

        //Quit Button Event
        private void button2_Click(object sender, EventArgs e)
        {
            //OpenCV
            if (timer1 != null) timer1.Enabled = false;
            if (capture != null) capture.Dispose();
            if (writer != null) writer.Dispose();
            //FrontPanel
            if (m_dev != null) m_dev.Dispose();
            //System
            this.Close();
        }

        //세팅
        private void button6_Click(object sender, EventArgs e)
        {
            //Chart Window Show
            if (form4 != null)
                form4.Close();
            form4 = new Form4(this, m_dev);//, &fpga_state);//FPGA_State fpga_state
            form4.Show(this);
        }

        //FPGA 상태확인과 초기화
        public void initFPGA(Form owner, string bitfile)
        {
            if ((form2 != null))
                form2.Close();
            if ((form4 != null))
                form4.Close();
            if ((form3 != null))
                form3.Close();


            try
            {
                if (m_dev != null) m_dev.Dispose();
                m_dev = new okCFrontPanel();
            }
            catch
            {
                MessageBox.Show("OpalKelly Frontpanel library is not operating.");
                fpga_state = FPGA_State.disconnected;
                button3.Text = "&FPGA";
                return;
            }
            m_dev = new okCFrontPanel();
            fpga_trial++;
            richTextBox1.Text += "--------------------{" + fpga_trial + "}---------------------\n";
            if (okCFrontPanel.ErrorCode.NoError != m_dev.OpenBySerial(""))
            {
                MessageBox.Show("A device could not be opened.  Is one connected?");
                fpga_state = FPGA_State.disconnected;
                button3.Text = "&FPGA";
                return;
            }
            // Get some general information about the device.
            okTDeviceInfo devInfo = new okTDeviceInfo();
            m_dev.GetDeviceInfo(devInfo);
            richTextBox1.Text += "Device firmware version: " +
                devInfo.deviceMajorVersion + "." +
                devInfo.deviceMinorVersion + "\n";
            richTextBox1.Text += "Device serial number: " + devInfo.serialNumber + "\n";
            richTextBox1.Text += "Device ID: " + devInfo.deviceID + "\n";

            // Setup the PLL from defaults.
            m_dev.LoadDefaultPLLConfiguration();


            //Dialog창
            filename = bitfile;
            if(filename == null)
            {
                if (DialogResult.OK != dlgOpen.ShowDialog())
                {
                    MessageBox.Show("No file selected");
                    fpga_state = FPGA_State.disconnected;
                    return;
                }
                filename = dlgOpen.FileName; //파일명 추출
            }

            // Download the configuration file.
            if (okCFrontPanel.ErrorCode.NoError != m_dev.ConfigureFPGA(filename))
            {
                MessageBox.Show("FPGA configuration failed. ({0} fail)", filename);
                fpga_state = FPGA_State.disconnected;
                return;
            }

            // Check for FrontPanel support in the FPGA configuration.
            if (false == m_dev.IsFrontPanelEnabled())
            {
                MessageBox.Show("FrontPanel support is not available.");
                fpga_state = FPGA_State.disconnected;
                return;
            }
            ///////////////////////////////////////////테스트
            try
            {
                m_dev.SetWireInValue((int)0x00, (uint)0xA01900C8, (uint)0xffffffff);
                m_dev.UpdateWireIns();

            }
            catch
            {
                MessageBox.Show("통신장애 또는 신호없음");
                fpga_state = FPGA_State.disconnected;
                return;
            }
            ///////////////////////////////////////////테스트

            richTextBox1.Text += "FrontPanel support is available." + "\n";
            richTextBox1.Text += "Bitfile : " + filename + "\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            fpga_state = FPGA_State.ready;
            button5.Enabled = true;
        }
    }
}
