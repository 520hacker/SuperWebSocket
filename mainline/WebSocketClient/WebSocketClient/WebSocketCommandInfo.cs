﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;
using SuperWebSocket.WebSocketClient.Protocol;

namespace SuperWebSocket.WebSocketClient
{
    public class WebSocketCommandInfo : ICommandInfo
    {
        public WebSocketCommandInfo()
        {

        }

        public WebSocketCommandInfo(string key)
        {
            Key = key;
        }

        public WebSocketCommandInfo(string key, string text)
        {
            Key = key;
            Text = text;
        }

        public WebSocketCommandInfo(IList<WebSocketDataFrame> frames)
        {
            var opCode = frames[0].OpCode;
            Key = opCode.ToString();

            int offset, length;

            if (opCode != 2)
            {
                var stringBuilder = new StringBuilder();

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                    length = (int)frame.ActualPayloadLength;

                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    stringBuilder.Append(frame.InnerData.Decode(Encoding.UTF8, offset, length));
                }

                Text = stringBuilder.ToString();
            }
            else
            {
                var resultBuffer = new byte[frames.Sum(f => (int)f.ActualPayloadLength)];

                int copied = 0;

                for (var i = 0; i < frames.Count; i++)
                {
                    var frame = frames[i];

                    offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;
                    length = (int)frame.ActualPayloadLength;

                    if (frame.HasMask)
                    {
                        frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
                    }

                    frame.InnerData.CopyTo(resultBuffer, offset, copied, length);
                }

                Data = resultBuffer;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketCommandInfo"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="left">The left.</param>
        public WebSocketCommandInfo(WebSocketDataFrame frame)
        {
            Key = frame.OpCode.ToString();

            int length = (int)frame.ActualPayloadLength;
            int offset = frame.InnerData.Count - (int)frame.ActualPayloadLength;


            if (frame.HasMask && length > 0)
            {
                frame.InnerData.DecodeMask(frame.MaskKey, offset, length);
            }

            if (frame.OpCode != 2)
            {
                if (length > 0)
                    Text = frame.InnerData.Decode(Encoding.UTF8, offset, length);
                else
                    Text = string.Empty;
            }
            else
            {
                if (length > 0)
                    Data = frame.InnerData.ToArrayData(offset, length);
                else
                    Data = new byte[0];
            }
        }

        public string Key { get; set; }

        public byte[] Data { get; set; }

        public string Text { get; set; }
    }
}
