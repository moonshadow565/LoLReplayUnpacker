using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ENetUnpack.ReplayParser
{
    public abstract class HttpProtocolHandler
    {
        enum HttpState
        {
            Done,
            GetText,
            GetBinary,
        }

        private HttpState _httpState = HttpState.Done;

        public virtual void HandleTextPacket(string data, float time)
        {

        }

        public virtual void HandleBinaryPacket(byte[] data, float time)
        {

        }

        public void Read(byte[] data, float time)
        {
            switch(_httpState)
            {
                case HttpState.GetBinary:
                    HandleGetBinary(data, time);
                    break;
                case HttpState.GetText:
                    HandleGetText(data, time);
                    break;
                case HttpState.Done:
                    break;
            }
        }

        private void HandleGetBinary(byte[] data, float time)
        {
            HandleBinaryPacket(data, time);
            _httpState = HttpState.Done;
        }

        private void HandleGetText(byte[] data, float time)
        {
            HandleTextPacket(Encoding.UTF8.GetString(data), time);
            _httpState = HttpState.Done;
        }

        private void HandleDone(byte[] data, float time)
        {
            if(data[0] == 'H' && data[1] == 'T' && data[2] == 'T' && data[3] == 'P')
            {
                HandleHttp(data, time);
            }
            else if(data[0] == 'G' && data[1] == 'E' && data[2] == 'T' && data[3] == ' ')
            {
                var requestText = Encoding.UTF8.GetString(data);
                if(requestText.StartsWith("GET /observer-mode/rest/consumer/getGameDataChunk"))
                {
                    _httpState = HttpState.GetBinary;
                }
                else
                {
                    _httpState = HttpState.GetText;
                }
            }
            else
            {
                throw new IOException("Bad chunk start!");
            }
        }

        private void HandleHttp(byte[] data, float time)
        {

        }
    }
}
