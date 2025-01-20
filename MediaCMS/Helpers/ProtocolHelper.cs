using OSC_Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfApp11.Helpers.ByteConverter;

namespace WpfApp11.Helpers
{
   public class ProtocolHelper
    {
        enum Serial
        {
            VW=1,
            CL=2,
            EL=3,
            DATACENTER=4
        }

        enum  VW{
            LIGHT_ON=1,
            LIGHT_OFF=2
        }

        enum CL
        {
            LIGHT_ON=1,
            LIGHT_OFF=2,
            BL_UP=3,
            BL_DOWN=4,
            AIR_ON=5,
            AIR_OFF=6
        }

        enum EL
        {
            OPEN1=1,
            CLOSE1=2,
            OPEN2=3,
            CLOSE2=4,
            OPENALL=5,
            CLOSEALL=6,
            LIGHT_ON=7,
            LIGHT_OFF = 8,
        }

        enum DATACENTER_DOOR
        {
            UP=1,
            DOWN=2
        }

        //비전월 조명
        SerialHelper serialHelper01;
        //라운지 조명 / 블라인드 / 공청기
        SerialHelper serialHelper02;
        //엘레베이터
        SerialHelper serialHelper03;
        //
        SerialHelper serialHelper04;
        public ProtocolHelper()
        {
            serialHelper01 = new SerialHelper("COM3");
            serialHelper02 = new SerialHelper("COM5");
            serialHelper03 = new SerialHelper("COM2");
            serialHelper04 = new SerialHelper("COM4");

        }

        public void Start()
        {
            
            ProtocolUdpHelper.Instance.PacketReceived -= Instance_PacketReceived; ;
            ProtocolUdpHelper.Instance.PacketReceived += Instance_PacketReceived; ;
        }
        public void Stop()
        {
            ProtocolUdpHelper.Instance.PacketReceived -= Instance_PacketReceived; ;
        }

