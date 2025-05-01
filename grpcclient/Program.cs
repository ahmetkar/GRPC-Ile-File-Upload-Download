using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using grpcFileTransportClient;

namespace grpcclient {

    public class Program {

        public static async Task Main(){

            var channel = GrpcChannel.ForAddress("http://localhost:5081");

            var client = new FileService.FileServiceClient(channel);
            string file = @"..\grpcfile\grpclient\";
            using FileStream stream = new FileStream(file,FileMode.Open);

            var content = new BytesContent {
                FileSize = stream.Length,
                ReadedByte = 0,
                Info =  new grpcFileTransportClient.FileInfo {FileName = Path.GetFileNameWithoutExtension(stream.Name),FileExtension=Path.GetExtension(stream.Name)}
            };

            var upload = client.FileUpload();
            byte[] buffer = new byte[2048];
            while((content.ReadedByte = await stream.ReadAsync(buffer,0,buffer.Length)) > 0){
                content.Buffer = ByteString.CopyFrom(buffer);
                await upload.RequestStream.WriteAsync(content);
            }

            await upload.RequestStream.CompleteAsync();
            stream.Close();
        }   
    }

}
