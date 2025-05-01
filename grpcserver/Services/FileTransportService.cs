
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using grpcFileTransportServer;
using static grpcFileTransportServer.FileService;

namespace grpcserver.Services {
    public class FileTransportService : FileServiceBase {
        
        public FileTransportService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        readonly IWebHostEnvironment WebHostEnvironment;

        public async override Task FileDownload(grpcFileTransportServer.FileInfo request, IServerStreamWriter<BytesContent> responseStream, ServerCallContext context)
        {
            
            string path = Path.Combine(WebHostEnvironment.WebRootPath,"files");
            using FileStream stream = new FileStream($"{path}/{request.FileName}{request.FileExtension}",FileMode.Open,FileAccess.Read);
            byte[] buffer = new byte[2048];

            BytesContent content = new BytesContent {
                FileSize = stream.Length,
                Info = new grpcFileTransportServer.FileInfo(){FileName = Path.GetFileNameWithoutExtension(stream.Name),FileExtension=Path.GetExtension(stream.Name)},
                ReadedByte = 0
            };

            while((content.ReadedByte = await stream.ReadAsync(buffer,0,buffer.Length))>0 ){
                content.Buffer = ByteString.CopyFrom(buffer);
                await responseStream.WriteAsync(content);
            }

            stream.Close(); 
        }

        public async override Task<Empty> FileUpload(IAsyncStreamReader<BytesContent> requestStream, ServerCallContext context)
        {
            string path = Path.Combine(WebHostEnvironment.WebRootPath,"files");
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);

            FileStream stream = null;
            try {
                int count = 0;
                decimal chunkSize = 0;
                while(await requestStream.MoveNext()){
                    if(count++ == 0) {
                        stream = new FileStream($"{path}/{requestStream.Current.Info.FileName}{requestStream.Current.Info.FileExtension}",FileMode.CreateNew);
                        stream.SetLength(requestStream.Current.FileSize);
                    }

                    var buffer = requestStream.Current.Buffer.ToByteArray();
                    await stream.WriteAsync(buffer,0,buffer.Length);

                    Console.WriteLine($"{Math.Round(((chunkSize+=requestStream.Current.ReadedByte)*100)/requestStream.Current.FileSize)}%");
                }
            }catch{

            }

            await stream.DisposeAsync();
            stream.Close();
            return new Empty();

            
        }
    }
}