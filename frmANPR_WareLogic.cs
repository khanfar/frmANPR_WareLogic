using ANPR_General.Entity;
using Microsoft.Office.Interop.Excel;
using SimpleLPR3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VidGrab.WinUtil;
using Rectangle = System.Drawing.Rectangle;
using ANPR_General.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;
using System.Diagnostics.Eventing.Reader;
using Org.BouncyCastle.Asn1.Ocsp;
using DataModel;
using VidGrab;
using DevExpress.XtraPrinting;

namespace ANPR_General
{
    public partial class frmANPR_WareLogic : Form
    {
        private bool FlagIsLangEngl = false;
        private bool FlagStart = false;
        private int ThersholdSeconds = 0;

        private int _res_x_cam1=0;
        private int _res_x_cam2=0;
        private int _res_x_cam3=0;
        private int _res_x_cam4=0;

        private int _res_y_cam1 = 0;
        private int _res_y_cam2 = 0;
        private int _res_y_cam3 = 0;
        private int _res_y_cam4 = 0;
        private bool StartupFlg = false;

        private bool cam1Recording = false;
        private bool cam2Recording = false;
        private bool cam3Recording = false;
        private bool cam4Recording = false;


        #region Programming Variables 
        public bool FlgSystemClose = false;
        public int Prg_Cam2_Vid_Type = 1;

        //*** if this flag is true that means listed type will skip and lsited id
        //will save as camera wise
        public bool FlgSkipListedType = false;

        public int PicAppends_Code = 1; // ** if one then it will save city also with fill box
        public int CityFormatCode = 1;

        public bool FlgWebPicSave = true;

        #endregion

        //************** End of Software Version Change *********

        private bool FlgCtrl1Zoom = false;
        private bool FlgCtrl2Zoom = false;
        private bool FlgCtrl3Zoom = false;
        private bool FlgCtrl4Zoom = false;

        int cam1Id = 0;
        int cam2Id = 0;
        int cam3Id = 0;
        int cam4Id = 0;

        private int ct = 0;
        ISimpleLPR _lpr;
        IProcessor _proc;
        string _curFile;
        /*Bitmap _curBitmap*/
        List<Candidate> _curCands;

        List<string> files;
        int enumF;
        Communication c;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
           (
           int nLeft,
           int nTop,
           int nRight,
           int nBottom,
           int nWidthEllipse,
           int nHeightEllipse
           );


        public frmANPR_WareLogic(bool startup)
        {
            InitializeComponent();
            StartupFlg = startup;
        }

