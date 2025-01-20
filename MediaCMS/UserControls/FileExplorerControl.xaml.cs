using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using FluentFTP;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using WpfApp11;
using System.Drawing;
using System.Windows.Interop;
using System.Net;
using System.Collections.Generic;
using WpfApp11.Helpers;
using WpfApp11.UserControls;

namespace MediaCMS
{
    public partial class FileExplorerControl : UserControl
    {
        public event EventHandler CloseRequested;

        public class FileSystemItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string FullPath { get; set; }
            public BitmapSource IconSource { get; set; }
        }

        private ObservableCollection<FileSystemItem> _leftItems;
        private ObservableCollection<FileSystemItem> _rightItems;
        private FtpClient _ftpClient;
        private string _currentLocalPath = @"C:\GL-MEDIA";
        private string _currentLocalPath_real = @"C:\GL-MEDIA";
        private string _currentFtpPath = "/";
        string ftp_port = "21";
        //private string _currentFtpPath = "ftp://192.168.0.5";


        bool _isconnect = false;
        MainWindow main;
        public FileExplorerControl()
        {
            InitializeComponent();




























            //_currentLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //string[] drives = Directory.GetLogicalDrives();
            //string drivesList = string.Join(Environment.NewLine, drives);







            //drivelist_panel.Children.Clear();

            //// 드라이브 목록을 가져오기

            //// 각 드라이브를 StackPanel에 버튼으로 추가
            //foreach (string drive in drives)
            //{
            //    // 드라이브를 나타내는 버튼 생성
            //    Button driveButton = new Button
            //    {
            //        Width = 80,
            //        Content = drive,
            //        Margin = new Thickness(10,0,0,0),
            //        FontSize = 25
            //    };

            //    // 버튼 클릭 시 드라이브 경로를 표시하는 이벤트 핸들러 추가
            //    driveButton.Click += (s, args) =>
            //    {
            //        //MessageBox.Show($"드라이브 선택됨: {drive}");
            //        LoadLocalDirectory(drive);
            //    };

            //    // StackPanel에 버튼 추가
            //    drivelist_panel.Children.Add(driveButton);
            //}

            //Button driveButton2 = new Button
            //{
            //    Width = 120,
            //    Content = "바탕화면",
            //    Margin = new Thickness(10, 0, 0, 0),
            //    FontSize = 25
            //};

            //// 버튼 클릭 시 드라이브 경로를 표시하는 이벤트 핸들러 추가
            //driveButton2.Click += (s, args) =>
            //{
            //    //MessageBox.Show($"드라이브 선택됨: {drive}");
            //    LoadLocalDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //};

            //// StackPanel에 버튼 추가
            //drivelist_panel.Children.Add(driveButton2);












            main = Application.Current.MainWindow as MainWindow;


            _leftItems = new ObservableCollection<FileSystemItem>();
            _rightItems = new ObservableCollection<FileSystemItem>();
            LeftFileListView.ItemsSource = _leftItems;
            RightFileListView.ItemsSource = _rightItems;
            LeftFileListView.MouseDoubleClick += LeftFileListView_MouseDoubleClick;
            RightFileListView.MouseDoubleClick += RightFileListView_MouseDoubleClick;

            LeftFileListView.SelectionChanged += LeftFileListView_SelectionChanged;

            RightFileListView.SelectionChanged += RightFileListView_SelectionChanged;

        }

        private void RightFileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems_ct = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();