        public void SerialTest(int index)
        {
            switch (index)
            {
                case 0:
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case 1:
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_OFF).ToString());
                    break;
                case 2:
                    SendSerial(Serial.EL.ToString(), ((int)EL.OPENALL).ToString());
                    break;
                case 3:
                    SendSerial(Serial.EL.ToString(), ((int)EL.CLOSEALL).ToString());
                    break;
                case 4:
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_ON).ToString());
                    break;
                case 5:
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_OFF).ToString());
                    break;
                case 6:
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_UP).ToString());
                    break;
                case 7:
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_DOWN).ToString());
                    break;
                case 8:
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_ON).ToString());
                    break;
                case 9:
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_OFF).ToString());
                    break;

                default:
                    break;
            }
        }

        public void SendSerial(string index, string msg)
        {
            switch (index)
            {
                case "VW":
                    Logger.Log2("Send Serial-COM03:" + msg);
                    serialHelper01.OpenConnection();
                    serialHelper01.SendData(msg);
                    serialHelper01.CloseConnection();
                    break;
                case "CL":
                    Logger.Log2("Send Serial-COM05:" + msg);
                    serialHelper02.OpenConnection();
                    serialHelper02.SendData(msg);
                    serialHelper02.CloseConnection();
                    break;
                case "EL":
                    Logger.Log2("Send Serial-COM02:" + msg);
                    serialHelper03.OpenConnection();
                    serialHelper03.SendData(msg);
                    serialHelper03.CloseConnection();
                    break;
                case "DATACENTER":
                    Logger.Log2("Send Serial-COM04:" + msg);
                    serialHelper04.OpenConnection();
                    serialHelper04.SendData(msg);
                    serialHelper04.CloseConnection();
                    break;
                default:
                    break;
            }
        }

        public void Instance_PacketReceived(string code)
        {
            Logger.Log2(code);

            code = code.ToUpper().Trim();

            switch (code)
            {
                case "IDLE_ALL":
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_DOWN).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync("OF_OFF", "192.168.0.37", 8020);

                    ProtocolUdpHelper.Instance.SendWithIpAsync("VW_IDLE", "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("VW_IDLE", "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());


                    break;
                case "IDLE_VW":
                    ProtocolUdpHelper.Instance.SendWithIpAsync("VW_IDLE", "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("VW_IDLE", "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case "IDLE_AI":
                    ProtocolUdpHelper.Instance.SendWithIpAsync("FACE_IDLE", "192.168.0.39", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("EV_IDLE", "192.168.0.11", 8020);
                    SendSerial(Serial.EL.ToString(), ((int)EL.CLOSEALL).ToString());
                    break;
                case "IDLE_HO":
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_DOWN).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync("CEO_IDLE", "192.168.0.37", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("WS_IDLE", "192.168.0.34", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("MB_IDLE", "192.168.0.36", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("MB_IDLE", "192.168.0.35", 8020);

                    break;
                case "IDLE_CL":
                    OSCSenderHelper.Instance.Send("192.168.0.14", "1");
                    break;
                case "IDLE_C":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "1");
                    ProtocolUdpHelper.Instance.SendWithIpAsync("DC_IDLE", "192.168.0.15", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync("MSP_IDLE", "192.168.0.13", 8020);
                    break;
                
                
                case "VW_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case "VW_MSG_NORMAL":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case "VW_MSG_CUSTOM":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case "VW_CEO_MUTEOFF_EN":
                case "VW_CEO_MUTEOFF":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_OFF).ToString());
                    break;

                case "VW_CEO_MUTEON_EN":   
                case "VW_CEO_MUTEON":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_OFF).ToString());
                    break;

                case "VW_BRAND_EN":
                case "VW_BRAND":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_OFF).ToString());
                    break;
                case "VW_CEO_END":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;
                case "VW_BRAND_END":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;

                case "VW_REPORT_EN":
                case "VW_REPORT":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.16", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.19", 8020);
                    SendSerial(Serial.VW.ToString(), ((int)VW.LIGHT_ON).ToString());
                    break;

                case "EN_OPEN":
                    SendSerial(Serial.EL.ToString(), ((int)EL.OPEN1).ToString());
                    break;
                case "EN_CLOSE":
                    SendSerial(Serial.EL.ToString(), ((int)EL.CLOSE1).ToString());
                    break;
                case "EX_OPEN":
                    SendSerial(Serial.EL.ToString(), ((int)EL.OPEN2).ToString());
                    break;
                case "EX_CLOSE":
                    SendSerial(Serial.EL.ToString(), ((int)EL.CLOSE2).ToString());
                    break;
                case "EX_ALL":
                    SendSerial(Serial.EL.ToString(), ((int)EL.OPENALL).ToString());
                    break;
                case "EN_ALL":
                    SendSerial(Serial.EL.ToString(), ((int)EL.CLOSEALL).ToString());
                    break;
                case "FACE_IDLE":          
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.39", 8020);
                    break;
                case "FACE_RECO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.39", 8020);
                    break;
                case "FACE_RECO_END":
                    SendSerial(Serial.EL.ToString(), EL.OPEN1.ToString());
                    break;
                case "EV_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.11", 8020);
                    break;

                
                case "EV_VIDEO_MUTEOFF":
                case "EV_VIDEO_MUTEOFF_EN":
                    SendSerial(Serial.EL.ToString(), ((int)EL.LIGHT_OFF).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.11", 8020);
                    break;

                case "EV_VIDEO_MUTEON":
                case "EV_VIDEO_MUTEON_EN":
                    SendSerial(Serial.EL.ToString(), ((int)EL.LIGHT_OFF).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.11", 8020);
                    break;

                case "EV_VIDEO":
                    Logger.Log2("192.168.0.11");
                    SendSerial(Serial.EL.ToString(), ((int)EL.LIGHT_OFF).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.11", 8020);
                    break;

                case "EV_VIDEO_END":
                    SendSerial(Serial.EL.ToString(), ((int)EL.LIGHT_ON).ToString());
                    SendSerial(Serial.EL.ToString(), ((int)EL.OPEN2).ToString());
                    break;

                case "OF_ON":
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_ON).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_ON).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_UP).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;
                case "OF_OFF":
                    SendSerial(Serial.CL.ToString(), ((int)CL.LIGHT_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.AIR_OFF).ToString());
                    SendSerial(Serial.CL.ToString(), ((int)CL.BL_DOWN).ToString());
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;
                case "CEO_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;

                case "CEO_SOLUTION_MUTEOFF":
                case "CEO_SOLUTION_MUTEOFF_EN":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;

                case "CEO_SOLUTION_MUTEON":
                case "CEO_SOLUTION_MUTEON_EN":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;

                case "CEO_SOLUTION":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;
                case "CEO_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.37", 8020);
                    break;
                case "MB_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.36", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.35", 8020);
                    break;
                case "MB_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.36", 8020);
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.35", 8020);
                    break;
                case "WS_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.34", 8020);
                    break;
                case "WS_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.34", 8020);
                    break;


                case "CL_IDLE":
                    OSCSenderHelper.Instance.Send("192.168.0.14", "1");
                    break;
                case "CL_VIDEO":
                    OSCSenderHelper.Instance.Send("192.168.0.14", "2");
                    break;

                case "SCP_IDLE":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "1");
                    break;
                case "SCP_VIDEO0":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "2");
                    break;
                case "SCP_VIDEO1":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "3");
                    break;
                case "SCP_VIDEO2":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "4");
                    break;
                case "SCP_VIDEO3":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "5");
                    break;
                case "SCP_VIDEO4":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "6");
                    break;
                case "SCP_VIDEO5":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "7");
                    break;
                case "SCP_VIDEO6":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "8");
                    break;
                case "SCP_VIDEO7":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "9");
                    break;
                case "SCP_VIDEO8":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "10");
                    break;

                case "SCP_VIDEO0_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "12");
                    break;
                case "SCP_VIDEO1_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "13");
                    break;
                case "SCP_VIDEO2_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "14");
                    break;
                case "SCP_VIDEO3_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "15");
                    break;
                case "SCP_VIDEO4_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "16");
                    break;
                case "SCP_VIDEO5_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "17");
                    break;
                case "SCP_VIDEO6_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "18");
                    break;
                case "SCP_VIDEO7_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "19");
                    break;
                case "SCP_VIDEO8_EN":
                    OSCSenderHelper.Instance.Send("192.168.0.12", "20");
                    break;

                case "DC_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.15", 8020);
                    SendSerial(Serial.DATACENTER.ToString(), ((int)DATACENTER_DOOR.UP).ToString());
                    break;
                case "DC_VIDEO_1":
                case "DC_VIDEO_2":
                case "DC_VIDEO_3":
                case "DC_VIDEO_4":
                case "DC_VIDEO_5":
                case "DC_VIDEO_6":
                case "DC_VIDEO_7":
                case "DC_VIDEO_8":
                case "DC_VIDEO_FULL":
                case "DC_SHOW":
                case "DC_HOME":
                case "DC_VIDEO_1_EN":
                case "DC_VIDEO_2_EN":
                case "DC_VIDEO_3_EN":
                case "DC_VIDEO_4_EN":
                case "DC_VIDEO_5_EN":
                case "DC_VIDEO_6_EN":
                case "DC_VIDEO_7_EN":
                case "DC_VIDEO_8_EN":
                case "DC_VIDEO_FULL_EN":
                case "DC_SHOW_EN":
                case "DC_HOME_EN":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.15", 8020);
                    SendSerial(Serial.DATACENTER.ToString(), ((int)DATACENTER_DOOR.DOWN).ToString());
                    break;
                case "DC_PPT":
                case "DC_OFF":
                case "DC_PPT_EN":
                case "DC_OFF_EN":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.15", 8020);
                    break;
                case "MSP_IDLE":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.13", 8020);
                    break;
                case "MSP_DEMO":
                    ProtocolUdpHelper.Instance.SendWithIpAsync(code, "192.168.0.13", 8020);
                    break;

                default:
                    break;
            }
        }

    }
}