        private void StartVideo()
        {

            try
            {
                //string _url = "rtsp://91.93.150.178:1161/main";

                string _url = "rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mp4";
                string _userName1 = "admin";
                string _password1 = "ms123456";

                string _userName2 = "admin";
                string _password2 = "Covv01*.";

                string _userName3 = "admin";
                string _password3 = "ms123456";

                string _userName4 = "admin";
                string _password4 = "ms123456";

                

                string streamURL1 = ConfigurationManager.AppSettings.Get("camer1URL");
                string streamURL2 = ConfigurationManager.AppSettings.Get("camer2URL");
                string streamURL3 = ConfigurationManager.AppSettings.Get("camer3URL");
                string streamURL4 = ConfigurationManager.AppSettings.Get("camer4URL");

                _userName1 = ConfigurationManager.AppSettings.Get("camer1UserId");
                _password1 = ConfigurationManager.AppSettings.Get("camer1Password");
                _url = streamURL1;

                ThersholdSeconds = Convert.ToInt16(ConfigurationManager.AppSettings.Get("thersholdsecond"));

                DAL dal = new DAL();

                DataSet ds = new DataSet();
                ds=dal.Read_CameraInfo();

                int cam_ct = 0;
                bool camEnable = false;

                chkCam1.Checked = false;
                chkCam2.Checked=false;
                chkCam3.Checked=false;
                chkCam4.Checked=false;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                     cam_ct += 1;
                    camEnable = Convert.ToBoolean(dr["Cam_Enable"].ToString());

                    if (cam_ct == 1)
                    {
                        streamURL1 = dr["Cam_StreamURL"].ToString();
                        _userName1 = dr["User_Name"].ToString();
                        _password1 = dr["User_Password"].ToString();
                        
                        cam1Id = Convert.ToInt16( dr["Cam_Id"].ToString());

                        if (dr["Resolut_Width"].ToString()!="")
                        {
                            _res_x_cam1 = Convert.ToInt16(dr["Resolut_Width"].ToString());
                        }
                        else
                        {
                            _res_x_cam1 = 1280;
                        }

                        if (dr["Resolut_Height"].ToString() != "")
                        {
                            _res_y_cam1 = Convert.ToInt16(dr["Resolut_Height"].ToString());
                        }
                        else
                        {
                            _res_y_cam1 = 1024;
                        }

                        if (dr["IsRecording"].ToString() != "")
                        {
                            cam1Recording = Convert.ToBoolean(dr["IsRecording"].ToString());
                        }


                        chkCam1.Checked = camEnable;
                       
                        
                    }

                    if (cam_ct == 2)
                    {
                        streamURL2 = dr["Cam_StreamURL"].ToString();
                        _userName2 = dr["User_Name"].ToString();
                        _password2 = dr["User_Password"].ToString();

                        cam2Id = Convert.ToInt16(dr["Cam_Id"].ToString());
                        if (dr["Resolut_Width"].ToString() != "")
                        {
                            _res_x_cam2 = Convert.ToInt16(dr["Resolut_Width"].ToString());
                        }
                        else
                        {
                            _res_x_cam2 = 1280;
                        }

                        if (dr["Resolut_Height"].ToString() != "")
                        {
                            _res_y_cam2 = Convert.ToInt16(dr["Resolut_Height"].ToString());
                        }
                        else
                        {
                            _res_y_cam2 = 1024;
                        }

                        if (dr["IsRecording"].ToString() != "")
                        {
                            cam2Recording = Convert.ToBoolean(dr["IsRecording"].ToString());
                        }

                        chkCam2.Checked = camEnable;
                    }

                    if (cam_ct == 3)
                    {
                        streamURL3 = dr["Cam_StreamURL"].ToString();
                        _userName3 = dr["User_Name"].ToString();
                        _password3 = dr["User_Password"].ToString();
                        cam3Id = Convert.ToInt16(dr["Cam_Id"].ToString());
                        if (dr["Resolut_Width"].ToString() != "")
                        {
                            _res_x_cam3 = Convert.ToInt16(dr["Resolut_Width"].ToString());
                        }
                        else
                        {
                            _res_x_cam3 = 1280;
                        }

                        if (dr["Resolut_Height"].ToString() != "")
                        {
                            _res_y_cam3 = Convert.ToInt16(dr["Resolut_Height"].ToString());
                        }
                        else
                        {
                            _res_y_cam3 = 1024;
                        }

                        if (dr["IsRecording"].ToString() != "")
                        {
                            cam3Recording = Convert.ToBoolean(dr["IsRecording"].ToString());
                        }

                        chkCam3.Checked = camEnable;
                    }

                    if (cam_ct == 4)
                    {
                        streamURL4 = dr["Cam_StreamURL"].ToString();
                        _userName4 = dr["User_Name"].ToString();
                        _password4 = dr["User_Password"].ToString();
                        cam4Id = Convert.ToInt16(dr["Cam_Id"].ToString());
                        if (dr["Resolut_Width"].ToString() != "")
                        {
                            _res_x_cam4 = Convert.ToInt16(dr["Resolut_Width"].ToString());
                        }
                        else
                        {
                            _res_x_cam4 = 1280;
                        }

                        if (dr["Resolut_Height"].ToString() != "")
                        {
                            _res_y_cam4 = Convert.ToInt16(dr["Resolut_Height"].ToString());
                        }
                        else
                        {
                            _res_y_cam4 = 1024;
                        }

                        if (dr["IsRecording"].ToString() != "")
                        {
                            cam4Recording = Convert.ToBoolean(dr["IsRecording"].ToString());
                        }

                        chkCam4.Checked = camEnable;
                    }

                }


                int no_of_chanels = 4;

                if (chkCam1.Checked)
                {
                    VideoGrabber1.VideoSource = VidGrab.TVideoSource.vs_IPCamera;
                    VideoGrabber1.IPCameraURL = streamURL1;
                    VideoGrabber1.SetAuthentication(VidGrab.TAuthenticationType.at_IPCamera, _userName1, _password1);
                    VideoGrabber1.MotionDetector_Enabled = true;
                    VideoGrabber1.AutoRefreshPreview = true;

                    VideoGrabber1.LicenseString = "84323139359020801837-29HEB";
                    VideoGrabber1.LicenseString = "DTSTDRTSP:9737956226915483678234-41HEB";
                    //VideoGrabber1.MotionDetector_GloballyIncOrDecSensitivity(1);
                    VideoGrabber1.MotionDetector_MotionResetMs = 3000;

                    if (cam1Recording)
                    {
                        //*********** Recording ****************
                        string recordingpath = c.Get_Path(Communication.PathType.Recording, cam1Id);
                        recordingpath += "\\cam1-" + DateTime.Now.ToString("HH-mm");
                        VideoGrabber1.RecordingFileName = recordingpath;
                        VideoGrabber1.RecordingMethod = VidGrab.TRecordingMethod.rm_MP4;
                        VideoGrabber1.StartRecording();
                        VideoGrabber1.RecordToNewFileNow(recordingpath, false);
                    }
                    else
                    {
                        VideoGrabber1.StartPreview();
                    }
                        

                }

                
                
                if (txtFilName.Text!="")
                {

                    if (Prg_Cam2_Vid_Type == 1) //******* For Video
                    {
                        Communication c = new Communication();
                        string filePath = c.Get_Path(Communication.PathType.BinPath);
                        //filePath = filePath + "\\car_M1.webm";

                        if(System.IO.File.Exists(txtFilName.Text))
                        {
                            filePath = txtFilName.Text;
                            VideoGrabber2.MotionDetector_Enabled = true;
                            VideoGrabber2.AutoRefreshPreview = true;

                            VideoGrabber2.LicenseString = "84323139359020801837-29HEB";
                            VideoGrabber2.LicenseString = "DTSTDRTSP:9737956226915483678234-41HEB";
                            VideoGrabber2.MotionDetector_MotionResetMs = 3000;
                            VideoGrabber2.VideoSource = VidGrab.TVideoSource.vs_VideoFileOrURL;
                            VideoGrabber2.VideoSource_FileOrURL = filePath;
                        }
                        else
                        {
                            MessageBox.Show("File is not Exit");
                        }

                       

                       
                        //VideoGrabber2.StartRecording();
                        //VideoGrabber2.StartPreview();

                    }
                    else if (Prg_Cam2_Vid_Type == 2)
                    {
                        VideoGrabber2.VideoSource = VidGrab.TVideoSource.vs_VideoCaptureDevice;

                    }
                    VideoGrabber2.StartPreview();
                }

                else
                {
                    if (chkCam2.Checked)
                    {

                        VideoGrabber2.VideoSource = VidGrab.TVideoSource.vs_IPCamera;
                        VideoGrabber2.IPCameraURL = streamURL2;
                        VideoGrabber2.SetAuthentication(VidGrab.TAuthenticationType.at_IPCamera, _userName2, _password2);
                        VideoGrabber2.MotionDetector_Enabled = true;
                        VideoGrabber2.AutoRefreshPreview = true;

                        VideoGrabber2.LicenseString = "84323139359020801837-29HEB";
                        VideoGrabber2.LicenseString = "DTSTDRTSP:9737956226915483678234-41HEB";
                        VideoGrabber2.MotionDetector_MotionResetMs = 3000;

                        //************ For recording ********************

                        if (cam2Recording)
                        {
                            //*********** Recording ****************
                            string recordingpath = c.Get_Path(Communication.PathType.Recording, cam2Id);
                            recordingpath += "\\cam1-" + DateTime.Now.ToString("HH-mm");
                            VideoGrabber2.RecordingFileName = recordingpath;
                            VideoGrabber2.RecordingMethod = VidGrab.TRecordingMethod.rm_MP4;
                            VideoGrabber2.StartRecording();
                            VideoGrabber2.RecordToNewFileNow(recordingpath, false);
                        }
                        else
                        {
                            VideoGrabber2.StartPreview();
                        }

                    }


                }


                if (chkCam3.Checked)
                {
                    if (no_of_chanels >= 3)
                    {
                        VideoGrabber3.VideoSource = VidGrab.TVideoSource.vs_IPCamera;
                        VideoGrabber3.IPCameraURL = streamURL3;
                        VideoGrabber3.SetAuthentication(VidGrab.TAuthenticationType.at_IPCamera, _userName3, _password3);
                        VideoGrabber3.MotionDetector_Enabled = true;
                        VideoGrabber3.AutoRefreshPreview = true;
                        VideoGrabber3.MotionDetector_MotionResetMs = 3000;

                        VideoGrabber3.LicenseString = "84323139359020801837-29HEB";
                        VideoGrabber3.LicenseString = "DTSTDRTSP:9737956226915483678234-41HEB";
           

                        if (cam3Recording)
                        {
                            //*********** Recording ****************
                            string recordingpath = c.Get_Path(Communication.PathType.Recording, cam3Id);
                            recordingpath += "\\cam3-" + DateTime.Now.ToString("HH-mm");
                            VideoGrabber3.RecordingFileName = recordingpath;
                            VideoGrabber3.RecordingMethod = VidGrab.TRecordingMethod.rm_MP4;
                            VideoGrabber3.StartRecording();
                            VideoGrabber3.RecordToNewFileNow(recordingpath, false);
                        }
                        else
                        {
                            VideoGrabber3.StartPreview();
                        }



                    }
                }

                if (chkCam4.Checked)
                {
                    if (no_of_chanels >= 4)
                    {
                        VideoGrabber4.VideoSource = VidGrab.TVideoSource.vs_IPCamera;
                        VideoGrabber4.IPCameraURL = streamURL4;
                        VideoGrabber4.SetAuthentication(VidGrab.TAuthenticationType.at_IPCamera, _userName4, _password4);
                        VideoGrabber4.MotionDetector_Enabled = true;
                        VideoGrabber4.AutoRefreshPreview = true;
                        VideoGrabber4.MotionDetector_MotionResetMs = 3000;
                        VideoGrabber4.LicenseString = "84323139359020801837-29HEB";
                        VideoGrabber4.LicenseString = "DTSTDRTSP:9737956226915483678234-41HEB";
                    

                        if (cam4Recording)
                        {
                            //*********** Recording ****************
                            string recordingpath = c.Get_Path(Communication.PathType.Recording, cam4Id);
                            recordingpath += "\\cam4-" + DateTime.Now.ToString("HH-mm");
                            VideoGrabber4.RecordingFileName = recordingpath;
                            VideoGrabber4.RecordingMethod = VidGrab.TRecordingMethod.rm_MP4;
                            VideoGrabber4.StartRecording();
                            VideoGrabber4.RecordToNewFileNow(recordingpath, false);
                        }
                        else
                        {
                            VideoGrabber4.StartPreview();
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "-" + MethodBase.GetCurrentMethod().Name);
            }
        }

        private void VideoGrabber1_OnMotionDetected(object sender, VidGrab.TOnMotionDetectedEventArgs e)
        {
            double current_ml = VideoGrabber1.MotionDetector_GlobalMotionRatio;
            ProcessANPR(sender, e, 1, current_ml);
        }

        private void VideoGrabber2_OnMotionDetected(object sender, VidGrab.TOnMotionDetectedEventArgs e)
        {
            double current_ml = VideoGrabber2.MotionDetector_GlobalMotionRatio;
            ProcessANPR(sender, e, 2, current_ml);
        }

        private void VideoGrabber3_OnMotionDetected(object sender, VidGrab.TOnMotionDetectedEventArgs e)
        {

            double current_ml = VideoGrabber3.MotionDetector_GlobalMotionRatio;
            ProcessANPR(sender, e, 3, current_ml);
        }

        private void VideoGrabber4_OnMotionDetected(object sender, VidGrab.TOnMotionDetectedEventArgs e)
        {
            double current_ml = VideoGrabber4.MotionDetector_GlobalMotionRatio;
            ProcessANPR(sender, e, 4, current_ml);
        }

        private void ProcessANPR(object sender, VidGrab.TOnMotionDetectedEventArgs e,int CamId, double current_ml)
        {
            string tag = " - tag1";

            try
            {

                int MaxMotionXLocation = 0;
                int MaxMotionYLocation = 0;

                int _resl_x = 1280;
                int _resl_y = 1024;

                double mt_lvl =  Convert.ToDouble(ConfigurationManager.AppSettings.Get("motionLevel"));


                //double current_ml = VideoGrabber1.MotionDetector_GlobalMotionRatio;

                SystemSetting s = new SystemSetting();
                CaptureSetting c = new CaptureSetting();
                c = s.CaptureSetting();

                tag = " - tag2";
                if (current_ml >= mt_lvl)
                {

                    try
                    {

                        if(CamId==1)
                        {
                            _resl_x = _res_x_cam1;
                            _resl_y = _res_y_cam1;

                            System.IntPtr FrameBitmap = (System.IntPtr)VideoGrabber1.GetLastFrameAsHBITMAP(0, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            if (FrameBitmap != System.IntPtr.Zero) // Check if the bitmap handle is valid
                            {
                                using (Image op_img = Image.FromHbitmap(FrameBitmap)) // Dispose of the Image object when done
                                {
                                    DeleteObject(FrameBitmap);
                                    ProcessFrame_LPR(op_img, CamId);
                                }
                                tag = " - tag3";

                            }
                        }        




                
                            
                            
                            

                        

                        if (CamId == 2)
                        {
                            _resl_x = _res_x_cam2;
                            _resl_y = _res_y_cam2;

                            System.IntPtr FrameBitmap = (System.IntPtr)VideoGrabber2.GetLastFrameAsHBITMAP(0, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            Image op_img = Image.FromHbitmap(FrameBitmap);
                            DeleteObject(FrameBitmap);
                            ProcessFrame_LPR(op_img, CamId);
                            tag = " - tag3";
                           
                        }

                        if (CamId == 3)
                        {
                            _resl_x = _res_x_cam3;
                            _resl_y = _res_y_cam3;

                            System.IntPtr FrameBitmap = (System.IntPtr)VideoGrabber3.GetLastFrameAsHBITMAP(0, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            Image op_img = Image.FromHbitmap(FrameBitmap);
                            DeleteObject(FrameBitmap);
                            ProcessFrame_LPR(op_img, CamId);
                            tag = " - tag3";
                            //Marshal.FreeHGlobal(FrameBitmap);
                            //************************ Previous Frame - 1

                            //System.IntPtr FrameBitmap1 = (System.IntPtr)VideoGrabber3.GetLastFrameAsHBITMAP(3, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            //Image op_img1 = Image.FromHbitmap(FrameBitmap1);
                            //DeleteObject(FrameBitmap1);
                            //ProcessFrame_LPR(op_img1, CamId);
                        }

                        if (CamId == 4)
                        {
                            _resl_x = _res_x_cam4;
                            _resl_y = _res_y_cam4;

                            System.IntPtr FrameBitmap = (System.IntPtr)VideoGrabber4.GetLastFrameAsHBITMAP(0, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            Image op_img = Image.FromHbitmap(FrameBitmap);
                            DeleteObject(FrameBitmap);
                            ProcessFrame_LPR(op_img, CamId);
                            tag = " - tag3";
                            //Marshal.FreeHGlobal(FrameBitmap);
                            //************************ Previous Frame - 1

                            //System.IntPtr FrameBitmap1 = (System.IntPtr)VideoGrabber4.GetLastFrameAsHBITMAP(3, false, 0, 0, 0, 0, _resl_x, _resl_y, 32);
                            //Image op_img1 = Image.FromHbitmap(FrameBitmap1);
                            //DeleteObject(FrameBitmap1);
                            //ProcessFrame_LPR(op_img1, CamId);
                        }

                    }
                    catch(Exception ex1)
                    {
                        MessageBox.Show(ex1.Message + tag + "-" + MethodBase.GetCurrentMethod().Name);
                    }

                    //********************* Save All Frames *********
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + tag, MethodBase.GetCurrentMethod().Name);
            }

        }

        public void ProcessFrame_LPR(Image op_img, int CamId)
        {
            string tag = " - tag1";
            try
            {

                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string binDirectory = Path.GetDirectoryName(currentDirectory);

                string imgPath = c.Get_Path(Communication.PathType.Motion_Img, CamId); ;

                ct += 1;

                imgPath = imgPath + DateTime.Now.ToString("mmss") + ct.ToString() + ".jpg";

                op_img.Save(imgPath);

                tag = " - tag2";
                ////********************** Live 

                if (_proc == null)
                {
                    string licPath = currentDirectory + "\\Third Party Lib\\key.xml";
                    try
                    {
                        
                        //MessageBox.Show(licPath);

                        _lpr.set_productKey(licPath);
                         // Enable Germany, Spain and the United Kingdom
                        _lpr.set_countryWeight("Colombia", 1.0f);
                        
                        // Apply changes
                        _lpr.realizeCountryWeights();

                        _proc = _lpr.createProcessor();
                        _proc.plateRegionDetectionEnabled = true;
                        _proc.cropToPlateRegionEnabled = true;

                       

                    }
                   catch(Exception ex2)
                    {
                        MessageBox.Show(ex2.Message, "Lic Key Reading error");
                        MessageBox.Show(licPath);
                    }
                }

                Bitmap bitmap = new Bitmap(op_img);
                tag = " - tag3";
                _curCands = analyzeBitmap(bitmap);

                //*******Draw text
                op_img= drawImage(bitmap);

                string _np = "";
                double _conf = 0;


                if (_curCands != null && _curCands.Count > 0)
                {

                    for (int i = 0; i < _curCands.Count; ++i)
                    {
                        if (_curCands[i].matches.Count > 0)
                        {

                            _np = _curCands[i].matches[0].text;
                            _conf = Convert.ToDouble(_curCands[i].plateDetectionConfidence);
                            tag = " - tag4";
                            if (Validate_Np(_np, _conf, CamId))
                            {
                                int _anpr_id = 0;
                                string _picPath = "";
                                string _picPath_Hd = "";
                                string _CamStr = "";

                                int camCode = 0;

                                if(CamId==1)
                                {
                                    camCode = cam1Id;
                                }
                                else if (CamId == 2)
                                {
                                    camCode = cam2Id;
                                }
                                else if (CamId == 3)
                                {
                                    camCode = cam3Id;
                                }
                                else if (CamId == 4)
                                {
                                    camCode = cam4Id;
                                }

                                _CamStr = "Cam-" + CamId.ToString();
                                string webPath = "";
                                SaveImg(_np, op_img, _CamStr, ref _picPath,ref _picPath_Hd, camCode, ref webPath);
                                _conf = Math.Round((_conf * 100),4) ;
                                _anpr_id = Save_ANPRData(_np, camCode, _picPath, _picPath_Hd, _conf, CamId, webPath, op_img);
                            }
                            tag = " - tag5";
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + tag + "-" + MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SaveImg(string text, System.Drawing.Image preview, string camid, ref string _picpath, ref string _picpathHD,int intcamID,ref string WebPath)
        {
            Image mPreviewImage;


            string imgpath = c.Get_Path(Communication.PathType.ANPR_Imge, intcamID);

            imgpath = imgpath + "" + camid + "\\" + DateTime.Now.ToString("dd-MM-yy");

            bool exists = System.IO.Directory.Exists(imgpath);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(imgpath);
            }

            _picpath = imgpath + "\\" + text + "-" + DateTime.Now.ToString("HH-mm-ss") + ".jpg";

            _picpath = _picpath.Replace("?", "");

            if (preview != null)
            {
                mPreviewImage = (System.Drawing.Image)preview.Clone();

                //mPreviewImage.Save(_picpath);


                //********************* Save the Picture **********
                //**************************************************

                var stream = new System.IO.MemoryStream();
                mPreviewImage.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;

                //Stream strm = mPreviewImage.;
                double compRatio = 0.3;
                ReduceImageSize_Save(compRatio, stream, _picpath);

                //******************* Save HD Pictures ********************
                //*********************************************************

                string imgpath_HD = c.Get_Path(Communication.PathType.ANPR_ImgeHD, intcamID);
                imgpath_HD = imgpath_HD  + camid + "\\" + DateTime.Now.ToString("dd-MM-yy");

                 exists = System.IO.Directory.Exists(imgpath_HD);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(imgpath_HD);
                }

                _picpathHD = imgpath_HD + "\\" + text + "-" + DateTime.Now.ToString("HH-mm-ss") + ".jpg";

                mPreviewImage.Save(_picpathHD);

                if (FlgWebPicSave)
                {

                     WebPath = c.Get_Path(Communication.PathType.WebImage, intcamID);

                    WebPath += text + "-" + DateTime.Now.ToString("HH-mm-ss") + ".jpg";
                    mPreviewImage.Save(WebPath);
                }


                //*********** End of Save Pic Path ****************************
            }

        }

        private void ReduceImageSize_Save(double scaleFactor, Stream sourcePath, string targetPath)
        {
            using (var image = System.Drawing.Image.FromStream(sourcePath))
            {
                var newWidth = (int)(image.Width * scaleFactor);
                var newHeight = (int)(image.Height * scaleFactor);
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                thumbnailImg.Save(targetPath, image.RawFormat);
            }
        }



        private Bitmap drawImage(Bitmap _curBitmap)
        {
            Bitmap bmp = new Bitmap(_curBitmap); 
            
           
                if (_curBitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Indexed)
                {
                    using (Graphics gfx = Graphics.FromImage(bmp))
                    {
                        gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                        Pen skyBluePen = new Pen(Brushes.DeepSkyBlue, 1.0f);
                        Pen springGreenPen = new Pen(Brushes.Yellow, 2.0f);
                        StringFormat sf = new StringFormat();
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        sf.FormatFlags = StringFormatFlags.NoClip;

                        foreach (Candidate cd in _curCands)
                        {
                            if (cd.plateDetectionConfidence > 0)
                            {
                                gfx.DrawPolygon(springGreenPen, cd.plateRegionVertices);
                            }

                            if (cd.matches.Count > 0)
                            {
                                CountryMatch cm = cd.matches[0];
                                float x_value = 0;
                                float font_value = 0;
                                int count = 0;

                                foreach (Element e in cm.elements)
                                {
                                    //gfx.DrawRectangle(skyBluePen, e.bbox);
                                    count += 1;


                                    Color c1 = Color.FromKnownColor(KnownColor.Crimson);
                                    Color c2 = Color.FromKnownColor(KnownColor.Blue);

                                    double fLambda = e.confidence; // Math.Log((double)e.confidence + 1.0) / Math.Log(2.0);
                                    double fLambda_1 = 1.0 - fLambda;

                                    double fR = (double)c1.R * fLambda + (double)c2.R * fLambda_1;
                                    double fG = (double)c1.G * fLambda + (double)c2.G * fLambda_1;
                                    double fB = (double)c1.B * fLambda + (double)c2.B * fLambda_1;

                                    fR = 255;
                                    fG = 255;
                                    fB = 0;

                                    float characterSpacing = 25; // Adjust the spacing between characters as needed
                                    float x = (float)e.bbox.Left + (float)e.bbox.Width + 60 / 2.0f;
                                    x += characterSpacing;

                                    if (count == 1)
                                    {
                                        x_value = x;
                                        font_value = (float)e.bbox.Height + 15;
                                    }
                                    else
                                    {
                                        x_value +=   font_value;
                                    }

                                    Color c3 = Color.FromArgb((int)fR, (int)fG, (int)fB);
                                    using (Brush brush = new SolidBrush(c3))
                                    {
                                        using (System.Drawing.Font fnt = new System.Drawing.Font("Tahoma", font_value, GraphicsUnit.Pixel))
                                        {
                                            x = x + 1;
                                            gfx.DrawString(e.glyph.ToString(),
                                                            fnt,
                                                            brush,
                                                            x_value,
                                                            (float)e.bbox.Bottom + (float)e.bbox.Height * 1.2f + 30 / 2.0f,
                                                           sf);

                                        }
                                    }
                                }

                                //******** If PicAppends_Code==1 is  then it will print black box
                                #region Black Box

                                    if (PicAppends_Code == 1) //******** IF this is one then it will show the city on pic
                                    {

                                        //***** Here we will do loop for cm and will print number plate *****

                                        // Calculate the dimensions of the rectangle
                                        float rectWidth = 300; // Increased width by 40 pixels
                                        float rectHeight = 280; // Add padding of 10 pixels

                                        float rectX = 5;
                                        float rectY = 5;

                                        float x = rectX + 25;
                                        float y = rectY + 5;

                                        using (System.Drawing.Font textFont = new System.Drawing.Font("Tahoma", 36, GraphicsUnit.Pixel))
                                        {
                                            using (SolidBrush textBrush = new SolidBrush(Color.Yellow))
                                            {
                                                RectangleF rect = new RectangleF(rectX, rectY, rectWidth, rectHeight);
                                                gfx.FillRectangle(Brushes.Black, rect);
                                                gfx.DrawString(cm.text, textFont, textBrush, x, y);
                                            }
                                        }

                                        //********* For Fetch City and write text 
                                        string np = cm.text;
                                        if (np.Length > 0)
                                        {
                                            char fc = np[0];
                                            char lc = np[np.Length - 1];

                                            DAL dal = new DAL();
                                            DataSet ds = new DataSet();

                                            ds = dal.Read_CitynCountry_F1(fc.ToString(), lc.ToString());
                                            string city = "";
                                            string country = "";

                                            if (ds.Tables[0].Rows.Count > 0)
                                            {
                                                city = ds.Tables[0].Rows[0]["City_Name"].ToString();
                                                country = ds.Tables[0].Rows[0]["Country_Name"].ToString();
                                            }

                                            if (city != "")
                                            {
                                                using (System.Drawing.Font textFont = new System.Drawing.Font("Tahoma", 38, GraphicsUnit.Pixel))
                                                {
                                                    using (SolidBrush textBrush = new SolidBrush(Color.Yellow))
                                                    {
                                                        RectangleF rect = new RectangleF(rectX, rectY, rectWidth, rectHeight);
                                                        y += 45;
                                                        gfx.DrawString(city, textFont, textBrush, x, y);
                                                        y += 45;
                                                        gfx.DrawString(country, textFont, textBrush, x, y);

                                                    //*********** For Date and Time ***************

                                                        y += 45;
                                                      string dt = DateTime.Now.ToString("dd-MM-yyyy");
                                                       gfx.DrawString(dt, textFont, textBrush, x, y);

                                                        y += 45;
                                                    dt = DateTime.Now.ToString("HH:mm:ss");
                                                    gfx.DrawString(dt, textFont, textBrush, x, y);

                                                    y += 45;
                                                   
                                                    gfx.DrawString("MWK", textFont, textBrush, x, y);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                #endregion

                            }
                        }

                        gfx.Flush();
                    }

                    //imgPlate.Image = bmp;
                }
                else
                {
                    //imgPlate.Image = _curBitmap;
                }
           
            return bmp;

        }


        private List<Candidate> analyzeBitmap(Bitmap bm)
        {
            List<Candidate> cds = null;

            Rectangle r = new Rectangle(0, 0, bm.Width, bm.Height);
            BitmapData bmd = null;

            // Only PixelFormat.Format24bppRgb and PixelFormat.Format8bppIndexed are supported

            switch (bm.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    {
                        Bitmap bmClone = new Bitmap(bm.Width, bm.Height, PixelFormat.Format24bppRgb);

                        using (Graphics g = Graphics.FromImage(bmClone))
                        {
                            g.DrawImage(bm, r);
                        }

                        bm = bmClone;

                        // Convert to RGB to grayscale employing the standard NTSC CRT coefficients.
                        // In spite of the pixel format description, the internal layout in memory is (A)BGR.

                        bmd = bm.LockBits(r, ImageLockMode.ReadOnly, bm.PixelFormat);
                        cds = _proc.analyze_C3(bmd.Scan0,
                                               (uint)bmd.Stride,
                                               (uint)bm.Width,
                                               (uint)bm.Height,
                                               0.114f, 0.587f, 0.299f); // GDI++ INTERNAL LAYOUT IS BGR!!!
                    }
                    break;

                case PixelFormat.Format24bppRgb:
                    {
                        // Convert to RGB to grayscale employing the standard NTSC CRT coefficients.
                        // In spite of the pixel format description, the internal layout in memory is (A)BGR.

                        bmd = bm.LockBits(r, ImageLockMode.ReadOnly, bm.PixelFormat);
                        cds = _proc.analyze_C3(bmd.Scan0,
                                               (uint)bmd.Stride,
                                               (uint)bm.Width,
                                               (uint)bm.Height,
                                               0.114f, 0.587f, 0.299f); // GDI++ INTERNAL LAYOUT IS BGR!!!
                    }
                    break;
                case PixelFormat.Format8bppIndexed:
                    {
                        // Employ the 8bpp indexed raster directly. This will fail miserably in case that the bitmap palette is
                        // not trivial e.g. 0 -> {0,0,0}, 1 -> {1,1,1}, .. , 255 -> {255,255,255}.

                        bmd = bm.LockBits(r, ImageLockMode.ReadOnly, bm.PixelFormat);
                        cds = _proc.analyze(bmd.Scan0,
                                            (uint)bmd.Stride,
                                            (uint)bm.Width,
                                            (uint)bm.Height);
                    }
                    break;
                default:
                    throw new Exception(String.Format("Unsupported pixel format: {0}", bm.PixelFormat.ToString()));
            }

            return cds;
        }

        private bool Validate_Np(string _np, double _conf, int camid)
        {
            bool chkFiltercity = false;
            bool chkFilterLen = false;

            bool FlgTesting = true;

            bool FlgVald = false;
            double _req_conf = 0;

            DAL dal = new DAL();

            DataSet ds = new DataSet();

            ds = dal.Read_CaptureSetting();

            int MinNPLength = 0;
            int MaxNPLength = 0;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                MinNPLength = Convert.ToInt16( dr["MinNPLength"].ToString());
                MaxNPLength = Convert.ToInt16(dr["MaxNPLength"].ToString());
            }


            if (_np=="")
            {
                FlgVald = false;
                return FlgVald;
            }

            if (IsDouble(txtConfd.Text))
            {
                _req_conf = Convert.ToDouble(txtConfd.Text);
            }

            //for test
           

            if (chkFiltercity == false )
            {

                if (_np.Length>2)
                {
                    //if (IsNumeric(_np.Substring(0, 2)) && IsNumeric(_np.Substring(_np.Length - 2, 2)))
                    //{
                    //    FlgVald = true;
                    //}
                    //else
                    //{
                    //    FlgVald = false; //test

                    //    if(!FlgTesting)
                    //    {
                    //        return FlgVald;
                    //    }
                       
                    //}
                }
               
            }


            //********* Check Confidence level . this is last check 
            if (_conf >= _req_conf)
            {
                FlgVald = true;
            }
            else
            {
                FlgVald = true;

                if (!FlgTesting)
                {
                    return FlgVald;
                }
            }


            if (chkFilterLen == false)
            {

                if (MinNPLength>0 && MaxNPLength>0)
                {
                    if (_np.Length>= MinNPLength && _np.Length <= MaxNPLength)
                    {
                        FlgVald = true;
                    }
                    else
                    {
                        FlgVald = false;
                        if (!FlgTesting)
                        {
                            return FlgVald;
                        }
                    }
                }
                else
                {
                    if (_np.Length > 3)
                    {
                        FlgVald = true;
                    }
                    else
                    {
                        FlgVald = false;
                        if (!FlgTesting)
                        {
                            return FlgVald;
                        }
                    }

                }
                
            }

            if (FlgTesting)
            {
                FlgVald=true;
            }

            //******* Check Duplicate *****

            string _Currt_NP = "";
            DateTime _Currt_NP_dt = DateTime.Now;

            _Currt_NP = _np;
            ANPRDetail anpr;

            anpr = GetLastNPDetail(camid, _Currt_NP);

            if (anpr != null)
            {
                if (anpr.ANPR_NumberPlate != "")
                {
                    TimeSpan ts = _Currt_NP_dt - anpr.ANPR_Time;

                    if (ts.TotalSeconds > ThersholdSeconds)
                    {
                        FlgVald = true;
                    }
                    else
                    {
                        FlgVald = false;
                    }

                }

            }



            return FlgVald;
        }

        private ANPRDetail GetLastNPDetail(int camid, string np)
        {
            ANPRDetail anpr = new ANPRDetail();
            try
            {
                anpr.ANPR_NumberPlate = "";

                DAL dAL = new DAL();
                DataSet ds;

                ds = dAL.Read_LastNumberPlate(camid, np);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    anpr.ANPR_NumberPlate = dr["ANPR_NumberPlate"].ToString();
                    anpr.ANPR_Time = Convert.ToDateTime(dr["ANPR_Time"].ToString());
                }


            }
            catch (Exception ex)
            {

            }

            return anpr;
        }


        private int Save_ANPRData(string _np, int _cameraCode, string _picPath, string _picPath_HD, double _conf, int _cameraId,string WebPath,System.Drawing.Image preview)
        {
            int anpr_id = 0;

            try
            {

                bool FlgEmail = false;


                DAL dal = new DAL();
                DataSet ds = new DataSet();
                ds = dal.Read_VehicleCode(_np);
                int lst_Code = 3;
                int vhc_id = 0;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    lst_Code = Convert.ToInt16(dr["vhc_ListCode"].ToString());
                    vhc_id = Convert.ToInt16(dr["vhc_Id"].ToString());
                }

                if (FlgSkipListedType)
                {
                    lst_Code = _cameraId;
                }

                System.Drawing.Image image;

                System.Drawing.Image image1;
                image1 = Properties.Resources.green_circle;

                System.Drawing.Image image2;
                image2 = Properties.Resources.red_circle;

                System.Drawing.Image image3;
                image3 = Properties.Resources.yellow_circle;

                System.Drawing.Image image4;
                image4 = Properties.Resources.blue_circle;

                System.Drawing.Image image5;
                image5 = Properties.Resources.pink_circle;


                image = image3;

                if (lst_Code == 1)
                {
                    image = image1;
                }
                if (lst_Code == 2) // ******** For Black List Car
                {
                    image = image2;
                    FlgEmail = true;

                    string blacklist = c.Get_Path(Communication.PathType.BlackList, 0); 

                    var stream = new System.IO.MemoryStream();
                    preview.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;
                    blacklist += _np + "-" + DateTime.Now.ToString("HH-mm-ss") + ".jpg";
                    double  compRatio = 0.3;
                    ReduceImageSize_Save(compRatio, stream, blacklist);

                }
                if (lst_Code == 3)
                {
                    image = image3;
                }
                if (lst_Code == 4)
                {
                    image = image4;
                }
                if (lst_Code == 5)
                {
                    image = image5;
                }

                gridList.Rows.Add(image, _np, DateTime.Now.ToString("dd-MM-yy HH:mm:ss"));

                ANPRDetail A = new ANPRDetail();

                A.Cam_Id = _cameraCode;
                A.vhc_Id = vhc_id;
                A.vhc_ListCode = lst_Code;
                A.ANPR_NumberPlate = _np;
                A.Pic_Path = _picPath;
                A.Pic_Path_HD = _picPath_HD;
                A.Confidence_Level = _conf;
                A.Web_Path = WebPath;

                string err = dal.SaveANPR(A, ref anpr_id, CityFormatCode);

                //if(err != "")
                //{
                //    MessageBox.Show(err);
                //}
                ProcessTrg p = new ProcessTrg();
                p.Process_RPI_Trg(A,txtRpi_Ip.Text);

                if (FlgEmail)
                {
                    SendEmail(_np, lst_Code);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "-" + MethodBase.GetCurrentMethod().Name, "System Error");
            }

            return anpr_id;
        }

        private void SendEmail(string _np,int lstCode)
        {
            Communication c = new Communication();
            string dt = DateTime.Now.ToString("dd-MM-yy HH:mm:ss tt");

            if(lstCode==2)
            {
                c.Send_BlackListEmail(_np, dt);
            }
           
        }
        private void SetStatus()
        {
            if (!FlagStart)
            {
                btnPlay.Enabled = true;
                btnStop.Enabled = false;
            }
            else
            {
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
            }

        }
        private void Set_Grid()
        {

            // Icon treeIcon = new Icon(this.GetType(), Properties.Resources.circle_32);
            DataGridViewImageColumn dgvImageColumn = new DataGridViewImageColumn();
            dgvImageColumn.HeaderText = "";
            dgvImageColumn.ImageLayout = DataGridViewImageCellLayout.NotSet;

            DataGridViewTextBoxColumn dgvNoPlateColumn = new DataGridViewTextBoxColumn();
            dgvNoPlateColumn.HeaderText = "Number Plate";

            DataGridViewTextBoxColumn dgvdtColumn = new DataGridViewTextBoxColumn();
            dgvdtColumn.HeaderText = "Date Time";

            //gridList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridList.RowTemplate.Height = 30;
            gridList.AllowUserToAddRows = false;

            gridList.Columns.Add(dgvImageColumn);
            gridList.Columns.Add(dgvNoPlateColumn);
            gridList.Columns.Add(dgvdtColumn);

            gridList.Columns[0].Width = 30;
            gridList.Columns[1].Width = 110;
            gridList.Columns[2].Width = 100;

            gridList.Columns[1].HeaderText = "Plate No.";
            gridList.Columns[2].HeaderText = "Time";


        }

        public bool IsNumeric(string value)
        {
            if (value == "")
            {
                return false;
            }
            return value.All(char.IsNumber);
        }
        public bool IsDouble(string _value)
        {
            bool flg = false;
            double price;

            bool isDouble = Double.TryParse(_value, out price);
            if (isDouble)
            {
                flg = true;
            }

            return flg;
        }



        private void btnPlay_Click(object sender, EventArgs e)
        {
            startClick();
        }

        private void startClick()
        {
            StartVideo();
            FlagStart = true;
            SetStatus();
        }

        private void Init()
        {
            btnPlay.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnPlay.Width, btnPlay.Height, 10, 10));
            btnStop.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnStop.Width, btnStop.Height, 10, 10));
            Set_Lang();
            GetCaptureSetting_Info();

            if(StartupFlg)
            {
                startClick();
            }

            timer_Recording.Interval = 5 * 60000; //**5 mintues
            timer_Recording.Enabled = true;

        }

        private void Set_Lang()
        {

            Communication c = new Communication();
            bool FlgEngl = c.IsLangEnglish();

            if (FlgEngl)
            {
                btnPlay.Text = "Start";
                btnStop.Text = "Stop";
            }

        }

        private void frmANPR_WareLogic_Load(object sender, EventArgs e)
        {
            c = new Communication();
            InitVideoEngine();
            SetStatus();
            Set_Grid();
            string Count_Code;
            Count_Code = ConfigurationManager.AppSettings.Get("CountryCode");
            //txtCountryCode.Text = Count_Code;
            Init();
        }

        private void InitVideoEngine()
        {
            try
            {
                EngineSetupParms setupP;
                setupP.cudaDeviceId = -1; // Use CPU
                setupP.enableImageProcessingWithGPU = false;
                setupP.enableClassificationWithGPU = false;
                setupP.maxConcurrentImageProcessingOps = 0;  // Use the default value.  

                _lpr = SimpleLPR.Setup(setupP);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to initialize the SimpleLPR library", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                throw;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
               
               StopPreview();
               
            }
            catch (Exception ex)
            {

            }
        }

        private void StopPreview()
        {
            try
            {
                FlagStart = false;
                SetStatus();
                VideoGrabber1.StopPreview();
                VideoGrabber2.StopPreview();
                VideoGrabber3.StopPreview();
                VideoGrabber4.StopPreview();
            }
            catch (Exception ex)
            {

            }

        }



        private void Test_ANPT()
        {
            string tag = " - tag1";

            try
            {

                int MaxMotionXLocation = 0;
                int MaxMotionYLocation = 0;

                int _resl_x = 1280;
                int _resl_y = 1024;

                double mt_lvl = Convert.ToDouble(ConfigurationManager.AppSettings.Get("motionLevel"));


                //double current_ml = VideoGrabber1.MotionDetector_GlobalMotionRatio;

               

                    try
                    {
                            try
                            {

                        System.IntPtr FrameBitmap = (System.IntPtr)VideoGrabber1.GetLastFrameAsHBITMAP(0, false, 0, 0, 0, 0, 0, 0, 0);

                              Image op_img = Image.FromHbitmap(FrameBitmap);
                            DeleteObject(FrameBitmap);

                             ProcessFrame_LPR(op_img, 1);
                         
                            tag = " - tag3";
                                
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message);
                            }

                 


                }
                    catch (Exception ex1)
                    {
                        //MessageBox.Show(ex1.Message + tag, MethodBase.GetCurrentMethod().Name);
                    }

                    //********************* Save All Frames *********

                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + tag, MethodBase.GetCurrentMethod().Name);
            }

        }

        private void VideoGrabber1_OnMotionNotDetected(object sender, VidGrab.TOnMotionNotDetectedEventArgs e)
        {
            //Test_ANPT();
        }

        private void GetCaptureSetting_Info()
        {
            try
            {


                DAL dal = new DAL();
                DataSet dsCapt = new DataSet();
                DataSet dsPrc_trg = new DataSet();

                DataSet dsCam = new DataSet();

                dsCapt = dal.Read_CaptureSetting();

                foreach (DataRow dr in dsCapt.Tables[0].Rows)
                {
                    double hours = Convert.ToDouble(dr["AccuracyLevel"].ToString());

                    txtConfd.Text ="0.90";

                }
            }
            catch (Exception ex1)
            {
                //MessageBox.Show(ex1.Message + "-" + MethodBase.GetCurrentMethod().Name);
            }

            //********************* Save All Frames *********


        }

        private void frmANPR_WareLogic_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void chkCam4_CheckedChanged(object sender, EventArgs e)
        {
            ////VideoGrabber2.Recording_st;
            ////VideoGrabber2.SetRecordingPauseCreatesNewFile="";
            //string recordingpath = c.Get_Path(Communication.PathType.Recording);
            //recordingpath += "\\cam1-" + DateTime.Now.ToString("HH-mm");
            //VideoGrabber2.HoldRecording = true;
            //VideoGrabber2.RecordToNewFileNow(recordingpath, false);
            //VideoGrabber2.ResumeRecording();
        }

        private void chkCam3_CheckedChanged(object sender, EventArgs e)
        {
            ////VideoGrabber2.SetRecordingPauseCreatesNewFile="";
            //string recordingpath = c.Get_Path(Communication.PathType.Recording);
            //recordingpath += "\\cam1-" + DateTime.Now.ToString("HH-mm") +".mp4";

            ////VideoGrabber1.ResumeRecording();
            //VideoGrabber1.RecordToNewFileNow(recordingpath, false);
            
        }

        private void Recording_Blocks()
        {
            if (cam1Recording)
            {
                string recordingpath = c.Get_Path(Communication.PathType.Recording, cam1Id);
                recordingpath += "\\cam1-" + DateTime.Now.ToString("HH-mm") + ".mp4";

                //VideoGrabber1.ResumeRecording();
                VideoGrabber1.RecordToNewFileNow(recordingpath, false);
            }

            if (cam2Recording)
            {
                string recordingpath = c.Get_Path(Communication.PathType.Recording, cam2Id);
                recordingpath += "\\cam2-" + DateTime.Now.ToString("HH-mm") + ".mp4";

                //VideoGrabber1.ResumeRecording();
                VideoGrabber2.RecordToNewFileNow(recordingpath, false);
            }

            if (cam3Recording)
            {
                string recordingpath = c.Get_Path(Communication.PathType.Recording, cam3Id);
                recordingpath += "\\cam3-" + DateTime.Now.ToString("HH-mm") + ".mp4";

                //VideoGrabber1.ResumeRecording();
                VideoGrabber3.RecordToNewFileNow(recordingpath, false);
            }

            if (cam4Recording)
            {
                string recordingpath = c.Get_Path(Communication.PathType.Recording, cam4Id);
                recordingpath += "\\cam4-" + DateTime.Now.ToString("HH-mm") + ".mp4";

                //VideoGrabber1.ResumeRecording();
                VideoGrabber4.RecordToNewFileNow(recordingpath, false);
            }

        }

        private void gridList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void timer_Recording_Tick(object sender, EventArgs e)
        {
            Recording_Blocks();
        }

        private void VideoGrabber1_OnFrameProgress(object sender, VidGrab.TOnFrameProgressEventArgs e)
        {

        }

        private void VideoGrabber1_Load(object sender, EventArgs e)
        {

        }

       

        private void ZoomInOutControl(int CrlId)
        {

            tbllayout_Frame.RowStyles.Clear();
            tbllayout_Frame.ColumnStyles.Clear();

            if (CrlId==1)
            {
                for (int i = 0; i < tbllayout_Frame.RowCount; i++)
                {
                    if (i == 0)
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 0));
                    }

                }

                // Set the ColumnStyles to 50% for each column
                for (int i = 0; i < tbllayout_Frame.ColumnCount; i++)
                {
                    if (i == 0)
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0));
                    }
                }

            }

