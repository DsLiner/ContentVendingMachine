using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Networking.Proximity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.Devices.WiFiDirect;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace ContentVendingMachine_Code
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Networking.Proximity.ProximityDevice proximityDevice;
        long subscribedMessageId = -1;

        public MainPage()
        {
            this.InitializeComponent();

            Synopsis.Text += "\n\n";
            try
            {
                proximityDevice = ProximityDevice.GetDefault();
                Synopsis.Text += proximityDevice.DeviceId + "\n\n";
            }
            catch (Exception e)
            {
                Synopsis.Text += e.ToString() + "\n\n";
            }

            if (proximityDevice != null)
            {
                proximityDevice.DeviceArrived += DeviceArrived;
                proximityDevice.DeviceDeparted += DeviceDeparted;
            }
            else
            {
                Synopsis.Text += "No proximity device found\n";
            }
        }

        long publishedUriId = -1;

        void DeviceArrived(ProximityDevice proximityDevice)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "Proximate device arrived\n\n";
                Synopsis.Text += "Wi-Fi Direct requested\n\n";
                Synopsis.Text += "ServerConnected\n\n";
            });

            //subscribedMessageId = proximityDevice.SubscribeForMessage("NDEF:wkt.HelloWorld", messageReceived);
            //subscribedMessageId = -1;

            //if(publishedUriId == -1)
            //    PublishLaunchApp();

            publishedUriId =
                proximityDevice.PublishUriMessage(new Uri("http://www.microsoft.com"));

            if (publishedUriId != -1)
                proximityDevice.StopPublishingMessage(publishedUriId);

            //SplitAndCombine.combineFile();

            //Connecting();
        }

        private async void Connecting()
        {
            WifiDirectFileTransfer wifiDirectFileTransfer = new WifiDirectFileTransfer();
            wifiDirectFileTransfer.Start();

            IEnumerable<PeerInformation> peerInformation = await wifiDirectFileTransfer.FindPeersAsync();

            string deviceId = peerInformation.ToString();

            string x = await Connect(deviceId);

            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += x;
            });
        }

        void DeviceDeparted(ProximityDevice proximityDevice)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "Proximate device departed\n\n";
                Synopsis.Text += "Collect Data\n\n";
                Synopsis.Text += "Combine Data\n\n";
            });
        }

        private void messageReceived(
            Windows.Networking.Proximity.ProximityDevice device,
            Windows.Networking.Proximity.ProximityMessage message)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "Message received: " + message.DataAsString + "\n";
            });
        }


        private void PublishLaunchApp()
        {
            proximityDevice = Windows.Networking.Proximity.ProximityDevice.GetDefault();

            if (proximityDevice != null)
            {
                // The format of the app launch string is: "<args>\tWindows\t<AppName>".
                // The string is tab or null delimited.

                // The <args> string must have at least one character.
                string launchArgs = "user=default";

                // The format of the AppName is: PackageFamilyName!PRAID.
                string praid = "MyAppId"; // The Application Id value from your package.appxmanifest.

                string appName = Windows.ApplicationModel.Package.Current.Id.FamilyName + "!" + praid;

                string launchAppMessage = launchArgs + "\tWindows\t" + appName;

                var dataWriter = new Windows.Storage.Streams.DataWriter();
                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                dataWriter.WriteString(launchAppMessage);
                publishedUriId =
                proximityDevice.PublishBinaryMessage(
                    "NDEF:WriteTag", dataWriter.DetachBuffer(), proximityWriteTagLaunchAppMessageTransmitCallback);
            }
        }

        private void proximityWriteTagLaunchAppMessageTransmitCallback(
            Windows.Networking.Proximity.ProximityDevice sender,
            long messageId)
        {
            // The LaunchApp message has been successfully written to a tag.

            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "message transmit complete\n";
            });
        }

        Windows.Devices.WiFiDirect.WiFiDirectDevice wfdDevice;
        private async System.Threading.Tasks.Task<String> Connect(string deviceId)
        {
            string result = "";

            try
            {
                // No device Id specified.
                if (String.IsNullOrEmpty(deviceId)) { return "Please specify a Wi- Fi Direct device Id."; }

                // Connect to the selected Wi-Fi Direct device.
                wfdDevice = await Windows.Devices.WiFiDirect.WiFiDirectDevice.FromIdAsync(deviceId);

                if (wfdDevice == null)
                {
                    result = "Connection to " + deviceId + " failed.";
                }

                // Register for connection status change notification.
                wfdDevice.ConnectionStatusChanged += new TypedEventHandler<Windows.Devices.WiFiDirect.WiFiDirectDevice, object>(OnConnectionChanged);

                // Get the EndpointPair information.
                var EndpointPairCollection = wfdDevice.GetConnectionEndpointPairs();

                if (EndpointPairCollection.Count > 0)
                {
                    var endpointPair = EndpointPairCollection[0];
                    result = "Local IP address " + endpointPair.LocalHostName.ToString() +
                        " connected to remote IP address " + endpointPair.RemoteHostName.ToString();
                }
                else
                {
                    result = "Connection to " + deviceId + " failed.";
                }
            }
            catch (Exception err)
            {
                // Handle error.
                result = "Error occurred: " + err.Message;
            }

            return result;
        }

        private void OnConnectionChanged(object sender, object arg)
        {
            Windows.Devices.WiFiDirect.WiFiDirectConnectionStatus status =
                (Windows.Devices.WiFiDirect.WiFiDirectConnectionStatus)arg;

            if (status == Windows.Devices.WiFiDirect.WiFiDirectConnectionStatus.Connected)
            {
                // Connection successful.
            }
            else
            {
                // Disconnected.
                Disconnect();
            }
        }

        private void Disconnect()
        {
            if (wfdDevice != null)
            {
                wfdDevice.Dispose();
            }
        }
    }

    public class WifiDirectFileTransfer : IDisposable
    {
        static readonly uint BLOCK_SIZE = 1024;

        Action<string> _verboseCb;
        IEnumerable<PeerInformation> _peerInformationList;


        public event EventHandler<ConnectionRequestedEventArgs> ConnectionRequested;

        public WifiDirectFileTransfer(Action<string> verboseCb = null)
        {
            _verboseCb = verboseCb;
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
        }

        public void Start()
        {
            //if ((Windows.Networking.Proximity.PeerFinder.SupportedDiscoveryTypes &
            //     Windows.Networking.Proximity.PeerDiscoveryTypes.Triggered) ==
            //     Windows.Networking.Proximity.PeerDiscoveryTypes.Triggered) ;
            ////{
            ////    //var result = "You can tap to connect a peer device that is " +
            //                         "also advertising for a connection.\n";
            //}
            //else
            //{
            //    var result = "Tap to connect is not supported.\n";
            //}

            //if ((Windows.Networking.Proximity.PeerFinder.SupportedDiscoveryTypes &
            //     Windows.Networking.Proximity.PeerDiscoveryTypes.Browse) !=
            //     Windows.Networking.Proximity.PeerDiscoveryTypes.Browse)
            //{
            //    var result = "Peer discovery using Wi-Fi Direct is not supported.\n";
            //}

            //PeerFinder.Start();
        }

        public async Task<IEnumerable<PeerInformation>> FindPeersAsync()
        {
            _peerInformationList = null;

            if ((PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Browse) ==
                                        PeerDiscoveryTypes.Browse)
            {
                if (PeerFinder.AllowWiFiDirect)
                {
                    // Find all discoverable peers with compatible roles
                    _peerInformationList = await PeerFinder.FindAllPeersAsync();
                    if (_peerInformationList == null)
                    {
                        Verbose("Found no peer");
                    }
                    else
                    {
                        Verbose(string.Format("I found {0} devices(s) executing this same app !", _peerInformationList.Count()));
                    }
                }
                else
                {
                    Verbose("WIFI direct not available");
                }
            }
            else
            {
                Verbose("Browse not available");
            }

            return _peerInformationList;
        }

        public async Task ConnectAndSendFileAsync(PeerInformation selectedPeer, StorageFile selectedFile)
        {
            var socket = await PeerFinder.ConnectAsync(selectedPeer);
            Verbose(string.Format("Connected to {0}, now processing transfer ...please wait", selectedPeer.DisplayName));
            await SendFileToPeerAsync(selectedPeer, socket, selectedFile);
        }

        public async Task<string> ReceiveFileAsync(PeerInformation requestingPeer, StorageFolder folder, string outputFilename = null)
        {
            Verbose("Connection in process...");
            StreamSocket socket = await PeerFinder.ConnectAsync(requestingPeer);
            Verbose("Receiving file...");

            return await ReceiveFileFomPeer(socket, folder, outputFilename);

        }


        // This gets called when we receive a connect request from a Peer
        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            Verbose(string.Format("Do you accept to receive a file from {0} ?", args.PeerInformation.DisplayName));
            if (ConnectionRequested != null)
            {
                ConnectionRequested(this, args);
            }
        }

        void Verbose(string message)
        {
            if (_verboseCb != null)
            {
                _verboseCb(message);
            }
        }


        private async Task SendFileToPeerAsync(PeerInformation selectedPeer, StreamSocket socket, StorageFile selectedFile)
        {
            byte[] buff = new byte[BLOCK_SIZE];
            var prop = await selectedFile.GetBasicPropertiesAsync();
            using (var dw = new DataWriter(socket.OutputStream))
            {

                // 1. Send the filename length
                dw.WriteInt32(selectedFile.Name.Length); // filename length is fixed
                                                            // 2. Send the filename
                dw.WriteString(selectedFile.Name);
                // 3. Send the file length
                dw.WriteUInt64(prop.Size);
                // 4. Send the file
                var fileStream = await selectedFile.OpenStreamForReadAsync();
                while (fileStream.Position < (long)prop.Size)
                {
                    var rlen = await fileStream.ReadAsync(buff, 0, buff.Length);
                    dw.WriteBytes(buff);
                }

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }



        private async Task<string> ReceiveFileFomPeer(StreamSocket socket, StorageFolder folder, string outputFilename = null)
        {
            StorageFile file;
            using (var rw = new DataReader(socket.InputStream))
            {
                // 1. Read the filename length
                await rw.LoadAsync(sizeof(Int32));
                var filenameLength = (uint)rw.ReadInt32();
                // 2. Read the filename
                await rw.LoadAsync(filenameLength);
                var originalFilename = rw.ReadString(filenameLength);
                if (outputFilename == null)
                {
                    outputFilename = originalFilename;
                }
                //3. Read the file length
                await rw.LoadAsync(sizeof(UInt64));
                var fileLength = rw.ReadUInt64();

                // 4. Reading file
                using (var memStream = await DownloadFile(rw, fileLength))
                {

                    file = await folder.CreateFileAsync(outputFilename, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream1 = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream1.GetOutputStreamAt(0));
                    }

                    Verbose("Et voila :)");

                    rw.DetachStream();
                }
            }

            return file.Path;
        }

        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                Verbose(string.Format("Receiving file...{0}/{1} bytes", memStream.Position, fileLength));
                var lenToRead = Math.Min(BLOCK_SIZE, fileLength - memStream.Position);
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }

        public void Dispose()
        {
            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
            PeerFinder.Stop();
        }
    }

    class SplitAndCombine
    {
        static string originalFilePath = @"C:\Users\USER\Desktop\test\";
        static string originalFileName = "S04E01.mkv";

        static string tempFilePath = @"C:\Data\Users\Public\temp\";

        static string cloneFilePath = @"C:\Data\Users\Public\"; // 나중에 복사 파일도 temp안으로 저장되도록 수정하기
        static string cloneFileName = "S04E01.mkv";

        static string serverDirectory = @"C:\Users\Public\server\";
        static string txtFileName = "FileList.txt";

        static FileInfo fInfo = new FileInfo(@"C:\Users\Public\test\" + "S04E01.mkv");// 파일의 정보를 담는 객체
        static long fileSize = fInfo.Length; // 파일의 총 사이즈를 담는 변수

        static int buffer_size = ((int)fileSize / 20) + 1; // 버퍼 사이즈 = 파일 사이즈 / 20


        //static void Main(string[] args)
        //{
        //    //try
        //    //{
        //        combineFile(); //분배된 파일을 조합하여 합친다
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //}
        //}
        
        public static void combineFile()
        {
            int fileCounter = 0;
            byte[] buffer = null;
            Stream sr = null;

            DirectoryInfo dir = null;

            try
            {
                dir = new DirectoryInfo(cloneFilePath);
            }
            catch { }

            if (dir.Exists == false)   // 만약 폴더가 존재하지 않으면
            {
                dir.Create();             // 새 폴더를 생성
            }

            //for (int i = 1; i < 21; i++)
            //{
            //    string sourceFile = tempFilePath + originalFileName + "_" + i;
            //    string destFile = tempFilePath + originalFileName + "_" + i;
            //    System.IO.File.Copy(sourceFile, destFile, true);
            //}

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(tempFilePath); // DirectoryInfo 객체 생성
            foreach (var item in di.GetFiles()) // 해당 디렉토리 내의 파일들 개수 만큼 count를 쌓는다
            {
                fileCounter++;
            }

            string[] files = new string[fileCounter]; // 파일 개수만큼 string 배열을 선언한다

            for (int i = 0; i < fileCounter; i++) // 해당 디렉토리에서 모든 파일이름 리스트를 string배열에 담는 부분
            {
                files = Directory.GetFiles(tempFilePath);
            }

            Stream sw = new FileStream(cloneFilePath + cloneFileName, FileMode.Create, FileAccess.Write); // file 읽기

            for (int i = 0; i < files.Length; i++)
            {
                sr = new FileStream(files[i], FileMode.Open, FileAccess.Read);

                buffer = new byte[buffer_size];
                int readCnt = 0;

                while ((readCnt = sr.Read(buffer, 0, buffer_size)) != 0) // 문자 하나씩 Read
                {
                    sw.Write(buffer, 0, readCnt);
                }
            }

            sw.Flush();
            sw.Dispose();
            System.IO.Directory.Delete(@"C:\Users\Public\temp", true); // 임시 폴더 삭제
        }
    }
}
