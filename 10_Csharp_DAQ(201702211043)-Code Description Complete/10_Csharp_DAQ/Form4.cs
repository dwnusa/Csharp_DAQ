using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpalKelly.FrontPanel;

namespace _10_Csharp_DAQ
{
    public partial class Form4 : Form
    {
        //System 변수
        public bool ChannelACheck
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }
        public bool ChannelBCheck
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }
        public bool ChannelCCheck
        {
            get { return checkBox3.Checked; }
            set { checkBox3.Checked = value; }
        }
        public bool ChannelDCheck
        {
            get { return checkBox4.Checked; }
            set { checkBox4.Checked = value; }
        }
        public bool RESETSTATE
        {
            get { return resetstate; }
            set { resetstate = value; }
        }
        bool resetstate;
        static Form2 form2;
        okCFrontPanel dev;
        Form1 form1;


        /*
         함수 목록
         1. Form4(Form owner, okCFrontPanel m_dev)                      : Form4 초기화, Form4 진입점, button객체 활/비활성화
         2. button1_Click(object sender, EventArgs e)                   : 체크박스 주요 설정들을 FPGA에 반영함 (Apply 버튼)
         3. button2_Click(object sender, EventArgs e)                   : (Capture 버튼) 설정된 내용으로 FPGA에서 데이터를 얻어 파형출력
         4. Form4_FormClosing(object sender, FormClosingEventArgs e)    : 

        */
        //Form4 진입점 및 GUI 초기화
        public Form4(Form owner, okCFrontPanel m_dev)
        {
            form1 = (Form1)owner;
            InitializeComponent();
            this.dev = m_dev;
            if (dev != null) //dev 객체가 있으면 버튼 활성화
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else //dev 객체가 없으면 버튼 비활성화
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //각 체크박스의 내용을 UInt32 변수에 한 비트씩 담아서 전송
            uint ep00wire = Convert.ToUInt32(ep00wire31.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire30.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire29.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire28.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire27.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire26.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire25.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire24.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire23.Checked);
            ep00wire = (ep00wire << 1) | Convert.ToUInt32(ep00wire22.Checked);
            ep00wire = (ep00wire << 8) | Convert.ToUInt32(ep00wire21_14.Value);
            ep00wire = (ep00wire << 14) | Convert.ToUInt32(ep00wire13_0.Value);
            //0xA01900C8
            if(ep00wire == 0xA01900C8)
            {
                int kk = 0;//초기세팅값
            }
            dev.SetWireInValue((int)0x00, (uint)ep00wire, (uint)0xffffffff);
            dev.UpdateWireIns();
            if (ep00wire30.Checked == true)
            {
                ep00wire = ep00wire & 0xbfffffff;
                dev.SetWireInValue((int)0x00, (uint)ep00wire, (uint)0xffffffff);
                dev.UpdateWireIns();
                ep00wire30.Checked = false;
            }
        }
        //파형을 출력하기 위해 Form2 객체를 생성하여 호출한다.
        private void button2_Click(object sender, EventArgs e)
        {
            //Chart Window Show
            if ((form2 != null))
                form2.Close();
            //bool resetstate;
            form2 = new Form2(Owner, this, dev, 1); //Form2 객체 생성
            if (resetstate == false) //초기화하지 않은경우
            {
                form2.Show(this); //Form2 객체 호출
            }
        }
        //Form4 종료할 때 메모리 해제
        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((form2 != null))
                form2.Close();
        }
    }
}
