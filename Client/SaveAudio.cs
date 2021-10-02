//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.CognitiveServices.Speech;
//using Microsoft.CognitiveServices.Speech.Audio;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Client
//{
//    public class SaveAudio : Controller
//    {
//        Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

//        public SaveAudio(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
//        {
//            _hostingEnvironment = hostingEnvironment;

//        }

//        [Route("api/[controller]/Save")]
//        [HttpPost]
//        public async Task<IActionResult> Save(IFormFile file)
//        {
//            if (file.ContentType != "audio/wav")
//            {
//                return BadRequest("Wrong file type");
//            }
//            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");//uploads where you want to save data inside wwwroot

//            //var filePath = Path.Combine(uploads, Path.GetRandomFileName() + ".wav");
//            var filePath = Path.Combine(uploads, file.FileName + ".wav");
//            string result = string.Empty;
//            using (var fileStream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(fileStream);
//            }
//            var speechConfig = SpeechConfig.FromSubscription("18d86c7414234aae8cfae7a8ad04c82e", "australiaeast");
//            //speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000"); // 10000ms
//            result = FromStream(speechConfig, filePath).Result; //.ContinueWith((x) => { System.IO.File.Delete(filePath.ToString()); });
//            System.IO.File.Delete(filePath.ToString());
//            return Ok(new { Result = result });
//        }

//        private async Task<string> FromStream(SpeechConfig speechConfig, string filePath)
//        {
//            using var reader = new BinaryReader(System.IO.File.OpenRead(filePath));
//            using var audioInputStream = AudioInputStream.CreatePushStream();
//            //using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
//            using var audioConfig = AudioConfig.FromWavFileInput(filePath);
//            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

//            byte[] readBytes;
//            do
//            {
//                readBytes = reader.ReadBytes(1024);
//                audioInputStream.Write(readBytes, readBytes.Length);
//            } while (readBytes.Length > 0);

//            var result = await recognizer.RecognizeOnceAsync();
//            return result.Text;
//            //Console.WriteLine($"RECOGNIZED: Text={result.Text}");
//        }
//    }
//}
