using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using ZedGraph;
using OpalKelly.FrontPanel;
using System.Runtime.InteropServices;

namespace _10_Csharp_DAQ
{
    public partial class Form2 : Form
    {
        //System (시스템용 변수)
        Stopwatch sw = new Stopwatch(); //디버깅용 타이머 (속도측정용)
        
        //FPGA_State fpga_state;

        //Opalkelly
        static int bufsize = 1024; //그래프에 출력용 버퍼의 사이즈
        byte[] freebuffer0 = new byte[bufsize * sizeof(int)]; //그래프 출력용 버퍼0
        byte[] freebuffer1 = new byte[bufsize * sizeof(int)]; //그래프 출력용 버퍼1
        byte[] freebuffer2 = new byte[bufsize * sizeof(int)]; //그래프 출력용 버퍼2
        //byte[] freebuffer3 = new byte[bufsize * sizeof(int)]; //그래프 출력용 버퍼3

        /*
        //함수목록 : 객체(버튼, 글자출력박스, 영상출력박스, 그래픽출력박스) 동작을 정의하는 함수 나열

        1. Form2(Form ownerowner, Form owner, okCFrontPanel m_dev, int sel) : Form2의 진입점이 되는 함수이므로 자동으로 실행됨
            Parameter1 : Form ownerowner (할아버지 객체의 주소)
            Parameter2 : Form owner (아버지 객체의 주소)
            Parameter3 : okCFrontPanel m_dev (Opalkelly FrontPanel 통신용 API 변수 주소)
            Parameter4 : int sel (0 : form1에서 호출, 1 : form4에서 호출)

        */
        //Form2 : 그래프출력창 (Form2 진입점) *************************************디버깅 시작지점**************************************
        public Form2(Form ownerowner, Form owner, okCFrontPanel m_dev, int sel)
        {
            InitializeComponent(); //Form2 GUI 초기화

            //첫번째 그래프창 (zedGraphControl2) : 선택채널 신호만 출력할 그래프창
            GraphPane Selected_Channel = zedGraphControl2.GraphPane; 
            Selected_Channel.Title.Text = "Selected Channel";
            Selected_Channel.XAxis.Title.Text = "Time";
            Selected_Channel.YAxis.Title.Text = "ADC Value";

            PointPairList Sel_XList = new PointPairList();
            PointPairList Sel_YList = new PointPairList();

            //두번째 그래프창 (zedGraphControl1) : 전체 채널 신호 출력 그래프창 (나중에 스펙트럼 출력용 그래프로 전환예정)
            GraphPane Total_Cahnnel = zedGraphControl1.GraphPane;
            Total_Cahnnel.Title.Text = "Total Energy Spectrum"; //임시 명칭
            Total_Cahnnel.XAxis.Title.Text = "Energy"; //임시 명칭
            Total_Cahnnel.YAxis.Title.Text = "Count"; //임시 명칭

            PointPairList RawAList = new PointPairList(); // A 채널의 신호를 담을 리스트 
            PointPairList RawBList = new PointPairList(); // B 채널의 신호를 담을 리스트
            PointPairList RawCList = new PointPairList(); // C 채널의 신호를 담을 리스트
            PointPairList RawDList = new PointPairList(); // D 채널의 신호를 담을 리스트

            //데이터 포인트 저장
            sw.Start();
            //데이터 읽어오기
            
            try
            {
                okCFrontPanel.ErrorCode error_mess = new okCFrontPanel.ErrorCode();

                lock (m_dev)
                {
                    error_mess = (okCFrontPanel.ErrorCode)
                        m_dev.ReadFromBlockPipeOut
                        (0xA1, bufsize, bufsize * sizeof(int), freebuffer1); //채널 A,B 데이터 획득
                }

                if (error_mess < 0)
                {
                    MessageBox.Show("통신장애 또는 신호없음(2)");

                    throw new System.ArgumentException("통신장애 또는 신호없음(2)");
                    //return;
                }
                lock (m_dev)
                {
                    error_mess = (okCFrontPanel.ErrorCode)
                        m_dev.ReadFromBlockPipeOut
                        (0xA2, bufsize, bufsize * sizeof(int), freebuffer2); //채널 C,D 데이터 획득
                }

                if (error_mess < 0)
                {
                    MessageBox.Show("통신장애 또는 신호없음(3)");

                    throw new System.ArgumentException("통신장애 또는 신호없음(3)");
                    //return;
                }

                switch(sel) // 어떤 form에서 호출되었는지 확인 (sel : 0 이면 form1, sel : 1 이면 form4)
                {
                    case 1:
                        //(form4에서 호출)
                        break;
                    case 2: //미정
                        break;
                    case 3: //미정
                        break;
                    case 4: //미정
                        break;
                    default: //sel==0 (form1에서 호출)
                        lock (m_dev)//
                        {
                            error_mess = (okCFrontPanel.ErrorCode)
                                m_dev.ReadFromBlockPipeOut
                                (0xA0, bufsize, bufsize * sizeof(int), freebuffer0); //(X,Y,SUM) 데이터 획득
                        }

                        if (error_mess < 0)
                        {
                            MessageBox.Show("통신장애 또는 신호없음(1)");
                            throw new System.ArgumentException("통신장애 또는 신호없음(1)");
                        }
                        break;
                }
            }
            catch
            {
                DialogResult temp = MessageBox.Show("FPGA 초기화 하시겠습니까?", "성공", MessageBoxButtons.YesNo);
                if (temp == DialogResult.Yes)
                {
                    string filename = ((Form1)ownerowner).FILENAME;
                    ((Form1)ownerowner).initFPGA(this, filename);
                    ((Form4)owner).RESETSTATE = true;
                    return;
                }
            }

            for (int i = 0; i < (bufsize-1) * sizeof(int); i += sizeof(int))
            {
                //파형 디코딩 (채널 : A,B) from freebuffer1
                int temp_A = (short)((freebuffer1[3 + i] << 8) | (freebuffer1[2 + i]));
                int temp_B = (short)((freebuffer1[1 + i] << 8) | (freebuffer1[0 + i]));
                //파형 디코딩 (채널 : C,D) from freebuffer2
                int temp_C = (short)((freebuffer2[3 + i] << 8) | (freebuffer2[2 + i]));
                int temp_D = (short)((freebuffer2[1 + i] << 8) | (freebuffer2[0 + i]));
                //(X,Y,SUM) 디코딩
                int temp_XYSUM = (freebuffer0[3 + i] << 24) | (freebuffer0[2 + i] << 16) | (freebuffer0[1 + i] << 8) | (freebuffer0[0 + i]);

                int valueA = temp_A; //채널 A
                int valueB = temp_B; //채널 B
                int valueC = temp_C; //채널 C
                int valueD = temp_D; //채널 D

                
                int valueX = (int)((temp_XYSUM & 0xFF800000) >> 23);//X좌표 9bit
                int valueY = (int)((temp_XYSUM & 0x007FC000) >> 14);//Y좌표 9bit
                int valueSUM = (int)((temp_XYSUM & 0x00003FFF));//Sum값 14bit
                
                double x = i / 4;

                //(X,Y)좌표 리스트에 추가
                Sel_XList.Add(x, valueX);
                Sel_YList.Add(x, valueY);

                //선택파형 리스트에 추가
                RawAList.Add(x, valueA);
                RawBList.Add(x, valueB);
                RawCList.Add(x, valueC);
                RawDList.Add(x, valueD);
            }

            LineItem mySelCurve;
            if (sel == 0) //Form1에서 실행된 경우, (X,Y) 좌표정보 출력
            {
                mySelCurve = Selected_Channel.AddCurve("X", Sel_XList, Color.Blue, SymbolType.Square);
                mySelCurve = Selected_Channel.AddCurve("Y", Sel_YList, Color.Green, SymbolType.Triangle);
            }
            else //Form4에서 실행된 경우, 선택채널 파형출력
            {
                //Form4(컨트롤창)에서 체크박스 표시여부 확인 후 선택적 출력
                bool ChA = ((Form4)owner).ChannelACheck;
                bool ChB = ((Form4)owner).ChannelBCheck;
                bool ChC = ((Form4)owner).ChannelCCheck;
                bool ChD = ((Form4)owner).ChannelDCheck;
                if (ChA)
                    mySelCurve = Selected_Channel.AddCurve("ChannelA", RawAList, Color.Blue, SymbolType.Square);
                if (ChB)
                    mySelCurve = Selected_Channel.AddCurve("ChannelB", RawBList, Color.Green, SymbolType.Triangle);
                if (ChC)
                    mySelCurve = Selected_Channel.AddCurve("ChannelC", RawCList, Color.Black, SymbolType.Star);
                if (ChD)
                    mySelCurve = Selected_Channel.AddCurve("ChannelD", RawDList, Color.Violet, SymbolType.Plus);
            }
            //전체 채널의 파형은 항상 출력함
            LineItem myCaptureCurve = Total_Cahnnel.AddCurve("ChannelA", RawAList, Color.Blue, SymbolType.Square);
            myCaptureCurve = Total_Cahnnel.AddCurve("ChannelB", RawBList, Color.Green, SymbolType.Triangle);
            myCaptureCurve = Total_Cahnnel.AddCurve("ChannelC", RawCList, Color.Black, SymbolType.Star);
            myCaptureCurve = Total_Cahnnel.AddCurve("ChannelD", RawDList, Color.Violet, SymbolType.Plus);
            //각 그래프창 축 재설정
            zedGraphControl2.AxisChange();
            zedGraphControl1.AxisChange();
            //코드속도 측정 및 출력
            sw.Stop(); 
            Console.WriteLine("Form2={0}\n", sw.Elapsed);
        }
    }
}
