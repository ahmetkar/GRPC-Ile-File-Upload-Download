
using System.Threading.Tasks;
using Grpc.Net.Client;
using grpcFileTransportDownloadClient;

namespace grpcdwnclient {

    public class Program {

        public static async Task Main(string[] args){
            
            var channel = GrpcChannel.ForAddress("http://localhost:5081");

            var client = new FileService.FileServiceClient(channel);
            
            string downloadpath = @"C:\Users\ahmet\Desktop\grpcfile\grpcdwnclient\downloads";

            var fileInfo = new grpcFileTransportDownloadClient.FileInfo {
                FileName ="deneme2",
                FileExtension=".mp4"
            };

            FileStream stream = null;

            var download = client.FileDownload(fileInfo);
            CancellationTokenSource source = new CancellationTokenSource();
            int count = 0;
            decimal chunkSize = 0;
            while(await download.ResponseStream.MoveNext(source.Token)){
                if(count++ == 0){
                    stream = new FileStream(@$"{downloadpath}/{download.ResponseStream.Current.Info.FileName}{download.ResponseStream.Current.Info.FileExtension}",FileMode.CreateNew);
                    stream.SetLength(download.ResponseStream.Current.FileSize);
                }
                var buffer = download.ResponseStream.Current.Buffer.ToByteArray();
                await stream.WriteAsync(buffer,0,download.ResponseStream.Current.ReadedByte);

                Console.WriteLine($"{Math.Round(((chunkSize+=download.ResponseStream.Current.ReadedByte)*100)/download.ResponseStream.Current.FileSize)}%");
            }

            await stream.DisposeAsync();
            stream.Close();

             
        }
    }
}