            if (selectedItems_ct.Count == 0)
            {








                set_ftp_select_not();
            }
            else if (selectedItems_ct.Count == 1)
            {














                if (selectedItems_ct[0].Type == "Parent Directory")
                {
                    set_ftp_select_not();
                }
                else
                {
                    set_ftp_select_one();

                }










            }
            else
            {
                set_ftp_select_multi();
            }
        }

        private void LeftFileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems_ct = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();

            if (selectedItems_ct.Count == 0)
            {
                set_local_select_not();
            }
            else if (selectedItems_ct.Count == 1)
            {




                if (selectedItems_ct[0].Type == "Parent Directory")
                {
                    set_local_select_not();
                }
                else
                {
                    set_local_select_one();


                }
            }
            else
            {
                set_local_select_multi();
            }
        }

        void set_local_select_not()
        {
            LocalCreateFolderButton.Visibility = Visibility.Visible;
            LocalEditButton.Visibility = Visibility.Collapsed;
            LocalDeleteButton.Visibility = Visibility.Collapsed;


        }

        void set_local_select_one()
        {
            LocalCreateFolderButton.Visibility = Visibility.Visible;
            LocalEditButton.Visibility = Visibility.Visible;
            LocalDeleteButton.Visibility = Visibility.Visible;
        }

        void set_local_select_multi()
        {
            LocalCreateFolderButton.Visibility = Visibility.Visible;
            LocalEditButton.Visibility = Visibility.Collapsed;
            LocalDeleteButton.Visibility = Visibility.Visible;
        }





        void set_ftp_select_not()
        {
            FtpCreateFolderButton.Visibility = Visibility.Visible;
            FtpEditButton.Visibility = Visibility.Collapsed;
            FtpDeleteButton.Visibility = Visibility.Collapsed;


        }

        void set_ftp_select_one()
        {
            FtpCreateFolderButton.Visibility = Visibility.Visible;
            FtpEditButton.Visibility = Visibility.Visible;
            FtpDeleteButton.Visibility = Visibility.Visible;
        }

        void set_ftp_select_multi()
        {
            FtpCreateFolderButton.Visibility = Visibility.Visible;
            FtpEditButton.Visibility = Visibility.Collapsed;
            FtpDeleteButton.Visibility = Visibility.Visible;
        }


        public void Initialize(string ftpAddress)
        {

            _currentLocalPath = main.local_path;

            if (!Directory.Exists(_currentLocalPath))
            {
                // 폴더가 존재하지 않으면 생성
                Directory.CreateDirectory(_currentLocalPath);
            }


            InitializeFtpConnection(ftpAddress);
            LoadLocalDirectory(_currentLocalPath);
            LoadFtpDirectory(_currentFtpPath);

            //cms_pc_name.Text = main.local_pc_name;

        }

        private void InitializeFtpConnection(string ftpAddress)
        {

            var target_ftp = "ftp://" + ftpAddress + ":" + ftp_port;
            try
            {
                ////원래====================================================================
                Uri ftpUri = new Uri(target_ftp);
                string host = ftpUri.Host;
                string username = "ftpuser";
                string password = "1";
                _ftpClient = new FtpClient(host, username, password);
                ////====================================================================





                //임시 ====================================================================
                //Uri ftpUri = new Uri("ftp://121.131.142.148:12923");
                //string host = ftpUri.Host;
                //string username = "engium";
                //string password = "1";
                //_ftpClient = new FtpClient(host, username, password, 12923);
                //====================================================================


                _ftpClient.Config.ConnectTimeout = 3000;


                _ftpClient.Connect();

                _isconnect = true;



            }
            catch (Exception ex)
            {
                _isconnect = false;
                MessageBox.Show("FTP 연결 오류: " + ex.Message);
                Logger.LogError("FTP 연결 오류: " + ex.Message);


            }
        }

        private void LoadLocalDirectory(string path)
        {

            if (_isconnect)
            {
                try
                {
                    _leftItems.Clear();
                    DirectoryInfo di = new DirectoryInfo(path);
                    if (path == _currentLocalPath)
                    {
                    }
                    else
                    {
                        if (di.Parent != null)
                        {

                            var item = new FileSystemItem
                            {
                                Name = "..",
                                Type = "Parent Directory",
                                FullPath = di.Parent.FullName,
                                IconSource = GetIconForFileType("folder")
                            };
                            _leftItems.Add(item);
                        }
                    }

                    foreach (var directory in di.GetDirectories())
                    {
                        BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_folder.png", UriKind.Relative));
                        var item = new FileSystemItem
                        {
                            Name = directory.Name,
                            Type = "Folder",
                            FullPath = directory.FullName,
                            IconSource = bitmapSource,


                        };
                        _leftItems.Add(item);
                    }

               

                    var listing2 = di.GetFiles();
                    //var sortedListing = listing
                    //    .OrderByDescending(item => item.Type == di.Directory)
                    //    .ThenBy(item => item.Name);

                    foreach (var item in listing2)
                    {

                        FileSystemItem fs = new FileSystemItem();
                        fs.Name = item.Name;
                        
                        fs.Type = "File";
                        fs.FullPath = item.FullName;


                        

                        string extension = Path.GetExtension(fs.FullPath).ToLower();

                        if (extension == "")
                        {
                            //fs.IconSource = GetIconForFileType(item.Type == FtpObjectType.Directory ? "folder" : item.Name);
                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_folder.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;
                        }
                        else if (extension == ".exe")
                        {
                            System.Drawing.Icon errorIcon = SystemIcons.Application;
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                errorIcon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            fs.IconSource = bitmapSource;
                        }
                        else if (extension == ".txt" || extension == ".zip" || extension == ".bat")
                        {

                            var temp_extension = extension.Split('.');



                            BitmapSource bitmapSource = new BitmapImage(new Uri($"../Images/icons/icon_{temp_extension[1]}.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;


                        }
                        else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_img.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;


                        }
                        else if (extension == ".mp4" || extension == ".wmv" || extension == ".avi" || extension == ".mkv")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_video.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }

                        else if (extension == ".mp3" || extension == ".wav")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_sound.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }
                        else if (extension == ".exe")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_exe.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }
                        else
                        {
                            //System.Drawing.Icon errorIcon = SystemIcons.Application;
                            //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            //    errorIcon.Handle,
                            //    Int32Rect.Empty,
                            //    BitmapSizeOptions.FromEmptyOptions());
                            //fs.IconSource = bitmapSource;



                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_etc.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;


                        }

                        _leftItems.Add(fs);
                    }

















































                    //_currentLocalPath = path;
                    //_currentLocalPath = @"C:\";
                    _currentLocalPath = main.local_path;
                    _currentLocalPath_real = path;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }

        }

        private void LoadFtpDirectory(string path)
        {

            if (_isconnect)
            {
                try
                {

                    _rightItems.Clear();
                    if (path != "/")
                    {
                        _rightItems.Add(new FileSystemItem
                        {
                            Name = "..",
                            Type = "Parent Directory",
                            FullPath = Path.GetDirectoryName(path).Replace('\\', '/'),
                            IconSource = GetIconForFileType("folder")
                        });
                    }

                    var listing = _ftpClient.GetListing(path);
                    var sortedListing = listing
                        .OrderByDescending(item => item.Type == FtpObjectType.Directory)
                        .ThenBy(item => item.Name);

                    foreach (var item in sortedListing)
                    {

                        FileSystemItem fs = new FileSystemItem();
                        fs.Name = item.Name;
                        fs.Type = item.Type == FtpObjectType.Directory ? "Folder" : "File";
                        fs.FullPath = item.FullName;



                        string extension = Path.GetExtension(fs.FullPath).ToLower();


                        if (extension == "")
                        {
                            //fs.IconSource = GetIconForFileType(item.Type == FtpObjectType.Directory ? "folder" : item.Name);
                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_folder.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;
                        }
                        else if (extension == ".exe")
                        {
                            System.Drawing.Icon errorIcon = SystemIcons.Application;
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                errorIcon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            fs.IconSource = bitmapSource;
                        }
                        else if (extension == ".txt" || extension == ".zip" || extension == ".bat")
                        {

                            var temp_extension = extension.Split('.');



                            BitmapSource bitmapSource = new BitmapImage(new Uri($"../Images/icons/icon_{temp_extension[1]}.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;


                        }
                        else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_img.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;


                        }
                        else if (extension == ".mp4" || extension == ".wmv" || extension == ".avi" || extension == ".mkv")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_video.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }

                        else if (extension == ".mp3" || extension == ".wav" )
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_sound.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }
                        else if (extension == ".exe")
                        {


                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_exe.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                        }
                        else
                        {
                            //System.Drawing.Icon errorIcon = SystemIcons.Application;
                            //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            //    errorIcon.Handle,
                            //    Int32Rect.Empty,
                            //    BitmapSizeOptions.FromEmptyOptions());
                            //fs.IconSource = bitmapSource;



                            BitmapSource bitmapSource = new BitmapImage(new Uri("../Images/icons/icon_etc.png", UriKind.Relative));
                            fs.IconSource = bitmapSource;

                            
                        }



                        _rightItems.Add(fs);
                    }

                    _currentFtpPath = path;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }
            else
            {
                main.OverlayGrid.Visibility = Visibility.Collapsed;
            }
        }





        //=======================================

        //private Icon GetShellIcon(int index)
        //{
        //    IntPtr hIcon;
        //    hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, @"C:\Windows\System32\shell32.dll", index);
        //    if (hIcon != IntPtr.Zero)
        //    {
        //        return Icon.FromHandle(hIcon);
        //    }
        //    return null;
        //}

        //// 네이티브 메서드 선언
        //private static class NativeMethods
        //{
        //    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //    public extern static IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);
        //}





        //private Icon GetIconFromDll(string dllPath, int iconIndex)
        //{
        //    IntPtr hIcon = IntPtr.Zero;
        //    try
        //    {
        //        hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, dllPath, iconIndex);
        //        if (hIcon != IntPtr.Zero)
        //        {
        //            return Icon.FromHandle(hIcon);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log2(ex.Message);
        //        MessageBox.Show($"Error extracting icon: {ex.Message}");
        //    }
        //    return null;
        //}

        //===================================================





        private void local_empty_click(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("빈거");
            //LeftFileListView.SelectedItems = 0;
            LeftFileListView.SelectedIndex = -1;


        }


        private void ftp_empty_click(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("빈거");
            //LeftFileListView.SelectedItems = 0;
            RightFileListView.SelectedIndex = -1;


        }






        private void LeftFileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListView)sender).SelectedItem as FileSystemItem;
            if (item != null && (item.Type == "Folder" || item.Type == "Parent Directory"))
            {
                LoadLocalDirectory(item.FullPath);
                e.Handled = true;
            }
        }

        private void RightFileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListView)sender).SelectedItem as FileSystemItem;
            if (item != null && (item.Type == "Folder" || item.Type == "Parent Directory"))
            {

                LoadFtpDirectory(item.FullPath);
                e.Handled = true;

            }
        }
        private long CalculateTotalSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            else if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                .Sum(filePath => new FileInfo(filePath).Length);
            }
            return 0;
        }

        private class TransferProgress
        {
            public long TransferredSize { get; set; }
        }

        private void goto_c(object sender, RoutedEventArgs e)
        {


            LoadLocalDirectory(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)));
        }

        private void goto_desktop(object sender, RoutedEventArgs e)
        {


            LoadLocalDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }



        private void LocalCreateFolder_Click(object sender, RoutedEventArgs e)
        {

            var uniqueFolderName = Get_local_UniqueFolderName("새폴더");
            var newFolderPath = $"{_currentLocalPath_real}/{uniqueFolderName}";

            Directory.CreateDirectory(newFolderPath);
            LoadLocalDirectory(_currentLocalPath_real);
        }

        public string Get_local_UniqueFolderName(string baseFolderName)
        {
            List<string> directories = new List<string>();
            DirectoryInfo di = new DirectoryInfo(_currentLocalPath_real);

            foreach (var directory in di.GetDirectories())
            {
                Console.WriteLine(directory.Name);
                directories.Add(directory.Name);

            }


            var newFolderName = "새폴더";
            var counter = 1;

            while (directories.Contains(newFolderName))
            {
                newFolderName = $"{baseFolderName} ({counter})";
                counter++;
            }

            return newFolderName;
        }



        private void LocalEdit_Click(object sender, RoutedEventArgs e)
        {

            var selectedItems_ct = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems_ct.Count > 1)
            {
                MessageBox.Show("한개의 이름만 변경 가능합니다.");
                return;
            }


            List<string> folder_name_list = new List<string>();
            List<string> file_name_list = new List<string>();

            DirectoryInfo di = new DirectoryInfo(_currentLocalPath_real);



            foreach (var directory in di.GetDirectories())
            {
                folder_name_list.Add(directory.Name);
            }

            foreach (var file in di.GetFiles())
            {

                file_name_list.Add(file.Name);

            }


            bool is_exit = false;




            var selectedItem = LeftFileListView.SelectedItem as FileSystemItem;
            if (selectedItem != null)
            {



                rename_dialog dialog = new rename_dialog
                {
                    Owner = Application.Current.MainWindow,
                };

                dialog.edit_name_inputbox.Text = selectedItem.Name;
                dialog.edit_name_inputbox.GotFocus += Edit_name_inputbox_GotFocus;

                dialog.edit_name_inputbox.Focus();

                bool? result = dialog.ShowDialog();

                if (dialog.DialogResult == true)
                {
                    string newName = dialog.edit_name_inputbox.Text;
                    if (!string.IsNullOrEmpty(newName))
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName);
                        if (selectedItem.Type == "Folder")
                        {
                            for (int i = 0; i < folder_name_list.Count; i++)
                            {
                                if (folder_name_list[i] == newName)
                                {
                                    is_exit = true;
                                }
                            }
                            if (is_exit)
                            {
                                MessageBox.Show("같은 이름이 있습니다");

                            }
                            else
                            {
                                Directory.Move(selectedItem.FullPath, newPath);
                                // dialog.Close();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < file_name_list.Count; i++)
                            {
                                if (file_name_list[i] == newName)
                                {
                                    is_exit = true;
                                }
                            }
                            if (is_exit)
                            {
                                MessageBox.Show("같은 이름이 있습니다");
                            }
                            else
                            {
                                File.Move(selectedItem.FullPath, newPath);
                                // dialog.Close();
                            }
                        }
                        LoadLocalDirectory(_currentLocalPath_real);
                    }
                    else
                    {
                        MessageBox.Show("이름을 넣어주세요");
                    }
                }

















                //InputDialog inputDialog = new InputDialog("새 이름을 입력하세요.", selectedItem.Name);
                //inputDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //if (inputDialog.ShowDialog() == true)
                //{
                //    string newName = inputDialog.Answer;
                //    if (!string.IsNullOrEmpty(newName))
                //    {
                //        string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName);
                //        if (selectedItem.Type == "Folder")
                //        {
                //            for (int i = 0; i < folder_name_list.Count; i++)
                //            {
                //                if (folder_name_list[i] == newName)
                //                {
                //                    is_exit = true;
                //                }
                //            }
                //            if (is_exit)
                //            {
                //                MessageBox.Show("같은 이름이 있습니다");

                //            }
                //            else
                //            {
                //                Directory.Move(selectedItem.FullPath, newPath);
                //            }
                //        }
                //        else
                //        {
                //            for (int i = 0; i < file_name_list.Count; i++)
                //            {
                //                if (file_name_list[i] == newName)
                //                {
                //                    is_exit = true;
                //                }
                //            }
                //            if (is_exit)
                //            {
                //                MessageBox.Show("같은 이름이 있습니다");
                //            }
                //            else
                //            {
                //                File.Move(selectedItem.FullPath, newPath);
                //            }
                //        }
                //        LoadLocalDirectory(_currentLocalPath_real);
                //    }
                //    else
                //    {
                //        MessageBox.Show("이름을 넣어주세요");
                //    }
                //}
            }
        }

        private void Edit_name_inputbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll(); // Select all text
            }
        }

        private void LocalDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

         









            delete_dialog dialog = new delete_dialog
            {
                Owner = Application.Current.MainWindow,

            };

            dialog.popup_msg.Text = "삭제하시겠습니까?\n삭제 후 되돌릴 수 없습니다.";


            bool? result = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

            if (dialog.DialogResult == true)
            {
                try
                {
                    foreach (var item in selectedItems)
                    {
                        if (item.Type == "Folder")
                        {
                            Directory.Delete(item.FullPath, true);
                        }
                        else
                        {
                            File.Delete(item.FullPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                 
                }
                finally
                {
                    LoadLocalDirectory(_currentLocalPath_real);
                }

            }









        }
    


        private void FtpCreateFolder_Click(object sender, RoutedEventArgs e)
        {

            var uniqueFolderName = Get_ftp_UniqueFolderName("새폴더");
            var newFolderPath = $"{_currentFtpPath}/{uniqueFolderName}";


            //string newFolderPath = _currentFtpPath + "/" + "New Folder";
            _ftpClient.CreateDirectory(newFolderPath);
            LoadFtpDirectory(_currentFtpPath);
        }

        public string Get_ftp_UniqueFolderName(string baseFolderName)
        {

            List<string> filename = new List<string>();
            var listing = _ftpClient.GetListing(_currentFtpPath);
            var sortedListing = listing.OrderByDescending(item => item.Type == FtpObjectType.Directory);

            List<string> directories = new List<string>();

            foreach (var item in sortedListing)
            {
                if (item.Type == FtpObjectType.Directory)
                {
                    directories.Add(item.Name);

                }
            }


            var newFolderName = "새폴더";
            var counter = 1;

            while (directories.Contains(newFolderName))
            {
                newFolderName = $"{baseFolderName} ({counter})";
                counter++;
            }

            return newFolderName;
        }






        private void FtpEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems_ct = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems_ct.Count > 1)
            {
                MessageBox.Show("한개의 이름만 변경 가능합니다.");
                return;
            }

            var listing = _ftpClient.GetListing(_currentFtpPath);
            var sortedListing = listing.OrderByDescending(item => item.Type == FtpObjectType.Directory);

            List<string> folder_name_list = new List<string>();
            List<string> file_name_list = new List<string>();

            foreach (var item in sortedListing)
            {
                if (item.Type == FtpObjectType.Directory)
                {
                    folder_name_list.Add(item.Name);

                }
                else if (item.Type == FtpObjectType.File)
                {
                    file_name_list.Add(item.Name);

                }
            }


            bool is_exit = false;

            var selectedItem = RightFileListView.SelectedItem as FileSystemItem;
            if (selectedItem != null)
            {






                rename_dialog dialog = new rename_dialog
                {
                    Owner = Application.Current.MainWindow,
                };

                dialog.edit_name_inputbox.Text = selectedItem.Name;
                dialog.edit_name_inputbox.GotFocus += Edit_name_inputbox_GotFocus;

                dialog.edit_name_inputbox.Focus();

                bool? result = dialog.ShowDialog();

                if (dialog.DialogResult == true)
                {
                    string newName = dialog.edit_name_inputbox.Text;
                    if (!string.IsNullOrEmpty(newName))
                    {
                        if (selectedItem.Type == "Folder")
                        {

                            for (int i = 0; i < folder_name_list.Count; i++)
                            {
                                if (folder_name_list[i] == newName)
                                {
                                    is_exit = true;
                                }
                            }

                        }
                        else if (selectedItem.Type == "File")
                        {

                            for (int i = 0; i < file_name_list.Count; i++)
                            {
                                if (file_name_list[i] == newName)
                                {
                                    is_exit = true;
                                }
                            }
                        }


                        if (is_exit)
                        {
                            MessageBox.Show("같은 이름이 있습니다");
                        }
                        else
                        {
                            string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName).Replace('\\', '/');
                            _ftpClient.Rename(selectedItem.FullPath, newPath);
                            LoadFtpDirectory(_currentFtpPath);
                        }

                    }
                    else
                    {
                        MessageBox.Show("이름을 넣어주세요");
                    }
                }
























                    //InputDialog inputDialog = new InputDialog("새 이름을 입력하세요.", selectedItem.Name);
                    //inputDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    //if (inputDialog.ShowDialog() == true)
                    //{
                    //    string newName = inputDialog.Answer;
                    //    if (!string.IsNullOrEmpty(newName))
                    //    {


                    //        if (selectedItem.Type == "Folder")
                    //        {

                    //            for (int i = 0; i < folder_name_list.Count; i++)
                    //            {
                    //                if(folder_name_list[i] == newName)
                    //                {
                    //                    is_exit = true;
                    //                }
                    //            }

                    //        }
                    //        else if (selectedItem.Type == "File")
                    //        {

                    //            for (int i = 0; i < file_name_list.Count; i++)
                    //            {
                    //                if (file_name_list[i] == newName)
                    //                {
                    //                    is_exit = true;
                    //                }
                    //            }
                    //        }


                    //        if (is_exit)
                    //        {
                    //            MessageBox.Show("같은 이름이 있습니다");
                    //        }
                    //        else
                    //        {
                    //            string newPath = Path.Combine(Path.GetDirectoryName(selectedItem.FullPath), newName).Replace('\\', '/');
                    //            _ftpClient.Rename(selectedItem.FullPath, newPath);
                    //            LoadFtpDirectory(_currentFtpPath);
                    //        }

                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show("이름을 넣어주세요");
                    //    }
                    //}
                }
        }

        private void FtpDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

















            delete_dialog dialog = new delete_dialog
            {
                Owner = Application.Current.MainWindow,

            };

            dialog.popup_msg.Text = "삭제하시겠습니까?\n삭제 후 되돌릴 수 없습니다.";


            bool? result = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

            if (dialog.DialogResult == true)
            {
                try
                {
                    foreach (var item in selectedItems)
                    {
                        if (item.Type == "Folder")
                        {
                            _ftpClient.DeleteDirectory(item.FullPath, FtpListOption.Recursive);
                        }
                        else
                        {
                            _ftpClient.DeleteFile(item.FullPath);
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    
                }
                finally
                {
                    LoadFtpDirectory(_currentFtpPath);
                }

            }







































            //if (MessageBox.Show($"선택한 {selectedItems.Count}개의 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            //{
            //    try
            //    {
            //        foreach (var item in selectedItems)
            //        {
            //            if (item.Type == "Folder")
            //            {
            //                _ftpClient.DeleteDirectory(item.FullPath, FtpListOption.Recursive);
            //            }
            //            else
            //            {
            //                _ftpClient.DeleteFile(item.FullPath);
            //            }
            //        }
            //        MessageBox.Show("선택한 항목이 삭제되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"삭제 중");
            //    }
            //    finally
            //    {
            //        LoadFtpDirectory(_currentFtpPath);
            //    }
            //}
        }

       
        private async void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = LeftFileListView.SelectedItems.Cast<FileSystemItem>().ToList();

            if (!selectedItems.Any())
            {
                MessageBox.Show("전송할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<string> filename = new List<string>();
            var listing = _ftpClient.GetListing(_currentFtpPath);
            var sortedListing = listing
                .OrderByDescending(item => item.Type == FtpObjectType.Directory)
                .ThenBy(item => item.Name);

            foreach (var item in sortedListing)
            {
                filename.Add(item.Name);
            }

            bool file_exit = false;

            for (int i = 0; i < filename.Count; i++)
            {
                for (int j = 0; j < selectedItems.Count; j++)
                {
                    if (filename[i] == selectedItems[j].Name)
                    {
                        file_exit = true;
                    }
                }
            }


            if (file_exit)
            {
                MessageBoxResult result = MessageBox.Show(
               "같은 이름의 파일이 있습니다. 덮어씌우시겠습니까?",   // 메시지
               "확인",                      // 제목
               MessageBoxButton.YesNo,      // 버튼 종류
               MessageBoxImage.Question     // 아이콘 종류
           );
             
                if (result == MessageBoxResult.Yes)
                {
                    // 사용자가 'Yes'를 클릭했을 때의 처리
                    await file_send_to_ftp(selectedItems);
                }
            }
            else
            {
                await file_send_to_ftp(selectedItems);
            }
        }

        private async Task file_send_to_ftp(List<FileSystemItem> selectedItems)
        {
            Transferpopup.Visibility = Visibility.Visible;
            TransferProgressBar.Visibility = Visibility.Visible;
            temp_before_value = 0;
            TransferProgressBar.Value = 0;
            TransferProgressText.Text = "0%";

            long totalSize = selectedItems.Sum(item => CalculateTotalSize(item.FullPath));
            List<long> file_size = new List<long>();

            for (int i = 0; i < selectedItems.Count; i++)
            {
                file_size.Add(CalculateTotalSize(selectedItems[i].FullPath));
            }


            int transfercount = 0;

            var progress = new TransferProgress();

            try
            {
                foreach (var item in selectedItems)
                {
                    long before_value = 0;
                    if (transfercount != 0)
                    {
                        for (int k = 0; k < transfercount; k++)
                        {
                            before_value += file_size[k];
                        }
                    }


                    await TransferItemAsync(item, _currentFtpPath, totalSize, progress);
                    transfercount++;
                }

                if (check_finish == true)
                {

                    confirm_dialog dialog = new confirm_dialog
                    {
                        Owner = Application.Current.MainWindow,

                    };



                    bool? result = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

                    if (dialog.DialogResult == true)
                    {
                        

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Transferpopup.Visibility = Visibility.Collapsed;
                TransferProgressBar.Visibility = Visibility.Collapsed;
                TransferProgressText.Text = "";
                LoadFtpDirectory(_currentFtpPath);
            }
        }

        private async void FtpToLocalTransferButton_Click(object sender, RoutedEventArgs e)
        {


            var selectedItems = RightFileListView.SelectedItems.Cast<FileSystemItem>().ToList();
            if (!selectedItems.Any())
            {
                MessageBox.Show("전송할 항목을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }




            List<string> filename = new List<string>();
            List<string> foldername = new List<string>();

            DirectoryInfo di = new DirectoryInfo(_currentLocalPath_real);


            foreach (var item in di.GetFiles())
            {
                filename.Add(item.Name);
            }

            foreach (var item in di.GetDirectories())
            {
                foldername.Add(item.Name);
            }

            bool file_exit = false;




            for (int j = 0; j < selectedItems.Count; j++)
            {


                if (selectedItems[j].Type == "Folder")
                {
                    for (int i = 0; i < foldername.Count; i++)
                    {
                        if (foldername[i] == selectedItems[j].Name)
                        {
                            file_exit = true;

                        }
                    }
                     
                }
                else if (selectedItems[j].Type == "File")
                {
                    for (int i = 0; i < filename.Count; i++)
                    {
                        if (filename[i] == selectedItems[j].Name)
                        {
                            file_exit = true;
                        }
                    }
                }



            }
           


            if (file_exit)
            {
                MessageBoxResult result = MessageBox.Show(
               "같은 이름의 파일이 있습니다. 덮어씌우시겠습니까?",   // 메시지
               "확인",                      // 제목
               MessageBoxButton.YesNo,      // 버튼 종류
               MessageBoxImage.Question     // 아이콘 종류
           );

                if (result == MessageBoxResult.Yes)
                {
                    // 사용자가 'Yes'를 클릭했을 때의 처리
                    //await file_send_to_ftp(selectedItems);

                    Transferpopup.Visibility = Visibility.Visible;
                    TransferProgressBar.Visibility = Visibility.Visible;
                    TransferProgressBar.Value = 0;
                    temp_before_value = 0;
                    TransferProgressText.Text = "0%";

                    long totalSize = await Task.Run(() => selectedItems.Sum(item => _ftpClient.GetFileSize(item.FullPath)));



                    for (int j = 0; j < selectedItems.Count; j++)
                    {


                        if (selectedItems[j].Type == "Folder")
                        {
                            for (int i = 0; i < foldername.Count; i++)
                            {
                                if (foldername[i] == selectedItems[j].Name)
                                {
                                    var erer = _ftpClient.GetListing(selectedItems[j].FullPath);

                                    foreach (var item in erer)
                                    {

                                        string tsts = item.Type == FtpObjectType.Directory ? "Folder" : "File";
                                        if (tsts == "File")
                                        {
                                            totalSize += _ftpClient.GetFileSize(item.FullName);
                                        }
                                        else
                                        {
                                            totalSize = getfoldersize(totalSize, item, tsts);
                                        }



                                    }
                                }
                            }
                        }
                    }



                    var progress = new TransferProgress();

                    try
                    {
                        foreach (var item in selectedItems)
                        {
                            await FtpToLocalTransferItemAsync(item, _currentLocalPath, totalSize, progress);
                        }


                        if (check_finish == true)
                        {
                            //MessageBox.Show("전송이 완료되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);


                            confirm_dialog dialog = new confirm_dialog
                            {
                                Owner = Application.Current.MainWindow,

                            };




                            bool? result2 = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

                            if (dialog.DialogResult == true)
                            {


                            }


                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        Transferpopup.Visibility = Visibility.Collapsed;
                        TransferProgressBar.Visibility = Visibility.Collapsed;
                        TransferProgressText.Text = "";

                        LoadLocalDirectory(_currentLocalPath_real);
                    }

                }
            }
            else
            {
                //await file_send_to_ftp(selectedItems);

                Transferpopup.Visibility = Visibility.Visible;
                TransferProgressBar.Visibility = Visibility.Visible;
                TransferProgressBar.Value = 0;
                TransferProgressText.Text = "0%";
                temp_before_value = 0;

                long totalSize = await Task.Run(() => selectedItems.Sum(item => _ftpClient.GetFileSize(item.FullPath)));










                for (int j = 0; j < selectedItems.Count; j++)
                {
                    if (selectedItems[j].Type == "Folder")
                    {

                        var erer = _ftpClient.GetListing(selectedItems[j].FullPath);

                        foreach (var item in erer)
                        {

                            string tsts = item.Type == FtpObjectType.Directory ? "Folder" : "File";
                            if (tsts == "File")
                            {
                                totalSize += _ftpClient.GetFileSize(item.FullName);
                            }
                            else
                            {
                                totalSize = getfoldersize(totalSize, item, tsts);
                            }

                        }
                    }
                }













                var progress = new TransferProgress();

                try
                {
                    foreach (var item in selectedItems)
                    {
                        await FtpToLocalTransferItemAsync(item, _currentLocalPath_real, totalSize, progress);
                    }


                    if (check_finish == true)
                    {

                        confirm_dialog dialog = new confirm_dialog
                        {
                            Owner = Application.Current.MainWindow,

                        };




                        bool? result = dialog.ShowDialog(); // 모달 다이얼로그로 표시됨

                        if (dialog.DialogResult == true)
                        {


                        }

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Transferpopup.Visibility = Visibility.Collapsed;
                    TransferProgressBar.Visibility = Visibility.Collapsed;
                    TransferProgressText.Text = "";
                    LoadLocalDirectory(_currentLocalPath_real);
                }
            }





        }

        private long getfoldersize(long totalSize, FtpListItem item, string tsts)
        {
            var ererwe = _ftpClient.GetListing(item.FullName);

            foreach (var itemw in ererwe)
            {

                string tstsd = itemw.Type == FtpObjectType.Directory ? "Folder" : "File";
                if (tstsd == "File")
                {
                    totalSize += _ftpClient.GetFileSize(itemw.FullName);
                }
                else
                {
                    totalSize = getfoldersize(totalSize, itemw, tstsd);
                }

            }

            return totalSize;
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }


        bool check_finish = false;


        long temp_before_value = 0;
        private async Task TransferItemAsync(FileSystemItem item, string destPath, long totalSize, TransferProgress progress)
        {
            try
            {
                if (item.Type == "File")
                {
                    try
                    {
                        var ftpPath = Path.Combine(destPath, item.Name).Replace("\\", "/");
                        await Task.Run(() =>
                        {
                            try
                            {
                                _ftpClient.UploadFile(item.FullPath, ftpPath, FtpRemoteExists.Overwrite, true, FtpVerify.None, ftpProgress =>
                                {
                                    check_finish = true;
                                    progress.TransferredSize = ftpProgress.TransferredBytes + temp_before_value;
                                    UpdateProgress(progress.TransferredSize, totalSize);
                                });
                            }
                            catch (System.Net.Sockets.SocketException ex)
                            {
                                Debug.WriteLine($"SocketException 발생: {ex.Message}");
                                check_finish = false;
                                throw; // 예외를 호출자에게 전달
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"업로드 중 오류 발생: {ex.Message}");
                                //MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

                               
                                check_finish = false;
                            }

                            temp_before_value = progress.TransferredSize;
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"작업 실행 중 오류 발생: {ex.Message}");
                        check_finish = false;
                    }



                }
                else if (item.Type == "Folder")
                {
                    var ftpFolderPath = Path.Combine(destPath, item.Name).Replace("\\", "/");
                    await Task.Run(() => _ftpClient.CreateDirectory(ftpFolderPath));

                    foreach (var file in Directory.GetFiles(item.FullPath))
                    {
                        var fileItem = new FileSystemItem
                        {
                            Name = Path.GetFileName(file),
                            Type = "File",
                            FullPath = file
                        };
                        await TransferItemAsync(fileItem, ftpFolderPath, totalSize, progress);
                    }

                    foreach (var dir in Directory.GetDirectories(item.FullPath))
                    {
                        var dirItem = new FileSystemItem
                        {
                            Name = Path.GetFileName(dir),
                            Type = "Folder",
                            FullPath = dir
                        };
                        await TransferItemAsync(dirItem, ftpFolderPath, totalSize, progress);
                    }
                }
            }
            catch(Exception e)
            {

            }
        }




        

        private async Task FtpToLocalTransferItemAsync(FileSystemItem item, string destPath, long totalSize, TransferProgress progress)
        {
            if (item.Type == "File")
            {
             

                try
                {
                    var localPath = Path.Combine(destPath, item.Name);
                    await Task.Run(() =>
                    {
                        try
                        {
                            _ftpClient.DownloadFile(localPath, item.FullPath, FtpLocalExists.Overwrite,  FtpVerify.None, ftpProgress =>
                            {
                                check_finish = true;
                                progress.TransferredSize = ftpProgress.TransferredBytes + temp_before_value;
                                UpdateProgress(progress.TransferredSize, totalSize);
                            });
                        }
                        catch (System.Net.Sockets.SocketException ex)
                        {
                            Debug.WriteLine($"SocketException 발생: {ex.Message}");
                            check_finish = false;
                            throw; // 예외를 호출자에게 전달
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"업로드 중 오류 발생: {ex.Message}");
                            //MessageBox.Show($"전송 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);


                            check_finish = false;
                        }
                        temp_before_value = progress.TransferredSize;
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"작업 실행 중 오류 발생: {ex.Message}");
                    check_finish = false;
                }







            }
            else if (item.Type == "Folder")
            {
                var localFolderPath = Path.Combine(destPath, item.Name);
                Directory.CreateDirectory(localFolderPath);

                var ftpListing = await Task.Run(() => _ftpClient.GetListing(item.FullPath));
                foreach (var ftpItem in ftpListing)
                {
                    var subItem = new FileSystemItem
                    {
                        Name = ftpItem.Name,
                        Type = ftpItem.Type == FtpObjectType.File ? "File" : "Folder",
                        FullPath = ftpItem.FullName
                    };
                    await FtpToLocalTransferItemAsync(subItem, localFolderPath, totalSize, progress);
                }
            }
        }

        private void UpdateProgress(long transferredSize, long totalSize)
        {
            
            double percentage = (double)transferredSize / totalSize * 100;
            int roundedPercentage = (int)Math.Round(percentage);

            Dispatcher.Invoke(() =>
            {
                TransferProgressBar.Value = roundedPercentage;
                TransferProgressText.Text = $"{roundedPercentage}%";
            });
        }









        //void fake_updateprogress()
        //{


        //    double percentage = (double)transferredSize / totalSize * 100;
        //    int roundedPercentage = (int)Math.Round(percentage);

        //    Dispatcher.Invoke(() =>
        //    {
        //        TransferProgressBar.Value = roundedPercentage;
        //        TransferProgressText.Text = $"{roundedPercentage}%";
        //    });






        //    PowerProgressBar.Maximum = (Progress_duration * 0.7) * 100;

        //    powerProgress += 1;
        //    PowerProgressBar.Value = powerProgress;


        //    if (powerProgress >= (Progress_duration * 0.7) * 100)
        //    {
        //        powerTimer.Stop();
        //        PowerOverlay.Visibility = Visibility.Collapsed;

        //        bool newState = PowerStatusText.Text == "전원 OFF";
        //        foreach (var item in dragItems)
        //        {
        //            item.Configuration.IsOn = newState;
        //            UpdateItemPowerState(item, newState);
        //        }
        //    }


        //}





















        //private void CloseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    CloseRequested?.Invoke(this, EventArgs.Empty);
        //}

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;

        private BitmapSource GetIconForFileType(string fileType)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_SMALLICON;

            if (fileType == "folder")
            {
                SHGetFileInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            }
            else
            {
                SHGetFileInfo(fileType, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            }

            BitmapSource bitmapSource = null;
            if (shfi.hIcon != IntPtr.Zero)
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    shfi.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(shfi.hIcon);  // 리소스 해제
            }

            return bitmapSource;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);


        private void StartHourComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as FrameworkElement;
            button.Cursor = Cursors.Hand;
        }

        private void StartHourComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as FrameworkElement;
            button.Cursor = Cursors.Arrow;
        }
    }
}