            if (CrlId == 2)
            {
                for (int i = 0; i < tbllayout_Frame.RowCount; i++)
                {
                    if (i == 0)
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 0));
                    }

                }

                // Set the ColumnStyles to 50% for each column
                for (int i = 0; i < tbllayout_Frame.ColumnCount; i++)
                {
                    if (i == 1)
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0));
                    }
                }

            }

            if (CrlId == 3)
            {
                for (int i = 0; i < tbllayout_Frame.RowCount; i++)
                {
                    if (i == 1)
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 0));
                    }

                }

                // Set the ColumnStyles to 50% for each column
                for (int i = 0; i < tbllayout_Frame.ColumnCount; i++)
                {
                    if (i == 0)
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0));
                    }
                }

            }

            if (CrlId == 4)
            {
                for (int i = 0; i < tbllayout_Frame.RowCount; i++)
                {
                    if (i == 1)
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 0));
                    }

                }

                // Set the ColumnStyles to 50% for each column
                for (int i = 0; i < tbllayout_Frame.ColumnCount; i++)
                {
                    if (i == 1)
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    }
                    else
                    {
                        tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0));
                    }
                }

            }


        }

        private void ResetLayoutControl()
        {
            // Adjust the RowStyles and ColumnStyles to accommodate the change in size of the maximized control
            tbllayout_Frame.RowStyles.Clear();
            tbllayout_Frame.ColumnStyles.Clear();

            // Set the RowStyles to 50% for each row
            for (int i = 0; i < tbllayout_Frame.RowCount; i++)
            {
               
               tbllayout_Frame.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
               

            }

            // Set the ColumnStyles to 50% for each column
            for (int i = 0; i < tbllayout_Frame.ColumnCount; i++)
            {
                tbllayout_Frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            }

        }



        private void VideoGrabber1_OnDblClick(object sender, EventArgs e)
        {
            

            if (!FlgCtrl1Zoom)
            {
                ZoomInOutControl(1);
                FlgCtrl1Zoom=true;
            }
            else
            {
                FlgCtrl1Zoom = false;
                ResetLayoutControl();
            }
        }

        private void VideoGrabber2_OnDblClick(object sender, EventArgs e)
        {
            if (!FlgCtrl2Zoom)
            {
                ZoomInOutControl(2);
                FlgCtrl2Zoom = true;
            }
            else
            {
                FlgCtrl2Zoom = false;
                ResetLayoutControl();
            }
        }

        private void VideoGrabber3_OnDblClick(object sender, EventArgs e)
        {

            if (!FlgCtrl3Zoom)
            {
                ZoomInOutControl(3);
                FlgCtrl3Zoom = true;
            }
            else
            {
                FlgCtrl3Zoom = false;
                ResetLayoutControl();
            }

        }

        private void VideoGrabber4_OnDblClick(object sender, EventArgs e)
        {
            if (!FlgCtrl4Zoom)
            {
                ZoomInOutControl(4);
                FlgCtrl4Zoom = true;
            }
            else
            {
                FlgCtrl4Zoom = false;
                ResetLayoutControl();
            }
        }

        private void VideoGrabber2_OnFrameBitmap(object sender, TOnFrameBitmapEventArgs e)
        {
            //Bitmap currentFrame = (Bitmap)e.bitmapInfo;
            Image frameImage = Image.FromHbitmap(e.bitmapInfo);
            ProcessFrame_LPR(frameImage, 2);
            frameImage.Dispose();
        }

        private void VideoGrabber2_OnFrameProgress(object sender, TOnFrameProgressEventArgs e)
        {
           
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set initial directory (optional)
            openFileDialog.InitialDirectory = "C:\\";

            // Set the filter for the file dialog (e.g., allow only image files)
            openFileDialog.Filter = "Video Files (*.mp4;*.avi;*.mkv;*.webm)|*.mp4;*.avi;*.mkv;*.webm";

            // Allow selecting multiple files (optional)
            openFileDialog.Multiselect = false;

            // Show the file dialog
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Retrieve the selected file path
                string selectedFilePath = openFileDialog.FileName;

                // Do something with the selected file path
                // For example, display the path in a text box
                txtFilName.Text = selectedFilePath;
            }
        }
    }
}
